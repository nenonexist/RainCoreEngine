using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

using RainCoreGraphics;

namespace RainCoreScene;

             
                                                                         
                                                                           
                                                            
                                                                            
                                                       
                                                                     
                                                                                         
                                                                         
                   
              
public sealed partial class ProceduralModel
{
    private sealed class Part
    {
        public int Vao, Vbo, Ebo;
        public int IndexCount;
        public int TextureId;
    }

    private readonly List<Part> _parts = new();

                                                                                    
                                                                                                   
    public Vector3 LocalMin { get; private set; }
    public Vector3 LocalMax { get; private set; }

    public void Render(Shader shader, Matrix4 modelMatrix)
    {
        shader.SetMatrix4("model", modelMatrix);
        shader.SetInt("useTexture", 0);
        foreach (var part in _parts)
        {
            shader.SetInt("useTexture", part.TextureId == 0 ? 0 : 1);
            if (part.TextureId != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, part.TextureId);
                shader.SetInt("modelTexture", 0);
            }
            GL.BindVertexArray(part.Vao);
            GL.DrawElements(PrimitiveType.Triangles, part.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
        shader.SetInt("useTexture", 0);
    }

                                  
    private static void AddQuad(List<float> vertexData, List<uint> indices, ref uint vi,
        Vector3 n, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 color, float unlit = 0f)
    {
        var uv = new[] { (0f, 0f), (1f, 0f), (1f, 1f), (0f, 1f) };
        var pts = new[] { a, b, c, d };
        for (int i = 0; i < 4; i++)
        {
            var p = pts[i];
            vertexData.AddRange(new[]
            {
                p.X, p.Y, p.Z, n.X, n.Y, n.Z,
                color.X, color.Y, color.Z, unlit,
                uv[i].Item1, uv[i].Item2, 0f,
            });
        }
        indices.AddRange(new[] { vi, vi + 1, vi + 2, vi, vi + 2, vi + 3 });
        vi += 4;
    }

    private static void AddTri(List<float> vertexData, List<uint> indices, ref uint vi,
        Vector3 n, Vector3 a, Vector3 b, Vector3 c, Vector3 color)
    {
        foreach (var p in new[] { a, b, c })
        {
            vertexData.AddRange(new[]
            {
                p.X, p.Y, p.Z, n.X, n.Y, n.Z,
                color.X, color.Y, color.Z, 0f,
                0f, 0f, 0f,
            });
        }
        indices.AddRange(new[] { vi, vi + 1, vi + 2 });
        vi += 3;
    }

    private static void AddBox(List<float> vertexData, List<uint> indices, ref uint vi, Vector3 min, Vector3 max, Vector3 color, float unlit = 0f)
    {
        var faces = new (Vector3 n, Vector3 a, Vector3 b, Vector3 c, Vector3 d)[]
        {
            (new(0,-1,0), new(min.X,min.Y,min.Z), new(max.X,min.Y,min.Z), new(max.X,min.Y,max.Z), new(min.X,min.Y,max.Z)),
            (new(0,1,0),  new(min.X,max.Y,max.Z), new(max.X,max.Y,max.Z), new(max.X,max.Y,min.Z), new(min.X,max.Y,min.Z)),
            (new(0,0,-1), new(min.X,min.Y,min.Z), new(min.X,max.Y,min.Z), new(max.X,max.Y,min.Z), new(max.X,min.Y,min.Z)),
            (new(0,0,1),  new(max.X,min.Y,max.Z), new(max.X,max.Y,max.Z), new(min.X,max.Y,max.Z), new(min.X,min.Y,max.Z)),
            (new(-1,0,0), new(min.X,min.Y,max.Z), new(min.X,max.Y,max.Z), new(min.X,max.Y,min.Z), new(min.X,min.Y,min.Z)),
            (new(1,0,0),  new(max.X,min.Y,min.Z), new(max.X,max.Y,min.Z), new(max.X,max.Y,max.Z), new(max.X,min.Y,max.Z)),
        };
        foreach (var (n, a, b, c, d) in faces)
            AddQuad(vertexData, indices, ref vi, n, a, b, c, d, color, unlit);
    }

    private static ProceduralModel FinalizeMesh(List<float> vertexData, List<uint> indices, Vector3 localMin, Vector3 localMax, string? texturePath = null)
    {
        var model = new ProceduralModel { LocalMin = localMin, LocalMax = localMax };
        var part = new Part { IndexCount = indices.Count };
        part.Vao = GL.GenVertexArray();
        part.Vbo = GL.GenBuffer();
        part.Ebo = GL.GenBuffer();
        if (!string.IsNullOrWhiteSpace(texturePath))
            part.TextureId = LoadTexture(texturePath);

        GL.BindVertexArray(part.Vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, part.Vbo);
        var vArr = vertexData.ToArray();
        GL.BufferData(BufferTarget.ArrayBuffer, vArr.Length * sizeof(float), vArr, BufferUsageHint.StaticDraw);

        int stride = 13 * sizeof(float);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, stride, 9 * sizeof(float));
        GL.EnableVertexAttribArray(3);
        GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, stride, 10 * sizeof(float));
        GL.EnableVertexAttribArray(4);
        GL.VertexAttribPointer(5, 1, VertexAttribPointerType.Float, false, stride, 12 * sizeof(float));
        GL.EnableVertexAttribArray(5);

        var iArr = indices.ToArray();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, part.Ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, iArr.Length * sizeof(uint), iArr, BufferUsageHint.StaticDraw);

        model._parts.Add(part);
        return model;
    }

    public static ProceduralModel CreateMeshFromObjData(List<float> vertexData, List<uint> indices,
        Vector3 localMin, Vector3 localMax, string? texturePath = null) =>
        FinalizeMesh(vertexData, indices, localMin, localMax, texturePath);

    public sealed class MeshPartData
    {
        public List<float> VertexData { get; init; } = new();
        public List<uint> Indices { get; init; } = new();
        public Vector3 LocalMin { get; init; }
        public Vector3 LocalMax { get; init; }
        public string? TexturePath { get; init; }
    }

    public static ProceduralModel CreateMeshPartsFromObjData(IReadOnlyList<MeshPartData> parts)
    {
        var model = new ProceduralModel
        {
            LocalMin = new Vector3(float.MaxValue),
            LocalMax = new Vector3(float.MinValue),
        };
        foreach (var data in parts)
        {
            if (data.Indices.Count == 0) continue;
            model.LocalMin = Vector3.ComponentMin(model.LocalMin, data.LocalMin);
            model.LocalMax = Vector3.ComponentMax(model.LocalMax, data.LocalMax);
            model._parts.Add(CreateGpuPart(data.VertexData, data.Indices, data.TexturePath));
        }
        return model;
    }

    private static Part CreateGpuPart(List<float> vertexData, List<uint> indices, string? texturePath)
    {
        var part = new Part { IndexCount = indices.Count };
        part.Vao = GL.GenVertexArray();
        part.Vbo = GL.GenBuffer();
        part.Ebo = GL.GenBuffer();
        if (!string.IsNullOrWhiteSpace(texturePath)) part.TextureId = LoadTexture(texturePath);

        GL.BindVertexArray(part.Vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, part.Vbo);
        var vArr = vertexData.ToArray();
        GL.BufferData(BufferTarget.ArrayBuffer, vArr.Length * sizeof(float), vArr, BufferUsageHint.StaticDraw);
        int stride = 13 * sizeof(float);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0); GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float)); GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float)); GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, stride, 9 * sizeof(float)); GL.EnableVertexAttribArray(3);
        GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, stride, 10 * sizeof(float)); GL.EnableVertexAttribArray(4);
        GL.VertexAttribPointer(5, 1, VertexAttribPointerType.Float, false, stride, 12 * sizeof(float)); GL.EnableVertexAttribArray(5);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, part.Ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.ToArray().Length * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);
        return part;
    }

    private static readonly Dictionary<string, int> TextureCache = new(StringComparer.OrdinalIgnoreCase);

    private static int LoadTexture(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (TextureCache.TryGetValue(fullPath, out var cached)) return cached;
        if (!File.Exists(fullPath)) return 0;

        using var stream = File.OpenRead(fullPath);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
            image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        TextureCache[fullPath] = texture;
        return texture;
    }
}
