using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace RainCoreScene;

public sealed partial class ProceduralModel
{
                                                                              
                                                                                            
                                                                                
                                                                              
                                                                           
                                             
    public static ProceduralModel CreateRoadSegment(Vector3 a, Vector3 b, float width, float thickness, Vector3 color)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;

        var dir = b - a;
        dir.Y = 0;
        if (dir.LengthSquared < 0.0001f) dir = new Vector3(0, 0, 1);
        dir = Vector3.Normalize(dir);
        var side = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, dir)) * (width * 0.5f);
        var up = new Vector3(0, thickness, 0);

        var a0 = a - side; var a1 = a + side;
        var b0 = b - side; var b1 = b + side;
        var ta0 = a0 + up; var ta1 = a1 + up;
        var tb0 = b0 + up; var tb1 = b1 + up;

        AddQuad(vertexData, indices, ref vi, Vector3.UnitY, ta0, tb0, tb1, ta1, color);
        AddQuad(vertexData, indices, ref vi, -Vector3.Cross(dir, Vector3.UnitY), a0, ta0, tb0, b0, color * 0.85f);
        AddQuad(vertexData, indices, ref vi, Vector3.Cross(dir, Vector3.UnitY), b1, tb1, ta1, a1, color * 0.85f);
        AddQuad(vertexData, indices, ref vi, -dir, a1, ta1, ta0, a0, color * 0.7f);
        AddQuad(vertexData, indices, ref vi, dir, b0, tb0, tb1, b1, color * 0.7f);

        var min = Vector3.ComponentMin(Vector3.ComponentMin(a0, a1), Vector3.ComponentMin(b0, b1));
        var max = Vector3.ComponentMax(Vector3.ComponentMax(ta0, ta1), Vector3.ComponentMax(tb0, tb1));
        return FinalizeMesh(vertexData, indices, min, max);
    }

    public static ProceduralModel CreateRiverSurface(Vector3[] waypoints, float width, Vector3 color)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var smoothPoints = new List<Vector3>();
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            var p0 = i == 0 ? waypoints[i] : waypoints[i - 1];
            var p1 = waypoints[i];
            var p2 = waypoints[i + 1];
            var p3 = i + 2 < waypoints.Length ? waypoints[i + 2] : p2;
            for (int step = 0; step < 6; step++)
                smoothPoints.Add(CatmullRom(p0, p1, p2, p3, step / 6f));
        }
        smoothPoints.Add(waypoints[^1]);

        var left = new Vector3[smoothPoints.Count];
        var right = new Vector3[smoothPoints.Count];
        var min = smoothPoints[0];
        var max = smoothPoints[0];

        for (int i = 0; i < smoothPoints.Count; i++)
        {
            var tangent = i == 0 ? smoothPoints[1] - smoothPoints[0]
                : i == smoothPoints.Count - 1 ? smoothPoints[i] - smoothPoints[i - 1]
                : smoothPoints[i + 1] - smoothPoints[i - 1];
            tangent.Y = 0f;
            tangent = Vector3.Normalize(tangent);
            var side = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, tangent)) * (width * 0.5f);
            left[i] = smoothPoints[i] - side;
            right[i] = smoothPoints[i] + side;
            min = Vector3.ComponentMin(min, Vector3.ComponentMin(left[i], right[i]));
            max = Vector3.ComponentMax(max, Vector3.ComponentMax(left[i], right[i]));
        }

        for (int i = 0; i < smoothPoints.Count - 1; i++)
        {
            var a = left[i];
            var b = left[i + 1];
            var c = right[i + 1];
            var d = right[i];
            AddQuad(vertexData, indices, ref vi, Vector3.UnitY, a, b, c, d, color);
        }
        max.Y += 0.04f;
        return FinalizeMesh(vertexData, indices, min, max);
    }

    public static ProceduralModel CreateRiverCracks(Vector3[] waypoints, int count, Vector3 color, int rngSeed)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var rng = new Random(rngSeed);
        for (int i = 1; i < waypoints.Length - 1 && i <= count; i++)
        {
            var point = waypoints[i] + new Vector3(((float)rng.NextDouble() - 0.5f) * 2.4f, 0.012f, ((float)rng.NextDouble() - 0.5f) * 1.6f);
            var tangent = Vector3.Normalize(waypoints[i + 1] - waypoints[i - 1]);
            tangent.Y = 0f;
            tangent = Vector3.Normalize(tangent);
            var side = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, tangent));
            var crackStart = point - side * (0.9f + (float)rng.NextDouble() * 0.9f);
            var crackEnd = point + side * (0.9f + (float)rng.NextDouble() * 0.9f);
            var thickness = tangent * 0.045f;
            AddQuad(vertexData, indices, ref vi, Vector3.UnitY, crackStart - thickness, crackEnd - thickness, crackEnd + thickness, crackStart + thickness, color);
        }
        return FinalizeMesh(vertexData, indices, new Vector3(-300f, -0.02f, -200f), new Vector3(300f, 0.05f, 400f));
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ((2f * p1) + (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    public static ProceduralModel CreateLakeSurface(float width, float depth, float y, Vector3 color)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        const int segments = 96;
        var center = new Vector3(0f, y, 0f);
        for (int i = 0; i < segments; i++)
        {
            float a0 = MathF.Tau * i / segments;
            float a1 = MathF.Tau * (i + 1) / segments;
            var p0 = new Vector3(MathF.Cos(a0) * width * 0.5f, y, MathF.Sin(a0) * depth * 0.5f);
            var p1 = new Vector3(MathF.Cos(a1) * width * 0.5f, y, MathF.Sin(a1) * depth * 0.5f);
            AddTri(vertexData, indices, ref vi, Vector3.UnitY, center, p0, p1, color * (0.985f + (i % 3) * 0.004f));
        }
        return FinalizeMesh(vertexData, indices, new Vector3(-width * 0.5f, y, -depth * 0.5f), new Vector3(width * 0.5f, y + 0.04f, depth * 0.5f));
    }

    public static ProceduralModel CreateLakeIceRing(float outerWidth, float outerDepth, float y,
        float holeWidth, float holeDepth, Vector3 color)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        const int segments = 64;
        for (int i = 0; i < segments; i++)
        {
            float a0 = MathF.Tau * i / segments;
            float a1 = MathF.Tau * (i + 1) / segments;
            var outer0 = new Vector3(MathF.Cos(a0) * outerWidth * 0.5f, y, MathF.Sin(a0) * outerDepth * 0.5f);
            var outer1 = new Vector3(MathF.Cos(a1) * outerWidth * 0.5f, y, MathF.Sin(a1) * outerDepth * 0.5f);
            var inner0 = new Vector3(MathF.Cos(a0) * holeWidth * 0.5f, y + 0.01f, MathF.Sin(a0) * holeDepth * 0.5f);
            var inner1 = new Vector3(MathF.Cos(a1) * holeWidth * 0.5f, y + 0.01f, MathF.Sin(a1) * holeDepth * 0.5f);
            AddQuad(vertexData, indices, ref vi, Vector3.UnitY, outer0, outer1, inner1, inner0, color * (0.90f + (i % 5) * 0.018f));
        }
        return FinalizeMesh(vertexData, indices, new Vector3(-outerWidth * 0.5f, y, -outerDepth * 0.5f), new Vector3(outerWidth * 0.5f, y + 0.04f, outerDepth * 0.5f));
    }

    private static Vector3 HeartPoint(float angle, float width, float depth, float y)
    {
        float sine = MathF.Sin(angle);
        float cosine = MathF.Cos(angle);
        float x = sine * sine * sine * width * 0.5f;
        float heartY = 13f * cosine - 5f * MathF.Cos(2f * angle) - 2f * MathF.Cos(3f * angle) - MathF.Cos(4f * angle);
        float z = -heartY * depth / 34f;
        return new Vector3(x, y, z);
    }

    public static ProceduralModel CreateSnowDrift(float width, float depth, float height, Vector3 color, int rngSeed)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var rng = new Random(rngSeed);
        const int segments = 9;
        var center = new Vector3(0f, height, 0f);
        for (int i = 0; i < segments; i++)
        {
            float a0 = MathF.Tau * i / segments;
            float a1 = MathF.Tau * (i + 1) / segments;
            float radius0 = 0.82f + (float)rng.NextDouble() * 0.18f;
            float radius1 = 0.82f + (float)rng.NextDouble() * 0.18f;
            var p0 = new Vector3(MathF.Cos(a0) * width * 0.5f * radius0, 0f, MathF.Sin(a0) * depth * 0.5f * radius0);
            var p1 = new Vector3(MathF.Cos(a1) * width * 0.5f * radius1, 0f, MathF.Sin(a1) * depth * 0.5f * radius1);
            AddTri(vertexData, indices, ref vi, Vector3.UnitY, p0, p1, center, color * (0.92f + (i % 3) * 0.025f));
        }
        return FinalizeMesh(vertexData, indices, new Vector3(-width * 0.5f, 0f, -depth * 0.5f), new Vector3(width * 0.5f, height, depth * 0.5f));
    }

    public static ProceduralModel CreateSnowFootprint(float length, float width, Vector3 color)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        const int segments = 8;
        var center = new Vector3(0f, 0.012f, 0f);
        for (int i = 0; i < segments; i++)
        {
            float a0 = MathF.Tau * i / segments;
            float a1 = MathF.Tau * (i + 1) / segments;
            var p0 = new Vector3(MathF.Cos(a0) * width * 0.5f, 0.012f, MathF.Sin(a0) * length * 0.5f);
            var p1 = new Vector3(MathF.Cos(a1) * width * 0.5f, 0.012f, MathF.Sin(a1) * length * 0.5f);
            AddTri(vertexData, indices, ref vi, Vector3.UnitY, center, p0, p1, color);
        }
        return FinalizeMesh(vertexData, indices, new Vector3(-width * 0.5f, 0.01f, -length * 0.5f), new Vector3(width * 0.5f, 0.02f, length * 0.5f));
    }
}
