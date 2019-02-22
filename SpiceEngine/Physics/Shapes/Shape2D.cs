﻿using OpenTK;
using SpiceEngine.Physics.Collisions;
using System;

namespace SpiceEngine.Physics.Shapes
{
    public abstract class Shape2D : IShape
    {
        public abstract IPartition ToPartition(Vector3 position);
        //dpublic abstract Vector2 GetFurthestPoint(Vector2 position, Vector2 direction);
        //public abstract bool CollidesWith(Vector2 position, Vector2 point);
        public abstract IShape Duplicate();
        public abstract float CalculateInertia(float mass);

        /*public static Collision3D GetCollision(Vector2 positionA, Shape2D shapeA, Vector2 positionB, Shape2D shapeB)
        {
            switch (shapeA)
            {
                case Rectangle rectangleA when shapeB is Rectangle rectangleB:
                    return Collides(positionA - rectangleA.Center, rectangleA, positionB - rectangleB.Center, rectangleB);
                case Rectangle rectangleA when shapeB is Circle circleB:
                    return Collides(positionA - rectangleA.Center, rectangleA, positionB - circleB.Center, circleB);
                case Circle circleA when shapeB is Rectangle rectangleB:
                    return Collides(positionB - circleA.Center, rectangleB, positionA - rectangleB.Center, circleA);
                case Circle circleA when shapeB is Circle circleB:
                    return Collides(positionA - circleA.Center, circleA, positionB - circleB.Center, circleB);
            }

            throw new NotImplementedException();
        }

        private static Collision3D GetCollision(Vector2 positionA, Rectangle rectangleA, Vector2 positionB, Rectangle rectangleB) =>
            positionA.X - rectangleA.Width / 2.0f < positionB.X + rectangleB.Width / 2.0f
            && positionA.X + rectangleA.Width / 2.0f > positionB.X - rectangleB.Width / 2.0f
            && positionA.Y - rectangleA.Height / 2.0f < positionB.Y + rectangleB.Height / 2.0f
            && positionA.Y + rectangleA.Height / 2.0f > positionB.Y - rectangleB.Height / 2.0f;

        private static Collision3D GetCollision(Vector2 positionA, Circle circleA, Vector2 positionB, Circle circleB)
        {
            var normal = positionB - positionA;
            var distanceSquared = normal.LengthSquared;
            var radius = circleA.Radius + circleB.Radius;

            var distanceSquared = Math.Pow(positionA.X - positionB.X, 2.0f) + Math.Pow(positionA.Y - positionB.Y, 2.0f);
            return distanceSquared < Math.Pow(circleA.Radius + circleB.Radius, 2.0f);
        }

        private static Collision3D GetCollision(Vector2 positionA, Rectangle rectangleA, Vector2 positionB, Circle circleB)
        {
            var closestX = (positionB.X > positionA.X + rectangleA.Width / 2.0f)
                ? positionA.X + rectangleA.Width / 2.0f
                : (positionB.X < positionA.X - rectangleA.Width / 2.0f)
                    ? positionA.X - rectangleA.Width / 2.0f
                    : positionB.X;

            var closestY = (positionB.Y > positionA.Y + rectangleA.Height / 2.0f)
                ? positionA.Y + rectangleA.Height / 2.0f
                : (positionB.Y < positionA.Y - rectangleA.Height / 2.0f)
                    ? positionA.Y - rectangleA.Height / 2.0f
                    : positionB.Y;

            var distanceSquared = Math.Pow(positionB.X - closestX, 2) + Math.Pow(positionB.Y - closestY, 2);
            return distanceSquared < Math.Pow(circleB.Radius, 2);
        }*/
    }
}
