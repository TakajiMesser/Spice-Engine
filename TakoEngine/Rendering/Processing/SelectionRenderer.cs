﻿using TakoEngine.GameObjects;
using TakoEngine.Helpers;
using TakoEngine.Lighting;
using TakoEngine.Meshes;
using TakoEngine.Outputs;
using TakoEngine.Rendering.Buffers;
using TakoEngine.Rendering.Matrices;
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
    /// <summary>
    /// Renders all game entities to a texture, with their colors reflecting their given ID's
    /// </summary>
    public class SelectionRenderer : Renderer
    {
        public Texture FinalTexture { get; protected set; }
        public Texture DepthStencilTexture { get; protected set; }

        public FrameBuffer GBuffer { get; private set; } = new FrameBuffer();

        internal ShaderProgram _selectionProgram;
        internal ShaderProgram _jointSelectionProgram;

        protected override void LoadPrograms()
        {
            _selectionProgram = new ShaderProgram(
                new Shader(ShaderType.VertexShader, File.ReadAllText(FilePathHelper.SELECTION_VERTEX_PATH)),
                new Shader(ShaderType.FragmentShader, File.ReadAllText(FilePathHelper.SELECTION_FRAGMENT_PATH))
            );

            _jointSelectionProgram = new ShaderProgram(
                new Shader(ShaderType.VertexShader, File.ReadAllText(FilePathHelper.SELECTION_SKINNING_VERTEX_PATH)),
                new Shader(ShaderType.FragmentShader, File.ReadAllText(FilePathHelper.SELECTION_FRAGMENT_PATH))
            );
        }

        public override void ResizeTextures(Resolution resolution)
        {
            FinalTexture.Resize(resolution.Width, resolution.Height, 0);
            FinalTexture.Bind();
            FinalTexture.ReserveMemory();

            DepthStencilTexture.Resize(resolution.Width, resolution.Height, 0);
            DepthStencilTexture.Bind();
            DepthStencilTexture.ReserveMemory();
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
                PixelType = PixelType.UnsignedByte,
                MinFilter = TextureMinFilter.Linear,
                MagFilter = TextureMagFilter.Linear,
                WrapMode = TextureWrapMode.Clamp
            };
            FinalTexture.Bind();
            FinalTexture.ReserveMemory();

            DepthStencilTexture = new Texture(resolution.Width, resolution.Height, 0)
            {
                Target = TextureTarget.Texture2D,
                EnableMipMap = false,
                EnableAnisotropy = false,
                PixelInternalFormat = PixelInternalFormat.Depth32fStencil8,
                PixelFormat = PixelFormat.DepthComponent,
                PixelType = PixelType.Float,
                MinFilter = TextureMinFilter.Linear,
                MagFilter = TextureMagFilter.Linear,
                WrapMode = TextureWrapMode.Clamp
            };
            DepthStencilTexture.Bind();
            DepthStencilTexture.ReserveMemory();
        }

        protected override void LoadBuffers()
        {
            GBuffer.Clear();
            GBuffer.Add(FramebufferAttachment.ColorAttachment0, FinalTexture);
            GBuffer.Add(FramebufferAttachment.DepthStencilAttachment, DepthStencilTexture);

            GBuffer.Bind(FramebufferTarget.Framebuffer);
            GBuffer.AttachAttachments();
            GBuffer.Unbind(FramebufferTarget.Framebuffer);
        }

        public int GetEntityIDFromPoint(Vector2 point)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, GBuffer._handle);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            var color = FinalTexture.ReadPixelColor((int)point.X, (int)point.Y);

            return (int)(color.X + color.Y * 256 + color.Z * 256 * 256);
        }

        private Vector4 GetColorFromID(int id)
        {
            return new Vector4()
            {
                X = ((id & 0x000000FF) >> 0) / 255.0f,
                Y = ((id & 0x0000FF00) >> 8) / 255.0f,
                Z = ((id & 0x00FF0000) >> 16) / 255.0f,
                W = 1.0f
            };
        }

        public void SelectionPass(Resolution resolution, Camera camera, IEnumerable<Brush> brushes, IEnumerable<GameObject> gameObjects)
        {
            _selectionProgram.Use();
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, GBuffer._handle);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            camera.Draw(_selectionProgram);

            foreach (var brush in brushes)
            {
                _selectionProgram.SetUniform("id", GetColorFromID(brush.ID));
                brush.Draw(_selectionProgram);
            }

            foreach (var gameObject in gameObjects)
            {
                _selectionProgram.SetUniform("id", GetColorFromID(gameObject.ID));
                gameObject.Draw(_selectionProgram);
            }
        }

        public void JointSelectionPass(Resolution resolution, Camera camera, IEnumerable<GameObject> gameObjects)
        {
            _jointSelectionProgram.Use();
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, GBuffer._handle);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            camera.Draw(_jointSelectionProgram);

            foreach (var gameObject in gameObjects)
            {
                _jointSelectionProgram.SetUniform("id", GetColorFromID(gameObject.ID));
                gameObject.Draw(_jointSelectionProgram);
            }
        }

        private IEnumerable<GameObject> PerformFrustumCulling(IEnumerable<GameObject> gameObjects)
        {
            // Don't render meshes that are not in the camera's view

            // Using the position of the gameObject, determine if we should render the mesh
            // We will also need a bounding sphere or bounding box from the mesh to determine this
            foreach (var gameObject in gameObjects)
            {
                Vector3 position = gameObject.Model.Position;
            }

            return gameObjects;
        }

        private IEnumerable<GameObject> PerformOcclusionCulling(IEnumerable<GameObject> gameObjects)
        {
            // Don't render meshes that are obscured by closer meshes
            foreach (var gameObject in gameObjects)
            {
                Vector3 position = gameObject.Model.Position;
            }

            return gameObjects;
        }
    }
}
