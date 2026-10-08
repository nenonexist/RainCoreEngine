using System.Text.Json;

namespace RainCore;

             
                                                                             
                                                                       
                                                                        
                                                                            
                                                                      
                                                                       
                                                               
                                                                                 
   
                                                                           
                                                        
                                               
              
public sealed record ProjectDescriptor(string Name, DateTime CreatedAtUtc, int FormatVersion, string StartScene,
    string? GameModuleAssembly = null, string? GameModuleTypeName = null)
{
    public const int CurrentFormatVersion = 1;
    private const string FileName = "project.json";

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private static string DescriptorPath(string projectPath) => Path.Combine(projectPath, FileName);

                                                                                     
                                                                                
                                                                                          
    public static bool Exists(string projectPath) => File.Exists(DescriptorPath(projectPath));

                                                                              
                                                                         
                                                                               
                                                                                        
    public static ProjectDescriptor Create(string projectPath, string name)
    {
        if (Directory.Exists(projectPath) && Directory.EnumerateFileSystemEntries(projectPath).Any())
            throw new InvalidOperationException($"Папка \"{projectPath}\" уже существует и не пуста.");

        foreach (var folder in new[] { "Scenes", "Scripts", "Models", "Images", "Sound", "Objects" })
            Directory.CreateDirectory(Path.Combine(projectPath, folder));

        var descriptor = new ProjectDescriptor(name, DateTime.UtcNow, CurrentFormatVersion, StartScene: "Main");
        Save(projectPath, descriptor);
        return descriptor;
    }

                                                                            
                                                                         
                                                                                
                                                                      
    public static ProjectDescriptor? TryLoad(string projectPath)
    {
        var path = DescriptorPath(projectPath);
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<ProjectDescriptor>(File.ReadAllText(path), Options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Project] Не удалось прочитать {path}: {ex.Message}");
            return null;
        }
    }

    public static void Save(string projectPath, ProjectDescriptor descriptor) =>
        File.WriteAllText(DescriptorPath(projectPath), JsonSerializer.Serialize(descriptor, Options));

                                                                                
                                                                             
                                                                           
                                                                                     
    public static IReadOnlyList<string> ListScenes(string projectPath)
    {
        var scenesDir = Path.Combine(projectPath, "Scenes");
        if (!Directory.Exists(scenesDir)) return Array.Empty<string>();

        return Directory.GetFiles(scenesDir, "*.roomscene.json")
            .Select(f => Path.GetFileName(f)[..^".roomscene.json".Length])
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

                                                                                                    
                                                                                         
                                                                                        
                                                                                      
                                                                                        
                                                                               
                                                                                
                                                              
       
                                                                                      
                                                                                           
                                                                                       
                                                                                        
                                                        
    public IGameModule? TryLoadGameModule(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(GameModuleAssembly) || string.IsNullOrWhiteSpace(GameModuleTypeName))
            return null;

        try
        {
            var assemblyPath = Path.Combine(projectPath, GameModuleAssembly);
            if (!File.Exists(assemblyPath))
            {
                Console.WriteLine($"[Project] GameModuleAssembly \"{assemblyPath}\" не найден - запуск без геймплейного слоя.");
                return null;
            }

            var assembly = System.Reflection.Assembly.LoadFrom(assemblyPath);
            var type = assembly.GetType(GameModuleTypeName);
            if (type == null)
            {
                Console.WriteLine($"[Project] Тип \"{GameModuleTypeName}\" не найден в \"{GameModuleAssembly}\" - запуск без геймплейного слоя.");
                return null;
            }

            if (Activator.CreateInstance(type) is not IGameModule module)
            {
                Console.WriteLine($"[Project] Тип \"{GameModuleTypeName}\" не реализует IGameModule или у него нет публичного конструктора без параметров - запуск без геймплейного слоя.");
                return null;
            }

            return module;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Project] Не удалось загрузить GameModule (\"{GameModuleAssembly}\" / \"{GameModuleTypeName}\"): {ex.Message} - запуск без геймплейного слоя.");
            return null;
        }
    }
}
