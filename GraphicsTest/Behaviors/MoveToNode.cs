﻿using TakoEngine.Scripting.BehaviorTrees.Leaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TakoEngine.Scripting.Behaviors;
using OpenTK;
using System.Runtime.Serialization;

namespace GraphicsTest.Behaviors
{
    public class MoveToNode : Node
    {
        public Vector3 Destination { get; private set; }
        public float Speed { get; private set; }

        public MoveToNode(Vector3 destination, float speed)
        {
            Destination = destination;
            Speed = speed;
        }

        public override BehaviorStatus Tick(BehaviorContext context)
        {
            var difference = Destination - context.Position;

            if (difference == Vector3.Zero)
            {
                return BehaviorStatus.Success;
            }
            else if (difference.Length < Speed)
            {
                context.Translation = difference;
            }
            else
            {
                context.Translation = difference.Normalized() * Speed;
            }

            return BehaviorStatus.Running;
        }
    }
}
