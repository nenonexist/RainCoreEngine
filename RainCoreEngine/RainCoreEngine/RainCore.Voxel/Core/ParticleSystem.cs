using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace RainCore;

             
                                                                              
                                                                        
                                                                               
                          
              
public sealed class ParticleSystemConfig
{
    public int ParticleCount;
    public float BoxRadius;                                                          
    public float TopOffset;                                                             
    public float InitialScatterBelow;                                                                                                         
    public float GroundMargin;                                                                     
    public float MaxFallBelowCamera;                                                                                      
    public float FallSpeed;
    public float DriftSpeed;                                                         
    public float PointSizePx;
    public Vector3 Color;                                                                
    public int RandomSeed;                                                                                             
    public bool CrossSprite;                                                    

                                                                                           
                                                                                     
                                                                                       
                                                                                   
                                                                                     
                                                                                      
                                                                                     
    public Func<World, Vector3, bool>? BiomeFilter;
}

             
                                                                  
                                                                           
                                                                        
                                                                            
                                                                             
                                                                              
   
                                                                             
                                                                        
                                                                         
                                                                              
             
   
                                                                        
                                                                       
                                                                          
                                                                              
                                                                             
                                                              
              
public sealed class ParticleSystem
{
    private readonly ParticleSystemConfig _config;
    private readonly Vector3[] _positions;
    private readonly float[] _groundY;                                                                
    private readonly float[] _phase;                                                            
    private readonly bool[] _biomeOk;                                                                                  
    private readonly float[] _vertexData;

    private int _vao;
    private int _vbo;
    private bool _active;

                                                                                    
                                                                               
                                                                                        
                                                                                       
                                                                                     
    private float _intensity = 1f;

    public ParticleSystem(ParticleSystemConfig config)
    {
        _config = config;
        _positions = new Vector3[config.ParticleCount];
        _groundY = new float[config.ParticleCount];
        _phase = new float[config.ParticleCount];
        _biomeOk = new bool[config.ParticleCount];
        _vertexData = new float[config.ParticleCount * 11];
    }

                                                                                                                                                                               
    public void Build(World world, Vector3 cameraPos)
    {
        var rng = new Random(_config.RandomSeed);
        for (int i = 0; i < _config.ParticleCount; i++)
        {
            _positions[i] = RandomPointAbove(cameraPos, rng, fullHeight: true);
            _groundY[i] = world.GetSurfaceHeight((int)MathF.Floor(_positions[i].X), (int)MathF.Floor(_positions[i].Z));
            _biomeOk[i] = _config.BiomeFilter?.Invoke(world, _positions[i]) ?? true;
            _phase[i] = (float)rng.NextDouble() * MathF.Tau;
        }

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexData.Length * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

        int stride = 11 * sizeof(float);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, stride, 9 * sizeof(float));
        GL.EnableVertexAttribArray(3);
        GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, stride, 10 * sizeof(float));
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

    private Vector3 RandomPointAbove(Vector3 cameraPos, Random rng, bool fullHeight)
    {
        float x = cameraPos.X + ((float)rng.NextDouble() * 2f - 1f) * _config.BoxRadius;
        float z = cameraPos.Z + ((float)rng.NextDouble() * 2f - 1f) * _config.BoxRadius;
                                                                                
                                                                               
                                                                              
        float y = fullHeight
            ? cameraPos.Y - _config.InitialScatterBelow + (float)rng.NextDouble() * (_config.TopOffset + _config.InitialScatterBelow)
            : cameraPos.Y + _config.TopOffset;
        return new Vector3(x, y, z);
    }

                                                                                                                                                  
    public void Update(float dt, World world, Vector3 cameraPos, Random rng)
    {
        if (!_active || _intensity <= 0f) return;

        for (int i = 0; i < _config.ParticleCount; i++)
        {
            ref var pos = ref _positions[i];
            _phase[i] += dt;
            pos.Y -= _config.FallSpeed * dt;
            pos.X += MathF.Sin(_phase[i]) * _config.DriftSpeed * dt;
            pos.Z += MathF.Cos(_phase[i] * 0.7f) * _config.DriftSpeed * dt;

                                                                                 
                                                                              
                                                                              
                                        
                                                                          
                                                                               
            bool landed = pos.Y < _groundY[i] + _config.GroundMargin
                || pos.Y < cameraPos.Y - _config.MaxFallBelowCamera;
            bool driftedAway = MathF.Abs(pos.X - cameraPos.X) > _config.BoxRadius || MathF.Abs(pos.Z - cameraPos.Z) > _config.BoxRadius;

            if (landed || driftedAway)
            {
                pos = RandomPointAbove(cameraPos, rng, fullHeight: false);
                _groundY[i] = world.GetSurfaceHeight((int)MathF.Floor(pos.X), (int)MathF.Floor(pos.Z));
                _biomeOk[i] = _config.BiomeFilter?.Invoke(world, pos) ?? true;
            }
        }
    }

    public void Render()
    {
        if (!_active) return;

                                                                                   
                                                                                  
                                                                                     
                                                     
          
                                                                                 
                                                                                    
                                                                                     
                                                                       
        int budget = Math.Clamp((int)MathF.Ceiling(_config.ParticleCount * _intensity), _intensity > 0f ? 1 : 0, _config.ParticleCount);

        int drawnCount = 0;
        for (int i = 0; i < budget; i++)
        {
            if (!_biomeOk[i]) continue;

            int b = drawnCount * 11;
            var pos = _positions[i];
            _vertexData[b] = pos.X; _vertexData[b + 1] = pos.Y; _vertexData[b + 2] = pos.Z;
            _vertexData[b + 3] = 0f; _vertexData[b + 4] = 1f; _vertexData[b + 5] = 0f;                                   
            _vertexData[b + 6] = _config.Color.X; _vertexData[b + 7] = _config.Color.Y; _vertexData[b + 8] = _config.Color.Z;
            _vertexData[b + 9] = 1f;                                         
            _vertexData[b + 10] = _config.CrossSprite ? 1f : 0f;
            drawnCount++;
        }

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, drawnCount * 11 * sizeof(float), _vertexData);

        GL.PointSize(_config.PointSizePx);
        GL.DrawArrays(PrimitiveType.Points, 0, drawnCount);
    }
}
