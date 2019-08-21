﻿using OpenTK;
using SpiceEngine.Entities;
using SpiceEngine.Entities.Selection;
using SpiceEngine.Rendering;
using SpiceEngine.Rendering.Materials;
using SpiceEngine.Rendering.Meshes;
using SpiceEngine.Rendering.Shaders;
using SpiceEngine.Rendering.Textures;
using SpiceEngine.Rendering.Vertices;
using SpiceEngine.Utilities;
using System;
using System.Linq;

namespace SauceEditorCore.Models.Entities
{
    public class FaceEntity : TexturedModelEntity<ModelFace>, ITextureBinder, ITexturePath, IDirectional
    {
        private Vector2 _texturePosition;
        private float _textureRotation;
        private Vector2 _textureScale;

        public override Vector3 XDirection => IsInTextureMode ? ModelShape.Tangent : Vector3.UnitX;
        public override Vector3 YDirection => IsInTextureMode ? Vector3.Cross(ModelShape.Normal, ModelShape.Tangent) : Vector3.UnitY;
        public override Vector3 ZDirection => IsInTextureMode ? Vector3.Zero : Vector3.UnitZ;

        public FaceEntity(ModelFace modelFace, TexturePaths texturePaths) : base(modelFace, texturePaths) { }

        public override bool CompareUniforms(IEntity entity) => entity is FaceEntity faceEntity
            && IsInTextureMode == faceEntity.IsInTextureMode
            && base.CompareUniforms(entity);

        public override IRenderable ToRenderable()
        {
            var meshBuild = new ModelBuilder(ModelShape);
            var meshVertices = meshBuild.GetVertices();

            var mesh = meshVertices.Any(v => v.IsAnimated)
                ? (IMesh)new Mesh<AnimatedVertex3D>(meshBuild.GetVertices().Select(v => v.ToJointVertex3D()).ToList(), meshBuild.TriangleIndices.AsEnumerable().Reverse().ToList())
                : new Mesh<Vertex3D>(meshBuild.GetVertices().Select(v => v.ToVertex3D()).ToList(), meshBuild.TriangleIndices.AsEnumerable().Reverse().ToList());

            mesh.Transform(_modelMatrix.WorldTransform);
            return mesh;
        }
    }
}
