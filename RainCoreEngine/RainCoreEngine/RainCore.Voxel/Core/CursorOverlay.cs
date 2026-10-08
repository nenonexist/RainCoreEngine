using System;
using System.Drawing;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

using RainCoreGraphics;

namespace RainCore;

             
                                                                               
                                                                           
                                                                        
                                                                                
                                                                             
                                      
   
                                                                             
                                                                                
                                                                          
                                                                                
                                                                   
              
public sealed class CursorOverlay
{
    private const string TextureFileName = "cursor.png";

                                                                                  
                                                                                
                                                                                
                                                                     
    private const float SizePx = 30f;

    private Shader? _shader;
    private int _vao, _vbo;
    private int _texture;
    private float _texWidth, _texHeight;

                                                                              
                                                                           
    public bool Enabled { get; set; }

                                                                                  
                                                                                     
                                                                                    
    public bool Loaded => _texture != 0;

    public void Build()
    {
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _shader = new Shader(Path.Combine(shadersDir, "ui.vert"), Path.Combine(shadersDir, "ui.frag"));

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        _texture = LoadUiTexture(TextureFileName, out _texWidth, out _texHeight);
        if (_texture == 0)
            Console.WriteLine($"[CursorOverlay] Не найден UI/{TextureFileName} - положите файл в UI/, иначе в меню/инвентаре останется системный курсор.");
    }

                                                                                      
                                                                                  
                                                                                    
                                                 
    public void Render(int framebufferWidth, int framebufferHeight, int windowWidth, int windowHeight, Vector2 windowPixel)
    {
        if (!Enabled || _shader == null || _texture == 0 || framebufferWidth <= 0 || framebufferHeight <= 0
            || windowWidth <= 0 || windowHeight <= 0) return;

        float scaleX = (float)framebufferWidth / windowWidth;
        float scaleY = (float)framebufferHeight / windowHeight;
        float mouseX = windowPixel.X * scaleX;
        float mouseY = windowPixel.Y * scaleY;

        float aspect = _texHeight > 0f ? _texWidth / _texHeight : 1f;
        float wPx = SizePx * scaleX;
        float hPx = SizePx / MathF.Max(0.01f, aspect) * scaleY;

                                                                                
                                                   
        float hotspotX = wPx * 0.97f;
        float hotspotY = hPx * 0.90f;
        float left = (mouseX - hotspotX) / framebufferWidth * 2f - 1f;
        float top = 1f - (mouseY - hotspotY) / framebufferHeight * 2f;
        float right = left + wPx / framebufferWidth * 2f;
        float bottom = top - hPx / framebufferHeight * 2f;

        float[] verts =
        {
            left, bottom, 0f, 1f,
            right, bottom, 1f, 1f,
            right, top, 1f, 0f,
            left, bottom, 0f, 1f,
            right, top, 1f, 0f,
            left, top, 0f, 0f,
        };

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, verts.Length * sizeof(float), verts);

        _shader.Use();
        _shader.SetInt("uText", 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        GL.Disable(EnableCap.Blend);
    }

                                                                                  
                                                                        
                                                                       
                                                               
    private static int LoadUiTexture(string fileName, out float width, out float height)
    {
        width = 0f;
        height = 0f;

        var candidateDirs = new[]
        {
            AppContext.BaseDirectory,
            Directory.GetCurrentDirectory(),
            Path.Combine(AppContext.BaseDirectory, "..", "..", ".."),
            Path.Combine(AppContext.BaseDirectory, "..", ".."),
        };

        string? foundPath = null;
        foreach (var dir in candidateDirs)
        {
            if (dir == null) continue;
            var path = Path.GetFullPath(Path.Combine(dir, "ui", fileName));
            if (File.Exists(path))
            {
                foundPath = path;
                break;
            }
        }

        if (foundPath == null) return 0;

        using var bmp = new Bitmap(foundPath);
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        int handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
            PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        bmp.UnlockBits(data);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        width = bmp.Width;
        height = bmp.Height;
        return handle;
    }
}
