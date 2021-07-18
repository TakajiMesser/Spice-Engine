﻿using SpiceEngine.GLFWBindings;
using SpiceEngine.GLFWBindings.GLEnums;
using SpiceEngineCore.Entities;
using SpiceEngineCore.Entities.Cameras;
using SpiceEngineCore.Entities.Lights;
using SpiceEngineCore.Helpers;
using SpiceEngineCore.Rendering;
using SweetGraphicsCore.Helpers;
using SweetGraphics.Properties;
using SweetGraphicsCore.Rendering.Meshes;
using SweetGraphicsCore.Rendering.Textures;
using SweetGraphicsCore.Shaders;
using System;

namespace SweetGraphicsCore.Renderers.Processing
{
    public class LightRenderer : Renderer
    {
        private ShaderProgram _stencilProgram;
        private ShaderProgram _pointLightProgram;
        private ShaderProgram _spotLightProgram;
        private ShaderProgram _simpleProgram;

        private SimpleMesh _pointLightMesh;
        private SimpleMesh _spotLightMesh;

        protected override void LoadPrograms(IRenderContext renderContext)
        {
            _stencilProgram = ShaderHelper.LoadProgram(renderContext,
                new[] { ShaderType.VertexShader },
                new[] { Resources.stencil_vert });

            _pointLightProgram = ShaderHelper.LoadProgram(renderContext,
                new[] { ShaderType.VertexShader, ShaderType.FragmentShader },
                new[] { Resources.light_vert, Resources.point_light_frag });

            _spotLightProgram = ShaderHelper.LoadProgram(renderContext,
                new[] { ShaderType.VertexShader, ShaderType.FragmentShader },
                new[] { Resources.light_vert, Resources.spot_light_frag });

            _simpleProgram = ShaderHelper.LoadProgram(renderContext,
                new[] { ShaderType.VertexShader, ShaderType.FragmentShader },
                new[] { Resources.simple_vert, Resources.simple_frag });
        }

        protected override void LoadTextures(IRenderContext renderContext, Resolution resolution)
        {
            /*DepthStencilTexture = new Texture(resolution.Width, resolution.Height, 0)
            {
                Target = TextureTarget.Texture2d,
                EnableMipMap = false,
                EnableAnisotropy = false,
                InternalFormat = InternalFormat.Depth32fStencil8,
                PixelFormat = PixelFormat.DepthComponent,
                PixelType = PixelType.Float,
                MinFilter = TextureMinFilter.Linear,
                MagFilter = TextureMagFilter.Linear,
                WrapMode = TextureWrapMode.Clamp
            };
            DepthStencilTexture.Bind();
            DepthStencilTexture.ReserveMemory();

            FinalTexture = new Texture(resolution.Width, resolution.Height, 0)
            {
                Target = TextureTarget.Texture2d,
                EnableMipMap = false,
                EnableAnisotropy = false,
                InternalFormat = InternalFormat.Rgba16f,
                PixelFormat = PixelFormat.Rgba,
                PixelType = PixelType.Float,
                MinFilter = TextureMinFilter.Linear,
                MagFilter = TextureMagFilter.Linear,
                WrapMode = TextureWrapMode.Clamp
            };
            FinalTexture.Bind();
            FinalTexture.ReserveMemory();*/
        }

        protected override void LoadBuffers(IRenderContext renderContext)
        {
            /*_frameBuffer.Clear();
            _frameBuffer.Add(FramebufferAttachment.ColorAttachment0, FinalTexture);
            _frameBuffer.Add(FramebufferAttachment.DepthStencilAttachment, DepthStencilTexture);

            _frameBuffer.Bind(FramebufferTarget.Framebuffer);
            _frameBuffer.AttachAttachments();
            _frameBuffer.Unbind(FramebufferTarget.Framebuffer);*/

            _pointLightMesh = SimpleMesh.LoadFromFile(renderContext, FilePathHelper.SPHERE_MESH_PATH, _pointLightProgram);
            _spotLightMesh = SimpleMesh.LoadFromFile(renderContext, FilePathHelper.CONE_MESH_PATH, _spotLightProgram);
        }

        protected override void Resize(Resolution resolution)
        {
            /*DepthStencilTexture.Resize(resolution.Width, resolution.Height, 0);
            DepthStencilTexture.Bind();
            DepthStencilTexture.ReserveMemory();

            FinalTexture.Resize(resolution.Width, resolution.Height, 0);
            FinalTexture.Bind();
            FinalTexture.ReserveMemory();*/
        }

        public void StencilPass(ILight light, ICamera camera, SimpleMesh mesh)
        {
            _stencilProgram.Bind();
            GL.DrawBuffer(DrawBufferMode.None);

            GL.DepthMask(false);
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.Clear(ClearBufferMask.StencilBufferBit);

            GL.StencilFunc(StencilFunction.Always, 0, 0);
            GL.StencilOpSeparate(StencilFaceDirection.Back, StencilOp.Keep, StencilOp.IncrWrap, StencilOp.Keep);
            GL.StencilOpSeparate(StencilFaceDirection.Front, StencilOp.Keep, StencilOp.DecrWrap, StencilOp.Keep);

            _stencilProgram.SetCamera(camera);
            _stencilProgram.SetLight(light);

            _stencilProgram.StencilPass(light);
            mesh.Draw();
        }

        public void LightPass(DeferredRenderer deferredRenderer, ILight light, ICamera camera, SimpleMesh mesh, Texture shadowMap, ShaderProgram program)
        {
            program.Bind();

            GL.StencilFunc(StencilFunction.Notequal, 0, 0xFF);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            program.BindTexture(deferredRenderer.PositionTexture, "positionMap", 0);
            program.BindTexture(deferredRenderer.ColorTexture, "colorMap", 1);
            program.BindTexture(deferredRenderer.NormalTexture, "normalMap", 2);
            program.BindTexture(deferredRenderer.DiffuseMaterialTexture, "diffuseMaterial", 3);
            program.BindTexture(deferredRenderer.SpecularTexture, "specularMap", 4);
            program.BindTexture(shadowMap, "shadowMap", 5);

            program.SetCamera(camera);
            program.SetUniform("cameraPosition", camera.Position);
            program.SetLight(light);

            program.LightPass(light);
            mesh.Draw();
        }

        public SimpleMesh GetMeshForLight(ILight light)
        {
            switch (light)
            {
                case PointLight _:
                    return _pointLightMesh;
                case SpotLight _:
                    return _spotLightMesh;
                default:
                    throw new NotImplementedException("Could not handle light type " + light.GetType());
            }
        }

        public ShaderProgram GetProgramForLight(ILight light)
        {
            switch (light)
            {
                case PointLight _:
                    return _pointLightProgram;
                case SpotLight _:
                    return _spotLightProgram;
                default:
                    throw new NotImplementedException("Could not handle light type " + light.GetType());
            }
        }
    }
}