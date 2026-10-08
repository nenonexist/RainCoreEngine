using System.Diagnostics;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace RainCore;

             
                                                                       
                                                                              
                                                                              
                                                                           
                                                                             
                                                                             
                                                                              
                                                                         
                                                                 
   
                                                                            
                                                                         
                                                                              
                                                                            
                                                      
   
                                                                            
                                                                              
                                                                            
                                                                              
                                                                    
                                                                         
                                                                     
                                            
              
public sealed class VideoPlayer : IDisposable
{
                                                                           
                                                                                
                                                         
    private const int Width = 256;
    private const int Height = 144;
    private const double Fps = 15.0;
    private const double FrameSeconds = 1.0 / Fps;

                                                                           
                                                                           
                          
    private const int MaxStderrChars = 800;

    private Process? _ffmpeg;
    private Thread? _readThread;
    private Thread? _stderrThread;
    private readonly object _lock = new();
    private byte[]? _pendingFrame;
    private volatile bool _stopping;
    private double _accum;
    private bool _gotAnyFrame;
    private readonly StringBuilder _stderrTail = new();

    public int TextureHandle { get; private set; }

                                                                                     
                                                                              
                                                                              
                                  
    public string? LastError { get; private set; }

                                                                            
                                                                                  
                                                                                 
                                                                                  
                                                             
    public static string? LastGlobalError { get; private set; }

    private static void SetGlobalError(string message) => LastGlobalError = message;

                                                                                    
                                                                            
                                                          
    public static void ClearGlobalErrorIfMatches(VideoPlayer player)
    {
        if (player.LastError != null && LastGlobalError == player.LastError)
            LastGlobalError = null;
    }

                                                                                 
                                                                           
                                                                          
                                                                           
                                                                           
                                                                      
    private static bool? _ffmpegAvailable;

    public static bool IsFfmpegAvailable()
    {
        if (_ffmpegAvailable.HasValue) return _ffmpegAvailable.Value;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = ResolveFfmpegExecutable(),
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var proc = Process.Start(psi);
            if (proc == null) { _ffmpegAvailable = false; return false; }
            bool exited = proc.WaitForExit(1500);
            if (!exited)
            {
                try { proc.Kill(entireProcessTree: true); } catch {                                 }
                _ffmpegAvailable = false;
                return false;
            }
            _ffmpegAvailable = proc.ExitCode == 0;
        }
        catch
        {
            _ffmpegAvailable = false;
        }

        return _ffmpegAvailable.Value;
    }

                                                                               
                                                                                  
                                                                           
    public void Play(string path)
    {
        Stop();
        LastError = null;
        _gotAnyFrame = false;

        if (!File.Exists(path))
        {
            LastError = $"файл не найден: {Path.GetFileName(path)}";
            SetGlobalError($"[ТВ] {LastError}");
            Console.WriteLine($"[ТВ] Файл не найден: {path}");
            return;
        }

        var ffmpegExe = ResolveFfmpegExecutable();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegExe,
                                                                                    
                                                                                  
                                                                                     
                                                                                         
                                                                               
                                                                                  
                Arguments = $"-loglevel error -stream_loop -1 -re -i \"{path}\" " +
                            $"-vf scale={Width}:{Height} -pix_fmt rgb24 -f rawvideo -an -",
                RedirectStandardOutput = true,
                                                                                    
                                                                                  
                                                                                      
                                                                                   
                                                                                      
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            _ffmpeg = Process.Start(psi);
        }
        catch (Exception ex)
        {
            LastError = $"не удалось запустить ffmpeg ({ex.Message})";
            SetGlobalError("[ТВ] ffmpeg не найден - положи ffmpeg[.exe] рядом с игрой или в PATH " +
                "(https://ffmpeg.org/download.html)");
            Console.WriteLine($"[ТВ] Не удалось запустить ffmpeg ({ex.Message}) - убедитесь, что " +
                "ffmpeg[.exe] есть в PATH или рядом с игрой. Скачать: https://ffmpeg.org/download.html");
            _ffmpeg = null;
            return;
        }

        _stopping = false;
        _accum = 0;
        lock (_lock) { _stderrTail.Clear(); }
        var proc = _ffmpeg;
        if (proc == null) return;
        _readThread = new Thread(() => ReadLoop(proc)) { IsBackground = true, Name = "VideoPlayer.ReadLoop" };
        _readThread.Start();
        _stderrThread = new Thread(() => StderrLoop(proc)) { IsBackground = true, Name = "VideoPlayer.StderrLoop" };
        _stderrThread.Start();
    }

                                                                                    
                                                                                  
                                                                                 
                                                                                            
    private static string ResolveFfmpegExecutable()
    {
        var local = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");
        return File.Exists(local) ? local : "ffmpeg";
    }

    private void StderrLoop(Process proc)
    {
        try
        {
            string? line;
            while ((line = proc.StandardError.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                lock (_lock)
                {
                    _stderrTail.Append(line).Append(' ');
                    if (_stderrTail.Length > MaxStderrChars)
                        _stderrTail.Remove(0, _stderrTail.Length - MaxStderrChars);
                }
            }
        }
        catch
        {
                                                                     
        }
    }

    private void ReadLoop(Process proc)
    {
        int frameSize = Width * Height * 3;
        var buffer = new byte[frameSize];
        Stream stream;
        try
        {
            stream = proc.StandardOutput.BaseStream;
        }
        catch
        {
            return;
        }

        try
        {
            while (!_stopping)
            {
                int read = 0;
                while (read < frameSize)
                {
                    int n = stream.Read(buffer, read, frameSize - read);
                    if (n <= 0)
                    {
                        ReportEarlyExit(proc);
                        return;                                                                   
                    }
                    read += n;
                }

                lock (_lock)
                {
                    _pendingFrame = (byte[])buffer.Clone();
                }
            }
        }
        catch
        {
                                                                                           
        }
    }

                                                                                     
                                                                                  
                                                                                
                                                                                  
                                                                                
                                                                  
    private void ReportEarlyExit(Process proc)
    {
        if (_stopping) return;                                      

        string tail;
        int exitCode;
        try
        {
            proc.WaitForExit(500);
            exitCode = proc.HasExited ? proc.ExitCode : -1;
        }
        catch
        {
            exitCode = -1;
        }

        lock (_lock) { tail = _stderrTail.ToString().Trim(); }

        string reason = string.IsNullOrEmpty(tail)
            ? $"ffmpeg завершился (код {exitCode}) не отдав ни одного кадра - вероятно, файл повреждён или формат не поддержан этой сборкой ffmpeg"
            : $"ffmpeg завершился (код {exitCode}): {tail}";

        LastError = reason;
        SetGlobalError($"[ТВ] {reason}");
        Console.WriteLine($"[ТВ] {reason}");
    }

                                                                               
                                                                            
    public void Update(float dt)
    {
        if (_ffmpeg == null) return;

        _accum += dt;
        if (_accum < FrameSeconds) return;
        _accum = 0;

        byte[]? frame;
        lock (_lock)
        {
            frame = _pendingFrame;
            _pendingFrame = null;
        }
        if (frame == null) return;

        if (!_gotAnyFrame)
        {
            _gotAnyFrame = true;
            LastError = null;
            ClearGlobalErrorIfMatches(this);
        }

        if (TextureHandle == 0)
        {
            TextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, Width, Height, 0,
                PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        }

        GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, PixelFormat.Rgb, PixelType.UnsignedByte, frame);
    }

    public void Stop()
    {
        _stopping = true;
        try { _ffmpeg?.Kill(entireProcessTree: true); } catch {                                          }
        _ffmpeg?.Dispose();
        _ffmpeg = null;
        lock (_lock) { _pendingFrame = null; }
        _accum = 0;
    }

    public void Dispose()
    {
        Stop();
        if (TextureHandle != 0)
        {
            GL.DeleteTexture(TextureHandle);
            TextureHandle = 0;
        }
    }
}
