﻿using SavoryPhysicsCore;
using SavoryPhysicsCore.Bodies;
using SpiceEngineCore.Entities.Actors;
using SpiceEngineCore.Geometry;
using SpiceEngineCore.Utilities;
using System;
using UmamiScriptingCore;
using UmamiScriptingCore.Behaviors;
using UmamiScriptingCore.Behaviors.Nodes;


namespace SampleGameProject.Resources.Behaviors.Nodes
{
    public class TurnTowardsNode : Node
    {
        public override BehaviorStatus Tick(BehaviorContext context)
        {
            var body = context.GetComponent<IBody>() as RigidBody;

            if (context.GetEntity() is IActor actor && body.LinearVelocity.IsSignificant())
            {
                float turnAngle = (float)Math.Atan2(body.LinearVelocity.Y, body.LinearVelocity.X);

                actor.Rotation = new Quaternion(0.0f, 0.0f, turnAngle);
                context.EulerRotation = new Vector3(context.EulerRotation.X, context.EulerRotation.Y, turnAngle);
            }

            return BehaviorStatus.Success;
        }

        public override void Reset() { }
    }
}
