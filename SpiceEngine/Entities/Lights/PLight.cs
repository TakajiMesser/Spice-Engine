﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using SpiceEngine.Outputs;
using SpiceEngine.Rendering.Shaders;
using SpiceEngine.Utilities;

namespace SpiceEngine.Entities.Lights
{
    /// <summary>
    /// This struct is used by the Forward Renderer, in a uniform buffer
    /// </summary>
    public struct PLight
    {
        public Vector3 Position { get; private set; }
        public float Radius { get; private set; }
        public Vector3 Color { get; private set; }
        public float Intensity { get; private set; }

        public PLight(Vector3 position, float radius, Vector3 color, float intensity)
        {
            Position = position;
            Radius = radius;
            Color = color;
            Intensity = intensity;
        }
    }
}