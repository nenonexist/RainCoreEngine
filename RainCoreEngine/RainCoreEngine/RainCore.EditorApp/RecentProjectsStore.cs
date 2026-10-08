using System.Text.Json;

namespace RainCore.EditorApp;

             
                                                                            
                                                                           
                                                                              
                                                                       
                                                                              
                                                                         
                                                                               
                                                                           
                                                                           
                                   
   
                                                                           
                                                                          
                                                                         
                                   
              
public static class RecentProjectsStore
{
    private const int MaxEntries = 10;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RainCore.Editor", "recent_projects.json");

                                                                           
                                                                          
                                                                              
                                                                           
                                                                       
                                             
    public static List<string> Load()
    {
        var path = FilePath;
        if (!File.Exists(path)) return new List<string>();

        try
        {
            var raw = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path), Options) ?? new List<string>();
            return raw.Where(ProjectDescriptor.Exists).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Editor] Не удалось прочитать recent_projects.json: {ex.Message}");
            return new List<string>();
        }
    }

                                                                                  
                                                                                     
                                                                               
                                                                          
    public static void AddOrPromote(string projectPath)
    {
        var full = Path.GetFullPath(projectPath);
        var list = Load();
        list.RemoveAll(p => string.Equals(Path.GetFullPath(p), full, StringComparison.OrdinalIgnoreCase));
        list.Insert(0, full);
        if (list.Count > MaxEntries) list.RemoveRange(MaxEntries, list.Count - MaxEntries);

        try
        {
            var dir = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(list, Options));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Editor] Не удалось сохранить recent_projects.json: {ex.Message}");
        }
    }
}
