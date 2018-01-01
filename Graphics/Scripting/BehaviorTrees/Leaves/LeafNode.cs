﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Graphics.Scripting.BehaviorTrees.Leaves
{
    [DataContract]
    public class LeafNode : Node
    {
        [DataMember]
        public Func<Dictionary<string, object>, BehaviorStatuses> Behavior { get; internal set; }

        public LeafNode() { }

        public override void Tick(Dictionary<string, object> variablesByName)
        {
            if (!Status.IsComplete())
            {
                Status = Behavior.Invoke(variablesByName);
            }
        }
    }
}
