﻿using OpenTK;
using SpiceEngineCore.Physics;
using System;

namespace SavoryPhysicsCore.Collisions
{
    public struct Oct : IPartition
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

        public float Length => (Max - Min).Length;

        public Oct(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public bool CanContain(Bounds bounds)
        {
            switch (bounds.Partition)
            {
                case Oct oct:
                    return Min.X < oct.Min.X
                        && Min.Y < oct.Min.Y
                        && Min.Z < oct.Min.Z
                        && Max.X > oct.Max.X
                        && Max.Y > oct.Max.Y
                        && Max.Z > oct.Max.Z;
            }

            throw new NotImplementedException();
        }
    }
}
