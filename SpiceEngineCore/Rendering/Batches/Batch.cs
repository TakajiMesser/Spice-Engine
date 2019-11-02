﻿using OpenTK;
using SpiceEngineCore.Entities;
using SpiceEngineCore.Rendering;
using SpiceEngineCore.Rendering.Matrices;
using SpiceEngineCore.Rendering.Shaders;
using SpiceEngineCore.Rendering.Textures;
using SpiceEngineCore.Rendering.Vertices;
using System;
using System.Collections.Generic;

namespace SpiceEngineCore.Rendering.Batches
{
    public abstract class Batch : IBatch
    {
        private List<int> _entityIDs = new List<int>();

        public IEnumerable<int> EntityIDs => _entityIDs;
        public int EntityCount => _entityIDs.Count;
        public bool IsLoaded { get; private set; }

        public virtual void AddEntity(int id, IRenderable renderable) => _entityIDs.Add(id);

        public virtual void Transform(int entityID, Transform transform) { }

        public virtual void TransformTexture(int entityID, Vector3 center, Vector2 translation, float rotation, Vector2 scale) { }

        public virtual void UpdateVertices(int entityID, Func<IVertex3D, IVertex3D> vertexUpdate) { }

        public virtual void RemoveEntity(int id) => _entityIDs.Remove(id);

        public virtual void Load() => IsLoaded = true;

        public abstract void Draw(IEntityProvider entityProvider, ShaderProgram shaderProgram, ITextureProvider textureProvider = null);
        public abstract IBatch Duplicate();
    }
}