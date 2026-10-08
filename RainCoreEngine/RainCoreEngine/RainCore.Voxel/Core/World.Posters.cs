using System.Text.Json;
using OpenTK.Mathematics;

namespace RainCore;

             
                                                                
                                                                       
                                                                        
                                                                              
                                                                              
                                                                        
                                                                             
                                                                         
                                                                              
                                                                               
                                   
              
public partial class World
{
    private readonly List<PosterInstance> _posters = new();

    public IReadOnlyList<PosterInstance> Posters => _posters;

    public void AddPoster(PosterInstance poster) => _posters.Add(poster);

                 
                                                                                
                                                                                 
                                                                                
                                                                        
                                                                             
                                                   
                  
    public bool RemoveNearestPoster(Vector3 point, float maxDistance)
    {
        PosterInstance? nearest = null;
        float bestDist = maxDistance;
        foreach (var poster in _posters)
        {
            float dist = (poster.Position - point).Length;
            if (dist <= bestDist)
            {
                bestDist = dist;
                nearest = poster;
            }
        }

        if (nearest == null) return false;
        nearest.Dispose();
        _posters.Remove(nearest);
        return true;
    }

                                                                            
                                                                           
                                                                                
                                                                               
                                                                               
                                                                            
                                                                       
                                                                           
                                                                          
                                                                         
    private record struct SavedPoster(string? ImagePath, float X, float Y, float Z, float Scale, float NormalX, float NormalY, float NormalZ, string? VideoPath);

                                                                                   
                                                                                  
                                                               
    public void SavePosterPlacements()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var list = _posters.Select(p =>
            {
                var normal = Vector3.Cross(p.Up, p.Right);                                                                   
                return new SavedPoster(p.ImagePath, p.Position.X, p.Position.Y, p.Position.Z, p.Scale, normal.X, normal.Y, normal.Z, p.VideoPath);
            }).ToList();
            AtomicFile.WriteAllText(Path.Combine(SaveDirectory, "posters.json"), JsonSerializer.Serialize(list));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось сохранить постеры: {ex.Message}");
        }
    }

                                                                                  
                                                                        
    private void LoadPosterPlacements()
    {
        var path = Path.Combine(SaveDirectory, "posters.json");
        if (!File.Exists(path)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<SavedPoster>>(File.ReadAllText(path));
            if (list == null) return;
            foreach (var p in list)
            {
                var normal = new Vector3(p.NormalX, p.NormalY, p.NormalZ);
                                                                                 
                                                                                       
                if (normal.LengthSquared < 0.01f) normal = -Vector3.UnitZ;
                _posters.Add(new PosterInstance(p.ImagePath, p.VideoPath, new Vector3(p.X, p.Y, p.Z), normal, p.Scale));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось загрузить постеры: {ex.Message}");
        }
    }
}
