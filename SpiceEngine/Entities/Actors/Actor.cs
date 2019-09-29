﻿using OpenTK;
using SpiceEngine.Rendering.Meshes;
using SpiceEngineCore.Entities;
using SpiceEngineCore.Rendering.Materials;
using SpiceEngineCore.Rendering.Matrices;
using SpiceEngineCore.Rendering.Shaders;
using SpiceEngineCore.Rendering.Textures;
using System.Collections.Generic;

namespace SpiceEngine.Entities.Actors
{
    public class Actor : TexturedEntity, IActor, IRotate, IScale, ITextureBinder, IModel
    {
        protected int _meshIndex = 0;

        public string Name { get; private set; }

        /// <summary>
        /// All models are assumed to have their "forward" direction in the positive X direction.
        /// If the model is oriented in a different direction, this quaternion should orient it from the assumed direction to the correct one.
        /// If the model is already oriented correctly, this should be the quaternion identity.
        /// </summary>
        public Quaternion Orientation { get; set; } = Quaternion.Identity;

        // TODO - Determine if quaternion multiplication order matters here
        public Quaternion Rotation
        {
            get => _modelMatrix.Rotation * Orientation.Inverted();
            set => _modelMatrix.Rotation = Orientation * value;
        }

        public Vector3 Scale
        {
            get => _modelMatrix.Scale;
            set => _modelMatrix.Scale = value;
        }

        public override void Transform(Transform transform)
        {
            base.Transform(new Transform()
            {
                Translation = transform.Translation,
                Rotation = Orientation * transform.Rotation,
                Scale = transform.Scale
            });
        }

        private List<Material> _materials = new List<Material>();
        private List<TextureMapping?> _textureMappings = new List<TextureMapping?>();

        public override Material Material => _materials[_meshIndex];
        public override TextureMapping? TextureMapping => _textureMappings[_meshIndex];

        public Actor(string name) => Name = name;

        public override void AddMaterial(Material material) => _materials.Add(material);
        public override void AddTextureMapping(TextureMapping? textureMapping) => _textureMappings.Add(textureMapping);

        public virtual Actor Duplicate(string name)
        {
            var actor = new Actor(name);
            actor.FromActor(this);
            return actor;
        }

        protected void FromActor(Actor actor)
        {
            Position = actor.Position;
            Orientation = actor.Orientation;
            Rotation = actor.Rotation;
            Scale = actor.Scale;

            _materials.AddRange(actor._materials);
            _textureMappings.AddRange(actor._textureMappings);
        }

        public void SetMeshIndex(int meshIndex) => _meshIndex = meshIndex;

        public override void SetUniforms(ShaderProgram program)
        {
            // TODO - Make this less janky. For now, we only want to set the model matrix once for all meshes, so bind it for the first mesh only
            if (_meshIndex == 0)
            {
                _modelMatrix.Set(program);
            }

            base.SetUniforms(program);
        }

        /*On Model Position Change -> if (Bounds != null)
        {
            Bounds.Center = value;
        }*/

        //public void ClearLights() => Model.ClearLights();
        //public void AddPointLights(IEnumerable<PointLight> lights) => Model.AddPointLights(lights);

        // Define how this object's state will be saved, if desired
        public virtual void OnSaveState() { }
    }
}
