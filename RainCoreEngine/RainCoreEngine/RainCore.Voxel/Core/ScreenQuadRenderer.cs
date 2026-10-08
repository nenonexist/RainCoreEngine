using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using RainCoreGraphics;

namespace RainCore;

             
                                                              
                                                                            
                                                                             
                                                                            
                                                                            
                                                                            
                                                            
              
public class ScreenQuadRenderer
{
    private Shader _shader = null!;
    private int _vao;
    private int _vbo;

    public void Build()
    {
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _shader = new Shader(Path.Combine(shadersDir, "screen.vert"), Path.Combine(shadersDir, "screen.frag"));

                                                                             
                                                                               
                                                                               
                                   
        float[] vertices =
        {
                                  
            -0.5f, -0.5f, 0f, 1f,
             0.5f, -0.5f, 1f, 1f,
             0.5f,  0.5f, 1f, 0f,

            -0.5f, -0.5f, 0f, 1f,
             0.5f,  0.5f, 1f, 0f,
            -0.5f,  0.5f, 0f, 0f,
        };

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        int stride = 4 * sizeof(float);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }

                                                                        
                                                                             
                                                                     
                                                                             
                                                                              
                                                     
    public void Draw(int textureHandle, Vector3 center, Vector3 right, Vector3 up, float width, float height, Camera camera)
    {
        if (textureHandle == 0) return;

        _shader.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);
        _shader.SetInt("uText", 0);
        _shader.SetVector3("center", center);
        _shader.SetVector3("right", right);
        _shader.SetVector3("up", up);
        _shader.SetVector2("size", new Vector2(width, height));
        _shader.SetMatrix4("view", camera.GetViewMatrix());
        _shader.SetMatrix4("projection", camera.GetProjectionMatrix());

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
}
