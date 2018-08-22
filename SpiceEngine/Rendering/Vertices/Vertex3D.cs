﻿using OpenTK;
using OpenTK.Graphics;
using System.Runtime.InteropServices;

namespace SpiceEngine.Rendering.Vertices
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex3D : IVertex3D
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector3 Tangent { get; set; }
        public Vector2 TextureCoords { get; set; }
        public Color4 Color { get; set; }

        public Vertex3D(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 textureCoords)
        {
            Position = position;
            Normal = normal;
            Tangent = tangent;
            TextureCoords = textureCoords;
            Color = new Color4();
        }

        public Vertex3D(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 textureCoords, Color4 color)
        {
            Position = position;
            Color = color;
            Normal = normal;
            Tangent = tangent;
            TextureCoords = textureCoords;
        }

        public Vertex3D Colored(Color4 color) => new Vertex3D()
        {
            Position = Position,
            Color = color,
            Normal = Normal,
            Tangent = Tangent,
            TextureCoords = TextureCoords,
        };
    }
}
