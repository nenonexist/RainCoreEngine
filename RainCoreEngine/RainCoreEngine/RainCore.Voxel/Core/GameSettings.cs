using System;
using System.IO;
using System.Text.Json;

namespace RainCore;

public enum GameWindowMode
{
    Normal,
    Maximized,
    Fullscreen
}

public sealed class GameSettings
{
    public float SfxVolume { get; set; } = 0.7f;
    public float MusicVolume { get; set; } = 0.5f;
    public float AmbientVolume { get; set; } = 0.25f;
    public GameWindowMode WindowMode { get; set; } = GameWindowMode.Normal;

                                                                                     
                                                                                        
                                                                                    
                                                                                    
                                                                                    
                                                                                   
                                                                                    
                                                                                  
                                                                                  
                                                            
    public float RenderDistance { get; set; } = 160f;

                                                                                  
                                                                                
                                                                                        
    public float VertexSnapGrid { get; set; } = 0f;

                                                                                  
                                                                                   
                                                                 
    public bool BloomEnabled { get; set; } = true;

                                                                                   
                                                                                  
                                                            
    public float BloomStrength { get; set; } = 0.45f;

                                                                               
                                                                                
                                                                                          
    public bool ShadowsEnabled { get; set; } = true;

    public float ShadowDistance { get; set; } = 96f;

    private static string SaveFilePath => Path.Combine(AppContext.BaseDirectory, "settings.json");

                                                                                      
                                                                                 
    public static GameSettings Current { get; private set; } = Load();

    public static GameSettings Load()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                var json = File.ReadAllText(SaveFilePath);
                var loaded = JsonSerializer.Deserialize<GameSettings>(json);
                if (loaded != null)
                    return loaded;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameSettings] Failed to load settings.json: {ex.Message}");
        }

        return new GameSettings();
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SaveFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameSettings] Failed to save settings.json: {ex.Message}");
        }
    }
}
