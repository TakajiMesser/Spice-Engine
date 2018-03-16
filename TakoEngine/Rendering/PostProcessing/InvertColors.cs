﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using TakoEngine.Helpers;
using TakoEngine.Outputs;
using TakoEngine.Rendering.Buffers;
using TakoEngine.Rendering.Processing;
using TakoEngine.Rendering.Shaders;
using TakoEngine.Rendering.Textures;
using TakoEngine.Rendering.Vertices;
using TakoEngine.Utilities;

namespace TakoEngine.Rendering.PostProcessing
{
    public class InvertColors : Renderer
    {
        public Texture FinalTexture { get; protected set; }

        private ShaderProgram _invertProgram;

        private int _vertexArrayHandle;
        private VertexBuffer<Vector3> _vertexBuffer = new VertexBuffer<Vector3>();
        protected FrameBuffer _frameBuffer = new FrameBuffer();

        protected override void LoadPrograms()
        {
            _invertProgram = new ShaderProgram(
                new Shader(ShaderType.VertexShader, File.ReadAllText(FilePathHelper.RENDER_2D_VERTEX_PATH)),
                new Shader(ShaderType.FragmentShader, File.ReadAllText(FilePathHelper.INVERT_SHADER_PATH)));
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
            _frameBuffer.Add(FramebufferAttachment.ColorAttachment0, FinalTexture);
            _frameBuffer.Bind(FramebufferTarget.Framebuffer);
            _frameBuffer.AttachAttachments();
            _frameBuffer.Unbind(FramebufferTarget.Framebuffer);

            _vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayHandle);
            _vertexBuffer.AddVertices(new[]
            {
                new Vector3(1.0f, 1.0f, 0.0f),
                new Vector3(-1.0f, 1.0f, 0.0f),
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f)
            });
            _vertexBuffer.Bind();
            _vertexBuffer.Buffer();

            var attribute = new VertexAttribute("vPosition", 3, VertexAttribPointerType.Float, UnitConversions.SizeOf<Vector3>(), 0);
            attribute.Set(_invertProgram.GetAttributeLocation("vPosition"));

            GL.BindVertexArray(0);
            _vertexBuffer.Unbind();
        }

        public override void ResizeTextures(Resolution resolution)
        {
            FinalTexture.Resize(resolution.Width, resolution.Height, 0);
            FinalTexture.Bind();
            FinalTexture.ReserveMemory();
        }

        public void Render(Resolution resolution, Texture texture)
        {
            _invertProgram.Use();
            _frameBuffer.Draw();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _invertProgram.BindTexture(texture, "sceneTexture", 0);

            GL.BindVertexArray(_vertexArrayHandle);
            _vertexBuffer.Bind();

            _vertexBuffer.DrawQuads();

            GL.BindVertexArray(0);
            _vertexBuffer.Unbind();
        }
    }
}
