﻿using OpenTK;
using System.Collections.Generic;
using System.Linq;
using SpiceEngine.Entities.Cameras;
using SpiceEngine.Entities.Lights;
using SpiceEngine.Entities.Models;
using SpiceEngine.Game;
using SpiceEngine.Inputs;
using SpiceEngine.Physics.Collision;
using SpiceEngine.Rendering.Shaders;
using SpiceEngine.Rendering.Textures;
using SpiceEngine.Scripting.Behaviors;
using SpiceEngine.Scripting.StimResponse;
using SpiceEngine.Rendering.Matrices;
using SpiceEngine.Rendering.Materials;

namespace SpiceEngine.Entities
{
    public class Actor : IEntity, IStimulate, ICollidable, IRotate, IScale
    {
        public int ID { get; set; }
        public string Name { get; private set; }

        /// <summary>
        /// All models are assumed to have their "forward" direction in the positive X direction.
        /// If the model is oriented in a different direction, this quaternion should orient it from the assumed direction to the correct one.
        /// If the model is already oriented correctly, this should be the quaternion identity.
        /// </summary>
        public Quaternion Orientation { get; set; } = Quaternion.Identity;

        public Vector3 Position
        {
            get => _modelMatrix.Translation;
            set => _modelMatrix.Translation = value;
        }

        public Quaternion Rotation
        {
            get => Orientation * _modelMatrix.Rotation;
            set => _modelMatrix.Rotation = Orientation * value;
        }

        private Vector3 _originalRotation;
        public Vector3 OriginalRotation
        {
            get => _originalRotation;
            set
            {
                _originalRotation = value;
                _modelMatrix.Rotation = Quaternion.FromEulerAngles(value);
            }
        }

        public Vector3 Scale
        {
            get => _modelMatrix.Scale;
            set => _modelMatrix.Scale = value;
        }

        public Behavior Behaviors { get; set; }
        public List<Stimulus> Stimuli { get; private set; } = new List<Stimulus>();
        public InputBinding InputMapping { get; set; } = new InputBinding();
        public Dictionary<string, GameProperty> Properties { get; private set; } = new Dictionary<string, GameProperty>();

        public Bounds Bounds { get; set; }
        public bool HasCollision { get; set; } = true;

        public Matrix4 ModelMatrix => _modelMatrix.Matrix;

        private ModelMatrix _modelMatrix = new ModelMatrix();

        private Dictionary<int, Material> _materialByMeshIndex = new Dictionary<int, Material>();
        private Dictionary<int, TextureMapping> _textureMappingByMeshIndex = new Dictionary<int, TextureMapping>();

        public Actor(string name)
        {
            Name = name;
        }

        public void AddMaterial(int meshIndex, Material material) => _materialByMeshIndex.Add(meshIndex, material);

        public void AddTextureMapping(int meshIndex, TextureMapping textureMapping) => _textureMappingByMeshIndex.Add(meshIndex, textureMapping);

        public virtual void SetUniforms(ShaderProgram program, TextureManager textureManager)
        {
            _modelMatrix.Set(program);
        }

        public virtual void SetUniforms(ShaderProgram program, TextureManager textureManager, int meshIndex)
        {
            _materialByMeshIndex[meshIndex].SetUniforms(program);
            if (textureManager != null && _textureMappingByMeshIndex.ContainsKey(meshIndex))
            {
                program.BindTextures(textureManager, _textureMappingByMeshIndex[meshIndex]);
            }
            else
            {
                program.UnbindTextures();
            }
        }

        /*On Model Position Change -> if (Bounds != null)
        {
            Bounds.Center = value;
        }*/

        //public void ClearLights() => Model.ClearLights();
        //public void AddPointLights(IEnumerable<PointLight> lights) => Model.AddPointLights(lights);

        public virtual void OnInitialization()
        {
            if (Behaviors != null)
            {
                Behaviors.Context.Actor = this;

                foreach (var property in Properties)
                {
                    if (property.Value.IsConstant)
                    {
                        Behaviors.Context.AddProperty(property.Key, property.Value.Value);
                    }
                }
            }
        }

        public virtual void OnHandleInput(InputManager inputManager, Camera camera)
        {
            if (Behaviors != null)
            {
                Behaviors.Context.InputManager = inputManager;
                Behaviors.Context.InputMapping = InputMapping;
                Behaviors.Context.Camera = camera;
            }
        }

        public virtual void OnUpdateFrame(IEnumerable<Bounds> colliders)
        {
            if (Behaviors != null)
            {
                //Behaviors.Context.Rotation = Rotation;
                Behaviors.Context.Colliders = colliders;

                foreach (var property in Properties.Where(p => !p.Value.IsConstant))
                {
                    Behaviors.Context.SetProperty(property.Key, property.Value);
                }

                Behaviors.Tick();

                if (Behaviors.Context.Translation != Vector3.Zero)
                {
                    HandleCollisions(Behaviors.Context.Translation, colliders);
                    Behaviors.Context.Translation = Vector3.Zero;
                }

                //Rotation = Quaternion.FromEulerAngles(Behaviors.Context.Rotation);
                //Model.Rotation = Behaviors.Context.QRotation;
            }
        }

        public virtual void HandleCollisions(Vector3 translation, IEnumerable<Bounds> colliders)
        {
            if (HasCollision && Bounds != null && translation != Vector3.Zero)
            {
                Bounds.Center = Position + translation;

                foreach (var collider in colliders)
                {
                    if (collider.AttachedEntity is ICollidable collidable && collidable.HasCollision)
                    {
                        switch (collider)
                        {
                            case BoundingCircle circle:
                                if (Bounds.CollidesWith(circle))
                                {
                                    // Correct the X translation
                                    Bounds.Center = new Vector3(Position.X + translation.X, Position.Y, Position.Z);
                                    if (Bounds.CollidesWith(circle))
                                    {
                                        translation.X = 0;
                                    }

                                    // Correct the Y translation
                                    Bounds.Center = new Vector3(Position.X, Position.Y + translation.Y, Position.Z);
                                    if (Bounds.CollidesWith(circle))
                                    {
                                        translation.Y = 0;
                                    }
                                }
                                break;

                            case BoundingBox box:
                                if (Bounds.CollidesWith(box))
                                {
                                    // Correct the X translation
                                    Bounds.Center = new Vector3(Position.X + translation.X, Position.Y, Position.Z);
                                    if (Bounds.CollidesWith(box))
                                    {
                                        translation.X = 0;
                                    }

                                    // Correct the Y translation
                                    Bounds.Center = new Vector3(Position.X, Position.Y + translation.Y, Position.Z);
                                    if (Bounds.CollidesWith(box))
                                    {
                                        translation.Y = 0;
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            Position += translation;
        }

        // Define how this object's state will be saved, if desired
        public virtual void OnSaveState() { }
    }
}