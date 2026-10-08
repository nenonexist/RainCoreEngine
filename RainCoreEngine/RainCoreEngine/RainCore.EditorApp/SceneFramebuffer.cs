using OpenTK.Graphics.OpenGL4;

namespace RainCore.EditorApp;

             
                                                                         
                                                                            
                                                                            
                                                            
   
                                                                              
                                                                      
                                
              
public sealed class SceneFramebuffer : IDisposable
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int ColorTexture { get; private set; }

    private int _fbo;
    private int _depthRenderBuffer;

    public SceneFramebuffer(int width, int height)
    {
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        Create();
    }

    private void Create()
    {
        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

        ColorTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, ColorTexture, 0);

        _depthRenderBuffer = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRenderBuffer);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Width, Height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, _depthRenderBuffer);

        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
            throw new Exception($"Editor: Scene View framebuffer incomplete: {status}");

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

                                                                              
                                                                          
                                                         
    public void Resize(int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        if (width == Width && height == Height) return;

        Width = width;
        Height = height;
        DeleteGLObjects();
        Create();
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.Viewport(0, 0, Width, Height);
    }

    public static void BindDefault(int windowWidth, int windowHeight)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(0, 0, windowWidth, windowHeight);
    }

    private void DeleteGLObjects()
    {
        GL.DeleteFramebuffer(_fbo);
        GL.DeleteTexture(ColorTexture);
        GL.DeleteRenderbuffer(_depthRenderBuffer);
    }

    public void Dispose() => DeleteGLObjects();
}
