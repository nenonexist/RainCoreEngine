using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace RainCore;

             
                                                                          
                                                                          
                                                                        
                                                                        
                 
   
                                                                            
                                                                           
                                                                          
              
public static class TextureAtlas
{
                 
                                                                              
                                                                             
                                                                            
                                                                 
       
                                                                              
                                                                                 
                                                                           
                                                                        
                                                                         
                                      
                  
    public static int LoadOrDefault(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"[Textures] Атлас блоков не найден ({path}) - блоки рисуются сплошным цветом.");
            return 0;
        }

        using var bmp = new Bitmap(path);
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        int handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
            PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        bmp.UnlockBits(data);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                                                                                     
                                                             
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        if (bmp.Width != bmp.Height)
            Console.WriteLine($"[Textures] Внимание: атлас {path} не квадратный ({bmp.Width}x{bmp.Height}) - сетка ячеек может выйти неровной по X/Y.");

        BlockTextures.SetGridSizeFromAtlasWidth(bmp.Width);
        Console.WriteLine($"[Textures] Атлас блоков загружен: {path} ({bmp.Width}x{bmp.Height}, сетка {BlockTextures.AtlasGridSize}x{BlockTextures.AtlasGridSize}, ячейка {BlockTextures.CellPixelSize}x{BlockTextures.CellPixelSize}px)");
        return handle;
    }

                 
                                                                       
                                                                            
                                                                           
                                                                          
                                                                              
                                                                           
                                                                       
       
                                                                            
                                                                             
                                                                            
                                                                               
                                                                       
                                                                           
                                                                     
                                                                          
                                                               
       
                                                                           
                                                                               
                                                                                 
                                                                              
                                                                               
                                                                              
                                                                           
                                                                                 
                                                                     
                                                                               
                             
       
                                                                           
                                                                          
                                                 
                  
    public static int LoadBlocksCombined(string baseAtlasPath, string blocksFolder)
    {
        int cell = BlockTextures.CellPixelSize;

        Bitmap? oldBmp = null;
        int oldGridSize = 0;
        if (File.Exists(baseAtlasPath))
        {
            oldBmp = new Bitmap(baseAtlasPath);
            if (oldBmp.Width != oldBmp.Height)
                Console.WriteLine($"[Textures] Внимание: атлас {baseAtlasPath} не квадратный ({oldBmp.Width}x{oldBmp.Height}) - сетка ячеек может выйти неровной по X/Y.");
            oldGridSize = Math.Max(1, oldBmp.Width / cell);
        }
        else
        {
            Console.WriteLine($"[Textures] Общий атлас не найден ({baseAtlasPath}) - используются только отдельные файлы из {blocksFolder}, если есть.");
        }

        var files = Directory.Exists(blocksFolder)
            ? Directory.GetFiles(blocksFolder, "*.png")
            : Array.Empty<string>();

        if (oldBmp == null && files.Length == 0)
        {
            Console.WriteLine($"[Textures] Ни общего атласа, ни отдельных текстур в {blocksFolder} не найдено - блоки рисуются сплошным цветом.");
            return 0;
        }

        int finalGridSize = FitGridSize(oldGridSize, files.Length);
        int pixelSize = finalGridSize * cell;

        using var canvas = new Bitmap(pixelSize, pixelSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(canvas))
        {
                                                                             
                                                                             
                                                                          
                                                                             
                                                                            
            g.CompositingMode = CompositingMode.SourceCopy;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            if (oldBmp != null)
                g.DrawImage(oldBmp, new Rectangle(0, 0, oldBmp.Width, oldBmp.Height));

            var sideCells = new Dictionary<string, (int Col, int Row)>(StringComparer.OrdinalIgnoreCase);
            var topCells = new Dictionary<string, (int Col, int Row)>(StringComparer.OrdinalIgnoreCase);
            var bottomCells = new Dictionary<string, (int Col, int Row)>(StringComparer.OrdinalIgnoreCase);
            int newSlotsPerRow = finalGridSize;
            for (int i = 0; i < files.Length; i++)
            {
                string stem = Path.GetFileNameWithoutExtension(files[i]);
                var (name, slot) = SplitFaceSuffix(stem);
                int row = oldGridSize + i / newSlotsPerRow;
                int col = i % newSlotsPerRow;

                using var img = new Bitmap(files[i]);
                if (img.Width != img.Height)
                    Console.WriteLine($"[Textures] Внимание: {files[i]} не квадратная картинка ({img.Width}x{img.Height}) - будет растянута под ячейку {cell}x{cell}.");

                var dest = new Rectangle(col * cell, row * cell, cell, cell);
                g.DrawImage(img, dest, new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);

                var slotMap = slot switch { FaceSlot.Top => topCells, FaceSlot.Bottom => bottomCells, _ => sideCells };
                if (slotMap.ContainsKey(name))
                    Console.WriteLine($"[Textures] Внимание: несколько файлов в {blocksFolder} дают одно и то же имя блока \"{name}\" ({slot}) без учёта регистра - использован последний по порядку.");
                slotMap[name] = (col, row);
                Console.WriteLine($"[Textures] Текстура блока \"{name}\" ({slot}) из {files[i]} назначена ячейке ({col},{row}).");
            }

            var autoCells = new Dictionary<string, BlockFaceTextures>(StringComparer.OrdinalIgnoreCase);
                                                                                    
                                                                                    
                                                                             
                                                                            
                                                                                   
            foreach (var name in sideCells.Keys.Concat(topCells.Keys).Concat(bottomCells.Keys).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                (int Col, int Row)? side = sideCells.TryGetValue(name, out var s) ? s : null;
                (int Col, int Row)? top = topCells.TryGetValue(name, out var t) ? t : null;
                (int Col, int Row)? bottom = bottomCells.TryGetValue(name, out var b) ? b : null;
                autoCells[name] = new BlockFaceTextures(side, top, bottom);
            }

            BlockTextures.SetAutoCells(autoCells);
        }

        oldBmp?.Dispose();

        int handle = UploadBitmap(canvas);
        BlockTextures.SetGridSizeFromAtlasWidth(pixelSize);
        Console.WriteLine($"[Textures] Атлас блоков собран: {pixelSize}x{pixelSize} (сетка {finalGridSize}x{finalGridSize}, старых ячеек {oldGridSize}x{oldGridSize}, отдельных файлов {files.Length}).");
        return handle;
    }

                                                                                                                                         
    private enum FaceSlot { Side, Top, Bottom }

                 
                                                                                         
                                                                                
                                                                    
                  
    private static (string BaseName, FaceSlot Slot) SplitFaceSuffix(string stem)
    {
        const string topSuffix = "_top";
        const string bottomSuffix = "_bottom";
        if (stem.EndsWith(topSuffix, StringComparison.OrdinalIgnoreCase))
            return (stem[..^topSuffix.Length], FaceSlot.Top);
        if (stem.EndsWith(bottomSuffix, StringComparison.OrdinalIgnoreCase))
            return (stem[..^bottomSuffix.Length], FaceSlot.Bottom);
        return (stem, FaceSlot.Side);
    }

                 
                                                                              
                                                                            
                                                                         
                                                                           
                                                                           
                                                                
                  
    private static int FitGridSize(int oldGridSize, int newCount)
    {
        if (newCount <= 0) return oldGridSize;
        int size = Math.Max(oldGridSize, 1);
        while ((long)size * (size - oldGridSize) < newCount) size++;
        return size;
    }

    private static int UploadBitmap(Bitmap bmp)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        int handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
            PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        bmp.UnlockBits(data);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        return handle;
    }

                 
                                                                                     
                                                                               
                                                                                                  
                                                                                       
                                      
                  
    public static int LoadImage(string path, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (!File.Exists(path))
        {
            Console.WriteLine($"[Textures] Картинка не найдена: {path}");
            return 0;
        }

        using var bmp = new Bitmap(path);
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        int handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
            PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        bmp.UnlockBits(data);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        width = bmp.Width;
        height = bmp.Height;
        return handle;
    }
}
