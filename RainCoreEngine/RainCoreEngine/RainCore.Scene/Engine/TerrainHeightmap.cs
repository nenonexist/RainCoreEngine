using System.Buffers.Binary;

namespace RainCoreScene;

public sealed class TerrainHeightmap
{
    private const int FileMagic = 0x4447484D;
    private const int FileVersion = 1;
    private readonly float[] _heights;

    public static TerrainHeightmap? Current { get; private set; }
    public int Resolution { get; }
    public float Width { get; }
    public float Depth { get; }
    public bool IsDirty { get; private set; }

    private TerrainHeightmap(int resolution, float width, float depth)
    {
        Resolution = resolution;
        Width = width;
        Depth = depth;
        _heights = new float[resolution * resolution];
    }

    public static TerrainHeightmap LoadOrGenerate(string path, int resolution, float width, float depth,
        Func<float, float, float> generator)
    {
        var map = new TerrainHeightmap(resolution, width, depth);
        if (!map.TryLoad(path))
        {
            for (int z = 0; z < resolution; z++)
            {
                float worldZ = TerrainConfig.MapMinZ + z / (float)(resolution - 1) * depth;
                for (int x = 0; x < resolution; x++)
                {
                    float worldX = TerrainConfig.MapMinX + x / (float)(resolution - 1) * width;
                    map._heights[z * resolution + x] = generator(worldX, worldZ);
                }
            }
            map.Save(path);
        }
        Current = map;
        return map;
    }

    public float Sample(float worldX, float worldZ)
    {
        float u = Math.Clamp((worldX - TerrainConfig.MapMinX) / Width, 0f, 1f) * (Resolution - 1);
        float v = Math.Clamp((worldZ - TerrainConfig.MapMinZ) / Depth, 0f, 1f) * (Resolution - 1);
        int x0 = Math.Clamp((int)MathF.Floor(u), 0, Resolution - 1);
        int z0 = Math.Clamp((int)MathF.Floor(v), 0, Resolution - 1);
        int x1 = Math.Min(x0 + 1, Resolution - 1);
        int z1 = Math.Min(z0 + 1, Resolution - 1);
        float tx = u - x0;
        float tz = v - z0;
        float a = _heights[z0 * Resolution + x0];
        float b = _heights[z0 * Resolution + x1];
        float c = _heights[z1 * Resolution + x0];
        float d = _heights[z1 * Resolution + x1];
        return MathHelperLerp(MathHelperLerp(a, b, tx), MathHelperLerp(c, d, tx), tz);
    }

    public void Sculpt(float centerX, float centerZ, float radius, float amount)
    {
        float minX = MathF.Max(TerrainConfig.MapMinX, centerX - radius);
        float maxX = MathF.Min(TerrainConfig.MapMaxX, centerX + radius);
        float minZ = MathF.Max(TerrainConfig.MapMinZ, centerZ - radius);
        float maxZ = MathF.Min(TerrainConfig.MapMaxZ, centerZ + radius);
        int xStart = Math.Max(0, (int)MathF.Floor((minX - TerrainConfig.MapMinX) / Width * (Resolution - 1)));
        int xEnd = Math.Min(Resolution - 1, (int)MathF.Ceiling((maxX - TerrainConfig.MapMinX) / Width * (Resolution - 1)));
        int zStart = Math.Max(0, (int)MathF.Floor((minZ - TerrainConfig.MapMinZ) / Depth * (Resolution - 1)));
        int zEnd = Math.Min(Resolution - 1, (int)MathF.Ceiling((maxZ - TerrainConfig.MapMinZ) / Depth * (Resolution - 1)));

        for (int z = zStart; z <= zEnd; z++)
        {
            float worldZ = TerrainConfig.MapMinZ + z / (float)(Resolution - 1) * Depth;
            for (int x = xStart; x <= xEnd; x++)
            {
                float worldX = TerrainConfig.MapMinX + x / (float)(Resolution - 1) * Width;
                float distance = MathF.Sqrt((worldX - centerX) * (worldX - centerX) + (worldZ - centerZ) * (worldZ - centerZ));
                if (distance > radius) continue;
                float falloff = 1f - Math.Clamp(distance / radius, 0f, 1f);
                falloff *= falloff * (3f - 2f * falloff);
                _heights[z * Resolution + x] += amount * falloff;
            }
        }
        IsDirty = true;
    }

    public void Flatten(float centerX, float centerZ, float radius, float targetHeight)
    {
        float minX = MathF.Max(TerrainConfig.MapMinX, centerX - radius);
        float maxX = MathF.Min(TerrainConfig.MapMaxX, centerX + radius);
        float minZ = MathF.Max(TerrainConfig.MapMinZ, centerZ - radius);
        float maxZ = MathF.Min(TerrainConfig.MapMaxZ, centerZ + radius);
        for (int z = 0; z < Resolution; z++)
        {
            float worldZ = TerrainConfig.MapMinZ + z / (float)(Resolution - 1) * Depth;
            if (worldZ < minZ || worldZ > maxZ) continue;
            for (int x = 0; x < Resolution; x++)
            {
                float worldX = TerrainConfig.MapMinX + x / (float)(Resolution - 1) * Width;
                float distance = MathF.Sqrt((worldX - centerX) * (worldX - centerX) + (worldZ - centerZ) * (worldZ - centerZ));
                if (distance > radius) continue;
                float falloff = 1f - Math.Clamp(distance / radius, 0f, 1f);
                falloff *= falloff * (3f - 2f * falloff);
                int index = z * Resolution + x;
                _heights[index] = _heights[index] + (targetHeight - _heights[index]) * falloff;
            }
        }
        IsDirty = true;
    }

    public void Reset(Func<float, float, float> generator)
    {
        for (int z = 0; z < Resolution; z++)
        {
            float worldZ = TerrainConfig.MapMinZ + z / (float)(Resolution - 1) * Depth;
            for (int x = 0; x < Resolution; x++)
            {
                float worldX = TerrainConfig.MapMinX + x / (float)(Resolution - 1) * Width;
                _heights[z * Resolution + x] = generator(worldX, worldZ);
            }
        }
        IsDirty = true;
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);
        writer.Write(FileMagic);
        writer.Write(FileVersion);
        writer.Write(Resolution);
        writer.Write(Width);
        writer.Write(Depth);
        foreach (float height in _heights) writer.Write(height);
        IsDirty = false;
    }

    private bool TryLoad(string path)
    {
        if (!File.Exists(path)) return false;
        try
        {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            if (reader.ReadInt32() != FileMagic || reader.ReadInt32() != FileVersion ||
                reader.ReadInt32() != Resolution || MathF.Abs(reader.ReadSingle() - Width) > 0.01f ||
                MathF.Abs(reader.ReadSingle() - Depth) > 0.01f)
                return false;
            for (int i = 0; i < _heights.Length; i++) _heights[i] = reader.ReadSingle();
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static float MathHelperLerp(float a, float b, float amount) => a + (b - a) * amount;
}
