using System.Text.Json;

namespace RainCore;

             
                                                                   
                                                                         
                                                                           
                                                                              
              
public static class GameFlags
{
    private static readonly Dictionary<string, bool> Switches = new();
    private static readonly Dictionary<string, int> Variables = new();

    public static bool Get(string name)
    {
        return Switches.TryGetValue(name, out var v) && v;
    }


    public static void Set(string name, bool value)
    {
        Switches[name] = value;
    }


    public static int GetVar(string name)
    {
        return Variables.TryGetValue(name, out var v) ? v : 0;
    }


    public static void SetVar(string name, int value)
    {
        Variables[name] = value;
    }


    public static void AddVar(string name, int delta)
    {
        Variables[name] = GetVar(name) + delta;
    }

    private static string SaveFilePath => Path.Combine(AppContext.BaseDirectory, "Saves", "flags.json");

    private record struct SaveFile(Dictionary<string, bool> Switches, Dictionary<string, int> Variables);

                                                                                                
                                                                                         
    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath)!);
            var data = new SaveFile(new Dictionary<string, bool>(Switches), new Dictionary<string, int>(Variables));
            AtomicFile.WriteAllText(SaveFilePath, JsonSerializer.Serialize(data));
            Console.WriteLine($"[Флаги] Сохранено: switches={Switches.Count}, variables={Variables.Count} -> {SaveFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Флаги] Не удалось сохранить: {ex.Message}");
        }
    }

                                                                                                
                                                                                             
                                                   
    public static void Load()
    {
        if (!File.Exists(SaveFilePath)) return;
        try
        {
            var data = JsonSerializer.Deserialize<SaveFile>(File.ReadAllText(SaveFilePath));
            Switches.Clear();
            Variables.Clear();
            foreach (var kv in data.Switches ?? new()) Switches[kv.Key] = kv.Value;
            foreach (var kv in data.Variables ?? new()) Variables[kv.Key] = kv.Value;
            Console.WriteLine($"[Флаги] Загружено: switches={Switches.Count}, variables={Variables.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Флаги] Не удалось загрузить: {ex.Message}");
        }
    }
}