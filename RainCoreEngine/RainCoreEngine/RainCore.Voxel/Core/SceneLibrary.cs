using System.Text.Json;
using System.Text.Json.Serialization;

namespace RainCore;

             
                                                                       
                                                                                
                                                                                
                                       
   
                                                                            
                                                                                 
                                                                             
                                                                                  
                                       
              
public static class SceneLibrary
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() },
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static string DirectoryPath => Path.Combine(AppContext.BaseDirectory, "Scenes");

    public static List<SceneDefinition> Load(IReadOnlyList<SceneDefinition> fallbackDefinitions)
    {
        var dir = DirectoryPath;
        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"[Сцены] Папка {dir} не найдена - использую встроенный список сцен по умолчанию.");
            return fallbackDefinitions.ToList();
        }

        var files = Directory.GetFiles(dir, "*.json");
        if (files.Length == 0)
        {
            Console.WriteLine($"[Сцены] В {dir} нет .json-файлов - использую встроенный список сцен по умолчанию.");
            return fallbackDefinitions.ToList();
        }

        var result = new List<SceneDefinition>();
        foreach (var file in files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var def = JsonSerializer.Deserialize<SceneDefinition>(File.ReadAllText(file), Options);
                if (def == null || string.IsNullOrWhiteSpace(def.Id))
                {
                    Console.WriteLine($"[Сцены] Пропущен {Path.GetFileName(file)}: пустой или нечитаемый Id.");
                    continue;
                }
                result.Add(def);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Сцены] Не удалось прочитать {Path.GetFileName(file)}: {ex.Message}");
            }
        }

        if (result.Count == 0)
        {
            Console.WriteLine("[Сцены] Ни один файл в Scenes/ не прочитался - использую встроенный список сцен по умолчанию.");
            return fallbackDefinitions.ToList();
        }

        result.Sort((a, b) => a.Order.CompareTo(b.Order));
        return result;
    }
}
