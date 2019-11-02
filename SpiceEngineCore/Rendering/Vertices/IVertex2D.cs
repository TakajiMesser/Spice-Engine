﻿using OpenTK;

namespace SpiceEngineCore.Rendering.Vertices
{
    public interface IVertex2D : IVertex
    {
        Vector2 Position { get; }
    }
}