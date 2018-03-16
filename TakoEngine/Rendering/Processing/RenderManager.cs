﻿using TakoEngine.GameObjects;
using TakoEngine.Helpers;
using TakoEngine.Lighting;
using TakoEngine.Meshes;
using TakoEngine.Outputs;
using TakoEngine.Rendering.Buffers;
using TakoEngine.Rendering.Matrices;
using TakoEngine.Rendering.PostProcessing;
using TakoEngine.Rendering.Shaders;
using TakoEngine.Rendering.Textures;
using TakoEngine.Rendering.Vertices;
using TakoEngine.Utilities;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakoEngine.Rendering.Processing
{
    public enum RenderModes
    {
        Wireframe,
        Diffuse,
        Lit,
        Full
    }

    public class RenderManager
    {
        public Resolution Resolution { get; private set; }
        public double Frequency { get; internal set; }

        private ForwardRenderer _forwardRenderer = new ForwardRenderer();
        private DeferredRenderer _deferredRenderer = new DeferredRenderer();
        private WireframeRenderer _wireframeRenderer = new WireframeRenderer();
        private ShadowRenderer _shadowRenderer = new ShadowRenderer();
        private LightRenderer _lightRenderer = new LightRenderer();
        private SkyboxRenderer _skyboxRenderer = new SkyboxRenderer();
        private SelectionRenderer _selectionRenderer = new SelectionRenderer();

        private Blur _blurRenderer = new Blur();
        private InvertColors _invertRenderer = new InvertColors();
        private TextRenderer _textRenderer = new TextRenderer();
        private RenderToScreen _renderToScreen = new RenderToScreen();

        public RenderManager(Resolution resolution) => Resolution = resolution;

        public void Load(IEnumerable<Brush> brushes, IEnumerable<GameObject> gameObjects, IEnumerable<string> skyboxTexturePaths)
        {
            _skyboxRenderer.SetTextures(skyboxTexturePaths);

            _forwardRenderer.Load(Resolution);
            _deferredRenderer.Load(Resolution);
            _wireframeRenderer.Load(Resolution);
            _shadowRenderer.Load(Resolution);
            _lightRenderer.Load(Resolution);
            _skyboxRenderer.Load(Resolution);
            _selectionRenderer.Load(Resolution);
            _blurRenderer.Load(Resolution);
            _invertRenderer.Load(Resolution);
            _textRenderer.Load(Resolution);
            _renderToScreen.Load(Resolution);

            foreach (var brush in brushes)
            {
                brush.Load(_deferredRenderer._geometryProgram);
            }

            foreach (var gameObject in gameObjects)
            {
                gameObject.Model.Load(_deferredRenderer._geometryProgram);
            }

            GL.ClearColor(Color4.Black);
        }

        public void Resize()
        {
            _forwardRenderer.ResizeTextures(Resolution);
            _deferredRenderer.ResizeTextures(Resolution);
            _wireframeRenderer.ResizeTextures(Resolution);
            _shadowRenderer.ResizeTextures(Resolution);
            _lightRenderer.ResizeTextures(Resolution);
            _skyboxRenderer.ResizeTextures(Resolution);
            _selectionRenderer.ResizeTextures(Resolution);
            _blurRenderer.ResizeTextures(Resolution);
            _invertRenderer.ResizeTextures(Resolution);
            _textRenderer.ResizeTextures(Resolution);
            _renderToScreen.ResizeTextures(Resolution);
        }

        public void RenderEntityIDs(Camera camera, List<Light> lights, List<Brush> brushes, List<GameObject> gameObjects)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _selectionRenderer.GBuffer._handle);
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            _selectionRenderer.SelectionPass(Resolution, camera, brushes, gameObjects.Where(g => g.Model is SimpleModel));
            _selectionRenderer.JointSelectionPass(Resolution, camera, gameObjects.Where(g => g.Model is AnimatedModel));

            //var texture = _selectionRenderer.FinalTexture;

            //GL.Disable(EnableCap.DepthTest);

            //_renderToScreen.Render(texture);
        }

        public int GetEntityIDFromPoint(Vector2 point) => _selectionRenderer.GetEntityIDFromPoint(point);

        public void RenderWireframe(Camera camera, List<Brush> brushes, List<GameObject> gameObjects)
        {
            //GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _deferredRenderer.GBuffer._handle);
            //GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            //GL.Disable(EnableCap.DepthTest);

            /*GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);*/

            _wireframeRenderer.WireframePass(Resolution, camera, brushes, gameObjects.Where(g => g.Model is SimpleModel));
            _wireframeRenderer.JointWireframePass(Resolution, camera, gameObjects.Where(g => g.Model is AnimatedModel));

            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);

            //_skyboxRenderer.Render(Resolution, camera, _deferredRenderer.GBuffer);

            // Read from GBuffer's final texture, so that we can post-process it
            //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _deferredRenderer.GBuffer._handle);
            //GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            var texture = _wireframeRenderer.FinalTexture;

            GL.Disable(EnableCap.DepthTest);

            //_invertRenderer.Render(texture);
            //_blurRenderer.Render(Resolution, texture, _deferredRenderer.VelocityTexture, 60.0f);
            //texture = _blurRenderer.FinalTexture;
            //GL.Viewport(0, 0, Resolution.Width, Resolution.Height);
            //_renderToScreen.Render(_deferredRenderer.VelocityTexture);
            _renderToScreen.Render(texture);
            _textRenderer.RenderText("FPS: " + Frequency.ToString("0.##"), 10, Resolution.Height - (10 + TextRenderer.GLYPH_HEIGHT));
        }

        public void RenderDiffuseFrame(TextureManager textureManager, Camera camera, List<Brush> brushes, List<GameObject> gameObjects)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _deferredRenderer.GBuffer._handle);
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            _deferredRenderer.GeometryPass(Resolution, textureManager, camera, brushes, gameObjects.Where(g => g.Model is SimpleModel));
            _deferredRenderer.JointGeometryPass(Resolution, textureManager, camera, gameObjects.Where(g => g.Model is AnimatedModel));

            /*GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Disable(EnableCap.StencilTest);
            GL.Disable(EnableCap.Blend);*/

            _skyboxRenderer.Render(Resolution, camera, _deferredRenderer.GBuffer);

            // Read from GBuffer's final texture, so that we can post-process it
            //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _deferredRenderer.GBuffer._handle);
            //GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            var texture = _deferredRenderer.ColorTexture;

            GL.Disable(EnableCap.DepthTest);

            //_invertRenderer.Render(texture);
            //_blurRenderer.Render(Resolution, texture, _deferredRenderer.VelocityTexture, 60.0f);
            //texture = _blurRenderer.FinalTexture;
            //GL.Viewport(0, 0, Resolution.Width, Resolution.Height);
            //_renderToScreen.Render(_deferredRenderer.VelocityTexture);
            _renderToScreen.Render(texture);
            _textRenderer.RenderText("FPS: " + Frequency.ToString("0.##"), 10, Resolution.Height - (10 + TextRenderer.GLYPH_HEIGHT));
        }

        public void RenderLitFrame(TextureManager textureManager, Camera camera, List<Light> lights, List<Brush> brushes, List<GameObject> gameObjects)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _deferredRenderer.GBuffer._handle);
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            _deferredRenderer.GeometryPass(Resolution, textureManager, camera, brushes, gameObjects.Where(g => g.Model is SimpleModel));
            _deferredRenderer.JointGeometryPass(Resolution, textureManager, camera, gameObjects.Where(g => g.Model is AnimatedModel));

            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            foreach (var light in lights)
            {
                var lightMesh = _lightRenderer.GetMeshForLight(light);
                _lightRenderer.StencilPass(Resolution, light, camera, lightMesh);

                GL.Disable(EnableCap.Blend);
                _shadowRenderer.Render(Resolution, camera, light, brushes, gameObjects);
                GL.Enable(EnableCap.Blend);

                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _deferredRenderer.GBuffer._handle);
                GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

                var lightProgram = _lightRenderer.GetProgramForLight(light);
                var shadowMap = (light is PointLight) ? _shadowRenderer.PointDepthCubeMap : _shadowRenderer.SpotDepthTexture;
                _lightRenderer.LightPass(Resolution, _deferredRenderer, light, camera, lightMesh, shadowMap, lightProgram);
            }

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Disable(EnableCap.StencilTest);
            GL.Disable(EnableCap.Blend);

            _skyboxRenderer.Render(Resolution, camera, _deferredRenderer.GBuffer);

            // Read from GBuffer's final texture, so that we can post-process it
            //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _deferredRenderer.GBuffer._handle);
            //GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            var texture = _deferredRenderer.FinalTexture;

            GL.Disable(EnableCap.DepthTest);

            //_invertRenderer.Render(texture);
            //_blurRenderer.Render(Resolution, texture, _deferredRenderer.VelocityTexture, 60.0f);
            //texture = _blurRenderer.FinalTexture;
            //GL.Viewport(0, 0, Resolution.Width, Resolution.Height);
            //_renderToScreen.Render(_deferredRenderer.VelocityTexture);
            _renderToScreen.Render(texture);
            _textRenderer.RenderText("FPS: " + Frequency.ToString("0.##"), 10, Resolution.Height - (10 + TextRenderer.GLYPH_HEIGHT));
        }

        public void RenderFullFrame(TextureManager textureManager, Camera camera, List<Light> lights, List<Brush> brushes, List<GameObject> gameObjects)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _deferredRenderer.GBuffer._handle);
            GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            _deferredRenderer.GeometryPass(Resolution, textureManager, camera, brushes, gameObjects.Where(g => g.Model is SimpleModel));
            _deferredRenderer.JointGeometryPass(Resolution, textureManager, camera, gameObjects.Where(g => g.Model is AnimatedModel));

            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            foreach (var light in lights)
            {
                var lightMesh = _lightRenderer.GetMeshForLight(light);
                _lightRenderer.StencilPass(Resolution, light, camera, lightMesh);

                GL.Disable(EnableCap.Blend);
                _shadowRenderer.Render(Resolution, camera, light, brushes, gameObjects);
                GL.Enable(EnableCap.Blend);

                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _deferredRenderer.GBuffer._handle);
                GL.Viewport(0, 0, Resolution.Width, Resolution.Height);

                var lightProgram = _lightRenderer.GetProgramForLight(light);
                var shadowMap = (light is PointLight) ? _shadowRenderer.PointDepthCubeMap : _shadowRenderer.SpotDepthTexture;
                _lightRenderer.LightPass(Resolution, _deferredRenderer, light, camera, lightMesh, shadowMap, lightProgram);
            }

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Disable(EnableCap.StencilTest);
            GL.Disable(EnableCap.Blend);

            _skyboxRenderer.Render(Resolution, camera, _deferredRenderer.GBuffer);

            // Read from GBuffer's final texture, so that we can post-process it
            //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _deferredRenderer.GBuffer._handle);
            //GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            var texture = _deferredRenderer.FinalTexture;

            GL.Disable(EnableCap.DepthTest);

            //_invertRenderer.Render(texture);
            _blurRenderer.Render(Resolution, texture, _deferredRenderer.VelocityTexture, 60.0f);
            texture = _blurRenderer.FinalTexture;
            //GL.Viewport(0, 0, Resolution.Width, Resolution.Height);
            //_renderToScreen.Render(_deferredRenderer.VelocityTexture);
            _renderToScreen.Render(texture);
            _textRenderer.RenderText("FPS: " + Frequency.ToString("0.##"), 10, Resolution.Height - (10 + TextRenderer.GLYPH_HEIGHT));
        }
    }
}
