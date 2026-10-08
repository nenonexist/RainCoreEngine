using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace RainCore;

             
                                                                              
                                                               
   
                                                                        
                                                                             
                                                                         
                                                                        
                                                                       
                                                                           
                                                         
   
                                                                       
                                                                       
                                                                       
                
              
public sealed class Chunk
{
                                                                                                                        
    public readonly Vector2i Coord;

    private readonly Dictionary<Vector3i, BlockType> _blocks = new();
                                                                                
                                                                               
                                                       
    private readonly Dictionary<Vector3i, BlockShape> _shapes = new();

                                                                                 
                                                                                 
                                                                             
                                                                                 
                                                                               
                                                                                
                                                                          
                                                                               
                                                                                       
    private readonly object _lock = new();

                                                                              
                                                                              
                                                                           
                                                                           
                                                                          
                                                                         
    private volatile bool _dirty = true;

    private int _vao;
    private int _vbo;
                                                                                   
                                                                                       
                                                                                           
                                                
    private int _ebo;
    private int _vertexCount;
    private int _indexCount;

                                                                          
                                                                          
                                                                            
                                                                           
                                                                                        
                                                                          
                                                                         
                                             
    private int _liquidVao;
    private int _liquidVbo;
    private int _liquidEbo;
    private int _liquidVertexCount;
    private int _liquidIndexCount;

                 
                                                                                     
                                                                                   
                                                                                      
                                                                                        
                                                                                  
                                                                                         
                                                                                     
                                                                                    
                                                                                         
                                                                                         
                    
                  
    private sealed class VertexBuffer
    {
        public float[] Data = new float[1024];
        public int Count;

        public void Add(float v)
        {
            if (Count == Data.Length) Array.Resize(ref Data, Data.Length * 2);
            Data[Count++] = v;
        }

        public void Clear() => Count = 0;
    }

    private readonly VertexBuffer _vertexScratch = new();
    private readonly VertexBuffer _liquidVertexScratch = new();

                 
                                                                                 
                                                                                     
                                                                                   
                                                                                       
                                                                                       
                                                                                         
                                                                                    
                  
    private sealed class IndexBuffer
    {
        public int[] Data = new int[1536];
        public int Count;

        public void Add(int v)
        {
            if (Count == Data.Length) Array.Resize(ref Data, Data.Length * 2);
            Data[Count++] = v;
        }

        public void Clear() => Count = 0;
    }

    private readonly IndexBuffer _indexScratch = new();
    private readonly IndexBuffer _liquidIndexScratch = new();

    public int BlockCount { get { lock (_lock) return _blocks.Count; } }
    public int VertexCount => _vertexCount;

                                                                                   
                                                                                
                                                                                
                                                                                 
                             
    public IReadOnlyDictionary<Vector3i, BlockType> Blocks => _blocks;

    public int MinX => Coord.X * World.ChunkSize;
    public int MinZ => Coord.Y * World.ChunkSize;
    public int MaxX => MinX + World.ChunkSize;
    public int MaxZ => MinZ + World.ChunkSize;

    public Chunk(Vector2i coord)
    {
        Coord = coord;
    }


    public BlockType GetLocal(Vector3i pos)
    {
        lock (_lock) return _blocks.TryGetValue(pos, out var t) ? t : BlockType.Air;
    }

                                                                               
                                                                               
                                                                           
                                                                               
                                                                                       
    public bool TryGetLocal(Vector3i pos, out BlockType type)
    {
        lock (_lock) return _blocks.TryGetValue(pos, out type);
    }


    public void SetLocal(Vector3i pos, BlockType type)
    {
        lock (_lock)
        {
            if (type == BlockType.Air) _blocks.Remove(pos);
            else _blocks[pos] = type;
            _shapes.Remove(pos);                                                                                                                                
        }
        _dirty = true;
    }

    public BlockShape GetLocalShape(Vector3i pos)
    {
        lock (_lock) return _shapes.TryGetValue(pos, out var shape) ? shape : BlockShape.Cube;
    }

                 
                                                                            
                                                                           
                                                                         
                                                                              
                  
    public void SetLocalShape(Vector3i pos, BlockShape shape)
    {
        lock (_lock)
        {
            if (shape == BlockShape.Cube) _shapes.Remove(pos);
            else _shapes[pos] = shape;
        }
        _dirty = true;
    }

    public void MarkDirty()
    {
        _dirty = true;
    }

                                                                                     
                                                                                   
                                                                              
                                                                                    
                                                                  
    public Dictionary<Vector3i, BlockType> SnapshotBlocks()
    {
        lock (_lock) return new Dictionary<Vector3i, BlockType>(_blocks);
    }

    public Dictionary<Vector3i, BlockShape> SnapshotShapes()
    {
        lock (_lock) return new Dictionary<Vector3i, BlockShape>(_shapes);
    }

    public (Dictionary<Vector3i, BlockType> Blocks, Dictionary<Vector3i, BlockShape> Shapes) SnapshotData()
    {
        lock (_lock)
        {
            return (new Dictionary<Vector3i, BlockType>(_blocks),
                    new Dictionary<Vector3i, BlockShape>(_shapes));
        }
    }


                                                                                                                                                
    public Vector3 ClosestPointXZ(Vector3 point)
    {
        float cx = Math.Clamp(point.X, MinX, MaxX);
        float cz = Math.Clamp(point.Z, MinZ, MaxZ);
        return new Vector3(cx, point.Y, cz);
    }

                                                                                          
    private static readonly (Vector3i Normal, Vector3[] Corners)[] Faces =
    {
        (new Vector3i(0, 0, -1), new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0) }),
        (new Vector3i(0, 0, 1),  new[] { new Vector3(1,0,1), new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(1,1,1) }),
        (new Vector3i(-1, 0, 0), new[] { new Vector3(0,0,1), new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(0,1,1) }),
        (new Vector3i(1, 0, 0),  new[] { new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(1,1,0) }),
        (new Vector3i(0, 1, 0),  new[] { new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(0,1,1) }),
        (new Vector3i(0, -1, 0), new[] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,0,0), new Vector3(0,0,0) }),
    };

    private static readonly Vector2[] FaceUV = { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
    private static readonly int[] TriOrder = { 0, 1, 2, 0, 2, 3 };
                                                                                                      
    private static readonly int[] TriOrderTriangle = { 0, 1, 2 };

                 
                                                                            
                                                                             
                                                                             
                                                                      
                                                                      
                                                                       
                                                                             
                                                                  
                  
                                                                                          
                                                                          
                                                                                             
                                                                              
                                                                                   
                                                                                 
                                                                                   
                                                                              
                                                                                            
    public bool NeedsMesh => _dirty || _vao == 0;

    public void EnsureMesh(Func<Vector3i, BlockType> neighborBlock, Vector3 tint, Func<Vector3i, byte> lightAt, Func<Vector3i, byte> liquidLevelAt, Func<Vector3i, Vector3> waterColorAt)
    {
        if (!_dirty && _vao != 0) return;
        Rebuild(neighborBlock, tint, lightAt, liquidLevelAt, waterColorAt);
        _dirty = false;
    }

    private void Rebuild(Func<Vector3i, BlockType> neighborBlock, Vector3 tint, Func<Vector3i, byte> lightAt, Func<Vector3i, byte> liquidLevelAt, Func<Vector3i, Vector3> waterColorAt)
    {
                                                                                 
                                                                                 
                                                                        
        var vertices = _vertexScratch;
        var liquidVertices = _liquidVertexScratch;
        var indices = _indexScratch;
        var liquidIndices = _liquidIndexScratch;
        vertices.Clear();
        liquidVertices.Clear();
        indices.Clear();
        liquidIndices.Clear();

                                                                             
                                                                        
                                                                              
                                                                                 
                                                                      
                                                                                 
                                                                                   
                                                                                   
                                                                                   
                                                                               
                                                                                     
        var snapshot = SnapshotData();
        var localLookup = snapshot.Blocks;
        var shapeLookup = snapshot.Shapes;

                                                                              
                                                                            
                                                                              
                                                                     
                                                                            
                                                                               
                                                                             
                                                                            
                                                                               
                                                                      
                                                                          
                                                                                
                                 
        var waterColorCache = new Dictionary<Vector2i, Vector3>();

        BlockType LocalOrWorldBlock(Vector3i neighborPos)
        {
            int localX = neighborPos.X - MinX;
            int localZ = neighborPos.Z - MinZ;
            if (localX >= 0 && localX < World.ChunkSize && localZ >= 0 && localZ < World.ChunkSize)
                return localLookup.TryGetValue(neighborPos, out var t) ? t : BlockType.Air;

                                                                               
                                                                 
            return neighborBlock(neighborPos);
        }

                                                                             
                                                                     
                                                                     
                                                                           
                                                                            
                                                                              
                                                                            
                                                                      
                                                                                        
                                                                                      
                                                                                          
                                                                             
        void EmitFace(VertexBuffer target, IndexBuffer targetIndices, Vector3 normal, IReadOnlyList<Vector3> corners, Vector3i pos, Vector3 color, bool unlit,
            float uMin, float vMin, float uMax, float vMax, float light, bool waterfall = false)
        {
                                                                                     
                                                                                         
                                                                                           
                                                                                         
                                                                                          
            int baseIndex = target.Count / 14;
            for (int i = 0; i < corners.Count; i++)
            {
                var corner = corners[i] + new Vector3(pos.X, pos.Y, pos.Z);
                var faceUv = FaceUV[i];
                float u = uMin + faceUv.X * (uMax - uMin);
                float v = vMin + faceUv.Y * (vMax - vMin);

                target.Add(corner.X); target.Add(corner.Y); target.Add(corner.Z);
                target.Add(normal.X); target.Add(normal.Y); target.Add(normal.Z);
                target.Add(color.X); target.Add(color.Y); target.Add(color.Z);
                target.Add(unlit ? 1f : 0f);
                target.Add(u); target.Add(v);
                target.Add(light);
                target.Add(waterfall ? 1f : 0f);
            }

            var order = corners.Count == 3 ? TriOrderTriangle : TriOrder;
            foreach (var o in order)
                targetIndices.Add(baseIndex + o);
        }

                                                                                     
                                                                                   
                                                                                      
                                                                                   
                                                                                     
                                                                                     
                                                                                   
                                         
        var waterCornerCache = new Dictionary<Vector3i, float>();

        foreach (var (pos, type) in localLookup)
        {
            bool unlit = BlockData.IsUnlit(type);
                                                                                
                                                                                
                                                                                
                                            
            var baseColor = type == BlockType.Water
                ? GetWaterColorCached(pos)
                : BlockData.GetColor(type);
            var color = unlit ? baseColor : baseColor * tint;
            var target = type == BlockType.Water ? liquidVertices : vertices;
            var targetIndices = type == BlockType.Water ? liquidIndices : indices;
                                                                                 
                                                                               
                                                                            
                                                                              
                                                   

            var shape = shapeLookup.TryGetValue(pos, out var storedShape) ? storedShape : BlockShape.Cube;
            if (type == BlockType.Water && shape == BlockShape.Cube)
            {
                                                                                       
                                                                                          
                                                                                         
                                                                                          
                                                                                  
                                                                            
                                                                                           
                foreach (var (normal, corners) in Faces)
                {
                    var neighborPos = pos + normal;
                    if (BlockData.ShouldCullFace(type, LocalOrWorldBlock(neighborPos), normal)) continue;
                    float light = type == BlockType.Snow ? -1f : lightAt(neighborPos) / (float)World.MaxLightLevel;
                    var (uMin, vMin, uMax, vMax) = BlockTextures.GetUvRect(type, normal);
                    var belowPos = pos + new Vector3i(0, -1, 0);
                    bool belowWater = LocalOrWorldBlock(belowPos) == BlockType.Water;
                    bool waterfall = normal.Y == 0 && (belowWater || LocalOrWorldBlock(belowPos) == BlockType.Air);

                    Vector3[]? sloped = null;
                    for (int i = 0; i < corners.Length; i++)
                    {
                        if (corners[i].Y < 0.5f && belowWater && normal.Y == 0)
                        {
                            sloped ??= (Vector3[])corners.Clone();
                            int lowerCx = corners[i].X > 0.5f ? 1 : 0;
                            int lowerCz = corners[i].Z > 0.5f ? 1 : 0;
                            float lowerHeight = WaterCornerHeight(belowPos, lowerCx, lowerCz, LocalOrWorldBlock, liquidLevelAt, waterCornerCache);
                            sloped[i] = new Vector3(corners[i].X, -1f + lowerHeight, corners[i].Z);
                            continue;
                        }
                        if (corners[i].Y < 0.5f) continue;                                        
                        sloped ??= (Vector3[])corners.Clone();
                        int cx = corners[i].X > 0.5f ? 1 : 0;
                        int cz = corners[i].Z > 0.5f ? 1 : 0;
                        float h = WaterCornerHeight(pos, cx, cz, LocalOrWorldBlock, liquidLevelAt, waterCornerCache);
                        sloped[i] = new Vector3(corners[i].X, h, corners[i].Z);
                    }

                    EmitFace(target, targetIndices, normal, sloped ?? corners, pos, color, unlit, uMin, vMin, uMax, vMax, light, waterfall);
                }
            }
            else if (shape == BlockShape.Cube)
            {
                                                                                    
                                                                          
                                                             
                foreach (var (normal, corners) in Faces)
                {
                    var neighborPos = pos + normal;
                    if (BlockData.ShouldCullFace(type, LocalOrWorldBlock(neighborPos), normal)) continue;
                    float light = type == BlockType.Snow ? -1f : lightAt(neighborPos) / (float)World.MaxLightLevel;
                    var (uMin, vMin, uMax, vMax) = BlockTextures.GetUvRect(type, normal);
                    EmitFace(target, targetIndices, normal, corners, pos, color, unlit, uMin, vMin, uMax, vMax, light);
                }
            }
            else
            {
                foreach (var face in BlockShapeGeometry.FacesFor(shape))
                {
                    var neighborPos = face.CullNormal.HasValue ? pos + face.CullNormal.Value : pos;
                    if (face.CullNormal.HasValue && BlockData.ShouldCullFace(type, LocalOrWorldBlock(neighborPos), face.CullNormal.Value)) continue;
                    float light = type == BlockType.Snow ? -1f : lightAt(neighborPos) / (float)World.MaxLightLevel;
                                                                                        
                    var (uMin, vMin, uMax, vMax) = BlockTextures.GetUvRect(type, face.Normal);
                    EmitFace(target, targetIndices, face.Normal, face.Corners, pos, color, unlit, uMin, vMin, uMax, vMax, light);
                }
            }
        }

        Vector3 GetWaterColorCached(Vector3i blockPos)
        {
            var key = new Vector2i(blockPos.X, blockPos.Z);
            if (waterColorCache.TryGetValue(key, out var cached)) return cached;
            cached = waterColorAt(blockPos);
            waterColorCache[key] = cached;
            return cached;
        }

        _vertexCount = vertices.Count / 14;
        _indexCount = indices.Count;
        UploadBuffer(ref _vao, ref _vbo, ref _ebo, vertices, indices);

        _liquidVertexCount = liquidVertices.Count / 14;
        _liquidIndexCount = liquidIndices.Count;
        UploadBuffer(ref _liquidVao, ref _liquidVbo, ref _liquidEbo, liquidVertices, liquidIndices);
    }

                                                                                       
                                                                                    
                                                                                       
                                                       
    private const float WaterSourceTopHeight = 0.92f;
    private const float WaterEdgeTopHeight = 0.5f;

                 
                                                                                   
                                                                                        
                                                                                       
                                                                                         
                                                                                    
                                                                                   
                                                                                   
                                                                                   
                                                                              
                                   
                  
    private static float WaterCornerHeight(Vector3i pos, int cx, int cz, Func<Vector3i, BlockType> neighborBlock,
        Func<Vector3i, byte> liquidLevelAt, Dictionary<Vector3i, float> cache)
    {
        var cornerKey = new Vector3i(pos.X + cx, pos.Y, pos.Z + cz);
        if (cache.TryGetValue(cornerKey, out var cachedHeight)) return cachedHeight;

        float sum = 0f;
        int count = 0;
        for (int dx = cx - 1; dx <= cx; dx++)
        {
            for (int dz = cz - 1; dz <= cz; dz++)
            {
                var cellPos = new Vector3i(pos.X + dx, pos.Y, pos.Z + dz);
                if (neighborBlock(cellPos) != BlockType.Water) continue;

                                                                                        
                                                                                  
                                                                                    
                                                                                        
                var abovePos = cellPos + new Vector3i(0, 1, 0);
                float amount = neighborBlock(abovePos) == BlockType.Water
                    ? 1f
                    : WaterLevelToAmount(liquidLevelAt(cellPos));
                sum += amount;
                count++;
            }
        }

                                                                                       
                                                                                    
        float fillAmount = count == 0 ? WaterLevelToAmount(liquidLevelAt(pos)) : sum / count;
        float height = WaterEdgeTopHeight + (WaterSourceTopHeight - WaterEdgeTopHeight) * fillAmount;

        cache[cornerKey] = height;
        return height;
    }

                                                                                              
                                                                                              
    private static float WaterLevelToAmount(byte level)
    {
        return 1f - Math.Clamp(level / (float)World.MaxFlowLevel, 0f, 1f);
    }

                 
                                                                              
                                                                          
                                                                        
                                                                  
                  
    private static void UploadBuffer(ref int vao, ref int vbo, ref int ebo, VertexBuffer vertices, IndexBuffer indices)
    {
        if (vao == 0)
        {
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();
        }

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                                                                                      
                                                                                          
                                                                                           
                                                                                   
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.Data, BufferUsageHint.DynamicDraw);

                                                                                        
                                                                                           
                                                                                          
                                                       
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), indices.Data, BufferUsageHint.DynamicDraw);

        int stride = 14 * sizeof(float);
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
        GL.VertexAttribPointer(6, 1, VertexAttribPointerType.Float, false, stride, 13 * sizeof(float));
        GL.EnableVertexAttribArray(6);
    }


    public void Render()
    {
        if (_indexCount == 0 || _vao == 0) return;
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
    }

                                                                                   
                                                                                    
                                                                                            
    public void RenderLiquid()
    {
        if (_liquidIndexCount == 0 || _liquidVao == 0) return;
        GL.BindVertexArray(_liquidVao);
        GL.DrawElements(PrimitiveType.Triangles, _liquidIndexCount, DrawElementsType.UnsignedInt, 0);
    }

                 
                                                                              
                                                                             
                                                                          
                                                                                
                                                                           
                                                                           
                        
                  
    public void ReleaseGpu()
    {
        if (_vao != 0) { GL.DeleteVertexArray(_vao); _vao = 0; }
        if (_vbo != 0) { GL.DeleteBuffer(_vbo); _vbo = 0; }
        if (_ebo != 0) { GL.DeleteBuffer(_ebo); _ebo = 0; }
        if (_liquidVao != 0) { GL.DeleteVertexArray(_liquidVao); _liquidVao = 0; }
        if (_liquidVbo != 0) { GL.DeleteBuffer(_liquidVbo); _liquidVbo = 0; }
        if (_liquidEbo != 0) { GL.DeleteBuffer(_liquidEbo); _liquidEbo = 0; }
        _vertexCount = 0;
        _indexCount = 0;
        _liquidVertexCount = 0;
        _liquidIndexCount = 0;
        _dirty = true;
    }
}



