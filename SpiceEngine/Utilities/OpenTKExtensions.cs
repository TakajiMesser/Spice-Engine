﻿using OpenTK;
using SpiceEngine.Rendering.Vertices;
using System.Drawing;

namespace SpiceEngine.Utilities
{
    public static class OpenTKExtensions
    {
        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }
    }
}