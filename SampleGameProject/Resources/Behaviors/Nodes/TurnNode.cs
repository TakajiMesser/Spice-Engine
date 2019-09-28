﻿using OpenTK;
using SpiceEngine.Scripting;
using SpiceEngine.Scripting.Nodes;
using SpiceEngine.Utilities;
using System;

namespace SampleGameProject.Resources.Behaviors.Nodes
{
    public class TurnNode : Node
    {
        public override BehaviorStatus Tick(BehaviorContext context)
        {
            var nEvadeTicks = context.ContainsVariable("nEvadeTicks") ? context.GetVariable<int>("nEvadeTicks") : 0;

            // Compare current position to location of mouse, and set rotation to face the mouse
            if (!context.InputProvider.IsDown(context.InputProvider.InputMapping.ItemWheel) && nEvadeTicks == 0 && context.InputProvider.IsMouseInWindow)
            {
                var clipSpacePosition = context.Camera.ViewProjectionMatrix.Inverted() * new Vector4(0.0f, 0.0f, 0.0f, 1.0f);//new Vector4(context.Position, 1.0f);
                var screenCoordinates = new Vector2()
                {
                    X = ((clipSpacePosition.X + 1.0f) / 2.0f) * context.InputProvider.WindowSize.Width,
                    Y = ((1.0f - clipSpacePosition.Y) / 2.0f) * context.InputProvider.WindowSize.Height,
                };

                if (context.InputProvider.MouseCoordinates.HasValue)
                {
                    var vectorBetween = context.InputProvider.MouseCoordinates.Value - screenCoordinates;
                    float turnAngle = -(float)Math.Atan2(vectorBetween.Y, vectorBetween.X);

                    // Need to add the angle that the camera's Up vector is turned from Vector3.UnitY
                    turnAngle += (float)Math.Atan2(context.Camera._viewMatrix.Up.Y, context.Camera._viewMatrix.Up.X) - MathExtensions.HALF_PI;

                    context.Actor.Rotation = new Quaternion(0.0f, 0.0f, turnAngle);
                    context.EulerRotation = new Vector3(context.EulerRotation.X, context.EulerRotation.Y, turnAngle);
                }
            }

            return BehaviorStatus.Success;
        }

        public override void Reset() { }
    }
}
