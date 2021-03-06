﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SpiceEngine.Properties;
using SpiceEngineCore.Outputs;
using SpiceEngineCore.Rendering.Shaders;
using SweetGraphicsCore.Buffers;
using SweetGraphicsCore.Rendering.Processing;
using SweetGraphicsCore.Rendering.Textures;
using SweetGraphicsCore.Vertices;
using System;

namespace SpiceEngine.Rendering.PostProcessing
{
    public class RenderToScreen : Renderer
    {
        public Texture FinalTexture { get; protected set; }
        public float Gamma { get; set; } = 2.2f;

        private ShaderProgram _render1DProgram;
        private ShaderProgram _render2DProgram;
        private ShaderProgram _render2DArrayProgram;
        private ShaderProgram _render3DProgram;
        private ShaderProgram _renderCubeProgram;
        private ShaderProgram _renderCubeArrayProgram;

        private VertexArray<Simple2DVertex> _vertexArray = new VertexArray<Simple2DVertex>();
        private VertexBuffer<Simple2DVertex> _vertexBuffer = new VertexBuffer<Simple2DVertex>();

        protected override void LoadPrograms()
        {
            _render1DProgram = new ShaderProgram(new Shader(ShaderType.FragmentShader, Resources.render_1D_frag));

            _render2DProgram = new ShaderProgram(new Shader(ShaderType.VertexShader, Resources.render_2D_vert), 
                new Shader(ShaderType.FragmentShader, Resources.render_2D_frag));

            _render2DArrayProgram = new ShaderProgram(new Shader(ShaderType.FragmentShader, Resources.render_2D_array_frag));

            _render3DProgram = new ShaderProgram(new Shader(ShaderType.FragmentShader, Resources.render_3D_frag));

            _renderCubeProgram = new ShaderProgram(new Shader(ShaderType.VertexShader, Resources.render_cube_vert),
                new Shader(ShaderType.FragmentShader, Resources.render_cube_frag));

            _renderCubeArrayProgram = new ShaderProgram(new Shader(ShaderType.VertexShader, Resources.render_cube_vert),
                new Shader(ShaderType.FragmentShader, Resources.render_cube_array_frag));
        }

        protected override void LoadTextures(Resolution resolution)
        {
            FinalTexture = new Texture(resolution.Width, resolution.Height, 0)
            {
                Target = TextureTarget.Texture2D,
                EnableMipMap = false,
                EnableAnisotropy = false,
                PixelInternalFormat = PixelInternalFormat.Rgba16f,
                PixelFormat = PixelFormat.Rgba,
                PixelType = PixelType.Float,
                MinFilter = TextureMinFilter.Linear,
                MagFilter = TextureMagFilter.Linear,
                WrapMode = TextureWrapMode.Clamp
            };
            FinalTexture.Bind();
            FinalTexture.ReserveMemory();
        }

        protected override void LoadBuffers()
        {
            _vertexBuffer.Bind();
            _vertexArray.Load();
            _vertexBuffer.Unbind();

            _vertexBuffer.Clear();
            _vertexBuffer.AddVertex(new Simple2DVertex(new Vector2(1.0f, 1.0f)));
            _vertexBuffer.AddVertex(new Simple2DVertex(new Vector2(-1.0f, 1.0f)));
            _vertexBuffer.AddVertex(new Simple2DVertex(new Vector2(-1.0f, -1.0f)));
            _vertexBuffer.AddVertex(new Simple2DVertex(new Vector2(1.0f, -1.0f)));
        }

        public override void ResizeTextures(Resolution resolution)
        {
            FinalTexture.Resize(resolution.Width, resolution.Height, 0);
            FinalTexture.Bind();
            FinalTexture.ReserveMemory();
        }

        public void Render(Texture texture, int channel = -1)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, FinalTexture.Width, FinalTexture.Height);

            switch (texture.Target)
            {
                case TextureTarget.Texture1D:
                    _render1DProgram.Use();
                    _render1DProgram.BindTexture(texture, "textureSampler", 0);
                    break;
                case TextureTarget.Texture2D:
                    _render2DProgram.Use();
                    _render2DProgram.BindTexture(texture, "textureSampler", 0);

                    _render2DProgram.SetUniform("gamma", Gamma);
                    _render2DProgram.SetUniform("channel", channel);
                    break;
                case TextureTarget.Texture3D:
                    break;
                case TextureTarget.Texture2DArray:
                    break;
                case TextureTarget.TextureCubeMapArray:
                    break;
                default:
                    throw new NotImplementedException("Cannot render texture target type " + texture.Target);
            }

            _vertexArray.Bind();
            _vertexBuffer.Bind();
            _vertexBuffer.Buffer();

            _vertexBuffer.DrawQuads();

            _vertexArray.Unbind();
            _vertexBuffer.Unbind();
        }
    }
}
