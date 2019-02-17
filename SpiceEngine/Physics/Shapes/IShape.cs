﻿using OpenTK;
using SpiceEngine.Physics.Collision;

namespace SpiceEngine.Physics.Shapes
{
    public interface IShape
    {
        float Mass { get; set; }
        float MomentOfInertia { get; }

        IPartition ToCollider(Vector3 position);
        IShape Duplicate();
    }
}
