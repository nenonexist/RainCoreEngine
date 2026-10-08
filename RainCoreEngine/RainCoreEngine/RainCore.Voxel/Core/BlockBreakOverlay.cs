using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using RainCoreGraphics;

namespace RainCore;

             
                                                                         
                                                                           
                                                                       
                                                                               
                                                                          
                                                                               
                                                                      
                                                                            
                                                                              
                                    
   
                                                                            
                                                                          
                                                                            
                                                                         
                                                                         
                                                                           
                                                                       
                                                                   
                                                                              
                                                                         
                                                                           
   
                                                                             
                                                                        
                                           
              
public sealed class BlockBreakOverlay
{
    private const int StageCount = 7;

                                                                        
                                                                              
                                                                           
                                                          
    private const float OverlayScale = 1.006f;

    private int _vao;
    private int _vbo;
    private readonly int[] _stageTextures = new int[StageCount];

    public void Build()
    {
                                                                           
                                                                        
                                                                        
                                                                         
                                                              
        float[] v = BuildCubeVertices13();

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, v.Length * sizeof(float), v, BufferUsageHint.StaticDraw);

                                                                               
                                                                   
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

        for (int i = 0; i < StageCount; i++)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Images", "cracks", $"crack_stage_{i}.png");
            _stageTextures[i] = TextureAtlas.LoadImage(path, out _, out _);
        }
    }

    private static float[] BuildCubeVertices13()
    {
        var faces = new (Vector3 normal, Vector3 right, Vector3 up)[]
        {
            (new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0)),        
            (new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 1, 0)),      
            (new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 1, 0)),       
            (new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0)),       
            (new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, -1)),       
            (new Vector3(0, -1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1)),       
        };

        var list = new List<float>();
        foreach (var (normal, right, up) in faces)
        {
            var center = normal * 0.5f;
            var p00 = center - right * 0.5f - up * 0.5f;
            var p10 = center + right * 0.5f - up * 0.5f;
            var p11 = center + right * 0.5f + up * 0.5f;
            var p01 = center - right * 0.5f + up * 0.5f;

            void Add(Vector3 p, float u, float vv)
            {
                list.Add(p.X); list.Add(p.Y); list.Add(p.Z);             
                list.Add(normal.X); list.Add(normal.Y); list.Add(normal.Z);          
                list.Add(1f); list.Add(1f); list.Add(1f);                          
                list.Add(1f);                                                  
                list.Add(u); list.Add(vv);                              
                list.Add(0f);                                                                            
            }

            Add(p00, 0f, 0f); Add(p10, 1f, 0f); Add(p11, 1f, 1f);
            Add(p00, 0f, 0f); Add(p11, 1f, 1f); Add(p01, 0f, 1f);
        }
        return list.ToArray();
    }

                                                                        
                                                                        
                                                                              
                                                                              
                                                                              
                                                                             
                                                                             
                                                                           
    public void Render(Vector3i cell, float progress01, Shader shader, Matrix4 view, Matrix4 projection)
    {
        if (progress01 <= 0f || _vao == 0) return;
        int stage = Math.Clamp((int)(progress01 * StageCount), 0, StageCount - 1);
        int tex = _stageTextures[stage];
        if (tex == 0) return;

        var center = new Vector3(cell.X + 0.5f, cell.Y + 0.5f, cell.Z + 0.5f);
        var model = Matrix4.CreateScale(OverlayScale) * Matrix4.CreateTranslation(center);

        shader.Use();
        shader.SetMatrix4("model", model);
        shader.SetMatrix4("view", view);
        shader.SetMatrix4("projection", projection);
        shader.SetInt("useTexture", 1);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, tex);
        shader.SetInt("modelTexture", 0);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
    }
}
