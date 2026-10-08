using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using RainCoreGraphics;

namespace RainCore;

public sealed class AmbientNatureParticles
{
    private const int FireflyCount = 36;
    private const int FallingLeafCount = 18;
    private const int SwampLightCount = 16;
    private readonly Vector3[] _fireflies = new Vector3[FireflyCount];
    private readonly Vector3[] _leaves = new Vector3[FallingLeafCount];
    private readonly Vector3[] _swampLights = new Vector3[SwampLightCount];
    private readonly float[] _phase = new float[FireflyCount + FallingLeafCount + SwampLightCount];
    private readonly float[] _vertexData = new float[(FireflyCount + FallingLeafCount + SwampLightCount) * 10];
    private readonly Random _rng = new(92741);
    private int _vao;
    private int _vbo;
    private float _time;

    public void Build(World world, Vector3 cameraPosition)
    {
        for (int i = 0; i < FireflyCount; i++) _fireflies[i] = RandomPosition(cameraPosition, 7f, 4f);
        for (int i = 0; i < FallingLeafCount; i++) _leaves[i] = RandomPosition(cameraPosition, 10f, 6f);
        for (int i = 0; i < SwampLightCount; i++) _swampLights[i] = RandomPosition(cameraPosition, 12f, 2f);
        for (int i = 0; i < _phase.Length; i++) _phase[i] = (float)_rng.NextDouble() * MathF.Tau;

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexData.Length * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        const int stride = 10 * sizeof(float);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, stride, 9 * sizeof(float));
        GL.EnableVertexAttribArray(3);
    }

    public void Update(float dt, World world, Vector3 cameraPosition, float dayFactor)
    {
        _time += dt;
        for (int i = 0; i < FallingLeafCount; i++)
        {
            _leaves[i].Y -= dt * (0.25f + 0.15f * MathF.Sin(_phase[FireflyCount + i]));
            _leaves[i].X += MathF.Sin(_time + _phase[FireflyCount + i]) * dt * 0.3f;
            if (_leaves[i].Y < cameraPosition.Y - 2f || DistanceXZ(_leaves[i], cameraPosition) > 12f)
                _leaves[i] = RandomPosition(cameraPosition, 10f, 6f);
        }

        for (int i = 0; i < FireflyCount; i++)
        {
            _fireflies[i].Y += MathF.Sin(_time * 1.5f + _phase[i]) * dt * 0.25f;
            if (DistanceXZ(_fireflies[i], cameraPosition) > 10f)
                _fireflies[i] = RandomPosition(cameraPosition, 7f, 4f);
        }
            for (int i = 0; i < SwampLightCount; i++)
            {
                if (DistanceXZ(_swampLights[i], cameraPosition) > 14f || !world.IsSwampAt((int)_swampLights[i].X, (int)_swampLights[i].Z))
                _swampLights[i] = RandomPosition(cameraPosition, 12f, 2f);
            }
    }

    public void Render(Shader shader, Vector3 cameraPosition, float dayFactor)
    {
        if (_vao == 0) return;
        int cursor = 0;
        float night = 1f - dayFactor;
        if (night > 0.05f)
        {
            for (int i = 0; i < FireflyCount; i++)
            {
                var fireflyColor = new Vector3(0.75f, 1f, 0.25f) * night;
                WriteVertex(cursor++, _fireflies[i], fireflyColor, night);
            }
        }
        for (int i = 0; i < FallingLeafCount; i++)
            WriteVertex(cursor++, _leaves[i], new Vector3(0.72f, 0.30f, 0.10f), 0.65f);
        for (int i = 0; i < SwampLightCount; i++)
        {
            float distance = DistanceXZ(_swampLights[i], cameraPosition);
            float glow = Math.Clamp((distance - 2f) / 5f, 0f, 1f);
            WriteVertex(cursor++, _swampLights[i], new Vector3(0.08f, 0.35f, 1f) * glow, glow);
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _vertexData.Length * sizeof(float), _vertexData);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.PointSize(3f);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Points, 0, cursor);
        GL.Disable(EnableCap.Blend);
    }

    private void WriteVertex(int index, Vector3 position, Vector3 color, float alpha)
    {
        int offset = index * 10;
        _vertexData[offset] = position.X; _vertexData[offset + 1] = position.Y; _vertexData[offset + 2] = position.Z;
        _vertexData[offset + 3] = 0f; _vertexData[offset + 4] = 1f; _vertexData[offset + 5] = 0f;
        _vertexData[offset + 6] = color.X; _vertexData[offset + 7] = color.Y; _vertexData[offset + 8] = color.Z;
        _vertexData[offset + 9] = 1f;
    }

    private Vector3 RandomPosition(Vector3 cameraPosition, float radius, float height)
    {
        return new Vector3(
            cameraPosition.X + ((float)_rng.NextDouble() * 2f - 1f) * radius,
            cameraPosition.Y + ((float)_rng.NextDouble() - 0.35f) * height,
            cameraPosition.Z + ((float)_rng.NextDouble() * 2f - 1f) * radius);
    }

    private static float DistanceXZ(Vector3 a, Vector3 b)
    {
        float x = a.X - b.X, z = a.Z - b.Z;
        return MathF.Sqrt(x * x + z * z);
    }
}
