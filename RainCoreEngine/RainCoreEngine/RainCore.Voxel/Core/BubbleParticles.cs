using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace RainCore;

             
                                                                            
                                                                            
                                                                            
                                                                               
                                                                             
                                                                          
   
                                                                               
                                                                               
                                                                              
                                                                              
                                                                           
                                                                       
                                                                             
   
                                                                           
                                                                             
                                                                               
                                                                             
                     
              
public class BubbleParticles
{
    private const int ParticleCount = 60;
    private const float SpawnRadius = 1.1f;                                                             
    private const float SpawnBelow = 0.9f;                                                              
    private const float RiseSpeed = 1.4f;
    private const float DriftSpeed = 0.6f;
    private const float BubbleLifetime = 4f;                                         
    private const float PointSizePx = 2.6f;
    private const float AmbientSpawnChance = 0.35f;                                                         

    private readonly Vector3[] _positions = new Vector3[ParticleCount];
    private readonly float[] _phase = new float[ParticleCount];
    private readonly float[] _life = new float[ParticleCount];
    private readonly bool[] _alive = new bool[ParticleCount];
    private readonly float[] _vertexData = new float[ParticleCount * 10];

    private int _vao;
    private int _vbo;
    private bool _active;                                                                      

                                                                                          
                                                                         
    public void Build()
    {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexData.Length * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

        int stride = 10 * sizeof(float);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, stride, 9 * sizeof(float));
        GL.EnableVertexAttribArray(3);
    }

                                                                                        
                                                                                     
                                                                                     
                                                         
    public void SetActive(bool active) => _active = active;

                                                                                          
                                                                                     
                                                                           
    public void Burst(Vector3 aroundPosition, Random rng, int count = 14)
    {
        int spawned = 0;
        for (int i = 0; i < ParticleCount && spawned < count; i++)
        {
            if (_alive[i]) continue;
            Spawn(i, aroundPosition, rng);
            spawned++;
        }
    }

    private void Spawn(int i, Vector3 aroundPosition, Random rng)
    {
        float ox = ((float)rng.NextDouble() * 2f - 1f) * SpawnRadius;
        float oz = ((float)rng.NextDouble() * 2f - 1f) * SpawnRadius;
        float oy = -(float)rng.NextDouble() * SpawnBelow;
        _positions[i] = aroundPosition + new Vector3(ox, oy, oz);
        _phase[i] = (float)rng.NextDouble() * MathF.Tau;
        _life[i] = 0f;
        _alive[i] = true;
    }

                                                                                         
                                                                                       
                                                                                     
    public void Update(float dt, World world, Vector3 playerPosition, Random rng)
    {
        for (int i = 0; i < ParticleCount; i++)
        {
            if (!_alive[i])
            {
                                                                                       
                                                                                       
                                                                                     
                if (_active && rng.NextDouble() < AmbientSpawnChance * dt)
                    Spawn(i, playerPosition, rng);
                continue;
            }

            ref var pos = ref _positions[i];
            _phase[i] += dt;
            _life[i] += dt;
            pos.Y += RiseSpeed * dt;
            pos.X += MathF.Sin(_phase[i] * 3.1f) * DriftSpeed * dt;
            pos.Z += MathF.Cos(_phase[i] * 2.3f) * DriftSpeed * dt;

            var cell = new Vector3i((int)MathF.Floor(pos.X), (int)MathF.Floor(pos.Y), (int)MathF.Floor(pos.Z));
            bool reachedSurface = world.GetBlock(cell) != BlockType.Water;                               
            bool tooOld = _life[i] > BubbleLifetime;

            if (reachedSurface || tooOld)
                _alive[i] = false;                                                                         
        }
    }

    public void Render()
    {
        int count = 0;
        for (int i = 0; i < ParticleCount; i++)
        {
            if (!_alive[i]) continue;
            int b = count * 10;
            var pos = _positions[i];
            _vertexData[b] = pos.X; _vertexData[b + 1] = pos.Y; _vertexData[b + 2] = pos.Z;
            _vertexData[b + 3] = 0f; _vertexData[b + 4] = 1f; _vertexData[b + 5] = 0f;                            
            _vertexData[b + 6] = 0.85f; _vertexData[b + 7] = 0.93f; _vertexData[b + 8] = 1f;                                
            _vertexData[b + 9] = 1f;                                         
            count++;
        }

        if (count == 0) return;

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, count * 10 * sizeof(float), _vertexData);

        GL.PointSize(PointSizePx);
        GL.DrawArrays(PrimitiveType.Points, 0, count);
    }
}
