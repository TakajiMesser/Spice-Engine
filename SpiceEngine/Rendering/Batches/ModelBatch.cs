﻿using SpiceEngine.Entities;
using SpiceEngine.Entities.Actors;
using SpiceEngine.Rendering.Meshes;
using SpiceEngine.Rendering.Shaders;
using SpiceEngine.Rendering.Textures;
using SpiceEngine.Rendering.Vertices;
using System.Collections.Generic;
using System.Linq;

namespace SpiceEngine.Rendering.Batches
{
    public class ModelBatch : IBatch
    {
        public int EntityID { get; private set; }
        public List<IMesh3D> Meshes { get; } = new List<IMesh3D>();
        public IEnumerable<IVertex3D> Vertices => Meshes.SelectMany(m => m.Vertices);

        public ModelBatch(int entityID, IEnumerable<IMesh3D> meshes)
        {
            EntityID = entityID;
            Meshes.AddRange(meshes);
        }

        public IBatch Duplicate(int entityID) => new ModelBatch(entityID, Meshes.Select(m => m.Duplicate()));

        public void Load()
        {
            foreach (var mesh in Meshes)
            {
                mesh.Load();
            }
        }

        public void Draw(IEntityProvider entityProvider, ShaderProgram shaderProgram, TextureManager textureManager = null)
        {
            var actor = (Actor)entityProvider.GetEntity(EntityID);
            actor.SetUniforms(shaderProgram, textureManager);

            for (var i = 0; i < Meshes.Count; i++)
            {
                actor.SetUniforms(shaderProgram, textureManager, i);
                Meshes[i].Draw();
            }
        }
    }
}
