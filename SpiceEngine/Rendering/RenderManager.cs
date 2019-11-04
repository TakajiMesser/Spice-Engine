﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SpiceEngine.Entities.Selection;
using SpiceEngine.Maps;
using SpiceEngine.Rendering.PostProcessing;
using SpiceEngine.Rendering.Processing;
using SpiceEngine.Rendering.Textures;
using SpiceEngine.Utilities;
using SpiceEngineCore.Components.Animations;
using SpiceEngineCore.Entities;
using SpiceEngineCore.Entities.Cameras;
using SpiceEngineCore.Entities.Layers;
using SpiceEngineCore.Entities.Lights;
using SpiceEngineCore.Entities.Volumes;
using SpiceEngineCore.Game.Loading;
using SpiceEngineCore.Game.Loading.Builders;
using SpiceEngineCore.Helpers;
using SpiceEngineCore.Maps;
using SpiceEngineCore.Outputs;
using SpiceEngineCore.Rendering;
using SpiceEngineCore.Rendering.Batches;
using SpiceEngineCore.Rendering.Billboards;
using SpiceEngineCore.Rendering.Meshes;
using SpiceEngineCore.Rendering.Models;
using SpiceEngineCore.Rendering.Textures;
using SpiceEngineCore.Rendering.Vertices;
using SpiceEngineCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SpiceEngine.Rendering
{
    public enum RenderModes
    {
        Wireframe,
        Diffuse,
        Lit,
        Full
    }

    public class RenderManager : ComponentLoader<IRenderableBuilder>, IGridRenderer
    {
        public RenderModes RenderMode { get; set; }
        public Resolution Resolution { get; private set; }
        public Resolution WindowSize { get; private set; }
        public double Frequency { get; internal set; }
        public bool RenderGrid { get; set; }

        public BatchManager BatchManager { get; private set; }
        public TextureManager TextureManager { get; } = new TextureManager();

        // TODO - Make this less janky
        public bool IsInEditorMode { get; set; }

        private IEntityProvider _entityProvider;
        private IAnimationProvider _animationProvider;
        private ISelectionProvider _selectionProvider;
        private ICamera _camera;

        //private ForwardRenderer _forwardRenderer = new ForwardRenderer();
        private DeferredRenderer _deferredRenderer = new DeferredRenderer();
        private WireframeRenderer _wireframeRenderer = new WireframeRenderer();
        private ShadowRenderer _shadowRenderer = new ShadowRenderer();
        private LightRenderer _lightRenderer = new LightRenderer();
        private SkyboxRenderer _skyboxRenderer = new SkyboxRenderer();
        private SelectionRenderer _selectionRenderer = new SelectionRenderer();
        private BillboardRenderer _billboardRenderer = new BillboardRenderer();

        private FXAARenderer _fxaaRenderer = new FXAARenderer();
        private Blur _blurRenderer = new Blur();
        private InvertColors _invertRenderer = new InvertColors();
        private TextRenderer _textRenderer = new TextRenderer();
        private RenderToScreen _renderToScreen = new RenderToScreen();

        private LogManager _logManager;

        private IInvoker _invoker;
        public IInvoker Invoker
        {
            get => _invoker;
            set
            {
                _invoker = value;
                TextureManager.Invoker = value;
            }
        }

        public RenderManager(Resolution resolution, Resolution windowSize)
        {
            Resolution = resolution;
            WindowSize = windowSize;

            _logManager = new LogManager(_textRenderer);
        }

        public void SetEntityProvider(IEntityProvider entityProvider)
        {
            _entityProvider = entityProvider;
            BatchManager = new BatchManager(_entityProvider, TextureManager);

            if (_animationProvider != null)
            {
                BatchManager.SetAnimationProvider(_animationProvider);
            }
        }

        public void SetAnimationProvider(IAnimationProvider animationProvider)
        {
            _animationProvider = animationProvider;
            BatchManager?.SetAnimationProvider(_animationProvider);
        }

        public void SetSelectionProvider(ISelectionProvider selectionProvider) => _selectionProvider = selectionProvider;
        public void SetCamera(ICamera camera) => _camera = camera;

        public void LoadFromMap(Map map) => _skyboxRenderer.SetTextures(map.SkyboxTextureFilePaths);

        public override void AddComponent(int entityID, IRenderableBuilder builder)
        {
            var renderable = builder.ToRenderable();

            if (renderable is IBillboard billboard)
            {
                billboard.LoadTexture(TextureManager);
            }
            else if (renderable is ITexturedMesh texturedMesh)
            {
                if (builder is MapBrush mapBrush)
                {
                    texturedMesh.TextureMapping = mapBrush.TexturesPaths.ToTextureMapping(TextureManager);
                }    
            }
            else if (renderable is IModel model)
            {
                if (builder is MapActor mapActor)
                {
                    using (var importer = new Assimp.AssimpContext())
                    {
                        var scene = importer.ImportFile(mapActor.ModelFilePath);

                        for (var i = 0; i < model.Meshes.Count; i++)
                        {
                            if (model.Meshes[i] is ITexturedMesh modelTexturedMesh)
                            {
                                var textureMapping = i < mapActor.TexturesPaths.Count
                                    ? mapActor.TexturesPaths[i].ToTextureMapping(TextureManager)
                                    : scene.Materials[scene.Meshes[i].MaterialIndex].ToTexturePaths(Path.GetDirectoryName(mapActor.ModelFilePath)).ToTextureMapping(TextureManager);

                                modelTexturedMesh.TextureMapping = textureMapping;
                            }
                        }
                    }
                }
            }

            AddEntity(entityID, renderable);
        }

        protected override Task LoadInternal()
        {
            // TODO - If Invoker is null, queue up this action
            return Invoker.RunAsync(() =>
            {
                //_forwardRenderer.Load(Resolution);
                _deferredRenderer.Load(Resolution);
                _wireframeRenderer.Load(Resolution);
                _shadowRenderer.Load(Resolution);
                _lightRenderer.Load(Resolution);
                _skyboxRenderer.Load(Resolution);
                _selectionRenderer.Load(Resolution);
                _billboardRenderer.Load(Resolution);
                _fxaaRenderer.Load(Resolution);
                _blurRenderer.Load(Resolution);
                _invertRenderer.Load(Resolution);
                _textRenderer.Load(Resolution);
                _renderToScreen.Load(WindowSize);

                GL.ClearColor(Color4.Black);
            });
        }

        protected override void LoadComponents()
        {
            // TODO - If Invoker is null, queue up this action
            Invoker.RunAsync(() =>
            {
                BatchManager.Load();
            });
        }

        public void AddEntity(int entityID, IRenderable renderable)
        {
            if (IsInEditorMode && renderable is IMesh mesh)
            {
                var entity = _entityProvider.GetEntity(entityID);

                if (entity is ITexturePath texturePath && entity is ITextureBinder textureBinder)
                {
                    var textureMapping = texturePath.TexturePaths.ToTextureMapping(TextureManager);
                    textureBinder.AddTextureMapping(textureMapping);
                }

                var colorID = SelectionHelper.GetColorFromID(entityID);
                var vertices = mesh.Vertices.Select(v => new EditorVertex3D(v, colorID)).ToList();

                BatchManager.AddEntity(entityID, new Mesh<EditorVertex3D>(vertices, mesh.TriangleIndices.ToList()));
            }
            else if (IsInEditorMode && renderable is IModel model)
            {
                // TODO - Do I not need to do this for IModels?
                /*var entity = _entityProvider.GetEntity(entityID);

                if (entity is ITexturePath texturePath && entity is ITextureBinder textureBinder)
                {
                    var textureMapping = texturePath.TexturePaths.ToTextureMapping(TextureManager);
                    textureBinder.AddTextureMapping(textureMapping);
                }*/

                var colorID = SelectionHelper.GetColorFromID(entityID);
                for (var i = 0; i < model.Meshes.Count; i++)
                {
                    var vertices = model.Meshes[i].Vertices.Select(v => new EditorVertex3D(v, colorID)).ToList();
                    model.Meshes[i] = new Mesh<EditorVertex3D>(vertices, model.Meshes[i].TriangleIndices.ToList());
                }

                BatchManager.AddEntity(entityID, renderable);
            }
            else if (IsInEditorMode && renderable is IBillboard billboard)
            {
                billboard.SetColor(SelectionHelper.GetColorFromID(entityID));
            }
            else
            {
                BatchManager.AddEntity(entityID, renderable);
            }
        }

        public void RemoveEntity(int entityID) => BatchManager.RemoveByEntityID(entityID);

        public void ResizeResolution()
        {
            if (IsLoaded)
            {
                //_forwardRenderer.ResizeTextures(Resolution);
                _deferredRenderer.ResizeTextures(Resolution);
                _wireframeRenderer.ResizeTextures(Resolution);
                _shadowRenderer.ResizeTextures(Resolution);
                _skyboxRenderer.ResizeTextures(Resolution);
                _lightRenderer.ResizeTextures(Resolution);
                _selectionRenderer.ResizeTextures(Resolution);
                _billboardRenderer.ResizeTextures(Resolution);
                _fxaaRenderer.ResizeTextures(Resolution);
                _blurRenderer.ResizeTextures(Resolution);
                _invertRenderer.ResizeTextures(Resolution);
                _textRenderer.ResizeTextures(Resolution);
            }
        }

        public void ResizeWindow()
        {
            if (IsLoaded)
            {
                _renderToScreen.ResizeTextures(WindowSize);
            }
        }

        /*public void RenderEntityIDs(Volume volume)
        {
            _selectionRenderer.BindForWriting();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            //_selectionRenderer.SelectionPass();
        }*/

        public void SetSelected(IEnumerable<int> entityIDs)
        {
            foreach (var entityID in entityIDs)
            {
                // TODO - Handle light selection differently, since lights are not stored in the BatchManager
                BatchManager.UpdateVertices(entityID, v => ((EditorVertex3D)v).Selected());
            }
        }

        public void SetDeselected(IEnumerable<int> entityIDs)
        {
            foreach (var entityID in entityIDs)
            {
                BatchManager.UpdateVertices(entityID, v => ((EditorVertex3D)v).Deselected());
            }
        }

        public int GetEntityIDFromPoint(Vector2 point)
        {
            _selectionRenderer.BindForReading();
            return _selectionRenderer.GetEntityIDFromPoint(point);
        }

        public void RenderSelection(IEnumerable<IEntity> entities, TransformModes transformMode)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.DepthFunc(DepthFunction.Always);

            foreach (var entity in entities)
            {
                if (entity is ILight light)
                {
                    var lightMesh = _lightRenderer.GetMeshForLight(light);
                    _wireframeRenderer.SelectionPass(_camera, light, lightMesh);
                    _billboardRenderer.RenderSelection(_camera, light);
                }
                else if (entity is Volume volume)
                {
                    // TODO - Render volumes (need to add mesh to BatchManager)
                    /*_wireframeRenderer.SelectionPass(entityProvider, camera, entity, BatchManager);
                    _billboardRenderer.RenderSelection(camera, volume, BatchManager);

                    _selectionRenderer.BindForWriting();
                    _billboardRenderer.RenderSelection(camera, volume, BatchManager);*/
                }
                else
                {
                    // TODO - Find out why selection appears to be updating ahead of entity
                    //_wireframeRenderer.SelectionPass(_entityProvider, _camera, entity, BatchManager);
                }
            }

            var lastEntity = entities.LastOrDefault();
            if (lastEntity != null)
            {
                // Render the RGB arrows over the selection
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.DepthFunc(DepthFunction.Less);

                RenderTransform(lastEntity, transformMode);

                // Render the RGB arrows into the selection buffer as well, which means that R, G, and B are "reserved" ID colors
                _selectionRenderer.BindForWriting();
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.DepthFunc(DepthFunction.Less);

                RenderTransform(lastEntity, transformMode);
            }
        }

        private void RenderTransform(IEntity entity, TransformModes transformMode)
        {
            switch (transformMode)
            {
                case TransformModes.Translate:
                    if (entity is IDirectional directional)
                    {
                        _selectionRenderer.RenderTranslationArrows(_camera, entity.Position, directional.XDirection, directional.YDirection, directional.ZDirection);
                    }
                    else
                    {
                        _selectionRenderer.RenderTranslationArrows(_camera, entity.Position);
                    }
                    break;
                case TransformModes.Rotate:
                    /*if (entity is IDirectional directional)
                    {
                        _selectionRenderer.RenderRotationRings(_camera, entity.Position, directional.XDirection, directional.YDirection, directional.ZDirection);
                    }
                    else
                    {*/
                        _selectionRenderer.RenderRotationRings(_camera, entity.Position);
                    //}
                    break;
                case TransformModes.Scale:
                    /*if (entity is IDirectional directional)
                    {
                        _selectionRenderer.RenderScaleLines(_camera, entity.Position, directional.XDirection, directional.YDirection, directional.ZDirection);
                    }
                    else
                    {*/
                        _selectionRenderer.RenderScaleLines(_camera, entity.Position);
                    //}
                    break;
            }
        }

        public void RotateGrid(float pitch, float yaw, float roll) => _wireframeRenderer.GridRotation = Quaternion.FromEulerAngles(pitch, yaw, roll);

        public void SetWireframeThickness(float thickness) => _wireframeRenderer.LineThickness = thickness;
        public void SetWireframeColor(Color4 color) => _wireframeRenderer.LineColor = color.ToVector4();
        public void SetSelectedWireframeThickness(float thickness) => _wireframeRenderer.SelectedLineThickness = thickness;
        public void SetSelectedWireframeColor(Color4 color) => _wireframeRenderer.SelectedLineColor = color.ToVector4();
        public void SetSelectedLightWireframeThickness(float thickness) => _wireframeRenderer.SelectedLightLineThickness = thickness;
        public void SetSelectedLightWireframeColor(Color4 color) => _wireframeRenderer.SelectedLightLineColor = color.ToVector4();

        public void SetGridUnit(float unit) => _wireframeRenderer.GridUnit = unit;
        public void SetGridLineThickness(float thickness) => _wireframeRenderer.GridLineThickness = thickness;
        public void SetGridUnitColor(Color4 color) => _wireframeRenderer.GridLineUnitColor = color.ToVector4();
        public void SetGridAxisColor(Color4 color) => _wireframeRenderer.GridLineAxisColor = color.ToVector4();
        public void SetGrid5Color(Color4 color) => _wireframeRenderer.GridLine5Color = color.ToVector4();
        public void SetGrid10Color(Color4 color) => _wireframeRenderer.GridLine10Color = color.ToVector4();

        //public bool RenderWireframe { get; set; }
        public bool LogToScreen { get; set; }

        protected override void Update()
        {
            // TODO - It would be better to have separate objects run different Update() implementations than to branch every loop iteration here
            switch (RenderMode)
            {
                case RenderModes.Wireframe:
                    RenderWireframe();
                    break;
                case RenderModes.Diffuse:
                    RenderDiffuseFrame();
                    break;
                case RenderModes.Lit:
                    RenderLitFrame();
                    break;
                case RenderModes.Full:
                    RenderFullFrame();
                    break;
            }

            if (IsInEditorMode)
            {
                RenderEntityIDs();

                // TODO - Determine how to handle this
                /*if (SelectionManager.SelectionCount > 0)
                {
                    _renderManager.RenderSelection(SelectionManager.SelectedEntities, TransformMode);
                }*/
            }
        }

        private void RenderEntityIDs()
        {
            _selectionRenderer.BindForWriting();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            _selectionRenderer.SelectionPass(_camera, BatchManager, _entityProvider.LayerProvider.GetEntityIDs(LayerTypes.Select));
            _billboardRenderer.RenderLightSelectIDs(_camera, _entityProvider.Lights.Where(l => _entityProvider.LayerProvider.GetEntityIDs(LayerTypes.Select).Contains(l.ID)));

            var vertexEntities = _entityProvider.LayerProvider.GetLayerEntityIDs("Vertices");
            if (vertexEntities.Any())
            {
                _billboardRenderer.RenderVertexSelectIDs(_camera, vertexEntities.Select(v => _entityProvider.GetEntity(v)));
            }
        }

        public void RenderWireframe()
        {
            _wireframeRenderer.BindForWriting();
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            if (RenderGrid)
            {
                _wireframeRenderer.RenderGridLines(_camera);
            }

            _wireframeRenderer.WireframePass(_camera, BatchManager);

            GL.Enable(EnableCap.CullFace);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            _billboardRenderer.RenderLights(_camera, _entityProvider.Lights);

            GL.Disable(EnableCap.DepthTest);

            _fxaaRenderer.Render(_wireframeRenderer.FinalTexture);
            _renderToScreen.Render(_fxaaRenderer.FinalTexture);
            _logManager.RenderToScreen();
        }

        public void RenderDiffuseFrame()
        {
            _deferredRenderer.BindForGeometryWriting();
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            _deferredRenderer.GeometryPass(_camera, BatchManager);

            _deferredRenderer.BindForDiffuseWriting();

            if (RenderGrid)
            {
                GL.Disable(EnableCap.CullFace);
                _wireframeRenderer.RenderGridLines(_camera);
                GL.Enable(EnableCap.CullFace);
            }

            _skyboxRenderer.Render(_camera);
            _billboardRenderer.GeometryPass(_camera, BatchManager);
            _billboardRenderer.RenderLights(_camera, _entityProvider.Lights);

            _deferredRenderer.BindForLitTransparentWriting();

            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcColor);
            GL.Disable(EnableCap.CullFace);

            _deferredRenderer.TransparentGeometryPass(_camera, BatchManager);

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

            GL.Disable(EnableCap.DepthTest);

            _renderToScreen.Render(_deferredRenderer.ColorTexture);
            _logManager.RenderToScreen();

            if (IsInEditorMode && _selectionProvider != null && _selectionProvider.SelectionCount > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

                GL.Disable(EnableCap.CullFace);
                GL.Enable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
                GL.DepthFunc(DepthFunction.Always);

                _wireframeRenderer.SelectionPass(_camera, _selectionProvider.SelectedIDs, BatchManager);
            }
        }

        public void RenderLitFrame()
        {
            _deferredRenderer.BindForGeometryWriting();
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            _deferredRenderer.GeometryPass(_camera, BatchManager);

            RenderLights();

            _deferredRenderer.BindForLitWriting();

            _skyboxRenderer.Render(_camera);
            _billboardRenderer.RenderLights(_camera, _entityProvider.Lights);

            if (RenderGrid)
            {
                GL.Disable(EnableCap.CullFace);
                _wireframeRenderer.RenderGridLines(_camera);
                GL.Enable(EnableCap.CullFace);
            }

            _deferredRenderer.BindForLitTransparentWriting();

            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcColor);
            GL.Disable(EnableCap.CullFace);

            _deferredRenderer.TransparentGeometryPass(_camera, BatchManager);

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

            GL.Disable(EnableCap.DepthTest);

            _renderToScreen.Render(_deferredRenderer.FinalTexture);
            _logManager.RenderToScreen();

            if (IsInEditorMode && _selectionProvider != null && _selectionProvider.SelectionCount > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

                GL.Disable(EnableCap.CullFace);
                GL.Enable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
                GL.DepthFunc(DepthFunction.Always);

                _wireframeRenderer.SelectionPass(_camera, _selectionProvider.SelectedIDs, BatchManager);
            }
        }

        public void RenderFullFrame()
        {
            _deferredRenderer.BindForGeometryWriting();
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            _deferredRenderer.GeometryPass(_camera, BatchManager);

            RenderLights();

            _deferredRenderer.BindForLitWriting();
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);
            _skyboxRenderer.Render(_camera);

            _deferredRenderer.BindForLitTransparentWriting();

            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcColor);
            GL.Disable(EnableCap.CullFace);

            _deferredRenderer.TransparentGeometryPass(_camera, BatchManager);

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

            RenderUIControls();
            _logManager.RenderToScreen();
        }

        private void RenderLights()
        {
            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            foreach (var light in _entityProvider.Lights)
            {
                var lightMesh = _lightRenderer.GetMeshForLight(light);
                _lightRenderer.StencilPass(light, _camera, lightMesh);

                GL.Disable(EnableCap.Blend);
                _shadowRenderer.Render(_camera, light, BatchManager);
                GL.Enable(EnableCap.Blend);

                _deferredRenderer.BindForLitWriting();
                GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

                var lightProgram = _lightRenderer.GetProgramForLight(light);
                var shadowMap = (light is PointLight) ? _shadowRenderer.PointDepthCubeMap : _shadowRenderer.SpotDepthTexture;
                _lightRenderer.LightPass(_deferredRenderer, light, _camera, lightMesh, shadowMap, lightProgram);
            }

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Disable(EnableCap.StencilTest);
            GL.Disable(EnableCap.Blend);
        }

        private void RenderUIControls()
        {
            foreach (var control in _entityProvider.Controls)
            {

            }

            _textRenderer.RenderText("FPS: " + Frequency.ToString("0.##"), Resolution.Width - 9 * (10 + TextRenderer.GLYPH_WIDTH), Resolution.Height - (10 + TextRenderer.GLYPH_HEIGHT), 1.0f);
        }
    }
}
