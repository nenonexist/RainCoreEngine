using OpenTK.Mathematics;

namespace RainCoreScene;

public static class AssetRegistry
{
    private static readonly Dictionary<string, ProceduralModel> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<string> Palette { get; } = BuildPalette();

    private static IReadOnlyList<string> BuildPalette()
    {
        var entries = new List<string>
        {
            "proc:beacon",
            "proc:gabled_house",
            "proc:ruined_hut",
            "proc:fallen_beam",
            "proc:marker_stone",
            "proc:grass_tuft",
        };

        var modelsDir = Path.Combine(AppContext.BaseDirectory, "Content", "Models");
        if (Directory.Exists(modelsDir))
        {
            foreach (var modelPath in Directory.EnumerateFiles(modelsDir, "*.obj", SearchOption.AllDirectories)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var relativePath = Path.GetRelativePath(AppContext.BaseDirectory, modelPath)
                    .Replace('\\', '/');
                var key = $"obj:{relativePath}";
                if (!entries.Contains(key, StringComparer.OrdinalIgnoreCase))
                    entries.Add(key);
            }
        }

        return entries;
    }

    public static ProceduralModel Resolve(string key)
    {
        if (Cache.TryGetValue(key, out var cached)) return cached;

        var model = key.ToLowerInvariant() switch
        {
            "proc:beacon" => ProceduralModel.CreateBeacon(4.2f, 0.09f, 0.6f,
                new Vector3(0.10f, 0.10f, 0.11f), new Vector3(0.85f, 0.15f, 0.14f)),
            "proc:gabled_house" => ProceduralModel.CreateGabledHouse(8f, 6.5f, 4.8f, 2.4f,
                new Vector3(0.68f, 0.69f, 0.67f), new Vector3(0.20f, 0.23f, 0.28f),
                new Vector3(0.86f, 0.88f, 0.90f)),
            "proc:ruined_hut" => ProceduralModel.CreateRuinedHut(),
            "proc:fallen_beam" => ProceduralModel.CreateBox(new Vector3(0.22f, 3.5f, 0.22f),
                new Vector3(0.16f, 0.15f, 0.13f)),
            "proc:marker_stone" => ProceduralModel.CreateLowPolyMound(1.4f, 1.1f, 0.9f,
                new Vector3(0.25f, 0.27f, 0.28f), 77),
            "proc:grass_tuft" => ProceduralModel.CreateGrassTuft(0.85f, 7,
                new Vector3(0.18f, 0.23f, 0.17f), new Vector3(0.62f, 0.70f, 0.48f), 511,
                spread: 0.30f, width: 0.06f),
            var objKey when objKey.StartsWith("obj:", StringComparison.OrdinalIgnoreCase) =>
                ModelLoader.LoadFromObj(ResolveAssetPath(objKey[4..]), new Vector3(0.70f, 0.72f, 0.74f)),
            _ => throw new ArgumentException($"Unknown scene model '{key}'.", nameof(key)),
        };

        Cache[key] = model;
        return model;
    }

    private static string ResolveAssetPath(string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.Combine(AppContext.BaseDirectory, path);
    }
}
