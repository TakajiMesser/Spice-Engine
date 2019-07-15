﻿using SpiceEngine.Entities;
using SpiceEngine.Entities.Actors;
using SpiceEngine.Entities.Brushes;
using SpiceEngine.Entities.Cameras;
using SpiceEngine.Entities.Lights;
using SpiceEngine.Entities.Volumes;
using SpiceEngine.Rendering.Meshes;
using SpiceEngine.Rendering.Shaders;
using SpiceEngine.Rendering.Textures;
using SpiceEngine.Rendering.Vertices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceEngine.Rendering.Batches
{
    public enum RenderTypes
    {
        OpaqueStatic,
        OpaqueAnimated,
        TransparentStatic,
        TransparentAnimated
    }

    public class BatchManager
    {
        private IEntityProvider _entityProvider;
        private ITextureProvider _textureProvider;

        private HashSet<int> _opaqueStaticIDs = new HashSet<int>();
        private HashSet<int> _opaqueAnimatedIDs = new HashSet<int>();
        private HashSet<int> _transparentStaticIDs = new HashSet<int>();
        private HashSet<int> _transparentAnimatedIDs = new HashSet<int>();

        private Dictionary<int, RenderTypes> _renderTypeByEntityID = new Dictionary<int, RenderTypes>();
        private Dictionary<int, int> _batchIndexByEntityID = new Dictionary<int, int>();
        private List<IBatch> _batches = new List<IBatch>();

        public bool IsLoaded { get; private set; } = false;

        public BatchManager(IEntityProvider entityProvider, ITextureProvider textureProvider)
        {
            _entityProvider = entityProvider;
            _textureProvider = textureProvider;
        }

        public void DuplicateBatch(int entityID, int newID)
        {
            var batch = GetBatch(entityID);
            
            // TODO = Handle texture mapping duplication outside of here (in RenderManager?)
            switch (batch)
            {
                case IMesh mesh:
                    AddEntity(newID, mesh.Duplicate());
                    break;
                case Model model:
                    AddEntity(newID, model.Duplicate());
                    break;
                case TextureID textureID:
                    AddEntity(newID, textureID.Duplicate());
                    break;
            }
        }

        public IBatch GetBatch(int entityID)
        {
            if (!_batchIndexByEntityID.ContainsKey(entityID)) throw new KeyNotFoundException("No batch found for entity ID " + entityID);

            var batchIndex = _batchIndexByEntityID[entityID];
            return _batches[batchIndex];
        }

        public void RemoveByEntityID(int entityID)
        {
            switch (_renderTypeByEntityID[entityID])
            {
                case RenderTypes.OpaqueStatic:
                    _opaqueStaticIDs.Remove(entityID);
                    break;
                case RenderTypes.OpaqueAnimated:
                    _opaqueAnimatedIDs.Remove(entityID);
                    break;
                case RenderTypes.TransparentStatic:
                    _transparentStaticIDs.Remove(entityID);
                    break;
                case RenderTypes.TransparentAnimated:
                    _transparentAnimatedIDs.Remove(entityID);
                    break;
            }

            _renderTypeByEntityID.Remove(entityID);

            var batchIndex = _batchIndexByEntityID[entityID];
            _batchIndexByEntityID.Remove(entityID);

            var batch = _batches[batchIndex];
            batch.RemoveEntity(entityID);

            if (!batch.EntityIDs.Any())
            {
                // TODO - This is horribly inefficient AND not thread-safe
                // Since we are removing this batch from the list, the dictionary values need to be updated accordingly
                _batches.RemoveAt(batchIndex);

                foreach (var entityIDKey in _batchIndexByEntityID.Keys.ToList())
                {
                    var batchIndexValue = _batchIndexByEntityID[entityIDKey];

                    if (batchIndexValue >= batchIndex)
                    {
                        _batchIndexByEntityID[entityIDKey] = batchIndexValue - 1;
                    }
                }
            }
        }

        public void AddEntity(int entityID, IRenderable renderable)
        {
            var opaqueIDs = renderable.IsAnimated ? _opaqueAnimatedIDs : _opaqueStaticIDs;
            var transparentIDs = renderable.IsAnimated ? _transparentAnimatedIDs : _transparentStaticIDs;

            renderable.AlphaChanged += (s, args) =>
            {
                if (!args.WasTransparent && args.IsTransparent)
                {
                    opaqueIDs.Remove(entityID);
                    transparentIDs.Add(entityID);
                    _renderTypeByEntityID[entityID] = renderable.IsAnimated ? RenderTypes.TransparentAnimated : RenderTypes.TransparentStatic;
                }
                else if (args.WasTransparent && !args.IsTransparent)
                {
                    transparentIDs.Remove(entityID);
                    opaqueIDs.Add(entityID);
                    _renderTypeByEntityID[entityID] = renderable.IsAnimated ? RenderTypes.OpaqueAnimated : RenderTypes.OpaqueStatic;
                }
            };

            if (renderable.IsTransparent)
            {
                transparentIDs.Add(entityID);
                _renderTypeByEntityID.Add(entityID, renderable.IsAnimated ? RenderTypes.TransparentAnimated : RenderTypes.TransparentStatic);
            }
            else
            {
                opaqueIDs.Add(entityID);
                _renderTypeByEntityID.Add(entityID, renderable.IsAnimated ? RenderTypes.OpaqueAnimated : RenderTypes.OpaqueStatic);
            }

            var batch = CreateOrGetBatch(entityID, renderable);
            batch.AddEntity(entityID, renderable);

            // TODO - What is this part doing? Do I need to do this for Model-Mesh entities as well?
            /*if (_entityProvider.GetEntity(entityID) is Brush brush)
            {
                batch.Transform(brush.ID, brush.GetModelMatrix());
            }*/

            //var entity = _entityProvider.GetEntity(entityID) as Entity;
            //batch.Transform(entityID, entity.GetModelMatrix());

            /*var entity = _entityProvider.GetEntity(entityID) as Entity;
            if (entity != null && !(entity is Actor) && !(entity is ILight) && !(entity is Volume))
            {
                batch.Transform(entity.ID, entity.GetModelMatrix());
            }*/

            //AddBatch(entityID, batch);
        }

        public void UpdateVertices(int entityID, Func<IVertex3D, IVertex3D> vertexUpdate)
        {
            var batch = GetBatch(entityID);
            batch.UpdateVertices(entityID, vertexUpdate);
        }

        private Batch CreateOrGetBatch(int entityID, IRenderable renderable)
        {
            var match = FindAppropriateBatch(entityID, renderable);

            if (match != null)
            {
                return match;
            }
            else
            {
                var batch = CreateBatch(renderable);
                _batches.Add(batch);

                var batchIndex = _batches.Count - 1;
                _batchIndexByEntityID.Add(entityID, batchIndex);

                //var entity = _entityProvider.GetEntity(entityID) as Entity;
                //batch.Transform(entityID, entity.GetModelMatrix());

                if (IsLoaded)
                {
                    batch.Load();
                }

                return batch;
            }
        }

        private Batch CreateBatch(IRenderable renderable)
        {
            switch (renderable)
            {
                case IMesh mesh:
                    return new MeshBatch(mesh);
                case Model model:
                    return new ModelBatch(model);
                case TextureID textureID:
                    return new BillboardBatch(textureID);
            }

            throw new ArgumentOutOfRangeException("Could not handle renderable of type " + renderable.GetType());
        }

        private Batch FindAppropriateBatch(int entityID, IRenderable renderable)
        {
            if (renderable is IMesh)
            {
                var entityA = _entityProvider.GetEntity(entityID);

                for (var i = 0; i < _batches.Count; i++)
                {
                    if (_batches[i] is MeshBatch meshBatch)
                    {
                        var entityB = _entityProvider.GetEntity(meshBatch.EntityIDs.First());

                        if (entityA.CompareUniforms(entityB))
                        {
                            entityA.Transformed += (s, args) =>
                            {
                                meshBatch.Transform(args.ID, args.Transform);
                            };

                            _batchIndexByEntityID.Add(entityID, i);
                            return meshBatch;
                        }
                    }
                }

                /*foreach (var meshBatch in _batches.OfType<MeshBatch>())
                {
                    var entityB = _entityProvider.GetEntity(meshBatch.EntityIDs.First());

                    if (entityA.CompareUniforms(entityB))
                    {
                        entityA.Transformed += (s, args) =>
                        {
                            meshBatch.Transform(args.ID, args.Transform);
                        };

                        return meshBatch;
                    }
                }*/
            }

            return null;
        }

        /*private void AddBatch(int entityID, IBatch batch)
        {
            _batches.Add(batch);

            var batchIndex = _batches.Count - 1;
            _batchIndexByEntityID.Add(entityID, batchIndex);

            if (IsLoaded)
            {
                batch.Load();
            }
        }*/

        public void Load(int entityID) => GetBatch(entityID).Load();

        public void Load()
        {
            foreach (var batch in _batches)
            {
                batch.Load();
            }

            IsLoaded = true;
        }

        public BatchAction CreateBatchAction() => new BatchAction(this);

        private void DrawEntities(ShaderProgram shader, HashSet<int> ids, Action<int> action = null)
        {
            var batchIndices = new HashSet<int>();

            foreach (var id in _entityProvider.EntityRenderIDs)
            {
                // TODO - Handle case where ids is NOT null, and we only want to render some of the entities within a single batch
                if ((ids == null || ids.Contains(id)))
                {
                    action?.Invoke(id);

                    var batchIndex = _batchIndexByEntityID[id];
                    if (!batchIndices.Contains(batchIndex))
                    {
                        batchIndices.Add(batchIndex);
                        _batches[batchIndex].Draw(_entityProvider, shader, _textureProvider);
                    }
                }
            }
        }

        private void DrawEntities(RenderTypes renderType, ShaderProgram shader, HashSet<int> ids, Action<int> action = null)
        {
            var batchIndices = new HashSet<int>();

            foreach (var id in _entityProvider.EntityRenderIDs)
            {
                // TODO - Handle case where ids is NOT null, and we only want to render some of the entities within a single batch
                if ((ids == null || ids.Contains(id)) && GetEntityIDSet(renderType).Contains(id))
                {
                    action?.Invoke(id);

                    var batchIndex = _batchIndexByEntityID[id];

                    if (!batchIndices.Contains(batchIndex))
                    {
                        batchIndices.Add(batchIndex);
                        _batches[batchIndex].Draw(_entityProvider, shader, _textureProvider);
                    }
                }
            }
        }

        private HashSet<int> GetEntityIDSet(RenderTypes renderType)
        {
            switch (renderType)
            {
                case RenderTypes.OpaqueStatic:
                    return _opaqueStaticIDs;
                case RenderTypes.OpaqueAnimated:
                    return _opaqueAnimatedIDs;
                case RenderTypes.TransparentStatic:
                    return _transparentStaticIDs;
                case RenderTypes.TransparentAnimated:
                    return _transparentAnimatedIDs;
            }

            throw new ArgumentOutOfRangeException("Could not handle render type " + renderType);
        }

        public class BatchAction
        {
            private BatchManager _batchManager;
            private Queue<Action> _commandQueue = new Queue<Action>();
            private HashSet<int> _entityIDs;

            public BatchAction(BatchManager batchManager) => _batchManager = batchManager;

            public ShaderProgram Shader { get; set; }
            public ICamera Camera { get; set; }

            public BatchAction SetShader(ShaderProgram shader)
            {
                _commandQueue.Enqueue(() =>
                {
                    Shader = shader;
                    Shader.Use();
                });
                return this;
            }

            public BatchAction SetCamera(ICamera camera)
            {
                _commandQueue.Enqueue(() =>
                {
                    Camera = camera;
                    Camera.SetUniforms(Shader);
                });
                return this;
            }

            public BatchAction SetCamera(ICamera camera, PointLight pointLight)
            {
                _commandQueue.Enqueue(() =>
                {
                    Camera = camera;
                    Camera.SetUniforms(Shader, pointLight);
                });
                return this;
            }

            public BatchAction SetCamera(ICamera camera, SpotLight spotLight)
            {
                _commandQueue.Enqueue(() =>
                {
                    Camera = camera;
                    Camera.SetUniforms(Shader, spotLight);
                });
                return this;
            }

            public BatchAction SetUniform<T>(string name, T value) where T : struct
            {
                _commandQueue.Enqueue(() => Shader.SetUniform<T>(name, value));
                return this;
            }

            public BatchAction SetEntityIDs(IEnumerable<int> ids)
            {
                _commandQueue.Enqueue(() => _entityIDs = new HashSet<int>(ids));
                return this;
            }

            public BatchAction ClearEntityIDs()
            {
                _commandQueue.Enqueue(() => _entityIDs = null);
                return this;
            }

            public BatchAction PerformAction(Action action)
            {
                _commandQueue.Enqueue(action);
                return this;
            }

            public BatchAction SetTexture(Texture texture, string name, int index)
            {
                _commandQueue.Enqueue(() => Shader.BindTexture(texture, name, index));
                return this;
            }

            public BatchAction RenderEntities()
            {
                _commandQueue.Enqueue(() => _batchManager.DrawEntities(Shader, _entityIDs));
                return this;
            }

            public BatchAction RenderOpaqueStatic()
            {
                _commandQueue.Enqueue(() => _batchManager.DrawEntities(RenderTypes.OpaqueStatic, Shader, _entityIDs));
                return this;
            }

            public BatchAction RenderOpaqueAnimated()
            {
                _commandQueue.Enqueue(() => _batchManager.DrawEntities(RenderTypes.OpaqueAnimated, Shader, _entityIDs));
                return this;
            }

            public BatchAction RenderTransparentStatic()
            {
                _commandQueue.Enqueue(() => _batchManager.DrawEntities(RenderTypes.TransparentStatic, Shader, _entityIDs));
                return this;
            }

            public BatchAction RenderTransparentAnimated()
            {
                _commandQueue.Enqueue(() => _batchManager.DrawEntities(RenderTypes.TransparentAnimated, Shader, _entityIDs));
                return this;
            }

            public BatchAction RenderOpaqueStaticWithAction(Action<int> action)
            {
                _commandQueue.Enqueue(() => _batchManager.DrawEntities(RenderTypes.OpaqueStatic, Shader, _entityIDs, action));
                return this;
            }

            public BatchAction RenderOpaqueAnimatedWithAction(Action<int> action)
            {
                _commandQueue.Enqueue(() => _batchManager.DrawEntities(RenderTypes.OpaqueAnimated, Shader, _entityIDs, action));
                return this;
            }

            public BatchAction RenderTransparentStaticWithAction(Action<int> action)
            {
                _commandQueue.Enqueue(() => _batchManager.DrawEntities(RenderTypes.TransparentStatic, Shader, _entityIDs, action));
                return this;
            }

            public BatchAction RenderTransparentAnimatedWithAction(Action<int> action)
            {
                _commandQueue.Enqueue(() => _batchManager.DrawEntities(RenderTypes.TransparentAnimated, Shader, _entityIDs, action));
                return this;
            }

            public void Execute()
            {
                while (_commandQueue.Any())
                {
                    Action action = _commandQueue.Dequeue();
                    action();
                }
            }
        }
    }
}
