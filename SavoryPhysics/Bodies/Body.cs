﻿using SavoryPhysicsCore.Shapes;
using SpiceEngineCore.Components;

using Color4 = SpiceEngineCore.Geometry.Color4;
using Matrix2 = SpiceEngineCore.Geometry.Matrix2;
using Matrix3 = SpiceEngineCore.Geometry.Matrix3;
using Matrix4 = SpiceEngineCore.Geometry.Matrix4;
using Quaternion = SpiceEngineCore.Geometry.Quaternion;
using Vector2 = SpiceEngineCore.Geometry.Vector2;
using Vector3 = SpiceEngineCore.Geometry.Vector3;
using Vector4 = SpiceEngineCore.Geometry.Vector4;

namespace SavoryPhysicsCore.Bodies
{
    public abstract class Body : Component, IBody
    {
        public Body(int entityID, IShape shape) : base(entityID) => Shape = shape;

        public IShape Shape { get; }
        public BodyStates State { get; set; }
        public float Restitution { get; set; }

        public bool IsPhysical { get; set; }
        public bool IsMovable => this is RigidBody || this is SoftBody;

        public virtual Vector3 UpdatePosition(Vector3 position, int nTicks) => position;
        
        public virtual Quaternion UpdateRotation(Quaternion rotation, int nTicks) => rotation;

        // An impulse is an instantaneous change in velocity
        public virtual void ApplyImpulse(Vector3 impulse) { }

        public virtual void ApplyVelocity(Vector3 velocity) { }

        // Assume the force here is applied directly to the center of mass
        public virtual void ApplyForce(Vector3 force) { }

        public virtual void ApplyPreciseForce(Vector3 force, Vector3 point) { }
    }
}