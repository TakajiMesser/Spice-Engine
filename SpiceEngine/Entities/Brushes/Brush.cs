﻿using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;
using System.Linq;
using SpiceEngine.Entities.Lights;
using SpiceEngine.Game;
using SpiceEngine.Helpers;
using SpiceEngine.Physics.Collision;
using SpiceEngine.Rendering.Materials;
using SpiceEngine.Rendering.Matrices;
using SpiceEngine.Rendering.Meshes;
using SpiceEngine.Rendering.Shaders;
using SpiceEngine.Rendering.Textures;
using SpiceEngine.Rendering.Vertices;
using SpiceEngine.Scripting.StimResponse;

namespace SpiceEngine.Entities.Brushes
{
    /// <summary>
    /// Brushes are static geometric shapes that are baked into a scene.
    /// Unlike meshes, brushes cannot be deformed.
    /// </summary>
    public class Brush : IEntity, IRotate, IScale
    {
        public int ID { get; set; }

        public Vector3 Position
        {
            get => _modelMatrix.Translation;
            set => _modelMatrix.Translation = value;
        }

        public Vector3 OriginalRotation { get; set; }

        public Quaternion Rotation
        {
            get => _modelMatrix.Rotation;
            set => _modelMatrix.Rotation = value;
        }

        public Vector3 Scale
        {
            get => _modelMatrix.Scale;
            set => _modelMatrix.Scale = value;
        }

        //public List<Vector3> Vertices => Mesh.Vertices.Select(v => v.Position).Distinct().ToList();
        public Matrix4 ModelMatrix => _modelMatrix.Matrix;

        private ModelMatrix _modelMatrix = new ModelMatrix();
        private Material _material;
        public TextureMapping TextureMapping { get; set; }

        public Brush(Material material)
        {
            _material = material;
            //SimpleMesh = new SimpleMesh(vertices.Select(v => v.Position).ToList(), triangleIndices, program);
        }

        public void SetUniforms(ShaderProgram program, TextureManager textureManager = null)
        {
            _modelMatrix.Set(program);
            _material.SetUniforms(program);

            if (textureManager != null && TextureMapping != null)
            {
                program.BindTextures(textureManager, TextureMapping);
            }
            else
            {
                program.UnbindTextures();
            }
        }
    }
}
