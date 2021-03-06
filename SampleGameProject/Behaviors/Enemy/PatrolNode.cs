﻿using OpenTK;
using SavoryPhysicsCore.Bodies;
using SpiceEngineCore.Entities.Actors;
using SpiceEngineCore.Utilities;
using System;
using UmamiScriptingCore.Behaviors;
using UmamiScriptingCore.Behaviors.Nodes;

namespace SampleGameProject.Behaviors.Enemy
{
    public class PatrolNode : Node
    {
        public Vector3 Destination { get; private set; }
        public float Speed { get; private set; }

        public PatrolNode(Vector3 destination, float speed)
        {
            Destination = destination;
            Speed = speed;
        }

        public override BehaviorStatus Tick(BehaviorContext context)
        {
            var difference = Destination - context.Position;

            if (!difference.IsSignificant())
            {
                return BehaviorStatus.Success;
            }
            else if (difference.Length < Speed)
            {
                ((RigidBody3D)context.Body).ApplyVelocity(difference);
            }
            else
            {
                ((RigidBody3D)context.Body).ApplyVelocity(difference.Normalized() * Speed);
            }

            if (context.Entity is IActor actor && ((RigidBody3D)context.Body).LinearVelocity.IsSignificant())
            {
                float turnAngle = (float)Math.Atan2(((RigidBody3D)context.Body).LinearVelocity.Y, ((RigidBody3D)context.Body).LinearVelocity.X);

                actor.Rotation = new Quaternion(0.0f, 0.0f, turnAngle);
                context.EulerRotation = new Vector3(context.EulerRotation.X, context.EulerRotation.Y, turnAngle);
            }

            return BehaviorStatus.Running;
        }

        public override void Reset() { }
    }
}
