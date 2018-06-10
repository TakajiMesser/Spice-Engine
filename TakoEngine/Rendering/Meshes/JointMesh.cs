﻿using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using TakoEngine.Entities.Lights;
using TakoEngine.Rendering.Animations;
using TakoEngine.Rendering.Buffers;
using TakoEngine.Rendering.Materials;
using TakoEngine.Rendering.Shaders;
using TakoEngine.Rendering.Textures;
using TakoEngine.Rendering.Vertices;
using TakoEngine.Utilities;

namespace TakoEngine.Rendering.Meshes
{
    public class JointMesh : IDisposable
    {
        public const int MAX_JOINTS = 100;

        private List<JointVertex> _vertices;
        private List<int> _triangleIndices;
        private VertexBuffer<JointVertex> _vertexBuffer;
        private VertexIndexBuffer _indexBuffer;
        private VertexArray<JointVertex> _vertexArray;
        private Material _material;
        //private LightBuffer _lightBuffer = new LightBuffer();
        private Matrix4[] _jointTransforms = ArrayExtensions.Initialize(MAX_JOINTS, Matrix4.Identity);

        public TextureMapping TextureMapping { get; set; }
        public List<JointVertex> Vertices => _vertices;

        public JointMesh(List<JointVertex> vertices, Material material, List<int> triangleIndices)
        {
            if (triangleIndices.Count % 3 != 0)
            {
                throw new ArgumentException(nameof(triangleIndices) + " must be divisible by three");
            }

            _vertices = vertices;
            _triangleIndices = triangleIndices;
            _material = material;
        }

        public void Load()
        {
            _vertexBuffer = new VertexBuffer<JointVertex>();
            _indexBuffer = new VertexIndexBuffer();
            _vertexArray = new VertexArray<JointVertex>();

            _vertexBuffer.AddVertices(_vertices);
            _indexBuffer.AddIndices(_triangleIndices.ConvertAll(i => (ushort)i));

            _vertexBuffer.Bind();
            _vertexArray.Load();
            _vertexBuffer.Unbind();

            //_lightBuffer.Load(program);
        }

        public void ClearVertices()
        {
            _vertices.Clear();
        }

        public void AddVertices(IEnumerable<JointVertex> vertices)
        {
            _vertices.AddRange(vertices);
        }

        public void RefreshVertices()
        {
            _vertexBuffer.Clear();
            _vertexBuffer.AddVertices(_vertices);
        }

        //public void ClearLights() => _lightBuffer.Clear();
        //public void AddPointLights(IEnumerable<PointLight> lights) => _lightBuffer.AddPointLights(lights);

        public void SetJointTransforms(MeshTransforms transforms)
        {
            for (var i = 0; i < MAX_JOINTS; i++)
            {
                _jointTransforms[i] = Matrix4.Zero;
            }

            foreach (var kvp in transforms.TransformsByBoneIndex)
            {
                _jointTransforms[kvp.Key] = kvp.Value;
            }
        }

        public void Draw(ShaderProgram program, TextureManager textureManager)
        {
            if (textureManager != null && TextureMapping != null)
            {
                program.BindTextures(textureManager, TextureMapping);
            }

            program.SetUniform("jointTransforms", _jointTransforms);
            _material.Draw(program);

            _vertexArray.Bind();
            _vertexBuffer.Bind();
            //_lightBuffer.Bind();
            _indexBuffer.Bind();

            _vertexBuffer.Buffer();
            //_lightBuffer.Buffer();
            _indexBuffer.Buffer();

            _indexBuffer.Draw();

            _vertexArray.Unbind();
            _vertexBuffer.Unbind();
            //_lightBuffer.Unbind();
            _indexBuffer.Unbind();
        }

        public void SaveToFile()
        {
            throw new NotImplementedException();
        }

        /*public static Mesh<Vertex> LoadFromFile(string filePath)
        {
            using (var importer = new Assimp.AssimpContext())
            {
                var scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.JoinIdenticalVertices | Assimp.PostProcessSteps.CalculateTangentSpace);

                // For now, assume every file has just one mesh
                var mesh = scene.Meshes.First();
                var material = scene.Materials[mesh.MaterialIndex];

                var vertices2 = new List<Vertex>();

                for (var i = 0; i < mesh.VertexCount; i++)
                {
                    var vertex = new Vertex()
                    {
                        Position = new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z),
                        Color = new Color4(),
                        MaterialIndex = 0
                    };

                    if (mesh.HasNormals)
                    {
                        vertex.Normal = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                    }

                    if (mesh.HasTextureCoords(0))
                    {
                        vertex.TextureCoords = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);
                    }

                    if (mesh.HasTangentBasis)
                    {
                        vertex.Tangent = new Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z);
                    }

                    vertices2.Add(vertex);
                }

                var triangleIndices2 = mesh.GetIndices().ToList();

                return new Mesh<Vertex>(vertices2, new List<Material> { new Material(material) }, triangleIndices2);
            }

            var materials = new List<Material>();
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var vertexIndices = new List<int>();
            var uvIndices = new List<int>();
            var normalIndices = new List<int>();
            var materialIndices = new List<int>();

            var materialIndexByName = new Dictionary<string, int>();
            int currentMaterialIndex = 0;

            foreach (var line in File.ReadLines(filePath))
            {
                var values = line.Split(' ');

                if (values.Length > 0)
                {
                    switch (values[0])
                    {
                        case "mtllib":
                            foreach (var material in Material.LoadFromFile(Path.GetDirectoryName(filePath) + "\\" + values[1]))
                            {
                                materialIndexByName.Add(material.Item1, materials.Count);
                                materials.Add(material.Item2);
                            }
                            break;
                        case "usemtl":
                            if (!materialIndexByName.ContainsKey(values[1]))
                            {
                                throw new ArgumentOutOfRangeException("Material " + values[1] + " not found");
                            }
                            currentMaterialIndex = materialIndexByName[values[1]];
                            break;
                        case "v":
                            vertices.Add(new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])));
                            break;
                        case "vt":
                            uvs.Add(new Vector2(float.Parse(values[1]), float.Parse(values[2])));
                            break;
                        case "vn":
                            normals.Add(new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])));
                            break;
                        case "f":
                            for (var i = 1; i <= 3; i++)
                            {
                                var indices = values[i].Split('/');

                                vertexIndices.Add(int.Parse(indices[0]) - 1);
                                uvIndices.Add(int.Parse(indices[1]) - 1);
                                normalIndices.Add(int.Parse(indices[2]) - 1);
                                materialIndices.Add(currentMaterialIndex);
                            }
                            break;
                    }
                }
            }

            var verticies = new List<Vertex>();
            var triangleIndices = new List<int>();

            Vector3 tangent = Vector3.Zero;

            for (var i = 0; i < vertexIndices.Count; i++)
            {
                // Grab vertexIndices, three at a time, to form each triangle
                if (i % 3 == 0)
                {
                    // For a given triangle with vertex positions P0, P1, P2 and corresponding UV texture coordinates T0, T1, and T2:
                    // deltaPos1 = P1 - P0;
                    // delgaPos2 = P2 - P0;
                    // deltaUv1 = T1 - T0;
                    // deltaUv2 = T2 - T0;
                    // r = 1 / (deltaUv1.x * deltaUv2.y - deltaUv1.y - deltaUv2.x);
                    // tangent = (deltaPos1 * deltaUv2.y - deltaPos2 * deltaUv1.y) * r;
                    var deltaPos1 = vertices[vertexIndices[i + 1]] - vertices[vertexIndices[i]];
                    var deltaPos2 = vertices[vertexIndices[i + 2]] - vertices[vertexIndices[i]];
                    var deltaUV1 = uvs[uvIndices[i + 1]] - uvs[uvIndices[i]];
                    var deltaUV2 = uvs[uvIndices[i + 1]] - uvs[uvIndices[i]];

                    var r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y - deltaUV2.X);
                    tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;
                }

                var uv = uvIndices[i] > 0 ? uvs[uvIndices[i]] : Vector2.Zero;

                var vertex = new Vertex(vertices[vertexIndices[i]], normals[normalIndices[i]], tangent.Normalized(), uv, materialIndices[i]);
                var existingIndex = verticies.FindIndex(v => v.Position == vertex.Position
                    && v.Normal == vertex.Normal
                    && v.TextureCoords == vertex.TextureCoords
                    && v.MaterialIndex == vertex.MaterialIndex);

                if (existingIndex >= 0)
                {
                    triangleIndices.Add(existingIndex);
                }
                else
                {
                    triangleIndices.Add(verticies.Count);
                    verticies.Add(vertex);
                }
            }

            return new Mesh<Vertex>(verticies, materials, triangleIndices);
        }*/

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && GraphicsContext.CurrentContext != null && !GraphicsContext.CurrentContext.IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                //GL.DeleteShader(Handle);
                disposedValue = true;
            }
        }

        ~JointMesh()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
