using System.Text.Json;

namespace RainCore;

             
                                                        
                                                                      
                                                                         
                                                                       
                                                                     
                                                                         
                                                                             
                                                           
              
public sealed class GameDatabase
{
    public List<ActorData> Actors { get; set; } = new();
    public List<SkillData> Skills { get; set; } = new();
    public List<ItemData> Items { get; set; } = new();
    public List<EnemyData> Enemies { get; set; } = new();
    public List<TroopData> Troops { get; set; } = new();

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private static string FilePath(string projectPath) => Path.Combine(projectPath, "Database", "database.json");

                                                                             
                                                                            
                                                                                    
    public static GameDatabase Load(string projectPath)
    {
        var path = FilePath(projectPath);
        if (!File.Exists(path)) return new GameDatabase();

        try
        {
            return JsonSerializer.Deserialize<GameDatabase>(File.ReadAllText(path), Options) ?? new GameDatabase();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Database] Не удалось прочитать {path}: {ex.Message}");
            return new GameDatabase();
        }
    }

    public void Save(string projectPath)
    {
        var path = FilePath(projectPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, Options));
    }

    public ActorData? FindActor(string id) => Actors.FirstOrDefault(a => a.Id == id);
    public SkillData? FindSkill(string id) => Skills.FirstOrDefault(s => s.Id == id);
    public ItemData? FindItem(string id) => Items.FirstOrDefault(i => i.Id == id);
    public EnemyData? FindEnemy(string id) => Enemies.FirstOrDefault(e => e.Id == id);
    public TroopData? FindTroop(string id) => Troops.FirstOrDefault(t => t.Id == id);
}
