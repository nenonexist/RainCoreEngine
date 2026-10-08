using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using RainCoreGraphics;

namespace RainCore;

             
                                                                
                                                                     
                                                                          
                                                                            
                                                                            
                                               
   
                                                                    
                                                                            
                                                  
                                                                      
                                                                          
                                                                            
              
                                                                           
                                                                     
                                                                   
                             
                                                                   
                                                                           
                                                                
   
                                                                          
                                                                          
                                                                      
                                                                      
                                                                           
                                                                            
                                                                         
                                                                           
            
              
public class CloudLayer
{
    private const int ClusterCount = 28;
    private const int MaxBoxesPerCluster = 3;

                                                                           
                                                                       
    private const float BoxRadius = 380f;

                                                                           
                                                                      
                                                                            
                                                                        
                                                                       
                                                                           
                                                                          
                          
    private const float CloudBaseHeight = 180f;
    private const float CloudHeightJitter = 20f;

                                                                         
                                                              
    private const float WindSpeed = 1.1f;
    private const float SpeedJitter = 0.4f;
    private static readonly Vector2 WindDir = Vector2.Normalize(new Vector2(0.6f, 1f));

                                                                            
                                                                  
    private const float BoxWidthMin = 45f;
    private const float BoxWidthMax = 125f;
    private const float BoxDepthMin = 45f;
    private const float BoxDepthMax = 125f;
    private const float BoxThicknessMin = 10f;
    private const float BoxThicknessMax = 22f;

                                                                                 
    private static readonly Vector3 DayColor = new(0.95f, 0.95f, 0.98f);
    private static readonly Vector3 NightColor = new(0.05f, 0.06f, 0.10f);
    private const float DayAlpha = 0.30f;
    private const float NightAlpha = 0.08f;

                                                                           
                                                                             
                    
    private const float TopFaceMul = 1.0f;
    private const float SideFaceMulNS = 0.85f;
    private const float SideFaceMulEW = 0.78f;
    private const float BottomFaceMul = 0.62f;

                                                                        
                                                                
                                                                        
                                                                        
                                                       
    private static readonly Vector3 StormColor = new(0.90f, 0.92f, 0.95f);
    private const float StormAlphaMul = 1f;
    private const float MaxStormAlpha = 0.78f;

    private struct CloudBox
    {
        public bool Active;
        public Vector3 LocalOffset;                                
        public float Width;
        public float Depth;
        public float Thickness;
    }

    private readonly Vector3[] _clusterCenter = new Vector3[ClusterCount];
    private readonly float[] _heightOffset = new float[ClusterCount];
    private readonly float[] _speed = new float[ClusterCount];
    private readonly CloudBox[] _boxes = new CloudBox[ClusterCount * MaxBoxesPerCluster];

                                                                          
                        
    private const int FloatsPerVertex = 7;
    private const int VertsPerBox = 36;                                                                                                    
    private readonly float[] _vertexData = new float[ClusterCount * MaxBoxesPerCluster * VertsPerBox * FloatsPerVertex];

    private Shader _shader = null!;
    private int _vao;
    private int _vbo;
    private bool _active = true;

                                                                                      
                                                                                         
    private float _dayFactor = 1f;

                                                                      
                                                                          
                                                                            
    private float _weatherFactor;

    public void SetActive(bool active) => _active = active;

    public void SetDayFactor(float dayFactor) => _dayFactor = Math.Clamp(dayFactor, 0f, 1f);

    public void SetWeatherFactor(float weatherFactor) => _weatherFactor = Math.Clamp(weatherFactor, 0f, 1f);

                                                                              
                                                                            
                                                                 
    public void Build(Vector3 cameraPos)
    {
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _shader = new Shader(Path.Combine(shadersDir, "cloud.vert"), Path.Combine(shadersDir, "cloud.frag"));

        var rng = new Random(24681);
        for (int i = 0; i < ClusterCount; i++)
        {
            var (x, z, hOffset) = RandomClusterOffsets(cameraPos, rng);
            _heightOffset[i] = hOffset;
            _clusterCenter[i] = new Vector3(x, CloudBaseHeight + hOffset, z);
            _speed[i] = WindSpeed + ((float)rng.NextDouble() * 2f - 1f) * SpeedJitter;

            GenerateClusterBoxes(i, rng);
        }

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexData.Length * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

        int stride = FloatsPerVertex * sizeof(float);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
    }

                                                                             
                                                                             
                                                                             
                                                                      
                                                                      
                                                  
    private void GenerateClusterBoxes(int clusterIndex, Random rng)
    {
        int baseSlot = clusterIndex * MaxBoxesPerCluster;
        float clusterThickness = BoxThicknessMin + (float)rng.NextDouble() * (BoxThicknessMax - BoxThicknessMin);

        var first = new CloudBox
        {
            Active = true,
            LocalOffset = Vector3.Zero,
            Width = BoxWidthMin + (float)rng.NextDouble() * (BoxWidthMax - BoxWidthMin),
            Depth = BoxDepthMin + (float)rng.NextDouble() * (BoxDepthMax - BoxDepthMin),
            Thickness = clusterThickness,
        };
        _boxes[baseSlot] = first;

        var prev = first;
        for (int slot = 1; slot < MaxBoxesPerCluster; slot++)
        {
                                                                         
                                                                      
                                           
            float chance = slot == 1 ? 0.7f : 0.45f;
            if (rng.NextDouble() > chance)
            {
                _boxes[baseSlot + slot] = default;                  
                continue;
            }

            float width = BoxWidthMin + (float)rng.NextDouble() * (BoxWidthMax - BoxWidthMin);
            float depth = BoxDepthMin + (float)rng.NextDouble() * (BoxDepthMax - BoxDepthMin);
                                                                            
                                                                      
                                                                          
            float thickness = Math.Clamp(clusterThickness + ((float)rng.NextDouble() * 2f - 1f) * 4f, BoxThicknessMin, BoxThicknessMax);

            bool alongX = rng.NextDouble() < 0.5;
            float sign = rng.NextDouble() < 0.5 ? -1f : 1f;
                                                                   
                                                                            
                                                                           
            float overlapFactor = 0.35f + (float)rng.NextDouble() * 0.4f;

            Vector3 offset;
            if (alongX)
            {
                float extent = (prev.Width + width) * 0.5f * overlapFactor;
                offset = new Vector3(sign * extent, 0f, prev.LocalOffset.Z);
            }
            else
            {
                float extent = (prev.Depth + depth) * 0.5f * overlapFactor;
                offset = new Vector3(prev.LocalOffset.X, 0f, sign * extent);
            }

                                                                          
                                                                     
            if (rng.NextDouble() < 0.3)
            {
                offset.Y = ((float)rng.NextDouble() * 2f - 1f) * thickness * 0.4f;
            }

            var box = new CloudBox
            {
                Active = true,
                LocalOffset = offset,
                Width = width,
                Depth = depth,
                Thickness = thickness,
            };
            _boxes[baseSlot + slot] = box;
            prev = box;
        }
    }

    private static (float x, float z, float heightOffset) RandomClusterOffsets(Vector3 cameraPos, Random rng)
    {
        float x = cameraPos.X + ((float)rng.NextDouble() * 2f - 1f) * BoxRadius;
        float z = cameraPos.Z + ((float)rng.NextDouble() * 2f - 1f) * BoxRadius;
        float hOffset = ((float)rng.NextDouble() * 2f - 1f) * CloudHeightJitter;
        return (x, z, hOffset);
    }

                                                                         
                                                                     
                                                                         
                                                                         
                                                                           
                                      
    public void Update(float dt, Vector3 cameraPos, Random rng)
    {
        if (!_active) return;

        for (int i = 0; i < ClusterCount; i++)
        {
            ref var pos = ref _clusterCenter[i];

            pos.X += WindDir.X * _speed[i] * dt;
            pos.Z += WindDir.Y * _speed[i] * dt;

            bool driftedAway = MathF.Abs(pos.X - cameraPos.X) > BoxRadius || MathF.Abs(pos.Z - cameraPos.Z) > BoxRadius;
            if (driftedAway)
            {
                var (nx, nz, nh) = RandomClusterOffsets(cameraPos, rng);
                pos.X = nx;
                pos.Z = nz;
                _heightOffset[i] = nh;
                pos.Y = CloudBaseHeight + nh;
                GenerateClusterBoxes(i, rng);
            }
        }
    }

                                                                                 
                                                                      
                                                                   
                                                                                       
    public void Render(Matrix4 view, Matrix4 projection)
    {
        if (!_active) return;

        Vector3 dayNightColor = Vector3.Lerp(NightColor, DayColor, _dayFactor);
        float alpha = NightAlpha + (DayAlpha - NightAlpha) * _dayFactor;

                                                                                
                                                                           
                                                                          
                                                                          
                                                                      
        Vector3 baseColor = Vector3.Lerp(dayNightColor, StormColor, _weatherFactor);
        float stormCover = Math.Clamp((_weatherFactor - 0.20f) / 0.80f, 0f, 1f);
        alpha = Math.Clamp(alpha * (1f + (StormAlphaMul - 1f) * _weatherFactor)
            + stormCover * 0.66f, 0f, MaxStormAlpha);

        int vertexCursor = 0;
        for (int c = 0; c < ClusterCount; c++)
        {
            Vector3 center = _clusterCenter[c];
            int baseSlot = c * MaxBoxesPerCluster;

            for (int slot = 0; slot < MaxBoxesPerCluster; slot++)
            {
                var box = _boxes[baseSlot + slot];
                if (!box.Active)
                {
                                                                           
                                                                        
                                                                          
                                                              
                    vertexCursor = WriteDegenerateBox(vertexCursor);
                    continue;
                }

                Vector3 boxCenter = center + box.LocalOffset;
                float halfW = box.Width * 0.5f;
                float halfD = box.Depth * 0.5f;
                float halfT = box.Thickness * 0.5f;

                float minX = boxCenter.X - halfW, maxX = boxCenter.X + halfW;
                float minY = boxCenter.Y - halfT, maxY = boxCenter.Y + halfT;
                float minZ = boxCenter.Z - halfD, maxZ = boxCenter.Z + halfD;

                vertexCursor = WriteBox(vertexCursor, minX, maxX, minY, maxY, minZ, maxZ, baseColor, alpha);
            }
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _vertexData.Length * sizeof(float), _vertexData);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(false);

        _shader.Use();
        _shader.SetMatrix4("model", Matrix4.Identity);
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, ClusterCount * MaxBoxesPerCluster * VertsPerBox);

        GL.DepthMask(true);
        GL.Disable(EnableCap.Blend);
    }

    private int WriteBox(int vertexCursor, float minX, float maxX, float minY, float maxY, float minZ, float maxZ, Vector3 baseColor, float alpha)
    {
                                               
        vertexCursor = WriteQuad(vertexCursor,
            new Vector3(minX, maxY, minZ), new Vector3(maxX, maxY, minZ),
            new Vector3(maxX, maxY, maxZ), new Vector3(minX, maxY, maxZ),
            baseColor * TopFaceMul, alpha);

                                                                             
                                            
        vertexCursor = WriteQuad(vertexCursor,
            new Vector3(minX, minY, minZ), new Vector3(maxX, minY, minZ),
            new Vector3(maxX, minY, maxZ), new Vector3(minX, minY, maxZ),
            baseColor * BottomFaceMul, alpha);

                                                                        
                                                                   
                                                                             
        vertexCursor = WriteQuad(vertexCursor,
            new Vector3(minX, minY, minZ), new Vector3(maxX, minY, minZ),
            new Vector3(maxX, maxY, minZ), new Vector3(minX, maxY, minZ),
            baseColor * SideFaceMulNS, alpha);

        vertexCursor = WriteQuad(vertexCursor,
            new Vector3(minX, minY, maxZ), new Vector3(maxX, minY, maxZ),
            new Vector3(maxX, maxY, maxZ), new Vector3(minX, maxY, maxZ),
            baseColor * SideFaceMulNS, alpha);

        vertexCursor = WriteQuad(vertexCursor,
            new Vector3(maxX, minY, minZ), new Vector3(maxX, minY, maxZ),
            new Vector3(maxX, maxY, maxZ), new Vector3(maxX, maxY, minZ),
            baseColor * SideFaceMulEW, alpha);

        vertexCursor = WriteQuad(vertexCursor,
            new Vector3(minX, minY, minZ), new Vector3(minX, minY, maxZ),
            new Vector3(minX, maxY, maxZ), new Vector3(minX, maxY, minZ),
            baseColor * SideFaceMulEW, alpha);

        return vertexCursor;
    }

    private int WriteDegenerateBox(int vertexCursor)
    {
        for (int i = 0; i < 6; i++)
        {
            vertexCursor = WriteQuad(vertexCursor, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, 0f);
        }
        return vertexCursor;
    }

                                                                       
                                                                       
                                                                     
                                                                 
                                                                            
    private int WriteQuad(int vertexCursor, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 color, float alpha)
    {
        WriteVertex(vertexCursor, p0, color, alpha); vertexCursor++;
        WriteVertex(vertexCursor, p1, color, alpha); vertexCursor++;
        WriteVertex(vertexCursor, p2, color, alpha); vertexCursor++;
        WriteVertex(vertexCursor, p0, color, alpha); vertexCursor++;
        WriteVertex(vertexCursor, p2, color, alpha); vertexCursor++;
        WriteVertex(vertexCursor, p3, color, alpha); vertexCursor++;
        return vertexCursor;
    }

    private void WriteVertex(int vertexIndex, Vector3 p, Vector3 color, float alpha)
    {
        int offset = vertexIndex * FloatsPerVertex;
        _vertexData[offset] = p.X;
        _vertexData[offset + 1] = p.Y;
        _vertexData[offset + 2] = p.Z;
        _vertexData[offset + 3] = color.X;
        _vertexData[offset + 4] = color.Y;
        _vertexData[offset + 5] = color.Z;
        _vertexData[offset + 6] = alpha;
    }
}
