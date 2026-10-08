using System.Text.Json;

namespace RainCore.EditorApp;

             
                                                                       
                                                                         
                                                                       
                                                                 
              
public sealed class EditorSettings
{
    public string DefaultNewProjectParentFolder { get; set; } = string.Empty;
    public float CameraMouseSensitivity { get; set; } = 0.15f;

                                                                     
                                                                             
                                                                             
                                                                           
                                                                      
                                                                      
    public int AutosaveIntervalSeconds { get; set; } = 0;

                                                                       
                                                                             
                                                                             
                                                                              
                                                                               
                                                                                
                                                               
                                                                               
                                                        
    public string PlayerCsprojPath { get; set; } = string.Empty;

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RainCore.Editor", "editor_settings.json");

    public static EditorSettings Load()
    {
        var path = FilePath;
        if (!File.Exists(path)) return new EditorSettings();
        try
        {
            return JsonSerializer.Deserialize<EditorSettings>(File.ReadAllText(path), Options) ?? new EditorSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Editor] Не удалось прочитать editor_settings.json: {ex.Message}");
            return new EditorSettings();
        }
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, Options));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Editor] Не удалось сохранить editor_settings.json: {ex.Message}");
        }
    }
}
