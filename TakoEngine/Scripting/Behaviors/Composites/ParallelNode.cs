﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakoEngine.Scripting.Behaviors.Composites
{
    public class ParallelNode : CompositeNode
    {
        public ParallelNode(IEnumerable<Node> nodes) : base(nodes) { }
        public ParallelNode(params Node[] nodes) : base(nodes) { }

        public override BehaviorStatus Tick(BehaviorContext context)
        {
            var status = BehaviorStatus.Success;

            foreach (var child in Children)
            {
                switch (child.Tick(context))
                {
                    case BehaviorStatus.Failure:
                        return BehaviorStatus.Failure;
                    case BehaviorStatus.Running:
                        status = BehaviorStatus.Running;
                        break;
                }
            }

            return status;
        }
    }
}
