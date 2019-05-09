﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceEngine.Rendering.Meshes
{
    // TODO - Pick a name that isn't shit
    public class MeshBuild
    {
        public List<MeshVertex> Vertices { get; set; } = new List<MeshVertex>();
        public List<int> TriangleIndices { get; set; } = new List<int>();

        public void AddTriangle(MeshTriangle triangle)
        {
            var indexA = AddVertex(triangle.VertexA, triangle.Normal, triangle.Tangent);
            var indexB = AddVertex(triangle.VertexB, triangle.Normal, triangle.Tangent);
            var indexC = AddVertex(triangle.VertexC, triangle.Normal, triangle.Tangent);

            TriangleIndices.Add(indexA);
            TriangleIndices.Add(indexB);
            TriangleIndices.Add(indexC);
        }

        private int AddVertex(Vector3 position, Vector3 normal, Vector3 tangent)
        {
            var index = Vertices.FindIndex(v => v.Position == position);
            MeshVertex vertex;

            if (index >= 0)
            {
                vertex = Vertices[index];
            }
            else
            {
                vertex = new MeshVertex()
                {
                    Position = position
                };

                index = Vertices.Count;
                Vertices.Add(vertex);
            }

            vertex.Normal += normal;
            vertex.Tangent += tangent;

            return index;
        }

        /*for (var i = 0; i < vertexIndices.Count; i++)
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
        }*/
    }
}
