using OpenTK.Mathematics;

namespace RainCoreScene;

public static class TerrainConfig
{
    public const float MapMinX = -1000f;
    public const float MapMaxX = 1000f;
    public const float MapMinZ = -1000f;
    public const float MapMaxZ = 1000f;

    public const float LakeX = 620f;
    public const float LakeZ = -260f;
    public const float LakeRadiusX = 220f;
    public const float LakeRadiusZ = 150f;
    public const float LakeDepth = 6.5f;
    public const float LakeWaterY = -1.15f;

    public const int NoiseSeedOffset = 1200;
    public const float WorldZOffset = (MapMinZ + MapMaxZ) * 0.5f;
    public const float MapWidth = MapMaxX - MapMinX;
    public const float MapDepth = MapMaxZ - MapMinZ;

    public static float LakeNormalizedDistance(float worldX, float worldZ)
    {
        float x = (worldX - LakeX) / LakeRadiusX;
        float z = (worldZ - LakeZ) / LakeRadiusZ;
        return MathF.Sqrt(x * x + z * z);
    }

    public static bool IsLakeSurface(float worldX, float worldZ) => LakeNormalizedDistance(worldX, worldZ) <= 1.08f;

    public static bool IsDeepLake(float worldX, float worldZ) => LakeNormalizedDistance(worldX, worldZ) < 0.72f;

    public static Vector3 GetNearestLakeShore(Vector3 position)
    {
        float dx = position.X - LakeX;
        float dz = position.Z - LakeZ;
        float normalizedDistance = LakeNormalizedDistance(position.X, position.Z);
        if (normalizedDistance < 0.001f)
        {
            dx = 0f;
            dz = -1f;
            normalizedDistance = 1f;
        }

        float shoreScale = 1.10f / normalizedDistance;
        return new Vector3(LakeX + dx * shoreScale, position.Y, LakeZ + dz * shoreScale);
    }
}
