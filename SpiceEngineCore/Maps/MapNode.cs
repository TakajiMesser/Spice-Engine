﻿using SpiceEngineCore.Scripting;
using SpiceEngineCore.Scripting.Nodes;
using SpiceEngineCore.Scripting.Nodes.Composites;
using SpiceEngineCore.Scripting.Nodes.Decorators;
using SpiceEngineCore.Scripting.Nodes.Leaves;
using SpiceEngineCore.Scripting.Scripts;
using SpiceEngineCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueType = SpiceEngineCore.Serialization.ValueType;

namespace SpiceEngineCore.Maps
{
    public class MapNode
    {
        public enum NodeTypes
        {
            Parallel,
            Selector,
            Sequence,
            InlineCondition,
            Repeater,
            InlineLeaf,
            Node
        };

        public MapNode() { }
        public MapNode(params object[] args) => Arguments.AddRange(args.Select(a => ValueType.Create(a)));

        /*public MapNode(params object[] args)
        {
            foreach (var arg in args)
            {
                int a = 3;
                if (arg is Vector3)
                {
                    a = 4;
                }

                var valueType = ValueType.Create(arg);
                Arguments.Add(valueType);
            }
        }*/

        //public MapNode(params ValueType[] args) => Arguments.AddRange(args);
        //public List<object> Arguments { get; set; } = new List<object>();
        public List<ValueType> Arguments{ get; set; } = new List<ValueType>();
        public NodeTypes NodeType { get; set; }
        public List<MapNode> Children { get; set; } = new List<MapNode>();

        public Predicate<BehaviorContext> InlineCondition { get; set; }
        public Func<BehaviorContext, BehaviorStatus> InlineAction { get; set; }

        // For more complex custom nodes, we must parse and analyze the node's C# code
        public Script Script { get; set; }

        public IEnumerable<Script> GetScripts()
        {
            if (NodeType == NodeTypes.Node)
            {
                yield return Script;
            }

            foreach (var child in Children)
            {
                foreach (var script in child.GetScripts())
                {
                    yield return script;
                }
            }
        }

        public Node ToNode()
        {
            switch (NodeType)
            {
                case NodeTypes.Parallel:
                    return new ParallelNode(Children.Select(c => c.ToNode()));
                case NodeTypes.Selector:
                    return new SelectorNode(Children.Select(c => c.ToNode()));
                case NodeTypes.Sequence:
                    return new SequenceNode(Children.Select(c => c.ToNode()));
                case NodeTypes.InlineCondition:
                    return new InlineConditionNode(InlineCondition, Children.First().ToNode());
                case NodeTypes.Repeater:
                    return new RepeaterNode(Children.First().ToNode());
                case NodeTypes.InlineLeaf:
                    return new InlineLeafNode(InlineAction);
                case NodeTypes.Node:
                    // We now need to DELAY this...
                    return new ScriptNode(Script, Arguments, Children.Select(c => c.ToNode()));
                    //return ParseContent(scriptCompiler);
            }

            // TODO - Perform a better default case
            return null;
        }

        /*private Node ParseContent(IScriptCompiler scriptCompiler)
        {
            // TODO - Perform script compilation in an earlier step
            scriptCompiler.AddScript(Script);
            scriptCompiler.CompileScripts();
            scriptCompiler.ClearScripts();

            if (Script.HasErrors)
            {
                // TODO - Handle errors by notifying user
            }
            else
            {
                var type = Script.ExportedType;

                if (type.IsSubclassOf(typeof(CompositeNode)))
                {
                    if (Arguments.Count > 0)
                    {
                        return (Node)Activator.CreateInstance(type, Children.Select(c => c.ToNode()), GetConvertedArguments().ToArray());
                    }
                    else
                    {
                        return (Node)Activator.CreateInstance(type, Children.Select(c => c.ToNode()));
                    }
                }
                else if (type.IsSubclassOf(typeof(DecoratorNode)))
                {
                    if (Arguments.Count > 0)
                    {
                        return (Node)Activator.CreateInstance(type, Children.First().ToNode(), GetConvertedArguments().ToArray());
                    }
                    else
                    {
                        return (Node)Activator.CreateInstance(type, Children.First().ToNode());
                    }
                }
                else if (type.IsSubclassOf(typeof(Node)))
                {
                    if (Arguments.Count > 0)
                    {
                        return (Node)Activator.CreateInstance(type, GetConvertedArguments().ToArray());
                    }
                    else
                    {
                        return (Node)Activator.CreateInstance(type);
                    }
                }
                else
                {
                    // TODO - Notify user that the type MUST derive from Node
                }
            }

            return null;
        }*/
    }
}