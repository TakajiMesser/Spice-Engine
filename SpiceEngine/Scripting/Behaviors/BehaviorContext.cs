﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceEngine.Entities;
using SpiceEngine.Entities.Cameras;
using SpiceEngine.Inputs;
using SpiceEngine.Physics.Collision;
using SpiceEngine.Scripting.StimResponse;

namespace SpiceEngine.Scripting.Behaviors
{
    public class BehaviorContext
    {
        /// <summary>
        /// The actor that this behavior belongs to
        /// </summary>
        public Actor Actor { get; internal set; }

        /// <summary>
        /// The set of colliders within range of this actor
        /// </summary>
        public IEnumerable<Bounds> Colliders { get; internal set; }

        public Camera Camera { get; internal set; }
        public InputManager InputManager { get; internal set; }
        public InputBinding InputMapping { get; internal set; }

        public Vector3 EulerRotation { get; set; }
        public Vector3 Translation { get; set; }

        public Dictionary<string, object> PropertiesByName { get; protected set; } = new Dictionary<string, object>();
        public Dictionary<string, object> VariablesByName { get; protected set; } = new Dictionary<string, object>();

        public bool ContainsProperty(string name) => PropertiesByName.ContainsKey(name);
        public void AddProperty(string name, object value) => PropertiesByName.Add(name, value);
        public T GetProperty<T>(string name) => (T)PropertiesByName[name];
        public void SetProperty(string name, object value) => PropertiesByName[name] = value;

        public bool ContainsVariable(string name) => VariablesByName.ContainsKey(name);
        public void AddVariable(string name, object value) => VariablesByName.Add(name, value);
        public void RemoveVariable(string name) => VariablesByName.Remove(name);
        public T GetVariable<T>(string name) => (T)VariablesByName[name];
        public T GetVariableOrDefault<T>(string name) => VariablesByName.ContainsKey(name) ? (T)VariablesByName[name] : default(T);
        public void SetVariable(string name, object value) => VariablesByName[name] = value;

        public void RemoveVariableIfExists(string name)
        {
            if (VariablesByName.ContainsKey(name))
            {
                VariablesByName.Remove(name);
            }
        }
    }
}
