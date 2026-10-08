using System.Text.Json;

namespace RainCore;

             
                                                                           
                                                                       
                                                                       
                                                                 
                                                                                   
                                                        
                                                                             
                                                                         
                                                                            
                                                                            
                                                                  
   
                                                                              
                                                                         
                                                                              
                                                                           
                   
              
public static class Level
{
    public static readonly string LevelsDirectory = Path.Combine(AppContext.BaseDirectory, "Levels");

    private record struct LevelBlockEntry(int X, int Y, int Z, BlockType Type, BlockShape Shape);
    private record struct LevelModelEntry(string ModelName, float X, float Y, float Z, float RotX, float RotY, float RotZ, float Scale);
    private record struct LevelFile(List<LevelBlockEntry> Blocks, List<LevelModelEntry> Models, int? DoorX, int? DoorY, int? DoorZ);

                                                                                          
                                                                                                      
    public static void Save(string name, Prefab prefab)
    {
        Directory.CreateDirectory(LevelsDirectory);
        var file = new LevelFile(
            prefab.Blocks.Select(b => new LevelBlockEntry(b.Offset.X, b.Offset.Y, b.Offset.Z, b.Type, b.Shape)).ToList(),
            prefab.Models.Select(m => new LevelModelEntry(m.ModelName, m.Offset.X, m.Offset.Y, m.Offset.Z,
                m.RotationDeg.X, m.RotationDeg.Y, m.RotationDeg.Z, m.Scale)).ToList(),
            prefab.DoorOffset?.X, prefab.DoorOffset?.Y, prefab.DoorOffset?.Z);
        AtomicFile.WriteAllText(Path.Combine(LevelsDirectory, name + ".json"), JsonSerializer.Serialize(file));
    }

                                                                                            
                                                                                
                                                                                              
    public static Prefab? Load(string name)
    {
        var path = Path.Combine(LevelsDirectory, name + ".json");
        if (!File.Exists(path))
        {
            Console.WriteLine($"[Осознанный сон] Уровень \"{name}\" не найден: {path}");
            return null;
        }

        try
        {
            var file = JsonSerializer.Deserialize<LevelFile>(File.ReadAllText(path));
            var prefab = new Prefab();
            foreach (var b in file.Blocks)
                prefab.Block(b.X, b.Y, b.Z, b.Type, b.Shape);
            foreach (var m in file.Models)
                prefab.Model(m.ModelName, m.X, m.Y, m.Z, new OpenTK.Mathematics.Vector3(m.RotX, m.RotY, m.RotZ), m.Scale);
            if (file.DoorX is int dx && file.DoorY is int dy && file.DoorZ is int dz)
                prefab.Door(dx, dy, dz);
            return prefab;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Осознанный сон] Не удалось загрузить уровень \"{name}\": {ex.Message}");
            return null;
        }
    }

                                                                                  
                                                                                 
                                         
    public static List<string> ListNames()
    {
        if (!Directory.Exists(LevelsDirectory)) return new List<string>();
        return Directory.GetFiles(LevelsDirectory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!)
            .OrderBy(n => n)
            .ToList();
    }
}
