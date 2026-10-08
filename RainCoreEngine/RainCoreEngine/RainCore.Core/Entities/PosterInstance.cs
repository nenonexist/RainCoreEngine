using OpenTK.Mathematics;

namespace RainCore;

             
                                                                             
                                                                     
                                                                            
                                                                            
                                                                     
                                                                            
                                                                             
                                                             
   
                                                                          
                                                                              
                                                                              
                                                                        
                                                                          
                                                                              
                                                                            
                                                                            
                                                                          
                                                                       
                                                                     
                                        
   
                                                                            
                                                                            
                                                                              
              
public sealed class PosterInstance : IDisposable
{
                                                                                   
                                                                             
                                                   
    public string? ImagePath { get; }

                                                                               
                                                                        
    public string? VideoPath { get; }

                                                                                
                                                         
    public bool IsVideo => VideoPath != null;

                                                                           
                                                                                
                         
    public Vector3 Position { get; private set; }

                                                                                 
                                                                              
                                                                               
                                                                              
                                                                                
                                                                             
                                                     
    public void SetPosition(Vector3 position) => Position = position;

                                                                                            
    public Vector3 Right { get; }

                                                                                           
    public Vector3 Up { get; }

                                                                    
                                                                             
                                                                                  
                                                                            
                                                                           
                                                                          
                                                                                  
                                                                                  
                                                                
    public float Scale { get; set; }

                                                                                       
                                                                                  
                                            
    private readonly VideoPlayer? _videoPlayer;

                 
                                                                                   
                                                                                  
                                                                              
                                                                           
                                                                                 
                                                                          
                  
    public PosterInstance(string? imagePath, string? videoPath, Vector3 position, Vector3 normal, float scale = 1f)
    {
        if (string.IsNullOrEmpty(imagePath) && string.IsNullOrEmpty(videoPath))
            throw new ArgumentException("У постера должна быть либо картинка (Images/), либо видео (Videos/).");

        ImagePath = string.IsNullOrEmpty(imagePath) ? null : imagePath;
        VideoPath = string.IsNullOrEmpty(videoPath) ? null : videoPath;
        Position = position;
        Scale = scale <= 0f ? 1f : scale;

        if (MathF.Abs(normal.Y) > 0.9f)
        {
                                                                             
                                                                              
                                               
            Right = Vector3.UnitX;
            Up = Vector3.UnitZ;
        }
        else
        {
            Right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, normal));
            Up = Vector3.UnitY;
        }

        if (VideoPath != null)
        {
            _videoPlayer = new VideoPlayer();
            _videoPlayer.Play(Path.Combine(AppContext.BaseDirectory, "Videos", VideoPath));
        }
    }

                                                                             
                                                                          
                                                                                    
    public void Update(float dt) => _videoPlayer?.Update(dt);

                                                                                      
                                                                               
                                                                                   
    public int VideoFrameTexture => _videoPlayer?.TextureHandle ?? 0;

                                                                                    
                                                                                   
                                                                                
                                                    
    public void Dispose() => _videoPlayer?.Dispose();
}
