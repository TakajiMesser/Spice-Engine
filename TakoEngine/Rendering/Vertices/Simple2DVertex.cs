﻿using OpenTK;
using System.Runtime.InteropServices;

namespace TakoEngine.Rendering.Vertices
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Simple2DVertex// : IVertex
    {
        public Vector2 Position;// { get; set; }

        public Simple2DVertex(Vector2 position)
        {
            Position = position;
        }
    }
}
