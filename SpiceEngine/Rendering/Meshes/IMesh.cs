﻿using OpenTK;
using SpiceEngine.Rendering.Matrices;
using SpiceEngine.Rendering.Vertices;
using System;
using System.Collections.Generic;

namespace SpiceEngine.Rendering.Meshes
{
    public interface IMesh : IRenderable
    {
        IEnumerable<IVertex3D> Vertices { get; }
        IEnumerable<int> TriangleIndices { get; }
        float Alpha { get; set; }

        void Load();
        void Draw();

        void Combine(IMesh mesh);

        void Transform(Transform transform);
        void Transform(Transform transform, int offset, int count);

        void TransformTexture(Vector3 center, Vector2 translation, float rotation, Vector2 scale);
        void TransformTexture(Vector3 center, Vector2 translation, float rotation, Vector2 scale, int offset, int count);

        void Update(Func<IVertex3D, IVertex3D> vertexUpdate);
        void Update(Func<IVertex3D, IVertex3D> vertexUpdate, int offset, int count);

        IMesh Duplicate();
    }
}
