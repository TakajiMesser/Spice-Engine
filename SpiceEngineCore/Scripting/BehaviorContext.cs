﻿using OpenTK;
using SpiceEngineCore.Entities;
using SpiceEngineCore.Entities.Actors;
using SpiceEngineCore.Entities.Cameras;
using SpiceEngineCore.Inputs;
using SpiceEngineCore.Physics.Bodies;
using SpiceEngineCore.Physics.Collisions;
using SpiceEngineCore.Scripting.StimResponse;
using System.Collections.Generic;
using System.Linq;

namespace SpiceEngineCore.Scripting
{
    public class BehaviorContext
    {
        private IEntityProvider _entityProvider;
        private ICollisionProvider _collisionProvider;
        private IStimulusProvider _stimulusProvider;

        public IActor Actor { get; internal set; }

        public Vector3 Position
        {
            get => ((Body3D)_collisionProvider.GetBody(Actor.ID)).Position;
            set => ((Body3D)_collisionProvider.GetBody(Actor.ID)).Position = value;
        }

        public IBody Body => _collisionProvider.GetBody(Actor.ID);

        public IEntityProvider GetEntityProvider() => _entityProvider;
        public IEntity GetEntity(int id) => _entityProvider.GetEntity(id);
        public IActor GetActor(string name) => (IActor)_entityProvider.GetEntity(name);

        public bool HasStimuli(int entityID, Stimulus stimulus) => _stimulusProvider.GetStimuli(entityID).Contains(stimulus);

        public IBody GetBody(int entityID) => _collisionProvider.GetBody(entityID);
        public IEnumerable<IBody> GetBodies() => _collisionProvider.GetCollisionIDs().Select(c => _collisionProvider.GetBody(c));
        public IEnumerable<IBody> GetColliderBodies() => _collisionProvider.GetCollisionIDs(Actor.ID).Select(c => _collisionProvider.GetBody(c));

        public ICamera Camera { get; internal set; }
        public IInputProvider InputProvider { get; internal set; }

        public Vector3 EulerRotation { get; set; }
        //public Vector3 Translation { get; set; }

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

        public void SetEntityProvider(IEntityProvider entityProvider) => _entityProvider = entityProvider;
        public void SetCollisionProvider(ICollisionProvider collisionProvider) => _collisionProvider = collisionProvider;
        public void SetInputProvider(IInputProvider inputProvider) => InputProvider = inputProvider;
        public void SetStimulusProvider(IStimulusProvider stimulusProvider) => _stimulusProvider = stimulusProvider;
    }
}