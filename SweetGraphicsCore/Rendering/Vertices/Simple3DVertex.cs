﻿using OpenTK;
using SpiceEngineCore.Rendering.Matrices;
using System.Runtime.InteropServices;

namespace SweetGraphicsCore.Vertices
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Simple3DVertex : IVertex3D
    {
        public Vector3 Position { get; private set; }

        public Simple3DVertex(Vector3 position) => Position = position;

        public Simple3DVertex(float x, float y, float z) => Position = new Vector3(x, y, z);

        public IVertex3D Transformed(Transform transform) => new Simple3DVertex((transform.ToMatrix() * new Vector4(Position, 1.0f)).Xyz);
    }
}
