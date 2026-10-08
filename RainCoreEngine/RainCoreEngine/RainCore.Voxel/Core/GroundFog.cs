using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using RainCoreGraphics;

namespace RainCore;

             
                                                                    
                                                                      
                                                                            
                                                                   
                                                                          
                                                                           
                                                                         
                                                                 
                                                                        
                                     
              
public class GroundFog
{
    private const int LayerCount = 3;
    private const int PatchesPerLayer = 22;
    private const int PatchCount = LayerCount * PatchesPerLayer;

    private const float BoxRadius = 18f;
    private const float PatchWidth = 7f;
    private const float WidthJitter = 3.5f;
    private const float PatchHeightMul = 1.2f;                                                                             

    private const float GroundOffset = 0.2f;

                                                                          
                                            
    private static readonly float[] LayerBaseHeight = { 0.2f, 1.1f, 2.2f };
    private static readonly float[] LayerHeightJitter = { 0.5f, 0.7f, 0.9f };
    private static readonly float[] LayerBaseAlpha = { 0.30f, 0.20f, 0.12f };                                                                                                                                

    private const float DriftSpeed = 0.18f;
    private const float WindSpeed = 0.12f;
    private static readonly Vector2 WindDir = Vector2.Normalize(new Vector2(1f, 0.4f));

    private const float AlphaPulseSpeed = 0.5f;
    private const float AlphaPulseAmount = 0.04f;

                                                                             
                                                                            
                     
    private static readonly Vector3 FogColor = new(0.62f, 0.64f, 0.68f);

    private readonly Vector3[] _positions = new Vector3[PatchCount];
    private readonly int[] _layer = new int[PatchCount];
    private readonly float[] _heightOffset = new float[PatchCount];
    private readonly float[] _width = new float[PatchCount];
    private readonly float[] _driftPhase = new float[PatchCount];
    private readonly float[] _alphaPhase = new float[PatchCount];
    private readonly float[] _seed = new float[PatchCount];                                                                                                                                                      
    private readonly int[] _groundX = new int[PatchCount];
    private readonly int[] _groundZ = new int[PatchCount];
                                                                                    
                                                                                       
                                                                                       
                                                                                  
                                                                                  
                                                                                                    
    private readonly float[] _biomeFactor = new float[PatchCount];
    private const int FloatsPerVertex = 10;                                                                                  
    private readonly float[] _vertexData = new float[PatchCount * 6 * FloatsPerVertex];

    private Shader _shader = null!;
    private int _vao;
    private int _vbo;
    private bool _active = true;
    private float _time;                                                                                  

                                                                                          
                                                                                           
                                                                                   
                                                                                            
                                                                                                       
    private float _intensity = 1f;

                                                                                                            
    public void Build(World world, Vector3 cameraPos)
    {
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _shader = new Shader(Path.Combine(shadersDir, "fog.vert"), Path.Combine(shadersDir, "fog.frag"));

        var rng = new Random(54321);
        for (int i = 0; i < PatchCount; i++)
        {
            int layer = i % LayerCount;
            _layer[i] = layer;
            var (x, z, hOffset) = RandomPatchOffsets(cameraPos, rng, layer);
            _heightOffset[i] = hOffset;
            _groundX[i] = (int)MathF.Floor(x);
            _groundZ[i] = (int)MathF.Floor(z);
            float groundY = world.GetSurfaceHeight(_groundX[i], _groundZ[i]);
            _positions[i] = new Vector3(x, groundY + 1f + hOffset, z);
            _width[i] = PatchWidth + (float)rng.NextDouble() * WidthJitter;
            _driftPhase[i] = (float)rng.NextDouble() * MathF.Tau;
            _alphaPhase[i] = (float)rng.NextDouble() * MathF.Tau;
            _seed[i] = (float)rng.NextDouble();
            _biomeFactor[i] = ComputeBiomeFactor(world, x, z);
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
        GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, stride, 7 * sizeof(float));
        GL.EnableVertexAttribArray(3);
        GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, stride, 9 * sizeof(float));
        GL.EnableVertexAttribArray(4);
    }

    public void SetActive(bool active)
    {
        _active = active;
    }

                                                                                     
                                                                                              
    public void SetIntensity(float intensity)
    {
        _intensity = Math.Clamp(intensity, 0f, 1f);
    }

    private static (float x, float z, float heightOffset) RandomPatchOffsets(Vector3 cameraPos, Random rng, int layer)
    {
        float x = cameraPos.X + ((float)rng.NextDouble() * 2f - 1f) * BoxRadius;
        float z = cameraPos.Z + ((float)rng.NextDouble() * 2f - 1f) * BoxRadius;
        float hOffset = GroundOffset + LayerBaseHeight[layer] + (float)rng.NextDouble() * LayerHeightJitter[layer];
        return (x, z, hOffset);
    }

                                                                                         
                                                                                    
                                                                                       
                                                          
    private static float ComputeBiomeFactor(World world, float x, float z)
    {
        if (world.Kind != WorldKind.ProceduralIsland) return 1f;

        int ix = (int)MathF.Floor(x);
        int iz = (int)MathF.Floor(z);
        float winterFactor = world.GetWinterFactorAt(ix, iz);
        float waterFactor = world.IsNearWaterAt(ix, iz) ? 1f : 0f;
        return MathF.Max(winterFactor, waterFactor);
    }

                                                                                                                                    
    public void Update(float dt, World world, Vector3 cameraPos, Random rng)
    {
        if (!_active) return;

        _time += dt;

        for (int i = 0; i < PatchCount; i++)
        {
            ref var pos = ref _positions[i];
            _driftPhase[i] += dt;
            _alphaPhase[i] += dt * AlphaPulseSpeed;

            pos.X += (MathF.Sin(_driftPhase[i]) * DriftSpeed + WindDir.X * WindSpeed) * dt;
            pos.Z += (MathF.Cos(_driftPhase[i] * 0.8f) * DriftSpeed + WindDir.Y * WindSpeed) * dt;

            bool driftedAway = MathF.Abs(pos.X - cameraPos.X) > BoxRadius || MathF.Abs(pos.Z - cameraPos.Z) > BoxRadius;
            if (driftedAway)
            {
                var (nx, nz, nh) = RandomPatchOffsets(cameraPos, rng, _layer[i]);
                pos.X = nx;
                pos.Z = nz;
                _heightOffset[i] = nh;
                _biomeFactor[i] = ComputeBiomeFactor(world, nx, nz);
            }

                                                                            
                                                                         
            int groundX = (int)MathF.Floor(pos.X);
            int groundZ = (int)MathF.Floor(pos.Z);
            if (groundX != _groundX[i] || groundZ != _groundZ[i])
            {
                _groundX[i] = groundX;
                _groundZ[i] = groundZ;
                pos.Y = world.GetSurfaceHeight(groundX, groundZ) + 1f + _heightOffset[i];
            }
        }
    }

                 
                                                                                                                     
                                                                                                      
                                                                                                         
                                                                                                       
                                                                                                             
                                                                                                        
                  
    public void Render(Matrix4 view, Matrix4 projection, Vector3 cameraRight, Vector3 cameraUp,
        int depthTexture, Vector2 screenSize, float nearPlane, float farPlane)
    {
        if (!_active) return;

        for (int i = 0; i < PatchCount; i++)
        {
            var pos = _positions[i];
            float halfW = _width[i] * 0.5f;
            float halfH = halfW * PatchHeightMul;
            float alpha = (LayerBaseAlpha[_layer[i]] + MathF.Sin(_alphaPhase[i]) * AlphaPulseAmount) * _intensity * _biomeFactor[i];
            if (alpha < 0f) alpha = 0f;

                                                                            
                                                                            
                                                                         
                             
            Vector3 right = cameraRight * halfW;
            Vector3 up = cameraUp * halfH;

            Vector3 p0 = pos - right - up;
            Vector3 p1 = pos + right - up;
            Vector3 p2 = pos + right + up;
            Vector3 p3 = pos - right + up;

            float seed = _seed[i];

            int b = i * 6 * FloatsPerVertex;
            WriteVertex(b, p0, alpha, -1f, -1f, seed);
            WriteVertex(b + FloatsPerVertex, p1, alpha, 1f, -1f, seed);
            WriteVertex(b + FloatsPerVertex * 2, p2, alpha, 1f, 1f, seed);
            WriteVertex(b + FloatsPerVertex * 3, p0, alpha, -1f, -1f, seed);
            WriteVertex(b + FloatsPerVertex * 4, p2, alpha, 1f, 1f, seed);
            WriteVertex(b + FloatsPerVertex * 5, p3, alpha, -1f, 1f, seed);
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
        _shader.SetFloat("uTime", _time);
        _shader.SetFloat("nearPlane", nearPlane);
        _shader.SetFloat("farPlane", farPlane);
        _shader.SetVector2("screenSize", screenSize);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, depthTexture);
        _shader.SetInt("depthTex", 1);
        GL.ActiveTexture(TextureUnit.Texture0);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, PatchCount * 6);

        GL.DepthMask(true);
        GL.Disable(EnableCap.Blend);
    }

    private void WriteVertex(int offset, Vector3 p, float alpha, float u, float v, float seed)
    {
        _vertexData[offset] = p.X;
        _vertexData[offset + 1] = p.Y;
        _vertexData[offset + 2] = p.Z;
        _vertexData[offset + 3] = FogColor.X;
        _vertexData[offset + 4] = FogColor.Y;
        _vertexData[offset + 5] = FogColor.Z;
        _vertexData[offset + 6] = alpha;
        _vertexData[offset + 7] = u;
        _vertexData[offset + 8] = v;
        _vertexData[offset + 9] = seed;
    }
}
