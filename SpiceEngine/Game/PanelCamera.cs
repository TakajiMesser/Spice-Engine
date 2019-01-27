﻿using OpenTK;
using SpiceEngine.Entities.Cameras;
using SpiceEngine.Outputs;
using SpiceEngine.Rendering;
using SpiceEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceEngine.Game
{
    public enum ViewTypes
    {
        X,
        Y,
        Z, 
        Perspective
    }

    public class PanelCamera
    {
        public const float MAX_ANGLE_Y = (float)Math.PI / 2.0f + 0.1f;
        public const float MIN_ANGLE_Y = -(float)Math.PI / 2.0f + 0.1f;

        private Resolution _resolution;
        private RenderManager _renderManager;

        // Yaw of zero should point in the direction of the X-Axis
        private float _yaw;

        // Pitch of zero should point in the direction of the X-Axis
        private float _pitch;

        public Camera Camera { get; private set; }
        public ViewTypes ViewType { get; set; }

        public PanelCamera(Resolution resolution, RenderManager renderManager)
        {
            _resolution = resolution;
            _renderManager = renderManager;
        }

        public void CenterView(IEnumerable<Vector3> positions)
        {
            switch (ViewType)
            {
                case ViewTypes.Perspective:
                    break;
                case ViewTypes.X:
                    var translationX = new Vector3()
                    {
                        X = 0.0f,
                        Y = positions.Average(p => p.Y) - Camera.Position.Y,
                        Z = positions.Average(p => p.Z) - Camera.Position.Z
                    };

                    Camera.Position += translationX;
                    Camera._viewMatrix.LookAt += translationX;
                    break;
                case ViewTypes.Y:
                    var translationY = new Vector3()
                    {
                        X = positions.Average(p => p.X) - Camera.Position.X,
                        Y = 0.0f,
                        Z = positions.Average(p => p.Z) - Camera.Position.Z
                    };

                    Camera.Position += translationY;
                    Camera._viewMatrix.LookAt += translationY;
                    break;
                case ViewTypes.Z:
                    var translationZ = new Vector3()
                    {
                        X = positions.Average(p => p.X) - Camera.Position.X,
                        Y = positions.Average(p => p.Y) - Camera.Position.Y,
                        Z = 0.0f
                    };

                    Camera.Position += translationZ;
                    Camera._viewMatrix.LookAt += translationZ;
                    break;
            }
        }

        public bool Zoom(int wheelDelta)
        {
            if (Camera is OrthographicCamera camera)
            {
                var width = camera.Width - wheelDelta * 0.01f;

                if (width > 0.0f)
                {
                    camera.Width = width;
                    return true;
                }
            }

            return false;
        }

        public void Load()
        {
            switch (ViewType)
            {
                case ViewTypes.Perspective:
                    Camera = new PerspectiveCamera("", _resolution, 0.1f, 1000.0f, UnitConversions.ToRadians(45.0f));
                    Camera.DetachFromEntity();
                    Camera._viewMatrix.Up = Vector3.UnitZ;
                    Camera._viewMatrix.LookAt = Camera.Position + Vector3.UnitY;
                    _yaw = 0.0f;
                    _pitch = 0.0f;
                    break;
                case ViewTypes.X:
                    Camera = new OrthographicCamera("", _resolution, -1000.0f, 1000.0f, 20.0f)
                    {
                        Position = Vector3.UnitX * -100.0f//new Vector3(_map.Boundaries.Min.X - 10.0f, 0.0f, 0.0f),
                    };
                    Camera._viewMatrix.Up = Vector3.UnitZ;
                    Camera._viewMatrix.LookAt = Camera.Position + Vector3.UnitX;
                    _renderManager.RotateGrid(0.0f, (float)Math.PI / 2.0f, 0.0f);
                    _yaw = (float)Math.PI / 2.0f;//90.0f;
                    _pitch = 0.0f;
                    break;
                case ViewTypes.Y:
                    Camera = new OrthographicCamera("", _resolution, -1000.0f, 1000.0f, 20.0f)
                    {
                        Position = Vector3.UnitY * -100.0f,//new Vector3(0.0f, _map.Boundaries.Min.Y - 10.0f, 0.0f),
                    };
                    Camera._viewMatrix.Up = Vector3.UnitZ;
                    Camera._viewMatrix.LookAt = Camera.Position + Vector3.UnitY;
                    _renderManager.RotateGrid(0.0f, 0.0f, (float)Math.PI / 2.0f);
                    _yaw = 0.0f;
                    _pitch = 0.0f;
                    break;
                case ViewTypes.Z:
                    Camera = new OrthographicCamera("", _resolution, -1000.0f, 1000.0f, 20.0f)
                    {
                        Position = Vector3.UnitZ * 100.0f,//new Vector3(0.0f, 0.0f, _map.Boundaries.Max.Z + 10.0f),
                    };
                    Camera._viewMatrix.Up = Vector3.UnitY;
                    Camera._viewMatrix.LookAt = Camera.Position - Vector3.UnitZ;
                    _yaw = 0.0f;
                    _pitch = (float)Math.PI / 2.0f;//90.0f;
                    break;
            }
        }

        public void Travel(Vector2 mouseDelta)
        {
            // Left mouse button allows "moving"
            if (mouseDelta != Vector2.Zero)
            {
                var translation = (Camera._viewMatrix.LookAt - Camera.Position) * mouseDelta.Y * 0.02f;
                Camera.Position -= translation;

                _yaw -= mouseDelta.X * 0.001f;
                CalculateLookAt();
                CalculateUp();
            }
        }

        public void Turn(Vector2 mouseDelta)
        {
            // Right mouse button allows "turning"
            if (mouseDelta != Vector2.Zero)
            {
                _yaw -= mouseDelta.X * 0.001f;
                _pitch -= mouseDelta.Y * 0.001f;
                _pitch = _pitch.Clamp(MIN_ANGLE_Y, MAX_ANGLE_Y);

                CalculateLookAt();
                CalculateUp();
            }
        }

        public void Strafe(Vector2 mouseDelta)
        {
            // Both mouse buttons allow "strafing"
            if (mouseDelta != Vector2.Zero)
            {
                var right = Vector3.Cross(Camera._viewMatrix.Up, Camera._viewMatrix.LookAt - Camera.Position).Normalized();

                var verticalTranslation = Camera._viewMatrix.Up * mouseDelta.Y * 0.02f;
                var horizontalTranslation = right * mouseDelta.X * 0.02f;

                Camera.Position -= verticalTranslation + horizontalTranslation;
                Camera._viewMatrix.LookAt -= verticalTranslation + horizontalTranslation;
            }
        }

        public void Pivot(Vector2 mouseDelta, Vector3 position)
        {
            if (ViewType == ViewTypes.Perspective && mouseDelta != Vector2.Zero)
            {
                // Determine new yaw and pitch
                _yaw = (float)Math.Asin((position.Y - Camera.Position.Y) / (position - Camera.Position).Xy.Length);
                _pitch = (float)Math.Asin((position.Z - Camera.Position.Z) / (position - Camera.Position).Xz.Length);

                if (position.X - Camera.Position.X < 0.0f)
                {
                    _yaw = (float)Math.PI - _yaw;
                }

                // Now, we can adjust our position accordingly
                _yaw += mouseDelta.X * 0.001f;
                _pitch += mouseDelta.Y * 0.001f;
                _pitch = _pitch.Clamp(MIN_ANGLE_Y, MAX_ANGLE_Y);

                CalculateTranslation(position);
                CalculateUp();
            }
        }

        private void CalculateTranslation(Vector3 position)
        {
            var distance = (Camera.Position - position).Length;

            /*var horizontal = distance * Math.Cos(_pitch);
            var vertical = distance * Math.Sin(_pitch);

            var translation = new Vector3()
            {
                X = -(float)(horizontal * Math.Sin(_yaw)),
                Y = -(float)(horizontal * Math.Cos(_yaw)),
                Z = (float)vertical
            };*/
            var translation = new Vector3()
            {
                X = distance * (float)(Math.Cos(_yaw) * Math.Cos(_pitch)),
                Y = distance * (float)(Math.Sin(_yaw) * Math.Cos(_pitch)),
                Z = distance * (float)Math.Sin(_pitch)
            };

            Camera.Position = position - translation;
            Camera._viewMatrix.LookAt = position;
        }

        private void CalculateLookAt()
        {
            /*var horizontal = Math.Cos(_pitch);
            var vertical = Math.Sin(_pitch);

            Camera._viewMatrix.LookAt = Camera.Position + new Vector3()
            {
                X = (float)(horizontal * Math.Sin(_yaw)),
                Y = (float)(horizontal * Math.Cos(_yaw)),
                Z = -(float)vertical
            };*/

            var translation = new Vector3()
            {
                X = (float)(Math.Cos(_yaw) * Math.Cos(_pitch)),
                Y = (float)(Math.Sin(_yaw) * Math.Cos(_pitch)),
                Z = (float)Math.Sin(_pitch)
            };

            Camera._viewMatrix.LookAt = Camera.Position + translation;
        }

        private void CalculateUp()
        {
            var yAngle = _pitch - (float)Math.PI / 2.0f;

            /*var horizontal = Math.Cos(yAngle);
            var vertical = Math.Sin(yAngle);

            Camera._viewMatrix.Up = new Vector3()
            {
                X = (float)(horizontal * Math.Sin(_yaw)),
                Y = (float)(horizontal * Math.Cos(_yaw)),
                Z = -(float)vertical
            };*/

            var translation = new Vector3()
            {
                X = (float)(Math.Cos(_yaw) * Math.Cos(yAngle)),
                Y = (float)(Math.Sin(_yaw) * Math.Cos(yAngle)),
                Z = (float)Math.Sin(yAngle)
            };


        }
    }
}
