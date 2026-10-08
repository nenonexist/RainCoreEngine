using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using RainCoreGraphics;

namespace RainCore;

             
                                                                              
                                                                            
                                                                             
                                                                             
                                                                        
                                                                          
   
                                                                        
                                                                         
                                                                           
                                                                        
   
                                                                            
                                                                      
                                                                            
                                                                            
                                 
              
public sealed class ScreenPictureLayer
{
    private sealed class Picture
    {
        public int TextureHandle;
        public int PixelWidth;
        public int PixelHeight;
        public float X;
        public float Y;
        public float Scale;
        public float Opacity;
    }

    private readonly Dictionary<int, Picture> _pictures = new();
    private Shader? _shader;
    private int _vao;
    private int _vbo;

    public void Build()
    {
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _shader = new Shader(Path.Combine(shadersDir, "picture.vert"), Path.Combine(shadersDir, "picture.frag"));

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }

                                                                                      
                                                                                   
                                                                                 
                                                                        
    public void Show(int id, string path, float x, float y, float scale, float opacity)
    {
        int handle = TextureAtlas.LoadImage(path, out int width, out int height);
        if (handle == 0) return;                                                                               

        if (_pictures.TryGetValue(id, out var existing))
            GL.DeleteTexture(existing.TextureHandle);                                                           

        _pictures[id] = new Picture
        {
            TextureHandle = handle,
            PixelWidth = width,
            PixelHeight = height,
            X = x,
            Y = y,
            Scale = scale > 0f ? scale : 1f,
            Opacity = Math.Clamp(opacity, 0f, 1f),
        };
    }

    public void Hide(int id)
    {
        if (!_pictures.TryGetValue(id, out var picture)) return;
        GL.DeleteTexture(picture.TextureHandle);
        _pictures.Remove(id);
    }

                                                                               
                                                                                  
                                                                                 
                                                                                    
    public void Render(int lowResWidth, int lowResHeight)
    {
        if (_shader == null || _pictures.Count == 0) return;

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader.Use();
        _shader.SetInt("uText", 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindVertexArray(_vao);

        foreach (var picture in _pictures.Values)
        {
            float halfWidthNdc = picture.PixelWidth / (float)lowResWidth * picture.Scale;
            float halfHeightNdc = picture.PixelHeight / (float)lowResHeight * picture.Scale;
            float left = picture.X - halfWidthNdc;
            float right = picture.X + halfWidthNdc;
            float bottom = picture.Y - halfHeightNdc;
            float top = picture.Y + halfHeightNdc;

            float[] verts =
            {
                left, bottom, 0f, 1f,
                right, bottom, 1f, 1f,
                right, top, 1f, 0f,
                left, bottom, 0f, 1f,
                right, top, 1f, 0f,
                left, top, 0f, 0f,
            };
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, verts.Length * sizeof(float), verts);

            _shader.SetFloat("uOpacity", picture.Opacity);
            GL.BindTexture(TextureTarget.Texture2D, picture.TextureHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }
    }
}
