﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceEngine.Entities;
using SpiceEngine.Entities.Models;
using SpiceEngine.Rendering.Buffers;
using SpiceEngine.Rendering.Meshes;
using SpiceEngine.Rendering.Shaders;
using SpiceEngine.Rendering.Textures;
using SpiceEngine.Rendering.Vertices;

namespace SpiceEngine.Rendering.Batches
{
    public class BatchManager
    {
        private List<MeshBatch> _brushBatches = new List<MeshBatch>();
        private List<MeshBatch> _volumeBatches = new List<MeshBatch>();
        private List<ModelBatch> _actorBatches = new List<ModelBatch>();
        private List<ModelBatch> _jointBatches = new List<ModelBatch>();

        private Dictionary<int, IBatch> _batchesByEntityID = new Dictionary<int, IBatch>();

        public IBatch GetBatch(int entityID)
        {
            if (_batchesByEntityID.ContainsKey(entityID))
            {
                return _batchesByEntityID[entityID];
            }

            throw new KeyNotFoundException("No batch found for entity ID " + entityID);
        }

        public void AddBrush(int entityID, IMesh3D mesh)
        {
            var batch = new MeshBatch(entityID, mesh);

            _brushBatches.Add(batch);
            _batchesByEntityID.Add(entityID, batch);
        }

        public void AddVolume(int entityID, IMesh3D mesh)
        {
            var batch = new MeshBatch(entityID, mesh);

            _volumeBatches.Add(batch);
            _batchesByEntityID.Add(entityID, batch);
        }

        public void AddActor(int entityID, IEnumerable<IMesh3D> meshes)
        {
            var batch = new ModelBatch(entityID, meshes);

            _actorBatches.Add(batch);
            _batchesByEntityID.Add(entityID, batch);
        }

        public void AddJoint(int entityID, IEnumerable<IMesh3D> meshes)
        {
            var batch = new ModelBatch(entityID, meshes);

            _jointBatches.Add(batch);
            _batchesByEntityID.Add(entityID, batch);
        }

        public void Load()
        {
            foreach (var batch in _brushBatches)
            {
                batch.Load();
            }

            foreach (var batch in _volumeBatches)
            {
                batch.Load();
            }

            foreach (var batch in _actorBatches)
            {
                batch.Load();
            }

            foreach (var batch in _jointBatches)
            {
                batch.Load();
            }
        }

        public void DrawBrushes(IEntityProvider entityProvider, ShaderProgram shaderProgram, TextureManager textureManager = null)
        {
            foreach (var batch in _brushBatches)
            {
                batch.Draw(entityProvider, shaderProgram, textureManager);
            }
        }

        public void DrawBrushes(IEntityProvider entityProvider, ShaderProgram shaderProgram, Action<int> batchAction, TextureManager textureManager = null)
        {
            foreach (var batch in _brushBatches)
            {
                batchAction(batch.EntityID);
                batch.Draw(entityProvider, shaderProgram, textureManager);
            }
        }

        public void DrawVolumes(IEntityProvider entityProvider, ShaderProgram shaderProgram, TextureManager textureManager = null)
        {
            foreach (var batch in _volumeBatches)
            {
                batch.Draw(entityProvider, shaderProgram, textureManager);
            }
        }

        public void DrawVolumes(IEntityProvider entityProvider, ShaderProgram shaderProgram, Action<int> batchAction, TextureManager textureManager = null)
        {
            foreach (var batch in _volumeBatches)
            {
                batchAction(batch.EntityID);
                batch.Draw(entityProvider, shaderProgram, textureManager);
            }
        }

        public void DrawActors(IEntityProvider entityProvider, ShaderProgram shaderProgram, TextureManager textureManager = null)
        {
            foreach (var batch in _actorBatches)
            {
                batch.Draw(entityProvider, shaderProgram, textureManager);
            }
        }

        public void DrawActors(IEntityProvider entityProvider, ShaderProgram shaderProgram, Action<int> batchAction, TextureManager textureManager = null)
        {
            foreach (var batch in _actorBatches)
            {
                batchAction(batch.EntityID);
                batch.Draw(entityProvider, shaderProgram, textureManager);
            }
        }

        public void DrawJoints(IEntityProvider entityProvider, ShaderProgram shaderProgram, TextureManager textureManager = null)
        {
            foreach (var batch in _jointBatches)
            {
                batch.Draw(entityProvider, shaderProgram, textureManager);
            }
        }

        public void DrawJoints(IEntityProvider entityProvider, ShaderProgram shaderProgram, Action<int> batchAction, TextureManager textureManager = null)
        {
            foreach (var batch in _jointBatches)
            {
                batchAction(batch.EntityID);
                batch.Draw(entityProvider, shaderProgram, textureManager);
            }
        }
    }
}