﻿using OpenTK;
using SpiceEngine.Rendering.Shaders;
using System;

namespace SpiceEngine.Rendering.Matrices
{
    public class ModelMatrix
    {
        public const string NAME = "modelMatrix";
        public const string PREVIOUS_NAME = "previousModelMatrix";

        private Transform _transform;

        public ModelMatrix() : this(new Transform()) { }
        public ModelMatrix(Vector3 position, Quaternion rotation, Vector3 scale) : this(new Transform(position, rotation, scale)) { }
        public ModelMatrix(Transform transform)
        {
            _transform = transform;
            CurrentMatrix = _transform.ToMatrix();
        }

        public Matrix4 CurrentMatrix { get; private set; }
        public Matrix4 PreviousMatrix { get; private set; }
        public Transform WorldTransform => _transform;

        public Vector3 Position
        {
            get => _transform.Translation;
            set => Transform(Matrices.Transform.FromTranslation(value - Position));
        }

        public Quaternion Rotation
        {
            get => _transform.Rotation;
            set => Transform(Matrices.Transform.FromRotation(value * Rotation.Inverted()));
        }

        public Vector3 Scale
        {
            get => _transform.Scale;
            set => Transform(Matrices.Transform.FromScale(value - Scale));
        }

        public event EventHandler<TransformEventArgs> Transformed;

        public void Transform(Transform transform)
        {
            Transformed?.Invoke(this, new TransformEventArgs(transform));
            _transform.Combine(transform);
            CurrentMatrix = _transform.ToMatrix();
        }

        public void Set(ShaderProgram program)
        {
            program.SetUniform(NAME, CurrentMatrix);
            program.SetUniform(PREVIOUS_NAME, PreviousMatrix);

            PreviousMatrix = CurrentMatrix;
        }
    }
}
