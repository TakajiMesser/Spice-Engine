﻿using Graphics.Scripting.BehaviorTrees.Leaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphics.Scripting.BehaviorTrees;
using Graphics.GameObjects;
using Graphics.Physics.Raycasting;
using System.Runtime.Serialization;
using Graphics.Scripting.BehaviorTrees.Decorators;

namespace GraphicsTest.Behaviors.Enemy
{
    [DataContract]
    public class IsPlayerInSightNode : ConditionNode
    {
        [DataMember]
        public float ViewAngle { get; set; }

        [DataMember]
        public float ViewDistance { get; set; }

        public IsPlayerInSightNode(float viewAngle, float viewDistance, Node node) : base(node)
        {
            ViewAngle = viewAngle;
            ViewDistance = viewDistance;
        }

        public override bool Condition(BehaviorContext context)
        {
            var player = context.Colliders.FirstOrDefault(c => c.AttachedObject.GetType() == typeof(GameObject) && ((GameObject)c.AttachedObject).Name == "Player");

            if (player != null)
            {
                var playerPosition = ((GameObject)player.AttachedObject).Model.Position;

                var playerDirection = playerPosition - context.Position;
                float playerAngle = (float)Math.Atan2(playerDirection.Y, playerDirection.X);

                var angleDifference = (playerAngle - context.Rotation.X + Math.PI) % (2 * Math.PI) - Math.PI;
                if (angleDifference < -Math.PI)
                {
                    angleDifference += (float)(2 * Math.PI);
                }

                if (Math.Abs(angleDifference) <= ViewAngle / 2.0f)
                {
                    // Perform a raycast to see if any other colliders obstruct our view of the player
                    // TODO - Filter colliders by their ability to obstruct vision
                    if (Raycast.TryRaycast(new Ray3(context.Position, playerDirection, ViewDistance), context.Colliders, out RaycastHit hit))
                    {
                        if (hit.Collider.AttachedObject.GetType() == typeof(GameObject) && ((GameObject)hit.Collider.AttachedObject).Name == "Player")
                        {
                            return true;
                        }
                    }
                }
            }

            if (context.ContainsVariable("nAlertTicks"))
            {
                int nAlertTicks = context.GetVariable<int>("nAlertTicks");

                if (nAlertTicks > 0)
                {
                    nAlertTicks--;
                    context.SetVariable("nAlertTicks", nAlertTicks);
                }
            }

            return false;
        }
    }
}