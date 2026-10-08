using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace RainCoreScene;

public sealed partial class ProceduralModel
{
    public static ProceduralModel CreateLowPolyMound(float width, float depth, float height, Vector3 color, int rngSeed)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var rng = new Random(rngSeed);
        AddRoundedMound(vertexData, indices, ref vi, Vector3.Zero, width, depth, height, color, rng);
        return FinalizeMesh(vertexData, indices, new Vector3(-width * 0.5f, 0f, -depth * 0.5f), new Vector3(width * 0.5f, height, depth * 0.5f));
    }

    public static ProceduralModel CreateMountainRange(float width, float depth, float height, Vector3 color, int rngSeed)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var rng = new Random(rngSeed);
        const int xSegments = 24;
        const int zSegments = 7;
        var grid = new Vector3[zSegments + 1, xSegments + 1];
        for (int zIndex = 0; zIndex <= zSegments; zIndex++)
        {
            float zT = zIndex / (float)zSegments * 2f - 1f;
            float crossSection = MathF.Pow(MathF.Max(0f, 1f - MathF.Abs(zT)), 0.72f);
            for (int xIndex = 0; xIndex <= xSegments; xIndex++)
            {
                float xT = xIndex / (float)xSegments * 2f - 1f;
                float x = xT * width * 0.5f;
                float arc = (1f - MathF.Abs(xT)) * depth * 0.42f;
                float ridge = 0.60f + 0.18f * MathF.Sin(xT * 9.5f + rngSeed * 0.013f)
                    + 0.10f * MathF.Cos(xT * 17f - rngSeed * 0.009f);
                float localHeight = height * Math.Clamp(ridge, 0.28f, 0.94f);
                float z = arc + zT * depth * 0.5f;
                grid[zIndex, xIndex] = new Vector3(x, -2f + localHeight * crossSection, z);
            }
        }
        for (int zIndex = 0; zIndex < zSegments; zIndex++)
        {
            for (int xIndex = 0; xIndex < xSegments; xIndex++)
            {
                var a = grid[zIndex, xIndex];
                var b = grid[zIndex, xIndex + 1];
                var c = grid[zIndex + 1, xIndex + 1];
                var d = grid[zIndex + 1, xIndex];
                var normal = Vector3.Cross(b - a, d - a).Normalized();
                var shade = 0.92f + ((xIndex + zIndex) % 4) * 0.025f;
                AddQuad(vertexData, indices, ref vi, normal, a, b, c, d, color * shade);
            }
        }
        return FinalizeMesh(vertexData, indices, new Vector3(-width * 0.5f, -2f, -depth * 0.5f), new Vector3(width * 0.5f, height, depth * 0.5f));
    }

    private static void AddRoundedMound(List<float> vertexData, List<uint> indices, ref uint vi,
        Vector3 center, float width, float depth, float height, Vector3 color, Random rng)
    {
        const int gridSize = 9;
        var grid = new Vector3[gridSize, gridSize];
        for (int zIndex = 0; zIndex < gridSize; zIndex++)
        {
            for (int xIndex = 0; xIndex < gridSize; xIndex++)
            {
                float x = xIndex / (float)(gridSize - 1) * 2f - 1f;
                float z = zIndex / (float)(gridSize - 1) * 2f - 1f;
                float edgeFalloff = MathF.Max(0f, 1f - MathF.Max(MathF.Abs(x), MathF.Abs(z)) * 0.92f);
                float broadWave = 0.50f + 0.24f * MathF.Sin(x * 2.4f + z * 0.8f) + 0.18f * MathF.Cos(z * 3.1f - x * 0.7f);
                float noise = 0.92f + (float)rng.NextDouble() * 0.16f;
                float y = height * Math.Clamp(broadWave * edgeFalloff * noise, 0f, 1f);
                grid[zIndex, xIndex] = center + new Vector3(x * width * 0.5f, y, z * depth * 0.5f);
            }
        }
        for (int zIndex = 0; zIndex < gridSize - 1; zIndex++)
        {
            for (int xIndex = 0; xIndex < gridSize - 1; xIndex++)
            {
                var a = grid[zIndex, xIndex];
                var b = grid[zIndex, xIndex + 1];
                var c = grid[zIndex + 1, xIndex + 1];
                var d = grid[zIndex + 1, xIndex];
                var normal = Vector3.Cross(b - a, d - a).Normalized();
                var shade = 0.86f + ((xIndex + zIndex) % 3) * 0.05f;
                AddQuad(vertexData, indices, ref vi, normal, a, b, c, d, color * shade);
            }
        }
    }

    private static void AddPointedMound(List<float> vertexData, List<uint> indices, ref uint vi,
        Vector3 center, float width, float depth, float height, Vector3 color, Random rng)
    {
        const int segments = 7;
        var baseRing = new Vector3[segments];
        var shoulderRing = new Vector3[segments];
        for (int segmentIndex = 0; segmentIndex < segments; segmentIndex++)
        {
            float angle = MathF.Tau * segmentIndex / segments;
            float jitter = 0.82f + (float)rng.NextDouble() * 0.18f;
            baseRing[segmentIndex] = center + new Vector3(MathF.Cos(angle) * width * 0.5f * jitter, 0f, MathF.Sin(angle) * depth * 0.5f * jitter);
            shoulderRing[segmentIndex] = center + new Vector3(MathF.Cos(angle) * width * 0.38f * jitter, height * 0.36f, MathF.Sin(angle) * depth * 0.38f * jitter);
        }
        var apex = center + new Vector3((float)(rng.NextDouble() - 0.5) * width * 0.18f, height, (float)(rng.NextDouble() - 0.5) * depth * 0.18f);
        for (int segmentIndex = 0; segmentIndex < segments; segmentIndex++)
        {
            int nextIndex = (segmentIndex + 1) % segments;
            var lowerNormal = Vector3.Cross(baseRing[nextIndex] - baseRing[segmentIndex], shoulderRing[segmentIndex] - baseRing[segmentIndex]).Normalized();
            AddQuad(vertexData, indices, ref vi, lowerNormal, baseRing[segmentIndex], baseRing[nextIndex], shoulderRing[nextIndex], shoulderRing[segmentIndex], color * (0.78f + segmentIndex % 3 * 0.07f));
            var upperNormal = Vector3.Cross(shoulderRing[nextIndex] - shoulderRing[segmentIndex], apex - shoulderRing[segmentIndex]).Normalized();
            AddTri(vertexData, indices, ref vi, upperNormal, shoulderRing[segmentIndex], shoulderRing[nextIndex], apex, color * (0.84f + segmentIndex % 2 * 0.08f));
        }
    }

    public static ProceduralModel CreateHeightmapTerrain(float width, float depth, int resolution, Vector3 color,
        int seed, float lakeCenterX, float lakeCenterZ, float lakeRadiusX, float lakeRadiusZ, float lakeDepth)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var grid = new Vector3[resolution + 1, resolution + 1];
        for (int zIndex = 0; zIndex <= resolution; zIndex++)
        {
            for (int xIndex = 0; xIndex <= resolution; xIndex++)
            {
                float x = xIndex / (float)resolution * width - width * 0.5f;
                float z = zIndex / (float)resolution * depth - depth * 0.5f;
                float worldX = x;
                float worldZ = z + TerrainConfig.WorldZOffset;
                float terrainHeight = TerrainHeightmap.Current?.Sample(worldX, worldZ)
                    ?? GenerateProceduralTerrainHeight(worldX, worldZ, seed);
                grid[zIndex, xIndex] = new Vector3(x, terrainHeight, z);
            }
        }

        for (int zIndex = 0; zIndex < resolution; zIndex++)
        {
            for (int xIndex = 0; xIndex < resolution; xIndex++)
            {
                var a = grid[zIndex, xIndex];
                var b = grid[zIndex, xIndex + 1];
                var c = grid[zIndex + 1, xIndex + 1];
                var d = grid[zIndex + 1, xIndex];
                var normal = Vector3.Cross(b - a, d - a).Normalized();
                AddQuad(vertexData, indices, ref vi, normal, a, b, c, d, color);
            }
        }
        return FinalizeMesh(vertexData, indices, new Vector3(-width * 0.5f, -lakeDepth, -depth * 0.5f), new Vector3(width * 0.5f, 8f, depth * 0.5f));
    }

    public static float SampleTerrainHeight(float x, float localZ, int seed,
        float lakeCenterX, float lakeCenterZ, float lakeRadiusX, float lakeRadiusZ, float lakeDepth)
    {
        float broad = FractalNoise(x * 0.010f, localZ * 0.010f, seed, 4);
        float detail = FractalNoise(x * 0.032f, localZ * 0.032f, seed + 19, 3);
        float macro = FractalNoise(x * 0.0045f, localZ * 0.0045f, seed + 71, 3);
        float ridge = 1f - MathF.Abs(FractalNoise(x * 0.007f, localZ * 0.007f, seed + 113, 3) * 2f - 1f);
        float terrainHeight = (broad * 0.48f + detail * 0.20f + macro * 0.22f + ridge * 0.10f - 0.5f) * 16f;
        float homeDistance = MathF.Sqrt(x * x + (localZ + TerrainConfig.WorldZOffset) * (localZ + TerrainConfig.WorldZOffset));
        float homeFlatten = 1f - SmoothRange(35f, 150f, homeDistance);
        terrainHeight *= 1f - homeFlatten;
        float lakeX = (x - lakeCenterX) / lakeRadiusX;
        float lakeZ = (localZ - lakeCenterZ) / lakeRadiusZ;
        float lakeDistance = MathF.Sqrt(lakeX * lakeX + lakeZ * lakeZ);
        float lakeFalloff = 1f - SmoothRange(0.72f, 1.05f, lakeDistance);
        terrainHeight = terrainHeight * (1f - lakeFalloff) - lakeDepth * lakeFalloff;
        float worldZ = localZ + TerrainConfig.WorldZOffset;
        if (worldZ > -115f && worldZ < 158f)
        {
            float riverCenterX = 66f + 25f * MathF.Sin((worldZ + 72f) * 0.034f);
            float riverDistance = MathF.Abs(x - riverCenterX);
            float riverCarve = 1f - SmoothRange(7f, 18f, riverDistance);
            terrainHeight -= 0.75f * riverCarve;
        }
        float westCoast = 1f - SmoothRange(-760f, -520f, x);
        terrainHeight = MathHelper.Lerp(terrainHeight, -1.35f, westCoast);
        return terrainHeight;
    }

    public static float SampleWorldTerrainHeight(float worldX, float worldZ, int seed = 1201)
    {
        if (TerrainHeightmap.Current is not null)
            return TerrainHeightmap.Current.Sample(worldX, worldZ);

        return GenerateProceduralTerrainHeight(worldX, worldZ, seed);
    }

    public static float GenerateProceduralTerrainHeight(float worldX, float worldZ, int seed = 1201)
    {
        return SampleTerrainHeight(worldX, worldZ - TerrainConfig.WorldZOffset, seed,
            TerrainConfig.LakeX, TerrainConfig.LakeZ - TerrainConfig.WorldZOffset,
            TerrainConfig.LakeRadiusX, TerrainConfig.LakeRadiusZ, TerrainConfig.LakeDepth);
    }

    private static float FractalNoise(float x, float z, int seed, int octaves)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float normalization = 0f;
        for (int octave = 0; octave < octaves; octave++)
        {
            total += GradientNoise(x * frequency, z * frequency, seed + octave * 97) * amplitude;
            normalization += amplitude;
            amplitude *= 0.5f;
            frequency *= 2f;
        }
        return total / normalization * 0.5f + 0.5f;
    }

    private static float GradientNoise(float x, float z, int seed)
    {
        int x0 = (int)MathF.Floor(x);
        int z0 = (int)MathF.Floor(z);
        float tx = x - x0;
        float tz = z - z0;
        float sx = tx * tx * (3f - 2f * tx);
        float sz = tz * tz * (3f - 2f * tz);
        float n00 = GradientDot(x0, z0, x, z, seed);
        float n10 = GradientDot(x0 + 1, z0, x, z, seed);
        float n01 = GradientDot(x0, z0 + 1, x, z, seed);
        float n11 = GradientDot(x0 + 1, z0 + 1, x, z, seed);
        return MathHelper.Lerp(MathHelper.Lerp(n00, n10, sx), MathHelper.Lerp(n01, n11, sx), sz);
    }

    private static float GradientDot(int gridX, int gridZ, float x, float z, int seed)
    {
        uint hash = (uint)(gridX * 374761393 + gridZ * 668265263 + seed * 1442695041);
        hash = (hash ^ (hash >> 13)) * 1274126177u;
        float angle = (hash & 1023) / 1023f * MathF.Tau;
        return MathF.Cos(angle) * (x - gridX) + MathF.Sin(angle) * (z - gridZ);
    }

    private static float SmoothRange(float start, float end, float value)
    {
        float t = Math.Clamp((value - start) / (end - start), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

                                                                            
                                                                          
                                                                                 
                                                                                 
                                                                                  
                                                                                
                                    
    public static ProceduralModel CreateSkylineRidge(float width, int segments, float minHeight, float maxHeight, Vector3 color, int rngSeed)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var rng = new Random(rngSeed);

        float step = width / segments;
        float x0 = -width * 0.5f;
        float prevH = minHeight + (float)rng.NextDouble() * (maxHeight - minHeight);

        for (int i = 0; i < segments; i++)
        {
            float xA = x0 + i * step;
            float xB = xA + step;
            float hA = prevH;
            float hB = minHeight + (float)rng.NextDouble() * (maxHeight - minHeight);
            prevH = hB;

            var a = new Vector3(xA, 0, 0);
            var b = new Vector3(xB, 0, 0);
            var c = new Vector3(xB, hB, 0);
            var d = new Vector3(xA, hA, 0);
            AddQuad(vertexData, indices, ref vi, Vector3.UnitZ, a, b, c, d, color, unlit: 1f);
        }

        return FinalizeMesh(vertexData, indices, new Vector3(x0, 0, -0.1f), new Vector3(x0 + width, maxHeight, 0.1f));
    }
}
