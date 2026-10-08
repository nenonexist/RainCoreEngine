using System.Text.Json;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

using RainCoreGraphics;

namespace RainCore;

             
                                                                             
                                                                       
                                                                         
                                                                            
                                                                         
                                                                            
                                                                      
                                                                           
                                                                            
                                                                          
                                                                      
                                                                
              
public class GlbModel
{
                                                                               
                                                                               
                                                                                
                                                                            
                                                                                 
    private class Part
    {
        public int Vao, Vbo, Ebo;
        public int IndexCount;
        public int TextureHandle;
    }

    private readonly List<Part> _parts = new();

                                                                                 
                                                                                
                                                                              
                                                       
    public Vector3 LocalMin { get; private set; }
    public Vector3 LocalMax { get; private set; }

    public static GlbModel LoadFromFile(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        uint magic = br.ReadUInt32();
        if (magic != 0x46546C67) throw new InvalidDataException($"Не GLB-файл: {path}");
        br.ReadUInt32();           
        br.ReadUInt32();                     

        string? json = null;
        byte[]? bin = null;

        while (ms.Position < ms.Length)
        {
            uint chunkLength = br.ReadUInt32();
            uint chunkType = br.ReadUInt32();
            byte[] chunkData = br.ReadBytes((int)chunkLength);
            if (chunkType == 0x4E4F534A) json = System.Text.Encoding.UTF8.GetString(chunkData);
            else if (chunkType == 0x004E4942) bin = chunkData;
        }

        if (json == null) throw new InvalidDataException($"В GLB нет JSON-чанка: {path}");
        if (bin == null) throw new InvalidDataException($"В GLB нет бинарного буфера (внешние .bin не поддержаны): {path}");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        WarnIfUnsupportedContent(root, path);

        var model = new GlbModel();
        var localMin = new Vector3(float.MaxValue);
        var localMax = new Vector3(float.MinValue);
        bool anyVertex = false;

                                                                                   
                                                                               
                                                                          
                                                
        var textureCache = new Dictionary<int, int>();

        int meshCount = root.TryGetProperty("meshes", out var meshesEl) ? meshesEl.GetArrayLength() : 0;
        for (int meshIdx = 0; meshIdx < meshCount; meshIdx++)
        {
            var mesh = meshesEl[meshIdx];
            var primitives = mesh.GetProperty("primitives");

            for (int primIdx = 0; primIdx < primitives.GetArrayLength(); primIdx++)
            {
                var prim = primitives[primIdx];

                                                                                
                                                                                 
                                                                             
                int mode = prim.TryGetProperty("mode", out var modeEl) ? modeEl.GetInt32() : 4;
                if (mode != 4)
                {
                    Console.WriteLine(
                        $"[GlbModel] '{Path.GetFileName(path)}': meshes[{meshIdx}].primitives[{primIdx}] " +
                        $"имеет mode={mode} (не TRIANGLES) - часть пропущена.");
                    continue;
                }

                if (!prim.TryGetProperty("attributes", out var attrs) || !attrs.TryGetProperty("POSITION", out var posAcc))
                    continue;                                

                float[] positions = ReadFloats(root, bin, posAcc.GetInt32());
                float[]? normals = attrs.TryGetProperty("NORMAL", out var nAcc) ? ReadFloats(root, bin, nAcc.GetInt32()) : null;
                float[]? uvs = attrs.TryGetProperty("TEXCOORD_0", out var uvAcc) ? ReadFloats(root, bin, uvAcc.GetInt32()) : null;
                uint[] indices = prim.TryGetProperty("indices", out var idxAcc)
                    ? ReadIndices(root, bin, idxAcc.GetInt32())
                    : BuildSequentialIndices(positions.Length / 3);

                int vertCount = positions.Length / 3;
                if (vertCount == 0) continue;
                anyVertex = true;

                for (int i = 0; i < vertCount; i++)
                {
                    var p = new Vector3(positions[i * 3 + 0], positions[i * 3 + 1], positions[i * 3 + 2]);
                    localMin = Vector3.ComponentMin(localMin, p);
                    localMax = Vector3.ComponentMax(localMax, p);
                }

                var vertexData = new float[vertCount * 13];
                for (int i = 0; i < vertCount; i++)
                {
                    vertexData[i * 13 + 0] = positions[i * 3 + 0];
                    vertexData[i * 13 + 1] = positions[i * 3 + 1];
                    vertexData[i * 13 + 2] = positions[i * 3 + 2];
                    vertexData[i * 13 + 3] = normals != null ? normals[i * 3 + 0] : 0f;
                    vertexData[i * 13 + 4] = normals != null ? normals[i * 3 + 1] : 1f;
                    vertexData[i * 13 + 5] = normals != null ? normals[i * 3 + 2] : 0f;
                    vertexData[i * 13 + 6] = 1f;                                           
                    vertexData[i * 13 + 7] = 1f;
                    vertexData[i * 13 + 8] = 1f;
                    vertexData[i * 13 + 9] = 0f;                                              
                    vertexData[i * 13 + 10] = uvs != null ? uvs[i * 2 + 0] : 0f;
                    vertexData[i * 13 + 11] = uvs != null ? uvs[i * 2 + 1] : 0f;
                                                                                       
                                                                                   
                                                                                   
                                                                                       
                                                                                        
                                                 
                    vertexData[i * 13 + 12] = 0f;
                }

                int textureHandle = TryLoadBaseColorTexture(root, bin, prim, path, textureCache);

                var part = new Part
                {
                    IndexCount = indices.Length,
                    TextureHandle = textureHandle
                };

                part.Vao = GL.GenVertexArray();
                part.Vbo = GL.GenBuffer();
                part.Ebo = GL.GenBuffer();

                GL.BindVertexArray(part.Vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, part.Vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

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

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, part.Ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

                model._parts.Add(part);
            }
        }

        model.LocalMin = anyVertex ? localMin : Vector3.Zero;
        model.LocalMax = anyVertex ? localMax : Vector3.Zero;

        if (model._parts.Count == 0)
            Console.WriteLine($"[GlbModel] ВНИМАНИЕ: '{Path.GetFileName(path)}' - не удалось загрузить ни одной части (0 частей).");

        return model;
    }

                                                                                                  
                                                                                
                                                                                
                                                                                          
    public void Render(Shader shader, Matrix4 modelMatrix)
    {
        shader.SetMatrix4("model", modelMatrix);

        foreach (var part in _parts)
        {
            if (part.TextureHandle != 0)
            {
                shader.SetInt("useTexture", 1);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, part.TextureHandle);
                shader.SetInt("modelTexture", 0);
            }
            else
            {
                shader.SetInt("useTexture", 0);
            }

            GL.BindVertexArray(part.Vao);
            GL.DrawElements(PrimitiveType.Triangles, part.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }

                 
                                                                                     
                                                                               
                                                                              
                  
    private static void WarnIfUnsupportedContent(JsonElement root, string path)
    {
        string name = Path.GetFileName(path);

        if (root.TryGetProperty("animations", out var animsEl) && animsEl.GetArrayLength() > 0)
        {
            Console.WriteLine(
                $"[GlbModel] ВНИМАНИЕ: '{name}' содержит {animsEl.GetArrayLength()} animation(s), " +
                "но скелетная анимация не поддержана - модель будет отрисована в статичной bind-позе.");
        }
    }

                                                                         
                                                                               

    private static (byte[] Bin, int Offset, int Stride, int Count, int CompCount, int CompType) GetAccessor(
        JsonElement root, byte[] bin, int accessorIndex)
    {
        var accessor = root.GetProperty("accessors")[accessorIndex];
        int bufferViewIndex = accessor.GetProperty("bufferView").GetInt32();
        int accByteOffset = accessor.TryGetProperty("byteOffset", out var bo) ? bo.GetInt32() : 0;
        int count = accessor.GetProperty("count").GetInt32();
        int componentType = accessor.GetProperty("componentType").GetInt32();
        string type = accessor.GetProperty("type").GetString()!;
        int compCount = type switch { "SCALAR" => 1, "VEC2" => 2, "VEC3" => 3, "VEC4" => 4, _ => 1 };

        var bufferView = root.GetProperty("bufferViews")[bufferViewIndex];
        int bvByteOffset = bufferView.TryGetProperty("byteOffset", out var bvo) ? bvo.GetInt32() : 0;
        int bvStride = bufferView.TryGetProperty("byteStride", out var bs) ? bs.GetInt32() : 0;

        return (bin, bvByteOffset + accByteOffset, bvStride, count, compCount, componentType);
    }

    private static float[] ReadFloats(JsonElement root, byte[] bin, int accessorIndex)
    {
        var (data, offset, stride, count, compCount, componentType) = GetAccessor(root, bin, accessorIndex);
        int compSize = componentType == 5126 ? 4 : throw new NotSupportedException(
            $"Поддержаны только FLOAT-аксессоры для POSITION/NORMAL/TEXCOORD_0 (componentType={componentType})");
        int elemStride = stride > 0 ? stride : compCount * compSize;
        var result = new float[count * compCount];

        for (int i = 0; i < count; i++)
        {
            int baseOff = offset + i * elemStride;
            for (int c = 0; c < compCount; c++)
                result[i * compCount + c] = BitConverter.ToSingle(data, baseOff + c * compSize);
        }
        return result;
    }

    private static uint[] ReadIndices(JsonElement root, byte[] bin, int accessorIndex)
    {
        var (data, offset, stride, count, _, componentType) = GetAccessor(root, bin, accessorIndex);
        int compSize = componentType switch { 5125 => 4, 5123 => 2, 5121 => 1, _ => 2 };
        int elemStride = stride > 0 ? stride : compSize;
        var result = new uint[count];

        for (int i = 0; i < count; i++)
        {
            int off = offset + i * elemStride;
            result[i] = componentType switch
            {
                5125 => BitConverter.ToUInt32(data, off),
                5123 => BitConverter.ToUInt16(data, off),
                5121 => data[off],
                _ => 0
            };
        }
        return result;
    }

    private static uint[] BuildSequentialIndices(int vertCount)
    {
        var indices = new uint[vertCount];
        for (int i = 0; i < vertCount; i++) indices[i] = (uint)i;
        return indices;
    }

                                                                                            
                                                                                          
                                                                         
    private static int TryLoadBaseColorTexture(JsonElement root, byte[] bin, JsonElement prim, string modelPath, Dictionary<int, int> textureCache)
    {
        if (!prim.TryGetProperty("material", out var matIdxEl)) return 0;
        int materialIndex = matIdxEl.GetInt32();

        if (textureCache.TryGetValue(materialIndex, out var cached)) return cached;

        int handle = LoadBaseColorTextureUncached(root, bin, materialIndex, modelPath);
        textureCache[materialIndex] = handle;                                                              
        return handle;
    }

    private static int LoadBaseColorTextureUncached(JsonElement root, byte[] bin, int materialIndex, string modelPath)
    {
        if (!root.TryGetProperty("materials", out var materials)) return 0;
        var material = materials[materialIndex];
        if (!material.TryGetProperty("pbrMetallicRoughness", out var pbr)) return 0;
        if (!pbr.TryGetProperty("baseColorTexture", out var baseColorTex)) return 0;

        int textureIndex = baseColorTex.GetProperty("index").GetInt32();
        int imageIndex = root.GetProperty("textures")[textureIndex].GetProperty("source").GetInt32();
        var image = root.GetProperty("images")[imageIndex];

        byte[] imgBytes;
        if (image.TryGetProperty("bufferView", out var bvEl))
        {
            var bufferView = root.GetProperty("bufferViews")[bvEl.GetInt32()];
            int off = bufferView.TryGetProperty("byteOffset", out var o) ? o.GetInt32() : 0;
            int len = bufferView.GetProperty("byteLength").GetInt32();
            imgBytes = new byte[len];
            Array.Copy(bin, off, imgBytes, 0, len);
        }
        else if (image.TryGetProperty("uri", out var uriEl))
        {
            string uri = uriEl.GetString()!;
            imgBytes = uri.StartsWith("data:")
                ? Convert.FromBase64String(uri[(uri.IndexOf(',') + 1)..])
                : File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(modelPath)!, uri));
        }
        else return 0;

        using var imgMs = new MemoryStream(imgBytes);
        using var bmp = new Bitmap(imgMs);
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        int handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
            PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        bmp.UnlockBits(data);

                                                                                                 
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        return handle;
    }

                 
                                                                           
                                                                     
                                                                      
                                                                          
                                                               
                                                                         
                                                                          
                                                                        
                                                                          
                                                                      
                  
    public static GlbModel CreatePrimitiveBox(Vector3 size, Vector3 color)
    {
        var model = new GlbModel();
        float hx = size.X * 0.5f, hz = size.Z * 0.5f, y1 = size.Y;

                                                                              
        var faces = new (Vector3 n, Vector3 a, Vector3 b, Vector3 c, Vector3 d)[]
        {
                                     
            (new(0, -1, 0), new(-hx, 0, -hz), new(hx, 0, -hz), new(hx, 0, hz), new(-hx, 0, hz)),
            (new(0, 1, 0), new(-hx, y1, hz), new(hx, y1, hz), new(hx, y1, -hz), new(-hx, y1, -hz)),
                              
            (new(0, 0, -1), new(-hx, 0, -hz), new(-hx, y1, -hz), new(hx, y1, -hz), new(hx, 0, -hz)),
            (new(0, 0, 1), new(hx, 0, hz), new(hx, y1, hz), new(-hx, y1, hz), new(-hx, 0, hz)),
            (new(-1, 0, 0), new(-hx, 0, hz), new(-hx, y1, hz), new(-hx, y1, -hz), new(-hx, 0, -hz)),
            (new(1, 0, 0), new(hx, 0, -hz), new(hx, y1, -hz), new(hx, y1, hz), new(hx, 0, hz)),
        };

        var vertexData = new List<float>();
        var indices = new List<uint>();
        var uv = new[] { (0f, 0f), (1f, 0f), (1f, 1f), (0f, 1f) };
        uint vi = 0;

        foreach (var (n, a, b, c, d) in faces)
        {
            foreach (var (p, idx) in new[] { (a, 0), (b, 1), (c, 2), (d, 3) })
            {
                vertexData.AddRange(new[]
                {
                    p.X, p.Y, p.Z, n.X, n.Y, n.Z,
                    color.X, color.Y, color.Z, 0f,
                    uv[idx].Item1, uv[idx].Item2, 0f,
                });
            }
            indices.AddRange(new[] { vi, vi + 1, vi + 2, vi, vi + 2, vi + 3 });
            vi += 4;
        }

        model.LocalMin = new Vector3(-hx, 0f, -hz);
        model.LocalMax = new Vector3(hx, y1, hz);

        var part = new Part { IndexCount = indices.Count, TextureHandle = 0 };
        part.Vao = GL.GenVertexArray();
        part.Vbo = GL.GenBuffer();
        part.Ebo = GL.GenBuffer();

        GL.BindVertexArray(part.Vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, part.Vbo);
        var vertexArray = vertexData.ToArray();
        GL.BufferData(BufferTarget.ArrayBuffer, vertexArray.Length * sizeof(float), vertexArray, BufferUsageHint.StaticDraw);

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

        var indexArray = indices.ToArray();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, part.Ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indexArray.Length * sizeof(uint), indexArray, BufferUsageHint.StaticDraw);

        model._parts.Add(part);
        return model;
    }
}

                                                                                                                                                    
public class ModelInstance
{
    public GlbModel Model { get; }
                                                                                         
                                                                                             
                                                           
    public string ModelKey { get; }
    public Vector3 Position;
    public Vector3 RotationDeg;
    public float Scale;
                                                                           
                                                                                    
                                                                                   
                                                                                               
    public bool Persist { get; set; } = true;

                 
                                                                         
                                                                      
                                                                         
                                                                             
                                                                        
                  
    public bool IsSolid { get; set; } = false;

                 
                                                                            
                                                                            
                                                                         
                                                                        
                                                                              
                                                 
                  
    public bool Visible { get; set; } = true;

    public ModelInstance(GlbModel model, string modelKey, Vector3 position, Vector3 rotationDeg = default, float scale = 1f)
    {
        Model = model;
        ModelKey = modelKey;
        Position = position;
        RotationDeg = rotationDeg;
        Scale = scale;
    }

    private Matrix4 GetModelMatrix()
    {
        return Matrix4.CreateScale(Scale)
        * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(RotationDeg.X))
        * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotationDeg.Y))
        * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(RotationDeg.Z))
        * Matrix4.CreateTranslation(Position);
    }

    public void Render(Shader shader)
    {
        if (!Visible) return;
        Model.Render(shader, GetModelMatrix());
    }

                 
                                                                          
                                                                                 
                                                                      
                                                                    
                                                                          
                                                                            
                                  
                  
    public void GetWorldBounds(out Vector3 min, out Vector3 max)
    {
        var a = Position + Model.LocalMin * Scale;
        var b = Position + Model.LocalMax * Scale;
        min = Vector3.ComponentMin(a, b);
        max = Vector3.ComponentMax(a, b);
    }
}
