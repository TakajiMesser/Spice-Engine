﻿using SpiceEngineCore.Geometry.Matrices;
using SpiceEngineCore.Geometry.Quaternions;
using SpiceEngineCore.Utilities;
using System;
using System.Runtime.InteropServices;

namespace SpiceEngineCore.Geometry.Vectors
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4 : IEquatable<Vector4>
    {
        public Vector4(Vector3 xyz, float w) : this(xyz.X, xyz.Y, xyz.Z, w) { }
        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float W { get; private set; }

        public Vector2 Xy => new Vector2(X, Y);
        public Vector3 Xyz => new Vector3(X, Y, Z);

        public float LengthSquared => X * X + Y * Y + Z * Z + W * W;

        public float Length => (float)Math.Sqrt(LengthSquared);

        public float LengthFast => 1.0f / MathExtensions.InverseSqrtFast(LengthSquared);

        public Vector4 Normalized()
        {
            var scale = 1.0f / Length;
            return new Vector4(X * scale, Y * scale, Z * scale, W * scale);
        }

        public Vector4 NormalizedFast()
        {
            var scale = MathExtensions.InverseSqrtFast(LengthSquared);
            return new Vector4(X * scale, Y * scale, Z * scale, W * scale);
        }

        public override string ToString() => "<" + X + "," + Y + "," + Z + "," + W + ">";

        public override bool Equals(object obj) => obj is Vector4 vector && Equals(vector);

        public bool Equals(Vector4 other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;

        public override int GetHashCode()
        {
            var hashCode = 707706286;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            hashCode = hashCode * -1521134295 + W.GetHashCode();
            return hashCode;
        }

        public static readonly Vector4 UnitX = new Vector4(1f, 0f, 0f, 0f);

        public static readonly Vector4 UnitY = new Vector4(0f, 1f, 0f, 0f);

        public static readonly Vector4 UnitZ = new Vector4(0f, 0f, 1f, 0f);

        public static readonly Vector4 UnitW = new Vector4(0f, 0f, 0f, 1f);

        public static readonly Vector4 Zero = new Vector4(0f, 0f, 0f, 0f);

        public static readonly Vector4 One = new Vector4(1f, 1f, 1f, 1f);

        public static bool operator ==(Vector4 left, Vector4 right) => left.Equals(right);

        public static bool operator !=(Vector4 left, Vector4 right) => !(left == right);

        public static Vector4 operator +(Vector4 left, Vector4 right) => new Vector4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);

        public static Vector4 operator -(Vector4 left, Vector4 right) => new Vector4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);

        public static Vector4 operator -(Vector4 vector) => new Vector4(-vector.X, -vector.Y, -vector.Z, -vector.W);

        public static Vector4 operator *(Vector4 vector, float scale) => new Vector4(vector.X * scale, vector.Y * scale, vector.Z * scale, vector.W * scale);

        public static Vector4 operator *(Vector4 left, Vector4 right) => new Vector4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);

        public static Vector4 operator *(Vector4 vector, Matrix4 matrix) => new Vector4(
            (vector.X * matrix.M00) + (vector.Y * matrix.M10) + (vector.Z * matrix.M20) + (vector.W * matrix.M30),
            (vector.X * matrix.M01) + (vector.Y * matrix.M11) + (vector.Z * matrix.M21) + (vector.W * matrix.M31),
            (vector.X * matrix.M02) + (vector.Y * matrix.M12) + (vector.Z * matrix.M22) + (vector.W * matrix.M32),
            (vector.X * matrix.M03) + (vector.Y * matrix.M13) + (vector.Z * matrix.M23) + (vector.W * matrix.M33));

        public static Vector4 operator *(Matrix4 matrix, Vector4 vector) => new Vector4(
            (matrix.M00 * vector.X) + (matrix.M01 * vector.Y) + (matrix.M02 * vector.Z) + (matrix.M03 * vector.W),
            (matrix.M10 * vector.X) + (matrix.M11 * vector.Y) + (matrix.M12 * vector.Z) + (matrix.M13 * vector.W),
            (matrix.M20 * vector.X) + (matrix.M21 * vector.Y) + (matrix.M22 * vector.Z) + (matrix.M23 * vector.W),
            (matrix.M30 * vector.X) + (matrix.M31 * vector.Y) + (matrix.M32 * vector.Z) + (matrix.M33 * vector.W));

        public static Vector4 operator *(Quaternion quaternion, Vector4 vector)
        {
            var vectorQuat = new Quaternion(vector.X, vector.Y, vector.Z, vector.W);
            var invertedQuat = quaternion.Inverted();
            var rotatedQuat = (quaternion * vectorQuat) * invertedQuat;

            return new Vector4(rotatedQuat.X, rotatedQuat.Y, rotatedQuat.Z, rotatedQuat.W);
        }

        public static Vector4 operator /(Vector4 left, Vector4 right) => new Vector4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);

        public static float Dot(Vector4 left, Vector4 right) => left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
    }
}