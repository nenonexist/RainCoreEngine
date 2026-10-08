using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace RainCore;

             
                                                                             
                                                                             
                                                           
                                 
   
                                                                             
                                                                    
                                                                        
                                                
   
                                                                             
                                                                             
                                                                          
                                                                            
                                                                       
                                                                           
                                                                          
                                                                         
               
              
public static class TextTexture
{
                                                                                                      
    public static float FontSizePx = 36f;
                                                                                               
    public static int Padding = 10;

                 
                                                             
                                                                            
                                                                          
                                                                         
                                                                           
                                                                       
                                                                            
                                                                           
                                 
                  
    public static (int textureHandle, float worldWidth, float worldHeight) Create(
        string text, Color color, float pixelsPerWorldUnit = 110f, bool monospace = false,
        string? fontFamilyName = null, float? fontSizePx = null)
    {
        using var fontFamily = ResolveFontFamily(fontFamilyName, monospace);
        using var font = new Font(fontFamily, fontSizePx ?? FontSizePx, FontStyle.Bold, GraphicsUnit.Pixel);

        SizeF measured;
        using (var measureBmp = new Bitmap(1, 1))
        using (var mg = Graphics.FromImage(measureBmp))
        {
                                                                              
                                                                         
                                                                             
                                                                      
                                                                            
                                                                        
                          
            mg.TextRenderingHint = TextRenderingHint.AntiAlias;
            measured = mg.MeasureString(text, font);
        }

        int width = Math.Max(64, (int)MathF.Ceiling(measured.Width) + Padding * 2);
        int height = Math.Max(32, (int)MathF.Ceiling(measured.Height) + Padding * 2);

        using var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Transparent);
                                                                                        
                                                                     
                                                                               
                                                                               
                                                                        
                                                                              
                                                                                
                                                                             
                                          
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.AntiAlias;

                                                                                            
                                                                                         
            using var shadowBrush = new SolidBrush(Color.FromArgb(160, 2, 3, 7));
            for (int ox = -1; ox <= 1; ox++)
                for (int oy = -1; oy <= 1; oy++)
                    if (ox != 0 || oy != 0)
                        g.DrawString(text, font, shadowBrush, Padding + ox + 1f, Padding + oy + 1f);

            using var outlineBrush = new SolidBrush(Color.FromArgb(220, 3, 6, 12));
            for (int ox = -2; ox <= 2; ox++)
                for (int oy = -2; oy <= 2; oy++)
                    if (ox != 0 || oy != 0)
                        g.DrawString(text, font, outlineBrush, Padding + ox + 1f, Padding + oy + 1f);

            using var brush = new SolidBrush(color);
            g.DrawString(text, font, brush, Padding + 1f, Padding + 1f);
        }

        int handle = UploadTexture(bmp);
        float worldWidth = width / pixelsPerWorldUnit;
        float worldHeight = height / pixelsPerWorldUnit;
        return (handle, worldWidth, worldHeight);
    }

                                                                                     
                                                                                       
                                                                                            
    private static FontFamily ResolveFontFamily(string? fontFamilyName, bool monospace)
    {
        if (!string.IsNullOrWhiteSpace(fontFamilyName))
        {
            try
            {
                return new FontFamily(fontFamilyName);
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"[TextTexture] Font \"{fontFamilyName}\" not found on the system - using fallback.");
            }
        }

        return new FontFamily(monospace ? GenericFontFamilies.Monospace : GenericFontFamilies.SansSerif);
    }

    private static int UploadTexture(Bitmap bmp)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        int handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
                                                                           
                                                                 
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
            PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        bmp.UnlockBits(data);

                                                                           
                                                                            
                                                                              
                                                                     
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        return handle;
    }
}
