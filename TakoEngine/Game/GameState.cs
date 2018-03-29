﻿using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using TakoEngine.Entities;
using TakoEngine.Entities.Cameras;
using TakoEngine.Entities.Lights;
using TakoEngine.Entities.Models;
using TakoEngine.Helpers;
using TakoEngine.Inputs;
using TakoEngine.Maps;
using TakoEngine.Outputs;
using TakoEngine.Physics.Collision;
using TakoEngine.Rendering.Processing;
using TakoEngine.Rendering.Textures;

namespace TakoEngine.Game
{
    public class GameState
    {
        private Resolution _resolution;
        internal InputState _inputState = new InputState();
        private RenderManager _renderManager;
        private TextureManager _textureManager = new TextureManager();
        private int _nextAvailableID = 1;

        internal Camera _camera;
        private List<Actor> _actors = new List<Actor>();
        private List<Brush> _brushes = new List<Brush>();
        private List<Light> _lights = new List<Light>();

        private QuadTree _actorQuads;
        private QuadTree _brushQuads;
        private QuadTree _lightQuads;

        public GameState(Resolution resolution, Resolution windowSize)
        {
            _resolution = resolution;
            _renderManager = new RenderManager(resolution, windowSize);
            _textureManager.EnableMipMapping = true;
            _textureManager.EnableAnisotropy = true;
        }

        public void LoadMap(Map map)
        {
            _camera = map.Camera.ToCamera(_resolution);

            LoadLightsFromMap(map);
            LoadBrushesFromMap(map);
            LoadActorsFromMap(map);

            _renderManager.Load(_brushes, _actors, map.SkyboxTextureFilePaths);
        }

        private void LoadLightsFromMap(Map map)
        {
            _lightQuads = new QuadTree(0, map.Boundaries);
            _lightQuads.InsertRange(map.Lights.Select(l => new BoundingCircle(l)));

            foreach (var mapLight in map.Lights)
            {
                AddEntity(mapLight);
            }
        }

        private void LoadBrushesFromMap(Map map)
        {
            _brushQuads = new QuadTree(0, map.Boundaries);

            foreach (var mapBrush in map.Brushes)
            {
                var brush = mapBrush.ToBrush();

                if (brush.HasCollision)
                {
                    _brushQuads.Insert(brush.Bounds);
                }
                brush.AddPointLights(_lightQuads.Retrieve(brush.Bounds).Where(c => c.AttachedEntity is PointLight).Select(c => (PointLight)c.AttachedEntity));
                brush.Mesh.TextureMapping = mapBrush.TexturesPaths.ToTextureMapping(_textureManager);

                AddEntity(brush);
            }
        }

        private void LoadActorsFromMap(Map map)
        {
            _actorQuads = new QuadTree(0, map.Boundaries);

            foreach (var mapActor in map.Actors)
            {
                var actor = mapActor.ToActor(_textureManager);

                switch (actor.Model)
                {
                    case SimpleModel s:
                        for (var i = 0; i < s.Meshes.Count; i++)
                        {
                            if (i < mapActor.TexturesPaths.Count)
                            {
                                s.Meshes[i].TextureMapping = mapActor.TexturesPaths[i].ToTextureMapping(_textureManager);
                            }
                        }
                        break;

                    case AnimatedModel a:
                        for (var i = 0; i < a.Meshes.Count; i++)
                        {
                            if (i < mapActor.TexturesPaths.Count)
                            {
                                a.Meshes[i].TextureMapping = mapActor.TexturesPaths[i].ToTextureMapping(_textureManager);
                            }
                        }
                        break;
                }

                if (map.Camera.AttachedActorName == actor.Name)
                {
                    _camera.AttachToEntity(actor, true, false);
                }

                AddEntity(actor);
            }
        }

        public Actor GetByName(string name) => _actors.First(g => g.Name == name);

        public IEntity GetByID(int id)
        {
            var actor = _actors.FirstOrDefault(g => g.ID == id);
            if (actor != null)
            {
                return actor;
            }
            else
            {
                var brush = _brushes.FirstOrDefault(b => b.ID == id);
                if (brush != null)
                {
                    return brush;
                }
                else
                {
                    var light = _lights.FirstOrDefault(l => l.ID == id);
                    if (light != null)
                    {
                        return light;
                    }
                    else
                    {
                        throw new KeyNotFoundException("Could not find any GameEntity with ID " + id);
                    }
                }
            }
        }

        public void AddEntity(IEntity entity)
        {
            // Assign a unique ID
            if (entity.ID == 0)
            {
                entity.ID = _nextAvailableID;
                _nextAvailableID++;
            }

            switch (entity)
            {
                case Actor actor:
                    if (string.IsNullOrEmpty(actor.Name)) throw new ArgumentException("Actor must have a name defined");
                    if (_actors.Any(g => g.Name == actor.Name)) throw new ArgumentException("Actor must have a unique name");
                    _actors.Add(actor);
                    break;
                case Brush brush:
                    _brushes.Add(brush);
                    break;
                case Light light:
                    _lights.Add(light);
                    break;
            }
        }

        public void Initialize()
        {
            foreach (var actor in _actors)
            {
                actor.OnInitialization();
            }
        }

        public void HandleInput()
        {
            _camera.OnHandleInput(_inputState);

            foreach (var actor in _actors)
            {
                actor.OnHandleInput(_inputState, _camera);
            }
        }

        public void UpdateFrame()
        {
            // Update the gameobject colliders every frame, since they could have moved
            _actorQuads.Clear();
            _actorQuads.InsertRange(_actors.Select(g => g.Bounds).Where(c => c != null));

            // For each object that has a non-zero transform, we need to determine the set of colliders to compare it against for hit detection
            foreach (var actor in _actors)
            {
                actor.ClearLights();
                actor.AddPointLights(_lightQuads.Retrieve(actor.Bounds)
                    .Where(c => c.AttachedEntity is PointLight)
                    .Select(c => (PointLight)c.AttachedEntity));

                var filteredColliders = _brushQuads.Retrieve(actor.Bounds)
                    .Concat(_actorQuads
                        .Retrieve(actor.Bounds)
                            .Where(c => ((Actor)c.AttachedEntity).Name != actor.Name));

                actor.OnUpdateFrame(filteredColliders);
            }

            _camera.OnUpdateFrame();

            PollForInput();
        }

        public void ResizeResolution() => _renderManager.ResizeResolution();
        public void ResizeWindow() => _renderManager.ResizeWindow();

        public void SetFrequency(double frequency) => _renderManager.Frequency = frequency;

        public void RenderFullFrame() => _renderManager.RenderFullFrame(_textureManager, _camera, _lights, _brushes, _actors);

        private void PollForInput() => _inputState.UpdateState(Keyboard.GetState(), Mouse.GetState());

        public void SaveToFile(string path) => throw new NotImplementedException();

        public static GameState LoadFromFile(string path) => throw new NotImplementedException();
    }
}