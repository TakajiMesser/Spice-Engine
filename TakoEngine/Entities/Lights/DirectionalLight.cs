﻿using OpenTK;
using TakoEngine.Outputs;
using TakoEngine.Rendering.Shaders;

namespace TakoEngine.Entities.Lights
{
    public class DirectionalLight : Light
    {
        public Vector3 OriginalRotation { get; set; }
        public Quaternion Rotation { get; set; }

        public Vector3 Direction => (new Vector4(0.0f, 0.0f, -1.0f, 1.0f) * Matrix4.CreateFromQuaternion(Rotation)).Xyz;
        public Matrix4 View => Matrix4.LookAt(Vector3.Zero, Vector3.Zero + Direction.Normalized(), Vector3.UnitZ);

        public Matrix4 GetProjection(Resolution resolution)
        {
            return Matrix4.CreateOrthographic(resolution.Width, resolution.Height, 0.1f, 100.0f);
        }

        public override void DrawForStencilPass(ShaderProgram program)
        {
            /*var model = Matrix4.Identity * Matrix4.CreateScale(Radius) * Matrix4.CreateTranslation(Position);
            program.SetUniform("modelMatrix", model);

            program.SetUniform("lightPosition", Position);
            program.SetUniform("lightRadius", Radius);
            program.SetUniform("lightColor", Color);
            program.SetUniform("lightIntensity", Intensity);*/
        }

        public override void DrawForLightPass(ShaderProgram program)
        {
            /*var model = Matrix4.Identity * Matrix4.CreateScale(Radius) * Matrix4.CreateTranslation(Position);
            program.SetUniform("modelMatrix", model);

            program.SetUniform("lightPosition", Position);
            program.SetUniform("lightRadius", Radius);
            program.SetUniform("lightColor", Color);
            program.SetUniform("lightIntensity", Intensity);*/
        }

        public DLight ToStruct() => new DLight(Color.Xyz, Intensity);
    }
}