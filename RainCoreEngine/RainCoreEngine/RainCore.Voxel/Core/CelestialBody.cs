using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using RainCoreGraphics;

namespace RainCore;

             
                                                                               
                                                                            
                                                                  
   
                                                                            
                                                                        
                                                                          
                                                                            
                                                                      
   
                                                                           
                                                                  
                                                                           
                                                                   
                                                                           
                                                                          
                                                                              
                                                                           
              
public class CelestialBody
{
                                                                                     
                                                                                 
                                                                              
                                                  
    private readonly string _textureFileName;

    private readonly Vector3 _fallbackColor;

                                                                                    
    private readonly string _label;

    private int _texture;
    private bool _hasTexture;
    private int _vao;
    private int _vbo;

                                                                                      
                                                                                               
    private readonly float[] _vertexData = new float[6 * 13];

    public CelestialBody(string textureFileName, Vector3 fallbackColor, string label)
    {
        _textureFileName = textureFileName;
        _fallbackColor = fallbackColor;
        _label = label;
    }

    public void Build()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Images", _textureFileName);
        _texture = TextureAtlas.LoadImage(path, out _, out _);
        _hasTexture = _texture != 0;
        if (!_hasTexture)
        {
            Console.WriteLine($"[{_label}] Images/{_textureFileName} не найден - рисуется плоской заливкой вместо картинки (см. CelestialBody.Build).");
        }

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexData.Length * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

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
    }

                                                                            
                                                                               
                                                                               
                                                                               
                             
       
                                                                              
                                                                                
                                                                                  
                                                                                
                                                                                
                                                                         
                                                                           
                                                                               
                                                                 
    public void Render(Shader shader, Matrix4 view, Matrix4 projection, Vector3 worldPosition, float halfSize, Vector3 cameraPosition, bool visible)
    {
        if (!visible || _vao == 0) return;

        Vector3 normal = worldPosition - cameraPosition;
        if (normal.LengthSquared < 0.0001f) normal = -Vector3.UnitZ;
        normal = Vector3.Normalize(normal);
        Vector3 worldUp = Vector3.UnitY;
        Vector3 rightDir = Vector3.Cross(worldUp, normal);
        if (rightDir.LengthSquared < 0.0001f) rightDir = Vector3.UnitX;                                                 
        rightDir = Vector3.Normalize(rightDir);
        Vector3 upDir = Vector3.Cross(normal, rightDir);

        Vector3 right = rightDir * halfSize;
        Vector3 up = upDir * halfSize;
        Vector3 p0 = worldPosition - right - up;
        Vector3 p1 = worldPosition + right - up;
        Vector3 p2 = worldPosition + right + up;
        Vector3 p3 = worldPosition - right + up;

                                                                                      
                                                                                   
                                                                                        
        Vector3 color = _hasTexture ? Vector3.One : _fallbackColor;

        WriteVertex(0, p0, 0f, 0f, color);
        WriteVertex(1, p1, 1f, 0f, color);
        WriteVertex(2, p2, 1f, 1f, color);
        WriteVertex(3, p0, 0f, 0f, color);
        WriteVertex(4, p2, 1f, 1f, color);
        WriteVertex(5, p3, 0f, 1f, color);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _vertexData.Length * sizeof(float), _vertexData);

        shader.Use();
        shader.SetMatrix4("model", Matrix4.Identity);
        shader.SetMatrix4("view", view);
        shader.SetMatrix4("projection", projection);
        shader.SetInt("useTexture", _hasTexture ? 1 : 0);
        if (_hasTexture)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            shader.SetInt("modelTexture", 0);
        }

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    private void WriteVertex(int slot, Vector3 p, float u, float v, Vector3 color)
    {
        int o = slot * 13;
        _vertexData[o] = p.X; _vertexData[o + 1] = p.Y; _vertexData[o + 2] = p.Z;
        _vertexData[o + 3] = 0f; _vertexData[o + 4] = 1f; _vertexData[o + 5] = 0f;                                    
        _vertexData[o + 6] = color.X; _vertexData[o + 7] = color.Y; _vertexData[o + 8] = color.Z;
        _vertexData[o + 9] = 1f;                                                
        _vertexData[o + 10] = u; _vertexData[o + 11] = v;
        _vertexData[o + 12] = 0f;                                                      
    }
}
