﻿using TakoEngine.Materials;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TakoEngine.Rendering.Vertices
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex// : IVertex
    {
        public Vector3 Position;// { get; set; }
        public Vector3 Normal;// { get; set; }
        public Vector3 Tangent;// { get; set; }
        public Color4 Color;// { get; set; }
        public Vector2 TextureCoords;// { get; set; }

        public Vertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 textureCoords)
        {
            Position = position;
            Normal = normal;
            Tangent = tangent;
            Color = new Color4();
            TextureCoords = textureCoords;
        }

        public Vertex Colored(Color4 color)
        {
            return new Vertex()
            {
                Position = Position,
                Normal = Normal,
                Tangent = Tangent,
                Color = color,
                TextureCoords = TextureCoords,
            };
        }
    }
}