using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using RainCoreGraphics;

namespace RainCore;

             
                                                                       
                                                                              
                                                                             
                                                                        
                                                                         
                                                                           
                                                                       
                                                                          
   
                                                                                 
                                                                                  
                                                                              
                            
              
public class GhostRenderer
{
    private const int MaxGhosts = 4;

                                                                            
                                                                           
    private const float FadeInSeconds = 1.2f;
    private const float HoldSeconds = 3.5f;
    private const float FadeOutSeconds = 1.8f;
    private const float LifetimeSeconds = FadeInSeconds + HoldSeconds + FadeOutSeconds;

    private const float DriftSpeed = 0.15f;                                              
    private const float BobHeight = 0.15f;
    private const float Width = 0.9f;
    private const float HeightMul = 2.6f;                                                                  
    private const float MaxAlpha = 0.5f;

                                                                                
                                                           
    private static readonly Vector3 GhostColor = new(0.80f, 0.85f, 0.92f);

    private sealed class GhostInstance
    {
        public Vector3 BasePos;
        public float Age;
        public float DriftPhase;
    }

    private const int FloatsPerVertex = 10;                                                                              
    private readonly List<GhostInstance> _instances = new();
    private readonly float[] _vertexData = new float[MaxGhosts * 6 * FloatsPerVertex];

    private Shader _shader = null!;
    private int _vao;
    private int _vbo;

                                                                                    
                                                                                       
                                                                                        
                                                                                        
                                                                                       
                                                                                              
    public void Build()
    {
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _shader = new Shader(Path.Combine(shadersDir, "fog.vert"), Path.Combine(shadersDir, "fog.frag"));

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

                                                                                              
                                                                                         
                                                                                       
                                                                            
    public void Spawn(Vector3 worldPosition, Random rng)
    {
        if (_instances.Count >= MaxGhosts) return;
        _instances.Add(new GhostInstance
        {
            BasePos = worldPosition,
            Age = 0f,
            DriftPhase = (float)rng.NextDouble() * MathF.Tau,
        });
    }

    public void Update(float dt)
    {
        for (int i = _instances.Count - 1; i >= 0; i--)
        {
            var g = _instances[i];
            g.Age += dt;
            g.DriftPhase += dt;
            if (g.Age >= LifetimeSeconds)
                _instances.RemoveAt(i);
        }
    }

                                                                                           
                                                                                  
    private static float LifecycleAlpha(float age)
    {
        if (age < FadeInSeconds)
            return age / FadeInSeconds;
        if (age < FadeInSeconds + HoldSeconds)
            return 1f;
        float fadeOutT = (age - FadeInSeconds - HoldSeconds) / FadeOutSeconds;
        return 1f - Math.Clamp(fadeOutT, 0f, 1f);
    }

                 
                                                                                       
                                                                                         
                                                                                              
                                                                   
                  
    public void Render(Matrix4 view, Matrix4 projection, Vector3 cameraRight, Vector3 cameraUp,
        int depthTexture, Vector2 screenSize, float nearPlane, float farPlane)
    {
        if (_instances.Count == 0) return;

        for (int i = 0; i < _instances.Count; i++)
        {
            var g = _instances[i];

                                                                               
                                                                        
            var pos = g.BasePos;
            pos.X += MathF.Sin(g.DriftPhase) * DriftSpeed * 0.3f;
            pos.Z += MathF.Cos(g.DriftPhase * 0.7f) * DriftSpeed * 0.3f;
            pos.Y += MathF.Sin(g.DriftPhase * 1.3f) * BobHeight + Width * HeightMul * 0.5f;                                         

            float halfW = Width * 0.5f;
            float halfH = halfW * HeightMul;
            float alpha = MaxAlpha * LifecycleAlpha(g.Age);

            Vector3 right = cameraRight * halfW;
            Vector3 up = cameraUp * halfH;

            Vector3 p0 = pos - right - up;
            Vector3 p1 = pos + right - up;
            Vector3 p2 = pos + right + up;
            Vector3 p3 = pos - right + up;

                                                                             
                                                                           
                                                                                 
            float seed = (g.DriftPhase % MathF.Tau) / MathF.Tau;

            int b = i * 6 * FloatsPerVertex;
            WriteVertex(b, p0, alpha, -1f, -1f, seed);
            WriteVertex(b + FloatsPerVertex, p1, alpha, 1f, -1f, seed);
            WriteVertex(b + FloatsPerVertex * 2, p2, alpha, 1f, 1f, seed);
            WriteVertex(b + FloatsPerVertex * 3, p0, alpha, -1f, -1f, seed);
            WriteVertex(b + FloatsPerVertex * 4, p2, alpha, 1f, 1f, seed);
            WriteVertex(b + FloatsPerVertex * 5, p3, alpha, -1f, 1f, seed);
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _instances.Count * 6 * FloatsPerVertex * sizeof(float), _vertexData);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(false);

        _shader.Use();
        _shader.SetMatrix4("model", Matrix4.Identity);
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);
        _shader.SetFloat("uTime", 0f);                                                                                                                       
        _shader.SetFloat("nearPlane", nearPlane);
        _shader.SetFloat("farPlane", farPlane);
        _shader.SetVector2("screenSize", screenSize);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, depthTexture);
        _shader.SetInt("depthTex", 1);
        GL.ActiveTexture(TextureUnit.Texture0);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, _instances.Count * 6);

        GL.DepthMask(true);
        GL.Disable(EnableCap.Blend);
    }

    private void WriteVertex(int offset, Vector3 p, float alpha, float u, float v, float seed)
    {
        _vertexData[offset] = p.X;
        _vertexData[offset + 1] = p.Y;
        _vertexData[offset + 2] = p.Z;
        _vertexData[offset + 3] = GhostColor.X;
        _vertexData[offset + 4] = GhostColor.Y;
        _vertexData[offset + 5] = GhostColor.Z;
        _vertexData[offset + 6] = alpha;
        _vertexData[offset + 7] = u;
        _vertexData[offset + 8] = v;
        _vertexData[offset + 9] = seed;
    }
}
