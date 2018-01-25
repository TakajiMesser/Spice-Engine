﻿using Graphics.GameObjects;
using Graphics.Lighting;
using Graphics.Meshes;
using Graphics.Rendering.Vertices;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphics.Physics.Collision
{
    public class BoundingCircle : Bounds
    {
        public float Radius { get; set; }

        public BoundingCircle(GameObject gameObject) : base(gameObject)
        {
            var maxDistanceSquared = gameObject.Mesh.Vertices.Select(v => v.Position.Xy.LengthSquared).Max();
            Radius = (float)Math.Sqrt(maxDistanceSquared);
        }

        public BoundingCircle(Brush brush) : base(brush)
        {
            var maxDistanceSquared = brush.Vertices.Select(v => v.Position.Xy.LengthSquared).Max();
            Radius = (float)Math.Sqrt(maxDistanceSquared);
        }

        public BoundingCircle(PointLight light) : base(light)
        {
            Radius = light.Radius;
        }

        public override bool CollidesWith(Vector3 point)
        {
            var distanceSquared = Math.Pow(point.X - Center.X, 2.0f) + Math.Pow(point.Y - Center.Y, 2.0f);
            return distanceSquared < Math.Pow(Radius, 2.0f);
        }

        public override bool CollidesWith(Bounds collider) => throw new NotImplementedException();

        public override bool CollidesWith(BoundingCircle boundingCircle)
        {
            var distanceSquared = Math.Pow(Center.X - boundingCircle.Center.X, 2.0f) + Math.Pow(Center.Y - boundingCircle.Center.Y, 2.0f);
            return distanceSquared < Math.Pow(Radius + boundingCircle.Radius, 2.0f);
        }

        public override bool CollidesWith(BoundingBox boundingBox) => HasCollision(this, boundingBox);

        public override Vector3 GetBorder(Vector3 direction)
        {
            var extended = Center.Xy + (direction.Xy.Normalized() * Radius);
            return new Vector3(extended.X, extended.Y, Center.Z);
        }
    }
}
