﻿namespace SpiceEngine.Physics.Collisions
{
    public interface ICollidable
    {
        Bounds Bounds { get; set; }
        bool HasCollision { get; set; }
    }
}
