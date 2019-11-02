﻿namespace SpiceEngineCore.Physics.Collisions
{
    public interface IPartition
    {
        float Length { get; }
        bool CanContain(Bounds bounds);
    }
}