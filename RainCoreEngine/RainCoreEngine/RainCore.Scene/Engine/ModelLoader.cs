using System.Globalization;
using OpenTK.Mathematics;

namespace RainCoreScene;

public static class ModelLoader
{
    private static readonly Dictionary<string, ProceduralModel> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static ProceduralModel LoadFromObj(string path, Vector3 fallbackColor)
    {
        var fullPath = Path.GetFullPath(path);
        if (Cache.TryGetValue(fullPath, out var cached)) return cached;
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("OBJ model was not found.", fullPath);

        var positions = new List<Vector3>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();
        var builders = new Dictionary<string, PartBuilder>(StringComparer.OrdinalIgnoreCase);
        var currentMaterial = "default";
        string? materialFile = null;

        foreach (var rawLine in File.ReadLines(fullPath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            switch (parts[0])
            {
                case "mtllib" when parts.Length >= 2:
                    materialFile = string.Join(' ', parts[1..]);
                    break;
                case "usemtl" when parts.Length >= 2:
                    currentMaterial = string.Join(' ', parts[1..]);
                    break;
                case "v":
                    positions.Add(new Vector3(Parse(parts[1]), Parse(parts[2]), Parse(parts[3])));
                    break;
                case "vt":
                    uvs.Add(new Vector2(Parse(parts[1]), Parse(parts[2])));
                    break;
                case "vn":
                    normals.Add(new Vector3(Parse(parts[1]), Parse(parts[2]), Parse(parts[3])).Normalized());
                    break;
                case "f" when parts.Length >= 4:
                    if (!builders.TryGetValue(currentMaterial, out var builder))
                    {
                        builder = new PartBuilder();
                        builders[currentMaterial] = builder;
                    }
                    AddFace(parts[1..], positions, uvs, normals, fallbackColor, builder);
                    break;
            }
        }

        if (builders.Count == 0 || builders.Values.All(builder => builder.Indices.Count == 0))
            throw new InvalidDataException($"OBJ model '{fullPath}' contains no faces.");

        var textures = ResolveMaterialTextures(fullPath, materialFile);
        var meshParts = builders.Select(pair => new ProceduralModel.MeshPartData
        {
            VertexData = pair.Value.VertexData,
            Indices = pair.Value.Indices,
            LocalMin = pair.Value.LocalMin,
            LocalMax = pair.Value.LocalMax,
            TexturePath = textures.GetValueOrDefault(pair.Key),
        }).ToList();
        var model = ProceduralModel.CreateMeshPartsFromObjData(meshParts);
        Cache[fullPath] = model;
        return model;
    }

    private static void AddFace(string[] faceTokens, List<Vector3> positions, List<Vector2> uvs,
        List<Vector3> normals, Vector3 color, PartBuilder builder)
    {
        var corners = faceTokens.Select(ParseFaceCorner).ToArray();
        var p0 = positions[ResolveIndex(corners[0].Position, positions.Count)];
        var p1 = positions[ResolveIndex(corners[1].Position, positions.Count)];
        var p2 = positions[ResolveIndex(corners[2].Position, positions.Count)];
        var faceNormal = Vector3.Cross(p1 - p0, p2 - p0).Normalized();

        for (int i = 1; i < corners.Length - 1; i++)
        {
            var triangle = new[] { corners[0], corners[i], corners[i + 1] };
            foreach (var corner in triangle)
            {
                var position = positions[ResolveIndex(corner.Position, positions.Count)];
                var normal = corner.Normal is null
                    ? faceNormal
                    : normals[ResolveIndex(corner.Normal.Value, normals.Count)];
                var uv = corner.TexCoord is null
                    ? Vector2.Zero
                    : uvs[ResolveIndex(corner.TexCoord.Value, uvs.Count)];
                builder.VertexData.AddRange(new[]
                {
                    position.X, position.Y, position.Z,
                    normal.X, normal.Y, normal.Z,
                    color.X, color.Y, color.Z,
                    0f, uv.X, uv.Y, 0f,
                });
                builder.Indices.Add(builder.VertexIndex++);
                builder.LocalMin = Vector3.ComponentMin(builder.LocalMin, position);
                builder.LocalMax = Vector3.ComponentMax(builder.LocalMax, position);
            }
        }
    }

    private static FaceCorner ParseFaceCorner(string token)
    {
        var parts = token.Split('/');
        int position = int.Parse(parts[0], CultureInfo.InvariantCulture);
        int? texCoord = parts.Length > 1 && parts[1].Length > 0
            ? int.Parse(parts[1], CultureInfo.InvariantCulture)
            : null;
        int? normal = parts.Length > 2 && parts[2].Length > 0
            ? int.Parse(parts[2], CultureInfo.InvariantCulture)
            : null;
        return new FaceCorner(position, texCoord, normal);
    }

    private static int ResolveIndex(int index, int count)
    {
        var resolved = index < 0 ? count + index : index - 1;
        if (resolved < 0 || resolved >= count)
            throw new InvalidDataException($"OBJ index {index} is outside a list of {count} items.");
        return resolved;
    }

    private static float Parse(string value) => float.Parse(value, CultureInfo.InvariantCulture);

    private static Dictionary<string, string> ResolveMaterialTextures(string objPath, string? materialFile)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(materialFile)) return result;
        var materialPath = Path.Combine(Path.GetDirectoryName(objPath)!, materialFile);
        if (!File.Exists(materialPath)) return result;
        string? currentMaterial = null;
        foreach (var rawLine in File.ReadLines(materialPath))
        {
            var parts = rawLine.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && parts[0].Equals("newmtl", StringComparison.OrdinalIgnoreCase))
                currentMaterial = string.Join(' ', parts[1..]);
            else if (parts.Length >= 2 && parts[0].Equals("map_Kd", StringComparison.OrdinalIgnoreCase) && currentMaterial is not null)
                result[currentMaterial] = Path.Combine(Path.GetDirectoryName(materialPath)!, string.Join(' ', parts[1..]));
        }
        return result;
    }

    private sealed class PartBuilder
    {
        public List<float> VertexData { get; } = new();
        public List<uint> Indices { get; } = new();
        public uint VertexIndex { get; set; }
        public Vector3 LocalMin { get; set; } = new(float.MaxValue);
        public Vector3 LocalMax { get; set; } = new(float.MinValue);
    }

    private readonly record struct FaceCorner(int Position, int? TexCoord, int? Normal);
}
