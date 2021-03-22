﻿using CitrusAnimationCore.Animations;
using SpiceEngine.GLFWBindings;
using SpiceEngine.GLFWBindings.GLEnums;
using SpiceEngine.Maps;
using SpiceEngine.Rendering.Batches;
using SpiceEngine.Rendering.PostProcessing;
using SpiceEngine.Utilities;
using SpiceEngineCore.Entities;
using SpiceEngineCore.Entities.Layers;
using SpiceEngineCore.Entities.Lights;
using SpiceEngineCore.Game;
using SpiceEngineCore.Game.Loading.Builders;
using SpiceEngineCore.Geometry;
using SpiceEngineCore.Maps;
using SpiceEngineCore.Rendering;
using StarchUICore;
using SweetGraphicsCore.Renderers.PostProcessing;
using SweetGraphicsCore.Renderers.Processing;
using SweetGraphicsCore.Rendering;
using SweetGraphicsCore.Rendering.Billboards;
using SweetGraphicsCore.Rendering.Meshes;
using SweetGraphicsCore.Rendering.Models;
using SweetGraphicsCore.Rendering.Textures;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TangyHIDCore.Outputs;

namespace SpiceEngine.Rendering
{
    public class RenderManager : RenderableLoader, IRender, IRenderProvider, IRenderContextProvider
    {
        private IAnimationProvider _animationProvider;
        private IUIProvider _uiProvider;
        private IInvoker _invoker;

        protected BatchManager _batchManager;

        protected LightRenderer _lightRenderer = new LightRenderer();
        protected BillboardRenderer _billboardRenderer = new BillboardRenderer();
        protected SelectionRenderer _selectionRenderer = new SelectionRenderer();
        protected TextRenderer _textRenderer = new TextRenderer();
        protected DeferredRenderer _deferredRenderer = new DeferredRenderer();
        protected ShadowRenderer _shadowRenderer = new ShadowRenderer();
        protected SkyboxRenderer _skyboxRenderer = new SkyboxRenderer();
        protected FXAARenderer _fxaaRenderer = new FXAARenderer();
        protected Blur _blurRenderer = new Blur();
        protected InvertColors _invertRenderer = new InvertColors();
        protected RenderToScreen _renderToScreen = new RenderToScreen();
        protected UIRenderer _uiRenderer = new UIRenderer();

        public RenderManager(IRenderContext context, Display display)
        {
            CurrentContext = context;
            Display = display;

            // TODO - We likely want to split the resolution by nCameras for splitscreen support
            Display.Window.ResolutionChanged += (s, args) =>
            {
                // TODO - This no longer works with our _entityProvider.ActiveCamera property
                foreach (var camera in _entityProvider.Cameras.Where(c => c.IsActive))
                {
                    camera.UpdateAspectRatio(args.Resolution.AspectRatio);
                }
            };

            TextureManager = new TextureManager(this);
            FontManager = new FontManager(this, TextureManager);
        }

        public IRenderContext CurrentContext { get; }
        public Display Display { get; }
        public double Frequency { get; set; }
        public bool RenderGrid { get; set; }
        public TextureManager TextureManager { get; }
        public FontManager FontManager { get; }
        public IInvoker Invoker
        {
            get => _invoker;
            set
            {
                _invoker = value;
                TextureManager.Invoker = value;
            }
        }

        public IRenderable GetRenderable(int entityID) => _componentByID[entityID];
        public IRenderable GetRenderableOrDefault(int entityID) => HasRenderable(entityID) ? GetRenderable(entityID) : default;

        public bool HasRenderable(int entityID) => _componentByID.ContainsKey(entityID);

        public override void SetEntityProvider(IEntityProvider entityProvider)
        {
            base.SetEntityProvider(entityProvider);

            /*_entityProvider.EntitiesAdded += (s, args) =>
            {
                foreach (var builder in args.Builders)
                {
                    builder.Item2.
                }
            };*/
            /*foreach (var camera in _entityProvider.Cameras.Where(c => c.IsActive))
            {
                camera.UpdateAspectRatio(Resolution.AspectRatio);
            }*/

            _batchManager = new BatchManager(_entityProvider, TextureManager);

            if (_animationProvider != null)
            {
                _batchManager.SetAnimationProvider(_animationProvider);
            }

            if (_uiProvider != null)
            {
                _batchManager.SetUIProvider(_uiProvider);
            }
        }

        public void SetAnimationProvider(IAnimationProvider animationProvider)
        {
            _animationProvider = animationProvider;
            _batchManager?.SetAnimationProvider(_animationProvider);
        }

        public void SetUIProvider(IUIProvider uiProvider)
        {
            _uiProvider = uiProvider;
            _uiProvider.SetTextureProvider(TextureManager);
            _batchManager?.SetUIProvider(_uiProvider);
        }

        public void LoadFromMap(IMap map)
        {
            if (map is Map castMap)
            {
                _skyboxRenderer.SetTextures(castMap.SkyboxTextureFilePaths);
            }
        }

        public override Task LoadBuilderAsync(int entityID, IRenderableBuilder builder) => Task.Run(() =>
        {
            var component = builder.ToRenderable();

            if (component != null)
            {
                if (component is IBillboard billboard)
                {
                    billboard.LoadTexture(TextureManager);
                }
                else if (component is ITexturedMesh texturedMesh)
                {
                    if (builder is ITexturePather texturePather)
                    {
                        var texturePaths = texturePather.TexturesPaths.FirstOrDefault();

                        if (texturePaths != null && !texturePaths.IsEmpty)
                        {
                            texturedMesh.TextureMapping = texturePaths.ToTextureMapping(TextureManager);
                        }
                    }
                }
                else if (component is IModel model)
                {
                    if (builder is ITexturePather texturePather)
                    {
                        if (builder is IModelPather modelPather && !string.IsNullOrEmpty(modelPather.ModelFilePath))
                        {
                            using (var importer = new Assimp.AssimpContext())
                            {
                                var scene = importer.ImportFile(modelPather.ModelFilePath);

                                for (var i = 0; i < model.Meshes.Count; i++)
                                {
                                    if (model.Meshes[i] is ITexturedMesh modelTexturedMesh)
                                    {
                                        var textureMapping = i < texturePather.TexturesPaths.Count
                                            ? texturePather.TexturesPaths[i].ToTextureMapping(TextureManager)
                                            : scene.Materials[scene.Meshes[i].MaterialIndex].ToTexturePaths(Path.GetDirectoryName(modelPather.ModelFilePath)).ToTextureMapping(TextureManager);

                                        modelTexturedMesh.TextureMapping = textureMapping;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (var i = 0; i < model.Meshes.Count; i++)
                            {
                                if (model.Meshes[i] is ITexturedMesh modelTexturedMesh && i < texturePather.TexturesPaths.Count)
                                {
                                    modelTexturedMesh.TextureMapping = texturePather.TexturesPaths[i].ToTextureMapping(TextureManager);
                                }
                            }
                        }
                    }
                }

                _componentAndIDQueue.Enqueue(Tuple.Create(component, entityID));
            }
        });

        protected override Task LoadInitial()
        {
            // TODO - If Invoker is null, queue up this action
            return Invoker.RunAsync(() =>
            {
                _deferredRenderer.Load(this, Display.Resolution);
                _shadowRenderer.Load(this, Display.Resolution);
                _lightRenderer.Load(this, Display.Resolution);
                _skyboxRenderer.Load(this, Display.Resolution);
                _billboardRenderer.Load(this, Display.Resolution);
                _selectionRenderer.Load(this, Display.Resolution);
                _fxaaRenderer.Load(this, Display.Resolution);
                _blurRenderer.Load(this, Display.Resolution);
                _invertRenderer.Load(this, Display.Resolution);
                _textRenderer.Load(this, Display.Resolution);
                _renderToScreen.Load(this, Display.Window);
                _uiRenderer.Load(this, Display.Window);

                //var font = FontManager.AddFontFile(TextRenderer.FONT_PATH, 14);
                //_logManager.SetFont(font);

                //GL.ClearColor(OpenTK.Graphics.Color4.Black);
            });
        }

        protected override void LoadComponents()
        {
            try
            {
                base.LoadComponents();

                // TODO - If Invoker is null, queue up this action
                //Invoker.RunSync(() => _batchManager.Load());
                //Invoker.ForceUpdate();
                //Invoker.RunAsync(() => _batchManager.Load()).ContinueWith(t => Invoker.ForceUpdate());
                Invoker.RunAsync(() =>
                {
                    try
                    {
                        _batchManager.Load(this);
                        //_uiProvider.Load();

                        Invoker.ForceUpdate();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // TODO - Can we remove this call?
        public void LoadBatcher() => _batchManager.Load(this);

        protected override void LoadComponent(int entityID, IRenderable component)
        {
            base.LoadComponent(entityID, component);
            _batchManager.AddEntity(entityID, component);
        }

        public void RemoveEntity(int entityID) => _batchManager.RemoveByEntityID(entityID);

        public void Duplicate(int entityID, int duplicateID) => _batchManager.DuplicateBatch(entityID, duplicateID);

        protected override void Update()
        {
            RenderFullFrame();
            RenderEntityIDs();
        }

        public void RenderFullFrame()
        {
            _deferredRenderer.BindForGeometryWriting();
            GL.Viewport(0, 0, Display.Resolution.Width, Display.Resolution.Height);
            var camera = _entityProvider.ActiveCamera;

            _deferredRenderer.GeometryPass(camera, _batchManager);

            RenderLights();

            _deferredRenderer.BindForLitWriting();
            GL.Viewport(0, 0, Display.Resolution.Width, Display.Resolution.Height);
            _skyboxRenderer.Render(camera);

            _deferredRenderer.BindForLitTransparentWriting();

            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationModeEXT.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcColor);
            GL.Disable(EnableCap.CullFace);

            _deferredRenderer.TransparentGeometryPass(camera, _batchManager);

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

            /*if (IsInEditorMode && _selectionProvider != null && _selectionProvider.SelectionCount > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

                GL.Disable(EnableCap.CullFace);
                GL.Enable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
                GL.DepthFunc(DepthFunction.Always);

                _wireframeRenderer.SelectionPass(_camera, _selectionProvider.SelectedIDs, BatchManager);
            }*/

            // Read from GBuffer's final texture, so that we can post-process it
            //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _deferredRenderer.GBuffer._handle);
            //GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            var texture = _deferredRenderer.FinalTexture;

            GL.Disable(EnableCap.DepthTest);

            //_invertRenderer.Render(texture);
            _blurRenderer.Render(texture, _deferredRenderer.VelocityTexture, 60.0f);
            texture = _blurRenderer.FinalTexture;
            
            _renderToScreen.Render(texture);

            RenderUI();
            //_logManager.RenderToScreen();
        }

        protected virtual void RenderEntityIDs()
        {
            // TODO - Perform check first to see if ANY selection IDs (via layers) even exist...
            _selectionRenderer.BindForWriting();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, Display.Resolution.Width, Display.Resolution.Height);

            var camera = _entityProvider.ActiveCamera;
            _selectionRenderer.SelectionPass(camera, _batchManager, _entityProvider.LayerProvider.GetEntityIDs(LayerTypes.Select));
            _billboardRenderer.RenderLightSelectIDs(camera, _entityProvider.Lights.Where(l => _entityProvider.LayerProvider.GetEntityIDs(LayerTypes.Select).Contains(l.ID)));
            _uiRenderer.RenderSelections(_batchManager, _uiProvider);
        }

        public int GetEntityIDFromSelection(Vector2 coordinates)
        {
            // Mouse coordinates are from top-left
            var mouseCoordinates = coordinates;

            // We need to convert these to instead be from bottom-left
            var windowCoordinates = new Vector2(mouseCoordinates.X, Display.Window.Height - mouseCoordinates.Y);

            // We now need to convert these coordinates from window-size to resolution-size
            var resolutionCoordinates = new Vector2(Display.Resolution.Width * windowCoordinates.X / Display.Window.Width, Display.Resolution.Height * windowCoordinates.Y / Display.Window.Height);

            _selectionRenderer.BindForReading();
            return _selectionRenderer.GetEntityIDFromPoint(resolutionCoordinates);
        }

        private void RenderLights()
        {
            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationModeEXT.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            var camera = _entityProvider.ActiveCamera;

            foreach (var light in _entityProvider.Lights)
            {
                var lightMesh = _lightRenderer.GetMeshForLight(light);
                _lightRenderer.StencilPass(light, camera, lightMesh);

                GL.Disable(EnableCap.Blend);
                _shadowRenderer.Render(camera, light, _batchManager);
                GL.Enable(EnableCap.Blend);

                _deferredRenderer.BindForLitWriting();
                GL.Viewport(0, 0, Display.Resolution.Width, Display.Resolution.Height);

                var lightProgram = _lightRenderer.GetProgramForLight(light);
                var shadowMap = (light is PointLight) ? _shadowRenderer.PointDepthCubeMap : _shadowRenderer.SpotDepthTexture;
                _lightRenderer.LightPass(_deferredRenderer, light, camera, lightMesh, shadowMap, lightProgram);
            }

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Disable(EnableCap.StencilTest);
            GL.Disable(EnableCap.Blend);
        }

        // TODO - Remove this test code and set fonts up more appropriately
        private bool _isFontSet = false;

        private void RenderUI()
        {
            _uiRenderer.Render(_batchManager, _uiProvider);

            //var font = FontManager.GetFont(TextRenderer.FONT_PATH);

            /*if (!_isFontSet)
            {
                var entityID = _entityProvider.GetEntity("Text 2C").ID;
                var uiElement = _uiProvider.GetUIElement(entityID);
                
                if (uiElement is Label textView)
                {
                    textView.Font = font;
                }

                _isFontSet = true;
            }*/

            _textRenderer.Render(_batchManager, _uiProvider);

            //var x = Resolution.Width - 9 * (10 + font.GlyphWidth);
            //var y = /*Resolution.Height - */(10 + font.GlyphHeight);
            //var y = 100;
            //_textRenderer.RenderText(font, "FPS: " + Frequency.ToString("0.##"), x, y);
        }
    }
}
