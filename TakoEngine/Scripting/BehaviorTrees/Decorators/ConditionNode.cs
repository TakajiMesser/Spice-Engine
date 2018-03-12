﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TakoEngine.Scripting.BehaviorTrees.Decorators
{
    [DataContract]
    public abstract class ConditionNode : DecoratorNode
    {
        public ConditionNode(Node node) : base(node) { }

        public override void Tick(BehaviorContext context)
        {
            if (!Status.IsComplete())
            {
                Status = BehaviorStatuses.Running;

                if (Condition(context))
                {
                    Child.Tick(context);
                    Status = Child.Status;
                }
                else
                {
                    Status = BehaviorStatuses.Failure;
                }
            }
        }

        public abstract bool Condition(BehaviorContext context);
    }
}