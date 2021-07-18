﻿using SpiceEngineCore.Rendering.Matrices;
using System;

using Color4 = SpiceEngineCore.Geometry.Color4;
using Matrix2 = SpiceEngineCore.Geometry.Matrix2;
using Matrix3 = SpiceEngineCore.Geometry.Matrix3;
using Matrix4 = SpiceEngineCore.Geometry.Matrix4;
using Quaternion = SpiceEngineCore.Geometry.Quaternion;
using Vector2 = SpiceEngineCore.Geometry.Vector2;
using Vector3 = SpiceEngineCore.Geometry.Vector3;
using Vector4 = SpiceEngineCore.Geometry.Vector4;

namespace SpiceEngineCore.Entities
{
    public class EntityTransformEventArgs : EventArgs
    {
        public int ID { get; }
        public Vector3 Position { get; }
        public Transform Transform { get; }

        public EntityTransformEventArgs(int id, Vector3 position, Transform transform)
        {
            ID = id;
            Position = position;
            Transform = transform;
        }
    }
}