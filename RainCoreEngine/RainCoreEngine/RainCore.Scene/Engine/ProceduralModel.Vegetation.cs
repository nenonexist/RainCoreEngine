using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace RainCoreScene;

public sealed partial class ProceduralModel
{
                                                                                  
                                                                                  
                                                                                
                                                                           
                             
    public static ProceduralModel CreateConiferSilhouette(float height, float baseRadius, int tiers, Vector3 color, int sides = 7, Vector3? snowColor = null, float snowAmount = 0f)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        float tierHeight = height / tiers;

        for (int t = 0; t < tiers; t++)
        {
            float y0 = t * tierHeight * 0.75f;
            float apex = y0 + tierHeight * 1.4f;
            float r0 = baseRadius * (1f - 0.75f * t / tiers);

                                                                                  
                                                                                 
            var tierColor = color;
            if (snowColor.HasValue && snowAmount > 0f)
            {
                float tierSnow = snowAmount * (t + 1) / tiers;
                tierColor = Vector3.Lerp(color, snowColor.Value, MathHelper.Clamp(tierSnow, 0f, 1f));
            }

            for (int i = 0; i < sides; i++)
            {
                float a0 = MathF.Tau * i / sides;
                float a1 = MathF.Tau * (i + 1) / sides;
                var p0 = new Vector3(MathF.Cos(a0) * r0, y0, MathF.Sin(a0) * r0);
                var p1 = new Vector3(MathF.Cos(a1) * r0, y0, MathF.Sin(a1) * r0);
                var apexP = new Vector3(0, apex, 0);
                var n = Vector3.Cross(p1 - p0, apexP - p0).Normalized();
                AddTri(vertexData, indices, ref vi, n, p0, p1, apexP, tierColor);
            }
        }

        return FinalizeMesh(vertexData, indices, new Vector3(-baseRadius, 0, -baseRadius), new Vector3(baseRadius, height, baseRadius));
    }

    public static ProceduralModel CreateSilhouetteTree(float height, float width, Vector3 color, Vector3 snowColor)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        float halfWidth = width * 0.5f;
        var trunk = width * 0.08f;
        AddBox(vertexData, indices, ref vi, new Vector3(-trunk, 0f, -trunk), new Vector3(trunk, height * 0.42f, trunk), color * 0.65f);

        void AddTreePlane(bool alongX)
        {
            var baseLeft = alongX ? new Vector3(-halfWidth, 0f, 0f) : new Vector3(0f, 0f, -halfWidth);
            var baseRight = alongX ? new Vector3(halfWidth, 0f, 0f) : new Vector3(0f, 0f, halfWidth);
            var shoulderLeft = alongX ? new Vector3(-width * 0.28f, height * 0.62f, 0f) : new Vector3(0f, height * 0.62f, -width * 0.28f);
            var shoulderRight = alongX ? new Vector3(width * 0.28f, height * 0.62f, 0f) : new Vector3(0f, height * 0.62f, width * 0.28f);
            var tip = new Vector3(0f, height, 0f);
            var normal = alongX ? Vector3.UnitZ : Vector3.UnitX;
            AddTri(vertexData, indices, ref vi, normal, baseLeft, baseRight, shoulderRight, color);
            AddTri(vertexData, indices, ref vi, normal, baseLeft, shoulderRight, shoulderLeft, color);
            AddTri(vertexData, indices, ref vi, normal, shoulderLeft, shoulderRight, tip, snowColor);
        }

        AddTreePlane(true);
        AddTreePlane(false);
        return FinalizeMesh(vertexData, indices, new Vector3(-halfWidth, 0f, -halfWidth), new Vector3(halfWidth, height, halfWidth));
    }

    public static ProceduralModel CreateReferenceTree(float height, float width, Vector3 trunkColor,
        Vector3 foliageColor, Vector3 snowColor, int rngSeed)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var rng = new Random(rngSeed);
        float trunkRadius = MathF.Max(0.12f, width * 0.055f);
        float trunkHeight = height * 0.68f;
        AddBox(vertexData, indices, ref vi, new Vector3(-trunkRadius, 0f, -trunkRadius),
            new Vector3(trunkRadius, trunkHeight, trunkRadius), trunkColor);

        for (int branchIndex = 0; branchIndex < 5; branchIndex++)
        {
            float branchY = trunkHeight * (0.42f + branchIndex * 0.12f);
            float branchWidth = width * (0.34f - branchIndex * 0.035f);
            var branchColor = trunkColor * (0.82f + branchIndex * 0.025f);
            AddBox(vertexData, indices, ref vi, new Vector3(-branchWidth, branchY, -trunkRadius * 0.55f),
                new Vector3(branchWidth, branchY + trunkRadius * 0.55f, trunkRadius * 0.55f), branchColor);
            AddBox(vertexData, indices, ref vi, new Vector3(-trunkRadius * 0.55f, branchY, -branchWidth),
                new Vector3(trunkRadius * 0.55f, branchY + trunkRadius * 0.55f, branchWidth), branchColor);
        }

        for (int clusterIndex = 0; clusterIndex < 7; clusterIndex++)
        {
            float y = height * (0.47f + clusterIndex * 0.075f);
            float radius = width * (0.22f - clusterIndex * 0.012f);
            var center = new Vector3(
                ((float)rng.NextDouble() - 0.5f) * width * 0.18f,
                y,
                ((float)rng.NextDouble() - 0.5f) * width * 0.18f);
            AddLowPolyFoliageCluster(vertexData, indices, ref vi, center, radius,
                clusterIndex >= 5 ? snowColor : foliageColor);
        }

        float extent = width * 0.5f;
        return FinalizeMesh(vertexData, indices, new Vector3(-extent, 0f, -extent), new Vector3(extent, height, extent));
    }

    private static void AddLowPolyFoliageCluster(List<float> vertexData, List<uint> indices, ref uint vi,
        Vector3 center, float radius, Vector3 color)
    {
        var top = center + new Vector3(0f, radius, 0f);
        var bottom = center - new Vector3(0f, radius, 0f);
        var left = center + new Vector3(-radius, 0f, 0f);
        var right = center + new Vector3(radius, 0f, 0f);
        var front = center + new Vector3(0f, 0f, radius);
        var back = center + new Vector3(0f, 0f, -radius);
        AddTri(vertexData, indices, ref vi, Vector3.UnitY, top, front, right, color);
        AddTri(vertexData, indices, ref vi, Vector3.UnitY, top, right, back, color * 0.92f);
        AddTri(vertexData, indices, ref vi, Vector3.UnitY, top, back, left, color * 0.86f);
        AddTri(vertexData, indices, ref vi, Vector3.UnitY, top, left, front, color * 0.95f);
        AddTri(vertexData, indices, ref vi, -Vector3.UnitY, bottom, right, front, color * 0.78f);
        AddTri(vertexData, indices, ref vi, -Vector3.UnitY, bottom, back, right, color * 0.74f);
        AddTri(vertexData, indices, ref vi, -Vector3.UnitY, bottom, left, back, color * 0.70f);
        AddTri(vertexData, indices, ref vi, -Vector3.UnitY, bottom, front, left, color * 0.76f);
    }

                                                                                 
                                                                         
    public static ProceduralModel CreateBareTree(float trunkHeight, float trunkRadius, int branchCount, Vector3 color, int rngSeed)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var rng = new Random(rngSeed);

        AddBox(vertexData, indices, ref vi, new Vector3(-trunkRadius, 0, -trunkRadius), new Vector3(trunkRadius, trunkHeight, trunkRadius), color);

        for (int i = 0; i < branchCount; i++)
        {
            float t = 0.45f + 0.5f * (float)rng.NextDouble();
            float branchY = trunkHeight * t;
            float angle = MathF.Tau * (float)rng.NextDouble();
            float tilt = MathF.PI * (0.15f + 0.25f * (float)rng.NextDouble());
            float len = trunkHeight * (0.2f + 0.25f * (float)rng.NextDouble());
            var dir = new Vector3(MathF.Sin(tilt) * MathF.Cos(angle), MathF.Cos(tilt), MathF.Sin(tilt) * MathF.Sin(angle));
            var start = new Vector3(0, branchY, 0);
            var end = start + dir * len;
            float br = trunkRadius * 0.4f;
            var bmin = Vector3.ComponentMin(start, end) - new Vector3(br, 0, br);
            var bmax = Vector3.ComponentMax(start, end) + new Vector3(br, br * 2, br);
            AddBox(vertexData, indices, ref vi, bmin, bmax, color);
        }

        float spread = trunkHeight * 0.5f;
        return FinalizeMesh(vertexData, indices, new Vector3(-spread, 0, -spread), new Vector3(spread, trunkHeight * 1.3f, spread));
    }

                                                                              
                                                                              
                                                                                 
    public static ProceduralModel CreateDreamCanopyBlob(float radius, int rings, int segments, Vector3 baseColor, float colorJitter, int rngSeed)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var rng = new Random(rngSeed);

        Vector3 Jitter(Vector3 c)
        {
            float j = ((float)rng.NextDouble() * 2f - 1f) * colorJitter;
            return new Vector3(
                Math.Clamp(c.X + j, 0f, 1f),
                Math.Clamp(c.Y + j, 0f, 1f),
                Math.Clamp(c.Z + j, 0f, 1f));
        }

        Vector3 PointOn(int ring, int seg)
        {
            float v = MathF.PI * ring / rings;
            float u = MathF.Tau * seg / segments;
            float x = MathF.Sin(v) * MathF.Cos(u);
            float y = MathF.Cos(v);
            float z = MathF.Sin(v) * MathF.Sin(u);
            return new Vector3(x, y, z) * radius + new Vector3(0, radius, 0);
        }

        for (int r = 0; r < rings; r++)
        {
            for (int s = 0; s < segments; s++)
            {
                var a = PointOn(r, s);
                var b = PointOn(r, s + 1);
                var c = PointOn(r + 1, s + 1);
                var d = PointOn(r + 1, s);
                var n = Vector3.Cross(b - a, d - a).Normalized();
                AddQuad(vertexData, indices, ref vi, n, a, b, c, d, Jitter(baseColor));
            }
        }

        return FinalizeMesh(vertexData, indices, new Vector3(-radius, 0, -radius), new Vector3(radius, radius * 2, radius));
    }

                                                                              
                                                                               
                                                                             
                                                                             
                                                                        
                        
    public static ProceduralModel CreateGrassTuft(float height, int bladeCount, Vector3 baseColor, Vector3 tipColor, int rngSeed, float spread = 0.22f, float width = 0.045f)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var rng = new Random(rngSeed);

        for (int b = 0; b < bladeCount; b++)
        {
            float azimuth = MathF.Tau * (float)rng.NextDouble();
            float outward = spread * (0.4f + 0.6f * (float)rng.NextDouble());
            float bladeHeight = height * (0.65f + 0.5f * (float)rng.NextDouble());
            float bend = MathHelper.DegreesToRadians(12f + 18f * (float)rng.NextDouble());
            float bladeWidth = width * (0.7f + 0.6f * (float)rng.NextDouble());
            var dir = new Vector3(MathF.Cos(azimuth), 0, MathF.Sin(azimuth));

                                                                                 
                                                                           
            var root = dir * outward * 0.3f;

                                                                                   
                                                                               
            var p0 = root;
            var p1 = root + new Vector3(0, bladeHeight * 0.55f, 0) + dir * MathF.Sin(bend * 0.5f) * bladeHeight * 0.15f;
            var p2 = root + new Vector3(0, bladeHeight, 0) + dir * MathF.Sin(bend) * bladeHeight * 0.35f;

                                                                                          
            var side = new Vector3(-dir.Z, 0, dir.X) * bladeWidth * 0.5f;

            var col0 = baseColor;
            var col1 = Vector3.Lerp(baseColor, tipColor, 0.5f);
            var col2 = tipColor;

            var n = Vector3.UnitY;
                                                                                    
            AddQuad(vertexData, indices, ref vi, n, p0 - side, p0 + side, p1 + side * 0.5f, p1 - side * 0.5f, col0);
                                                                                                    
            AddTri(vertexData, indices, ref vi, n, p1 - side * 0.5f, p1 + side * 0.5f, p2, col2);
        }

        float extent = height * 0.5f + spread;
        return FinalizeMesh(vertexData, indices, new Vector3(-extent, 0, -extent), new Vector3(extent, height * 1.1f, extent));
    }

    public static ProceduralModel CreateBillboardGrass(float height, float width, Vector3 color)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        float half = width * 0.5f;
        var tip = new Vector3(0f, height, 0f);
        AddTri(vertexData, indices, ref vi, Vector3.UnitZ,
            new Vector3(-half, 0f, 0f), new Vector3(half, 0f, 0f), tip, color);
        AddTri(vertexData, indices, ref vi, Vector3.UnitX,
            new Vector3(0f, 0f, -half), new Vector3(0f, 0f, half), tip, color * 0.85f);
        return FinalizeMesh(vertexData, indices, new Vector3(-half, 0f, -half), new Vector3(half, height, half));
    }
}
