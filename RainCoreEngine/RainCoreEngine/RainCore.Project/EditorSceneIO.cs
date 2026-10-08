using System.Text.Json;
using OpenTK.Mathematics;

namespace RainCore;

             
                                                                        
                                                                       
                                                                             
                                                                    
                                                                            
                                                                        
                                                                    
                                                                        
                                                                       
                                                                    
                                                                 
   
                                                       
                                                                             
                                                                                   
                                                                            
                                                                          
                                                                              
                                                                                        
                                                                         
                                                                         
                                                                              
   
                                                                             
                                                                      
                                                                            
                                                                    
                                                                            
                                                                          
                                                                        
              
public sealed record EditorScenePropDto(
    string ModelKey, float X, float Y, float Z,
    float RX, float RY, float RZ, float Scale, bool IsSolid, bool Visible);

                                                                           
                                                                            
                                                                   
                                                                  
                                                                            
                                                                           
                                                                         
                                                                              
                                                                        
                                                                        
public sealed record EditorSceneEventDto(string Id, string DisplayName, float X, float Y, float Z, string ActiveText);

                                                                     
                                                                            
                                                                           
                                                                         
                                                                               
                                                                         
                                                                             
                                                                            
                                                                     
                                                             
   
                                                                             
                                                                         
                                                                    
                                                                     
                                                                    
                                                                         
                                                                         
                                                                          
                                                                       
                                                              
public sealed record EditorSceneTriggerDto(
    float CenterX, float CenterY, float CenterZ, float Radius, TriggerType Type,
    string? RequiredFlag, bool RequiredFlagValue, bool RunOnce,
    float ChancePerCheck, float CheckIntervalSeconds,
    List<SavedCommand> Commands,
    string MarkerPath = "", float MarkerScale = 1f, float YawDegrees = 0f,
    MovementPattern Movement = MovementPattern.Static, List<EditorSceneVec3Dto>? PatrolPoints = null,
    List<EditorScenePageDto>? Pages = null);

                                                                         
                                                                         
                                                                        
public sealed record EditorSceneVec3Dto(float X, float Y, float Z);

                                                                          
                                                                               
                                                                               
                                                                              
                                                                    
                                                                        
public sealed record EditorScenePageDto(string? RequiredFlag, bool RequiredFlagValue, List<SavedCommand> Commands);

                                                                                     
                                                                           
                                         
public sealed record EditorScenePosterDto(string? ImagePath, string? VideoPath,
    float X, float Y, float Z, float Scale, float NormalX, float NormalY, float NormalZ);

public sealed record EditorSceneMoodZoneDto(
    float CenterX, float CenterY, float CenterZ, float Radius,
    float TargetValue, float PullRatePerSecond, float ReleaseSeconds);

public sealed record EditorSceneMoodEventDto(
    float CenterX, float CenterY, float CenterZ, float Radius, MoodEventEffect Effect,
    float CooldownSeconds, float BaseChance, string SoundName);

                                                                      
                                                                      
                                                                         
                                                                       
                                                                       
                                                                        
                                                                       
                                                                           
                                                                      
                                                                             
                                                                           
                                                                          
                                                                       
                                                                            
   
                                                                           
                                                                           
                                                                            
                                                                           
                                                                         
                                                                            
   
                                                                
                                                                        
                                                                      
                                    
public sealed record EditorSceneDto(string Name, List<EditorScenePropDto> Props, List<EditorSceneEventDto> Events,
    int NextEventNumber = 1, List<EditorSceneTriggerDto>? Triggers = null,
    float? PlayerStartX = null, float? PlayerStartY = null, float? PlayerStartZ = null, float PlayerStartYaw = 0f,
    List<EditorScenePosterDto>? Posters = null, List<EditorSceneMoodZoneDto>? MoodZones = null,
    List<EditorSceneMoodEventDto>? MoodEvents = null,
    EditorSceneEnvironmentDto? Environment = null);

                                                                                                 
                                                                                           
                                                             
public sealed record EditorSceneEnvironmentDto(
    float BackgroundR, float BackgroundG, float BackgroundB, float AmbientIntensity,
    bool FogEnabled, float FogR, float FogG, float FogB, float FogStart, float FogEnd, float FogOpacity,
    float Exposure, float Contrast, float Saturation, float VertexSnap, float DitherStrength, float DitherLevels);

public static class EditorSceneIO
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

                                                                                 
                                                                   
                                                                            
                                                        
    public const string PrimitivePrefix = "primitive:";

    private static string ScenesDir(string projectPath) => Path.Combine(projectPath, "Scenes");
    private static string ModelsDir(string projectPath) => Path.Combine(projectPath, "Models");
    private static string ScenePath(string projectPath, string sceneName) =>
        Path.Combine(ScenesDir(projectPath), sceneName + ".roomscene.json");

                                                                                        
                                                                              
                                                                           
                                                                           
                                                                                
                                                                          
                                                                       
       
                                                                                
                                                                            
                                                                                   
    public static MeshRoomLevel LoadOrCreate(string projectPath, string sceneName,
        Dictionary<string, GlbModel> modelCache, out int nextEventNumber)
    {
        nextEventNumber = 1;

        var path = ScenePath(projectPath, sceneName);
        if (!File.Exists(path)) return new MeshRoomLevel(sceneName);

        try
        {
            return FromDto(JsonSerializer.Deserialize<EditorSceneDto>(File.ReadAllText(path), Options),
                sceneName, projectPath, modelCache, out nextEventNumber);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Editor] Failed to load scene \"{sceneName}\": {ex.Message}");
            return new MeshRoomLevel(sceneName);
        }
    }

                                                                                  
                                                                             
                                                                                 
                                                                                     
                                                                         
                                                                                   
                                                                            
                                                                             
                                                                             
                                                  
    public static string SerializeToJson(MeshRoomLevel level, string sceneName, int nextEventNumber) =>
        JsonSerializer.Serialize(BuildDto(level, sceneName, nextEventNumber), Options);

                                                                                    
                                                                              
                                                                                 
                                                                                 
                                                         
    public static MeshRoomLevel DeserializeFromJson(string json, string sceneName, string projectPath,
        Dictionary<string, GlbModel> modelCache, out int nextEventNumber) =>
        FromDto(JsonSerializer.Deserialize<EditorSceneDto>(json, Options), sceneName, projectPath, modelCache, out nextEventNumber);

    private static MeshRoomLevel FromDto(EditorSceneDto? dto, string sceneName, string projectPath,
        Dictionary<string, GlbModel> modelCache, out int nextEventNumber)
    {
        var level = new MeshRoomLevel(sceneName);
        nextEventNumber = 1;
        if (dto == null) return level;

        foreach (var p in dto.Props)
        {
            var model = ResolveModel(projectPath, p.ModelKey, modelCache);
            if (model == null) continue;                                                                   

            var instance = level.AddModel(model, p.ModelKey,
                new Vector3(p.X, p.Y, p.Z), new Vector3(p.RX, p.RY, p.RZ), p.Scale);
            instance.IsSolid = p.IsSolid;
            instance.Visible = p.Visible;
        }

        foreach (var e in dto.Events)
        {
            level.AddInteractable(new InteractableObject(e.Id, e.DisplayName,
                new Vector3(e.X, e.Y, e.Z), activeText: e.ActiveText));
        }

        foreach (var t in dto.Triggers ?? new List<EditorSceneTriggerDto>())
        {
            List<EventCommand> commands;
            List<EventPage> pages;
            try
            {
                commands = t.Commands.Select(EventCommandCodec.Decode).ToList();
                pages = (t.Pages ?? new List<EditorScenePageDto>()).Select(p => new EventPage(
                    p.RequiredFlag, p.RequiredFlagValue,
                    p.Commands.Select(EventCommandCodec.Decode).ToList())).ToList();
            }
            catch (Exception ex)
            {
                                                                                   
                                                                                
                                                                                 
                                         
                Console.WriteLine($"[Editor] Trigger at ({t.CenterX},{t.CenterY},{t.CenterZ}) not loaded: {ex.Message}");
                continue;
            }

            var trigger = new TriggerZone(
                new Vector3(t.CenterX, t.CenterY, t.CenterZ), t.Radius, t.Type, commands,
                t.RequiredFlag, t.RequiredFlagValue, t.RunOnce, t.ChancePerCheck, t.CheckIntervalSeconds,
                pages: pages.Count > 0 ? pages : null,
                markerPath: t.MarkerPath, markerScale: t.MarkerScale,
                movement: t.Movement, patrolPoints: (t.PatrolPoints ?? new List<EditorSceneVec3Dto>())
                    .Select(p => new Vector3(p.X, p.Y, p.Z)).ToArray());
            level.AddTrigger(trigger);
        }

        foreach (var p in dto.Posters ?? new List<EditorScenePosterDto>())
        {
            var normal = new Vector3(p.NormalX, p.NormalY, p.NormalZ);
                                                                                    
                                                                   
            if (normal.LengthSquared < 0.01f) normal = -Vector3.UnitZ;
            level.AddPoster(new PosterInstance(p.ImagePath, p.VideoPath, new Vector3(p.X, p.Y, p.Z), normal, p.Scale));
        }

        foreach (var zoneDto in dto.MoodZones ?? new List<EditorSceneMoodZoneDto>())
        {
            level.AddMoodZone(new MoodZone(new Vector3(zoneDto.CenterX, zoneDto.CenterY, zoneDto.CenterZ), zoneDto.Radius,
                zoneDto.TargetValue, zoneDto.PullRatePerSecond, zoneDto.ReleaseSeconds));
        }

        foreach (var m in dto.MoodEvents ?? new List<EditorSceneMoodEventDto>())
        {
            level.AddMoodEvent(new MoodEvent(new Vector3(m.CenterX, m.CenterY, m.CenterZ), m.Radius, m.Effect,
                m.CooldownSeconds, m.BaseChance, m.SoundName));
        }

                                                                           
                                                                          
                                                                         
                                                                      
                                                                   
                       
        nextEventNumber = Math.Max(dto.NextEventNumber, InferNextEventNumber(dto.Events));

        if (dto.PlayerStartX is { } x && dto.PlayerStartY is { } y && dto.PlayerStartZ is { } z)
        {
            level.PlayerStartPosition = new Vector3(x, y, z);
            level.PlayerStartYawDegrees = dto.PlayerStartYaw;
        }

        if (dto.Environment != null)
            level.Environment = EnvironmentFromDto(dto.Environment);

        return level;
    }

                                                                                     
                                                                               
                                                                            
                                                                                       
    private static int InferNextEventNumber(List<EditorSceneEventDto> events)
    {
        int max = 0;
        foreach (var e in events)
        {
            var idx = e.Id.LastIndexOf('_');
            if (idx >= 0 && int.TryParse(e.Id.AsSpan(idx + 1), out var n))
                max = Math.Max(max, n);
        }
        return max + 1;
    }

                                                                                    
                                                                                          
                                                                                
                                                                           
                                                                         
                                                                               
                                                                              
                                                                              
                            
    public static void Save(string projectPath, string sceneName, MeshRoomLevel level, int nextEventNumber)
    {
        Directory.CreateDirectory(ScenesDir(projectPath));
        var dto = BuildDto(level, sceneName, nextEventNumber);
        File.WriteAllText(ScenePath(projectPath, sceneName), JsonSerializer.Serialize(dto, Options));
    }

    private static EditorSceneDto BuildDto(MeshRoomLevel level, string sceneName, int nextEventNumber)
    {
        var props = level.Models.Select(m => new EditorScenePropDto(
            m.ModelKey, m.Position.X, m.Position.Y, m.Position.Z,
            m.RotationDeg.X, m.RotationDeg.Y, m.RotationDeg.Z,
            m.Scale, m.IsSolid, m.Visible)).ToList();

        var events = level.Interactables.Select(e => new EditorSceneEventDto(
            e.Id, e.DisplayName, e.Position.X, e.Position.Y, e.Position.Z, e.ActiveText)).ToList();

        var triggers = new List<EditorSceneTriggerDto>();
        foreach (var t in level.Triggers)
        {
            List<SavedCommand> commands;
            List<EditorScenePageDto> pages;
            try
            {
                commands = t.Commands.Select(EventCommandCodec.Encode).ToList();
                pages = t.Pages.Select(p => new EditorScenePageDto(
                    p.RequiredFlag, p.RequiredFlagValue,
                    p.Commands.Select(EventCommandCodec.Encode).ToList())).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Editor] Trigger at {t.Center} not saved (command encode failed): {ex.Message}");
                continue;
            }

            triggers.Add(new EditorSceneTriggerDto(
                t.Center.X, t.Center.Y, t.Center.Z, t.Radius, t.Type,
                t.RequiredFlag, t.RequiredFlagValue, t.RunOnce,
                t.ChancePerCheck, t.CheckIntervalSeconds, commands,
                t.MarkerPath, t.MarkerScale, t.YawDegrees,
                t.Movement, t.PatrolPoints.Select(p => new EditorSceneVec3Dto(p.X, p.Y, p.Z)).ToList(),
                pages.Count > 0 ? pages : null));
        }

        var posters = level.Posters.Select(p =>
        {
                                                                                  
                                                          
            var normal = Vector3.Cross(p.Up, p.Right);
            return new EditorScenePosterDto(p.ImagePath, p.VideoPath, p.Position.X, p.Position.Y, p.Position.Z,
                p.Scale, normal.X, normal.Y, normal.Z);
        }).ToList();

        var moodZones = level.MoodZones.Select(z => new EditorSceneMoodZoneDto(
            z.Center.X, z.Center.Y, z.Center.Z, z.Radius, z.TargetValue, z.PullRatePerSecond, z.ReleaseSeconds)).ToList();

        var moodEvents = level.MoodEvents.Select(m => new EditorSceneMoodEventDto(
            m.Center.X, m.Center.Y, m.Center.Z, m.Radius, m.Effect, m.CooldownSeconds, m.BaseChance, m.SoundName)).ToList();

        return new EditorSceneDto(sceneName, props, events, nextEventNumber, triggers,
            level.PlayerStartPosition?.X, level.PlayerStartPosition?.Y, level.PlayerStartPosition?.Z,
            level.PlayerStartYawDegrees, posters, moodZones, moodEvents, EnvironmentToDto(level.Environment));
    }

    public static EditorSceneEnvironmentDto EnvironmentToDto(SceneEnvironment e) => new(
        e.Background.X, e.Background.Y, e.Background.Z, e.AmbientIntensity,
        e.FogEnabled, e.FogColor.X, e.FogColor.Y, e.FogColor.Z, e.FogStart, e.FogEnd, e.FogOpacity,
        e.Exposure, e.Contrast, e.Saturation, e.VertexSnap, e.DitherStrength, e.DitherLevels);

    public static SceneEnvironment EnvironmentFromDto(EditorSceneEnvironmentDto d) => new()
    {
        Background = new Vector3(d.BackgroundR, d.BackgroundG, d.BackgroundB),
        AmbientIntensity = d.AmbientIntensity,
        FogEnabled = d.FogEnabled,
        FogColor = new Vector3(d.FogR, d.FogG, d.FogB),
        FogStart = d.FogStart,
        FogEnd = d.FogEnd,
        FogOpacity = d.FogOpacity,
        Exposure = d.Exposure,
        Contrast = d.Contrast,
        Saturation = d.Saturation,
        VertexSnap = d.VertexSnap,
        DitherStrength = d.DitherStrength,
        DitherLevels = d.DitherLevels,
    };

                                                                      
                                                                             
                                                                               
                                                                                  
                                                                           
                                                                      
                                                                  
                                                                                        
    public static GlbModel? ResolveModel(string projectPath, string modelKey, Dictionary<string, GlbModel> cache)
    {
        if (cache.TryGetValue(modelKey, out var cached)) return cached;

        if (modelKey.StartsWith(PrimitivePrefix, StringComparison.Ordinal))
        {
            var primitive = BuildPrimitive(modelKey);
            if (primitive != null) cache[modelKey] = primitive;
            return primitive;
        }

        var path = Path.Combine(ModelsDir(projectPath), modelKey + ".glb");
        if (!File.Exists(path))
        {
            Console.WriteLine($"[Editor] Model \"{modelKey}.glb\" not found in {ModelsDir(projectPath)}");
            return null;
        }

        try
        {
            var model = GlbModel.LoadFromFile(path);
            cache[modelKey] = model;
            return model;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Editor] Failed to load model \"{modelKey}.glb\": {ex.Message}");
            return null;
        }
    }

                                                                            
                                                                         
                                                                       
                                                                          
                                                                    
                                                                        
    private static GlbModel? BuildPrimitive(string modelKey) => modelKey switch
    {
        "primitive:floor" => GlbModel.CreatePrimitiveBox(new Vector3(4f, 0.2f, 4f), new Vector3(0.42f, 0.40f, 0.38f)),
        "primitive:wall" => GlbModel.CreatePrimitiveBox(new Vector3(4f, 3f, 0.2f), new Vector3(0.55f, 0.53f, 0.50f)),
        "primitive:ceiling" => GlbModel.CreatePrimitiveBox(new Vector3(4f, 0.2f, 4f), new Vector3(0.30f, 0.29f, 0.30f)),
        _ => null,
    };
}
