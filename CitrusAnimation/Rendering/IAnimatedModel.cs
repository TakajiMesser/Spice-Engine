﻿using CitrusAnimationCore.Animations;
using CitrusAnimationCore.Bones;
using SweetGraphicsCore.Rendering.Models;

using Color4 = SpiceEngineCore.Geometry.Color4;
using Matrix2 = SpiceEngineCore.Geometry.Matrix2;
using Matrix3 = SpiceEngineCore.Geometry.Matrix3;
using Matrix4 = SpiceEngineCore.Geometry.Matrix4;
using Quaternion = SpiceEngineCore.Geometry.Quaternion;
using Vector2 = SpiceEngineCore.Geometry.Vector2;
using Vector3 = SpiceEngineCore.Geometry.Vector3;
using Vector4 = SpiceEngineCore.Geometry.Vector4;

namespace CitrusAnimationCore.Rendering
{
    public interface IAnimatedModel : IModel, IAnimate
    {
        Joint RootJoint { get; }
        Matrix4 GlobalInverseTransform { get; }

        Matrix4[] GetJointTransforms(int meshIndex);
    }
}