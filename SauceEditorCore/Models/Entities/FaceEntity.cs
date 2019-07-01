﻿using OpenTK;
using SpiceEngine.Entities;
using SpiceEngine.Maps;
using SpiceEngine.Rendering;
using SpiceEngine.Rendering.Meshes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SauceEditorCore.Models.Entities
{
    public class FaceEntity : IModelEntity
    {
        public int ID { get; set; }
        public Vector3 Position { get; set; }
        public MeshFace Face { get; }

        public FaceEntity(MeshFace meshFace) => Face = meshFace;

        public IRenderable ToRenderable()
        {
            var meshBuild = new MeshBuild(Face);
            var meshVertices = meshBuild.GetVertices();

            if (meshVertices.Any(v => v.IsAnimated))
            {
                var vertices = meshBuild.GetVertices().Select(v => v.ToVertex3D());
                return new Mesh<Vertex3D>(vertices, meshBuild.TriangleIndices);
            }
            else
            {
                var vertices = meshBuild.GetVertices().Select(v => v.ToJointVertex3D());
                return new Mesh<JointVertex3D>(vertices, meshBuild.TriangleIndices);
            }
        }
    }
}