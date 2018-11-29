﻿using OpenTK;
using SpiceEngine.Entities.Cameras;
using SpiceEngine.Outputs;
using SpiceEngine.Rendering.Matrices;

namespace SpiceEngine.Maps
{
    public class MapCamera
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public string AttachedActorName { get; set; }
        public ProjectionTypes Type { get; set; }
        public float ZNear { get; set; }
        public float ZFar { get; set; }

        /// <summary>
        /// Only relevant for orthographic cameras
        /// </summary>
        public float StartingWidth { get; set; }

        /// <summary>
        /// Only relevant for perspective cameras
        /// </summary>
        public float FieldOfViewY { get; set; }

        public Camera ToCamera(Resolution resolution)
        {
            var camera = Type == ProjectionTypes.Orthographic
                ? (Camera)new OrthographicCamera(Name, resolution, ZNear, ZFar, StartingWidth)
                : new PerspectiveCamera(Name, resolution, ZNear, ZFar, FieldOfViewY);

            if (Position != null)
            {
                camera.Position = Position;
            }

            return camera;
        }
    }
}