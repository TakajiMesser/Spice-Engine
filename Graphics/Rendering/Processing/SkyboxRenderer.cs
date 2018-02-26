﻿using Graphics.GameObjects;
using Graphics.Helpers;
using Graphics.Meshes;
using Graphics.Outputs;
using Graphics.Rendering.Buffers;
using Graphics.Rendering.Matrices;
using Graphics.Rendering.Shaders;
using Graphics.Rendering.Textures;
using Graphics.Rendering.Vertices;
using Graphics.Utilities;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphics.Rendering.Processing
{
    public class SkyboxRenderer : Renderer
    {
        public Texture SkyTexture { get; protected set; }
        
        internal ShaderProgram _program;
        private SimpleMesh _cubeMesh;
        private List<string> _texturePaths = new List<string>();

        public void SetTextures(IEnumerable<string> texturePath)
        {
            _texturePaths.Clear();
            _texturePaths.AddRange(texturePath);
        }

        protected override void LoadPrograms()
        {
            _program = new ShaderProgram(
                new Shader(ShaderType.VertexShader, File.ReadAllText(FilePathHelper.SKYBOX_VERTEX_PATH)),
                new Shader(ShaderType.FragmentShader, File.ReadAllText(FilePathHelper.SKYBOX_FRAGMENT_PATH))
            );
        }

        protected override void LoadTextures(Resolution resolution)
        {
            SkyTexture = Texture.LoadFromFile(_texturePaths, TextureTarget.TextureCubeMap, true, true);
        }

        public override void ResizeTextures(Resolution resolution) { }

        protected override void LoadBuffers()
        {
            _cubeMesh = SimpleMesh.LoadFromFile(FilePathHelper.CUBE_MESH_PATH, _program);
        }

        public void Render(Resolution resolution, Camera camera, FrameBuffer gBuffer)
        {
            _program.Use();
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, gBuffer._handle);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment6);

            //GL.ClearColor(Color4.Purple);
            //GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, resolution.Width, resolution.Height);

            int oldCullFaceMode = GL.GetInteger(GetPName.CullFaceMode);
            int oldDepthFunc = GL.GetInteger(GetPName.DepthFunc);

            //GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.CullFace(CullFaceMode.Front);
            GL.DepthFunc(DepthFunction.Lequal);

            _program.BindTexture(SkyTexture, "mainTexture", 0);

            camera.Draw(_program);
            _program.SetUniform(ModelMatrix.NAME, Matrix4.CreateTranslation(camera.Position));
            _cubeMesh.Draw();

            GL.CullFace((CullFaceMode)oldCullFaceMode);
            GL.DepthFunc((DepthFunction)oldDepthFunc);
        }
    }
}