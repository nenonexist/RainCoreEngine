using System.Text.Json;
using ImGuiNET;
using OpenTK.Mathematics;

namespace RainCore.EditorApp;

             
                                                                                                                 
                                                                                                                
                                                                                                           
                                                                                             
              
public sealed partial class EditorWindow
{
    private bool _showEnvironmentWindow;
    private SceneEnvironment? _envPreview;
    private string _envPreviewName = string.Empty;
    private string _envPresetName = "My Preset";
    private List<(string Name, SceneEnvironment Env)>? _userPresets;

    private SceneEnvironment EffectiveEnvironment() => _playMode ? _level.Environment : (_envPreview ?? _level.Environment);

    private string UserPresetsPath => Path.Combine(_projectPath, "environment_presets.json");

    private sealed record UserPresetDto(string Name, EditorSceneEnvironmentDto Env);

    private List<(string Name, SceneEnvironment Env)> UserPresets()
    {
        if (_userPresets != null) return _userPresets;
        _userPresets = new List<(string, SceneEnvironment)>();
        try
        {
            if (File.Exists(UserPresetsPath))
            {
                var dtos = JsonSerializer.Deserialize<List<UserPresetDto>>(File.ReadAllText(UserPresetsPath));
                if (dtos != null)
                    foreach (var d in dtos) _userPresets.Add((d.Name, EditorSceneIO.EnvironmentFromDto(d.Env)));
            }
        }
        catch (Exception ex) { _statusMessage = $"Could not read environment_presets.json: {ex.Message}"; }
        return _userPresets;
    }

    private void SaveUserPresets()
    {
        try
        {
            var dtos = UserPresets().Select(p => new UserPresetDto(p.Name, EditorSceneIO.EnvironmentToDto(p.Env))).ToList();
            File.WriteAllText(UserPresetsPath, JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) { _statusMessage = $"Could not save preset: {ex.Message}"; }
    }

                                                                                              
    private void SetEnvironment(SceneEnvironment env, string name)
    {
        _envPreview = null;
        _level.Environment = env;
        _statusMessage = $"Atmosphere: {name}";
    }

    private static System.Numerics.Vector3 ToNum(Vector3 v) => new(v.X, v.Y, v.Z);
    private static Vector3 FromNum(System.Numerics.Vector3 v) => new(v.X, v.Y, v.Z);

    private void DrawEnvironmentWindow()
    {
        if (!_showEnvironmentWindow) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(360, 560), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Environment", ref _showEnvironmentWindow)) { ImGui.End(); return; }

        ImGui.TextDisabled($"Scene: {_sceneName}");
        HelpMarker("The atmosphere belongs to the scene: it is saved with it and used by the built game.\nThe Scene View shows it only in 'Scene Lighting' mode.");
        if (_lightMode != ViewportLighting.Scene && !_playMode)
        {
            ImGui.TextColored(new System.Numerics.Vector4(1f, 0.8f, 0.3f, 1f), "Scene View is not in Scene Lighting.");
            ImGui.SameLine();
            if (ImGui.SmallButton("Switch")) _lightMode = ViewportLighting.Scene;
        }
        ImGui.Separator();

        bool previewing = _envPreview != null;
        var env = _envPreview ?? _level.Environment;
        var edited = env;

                          
        ImGui.TextUnformatted("Atmosphere Preset");
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##envPreset", previewing ? $"Preview: {_envPreviewName}" : "Choose a preset..."))
        {
            foreach (var (name, preset) in SceneEnvironment.BuiltInPresets)
                if (ImGui.Selectable(name)) { _envPreview = preset; _envPreviewName = name; }
            var user = UserPresets();
            if (user.Count > 0) { ImGui.Separator(); ImGui.TextDisabled("Project presets"); }
            foreach (var (name, preset) in user)
                if (ImGui.Selectable(name + "##user")) { _envPreview = preset; _envPreviewName = name; }
            ImGui.EndCombo();
        }
        Tip("Pick a preset to PREVIEW it. Your scene is not changed until you press Apply.");

        if (previewing)
        {
            ImGui.TextColored(new System.Numerics.Vector4(0.5f, 0.8f, 1f, 1f), $"Previewing \"{_envPreviewName}\" - not applied yet.");
            if (ImGui.Button("Apply")) { SetEnvironment(_envPreview!, _envPreviewName); env = _level.Environment; edited = env; }
            ImGui.SameLine();
            if (ImGui.Button("Cancel")) { _envPreview = null; env = _level.Environment; edited = env; }
        }
        ImGui.Separator();

                      
        if (ImGui.CollapsingHeader("Sky / Background", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var bg = ToNum(env.Background);
            if (ImGui.ColorEdit3("Background", ref bg)) edited = edited with { Background = FromNum(bg) };
            Tip("Colour of the empty space behind everything.");
        }

                          
        if (ImGui.CollapsingHeader("Ambient", ImGuiTreeNodeFlags.DefaultOpen))
        {
            float amb = env.AmbientIntensity;
            if (ImGui.SliderFloat("Ambient Intensity", ref amb, 0f, 2.5f, "%.2f")) edited = edited with { AmbientIntensity = amb };
            Tip("Intensity of the general scattered light.\n0 = no ambient light\n1 = standard intensity");
        }

                      
        if (ImGui.CollapsingHeader("Fog", ImGuiTreeNodeFlags.DefaultOpen))
        {
            bool fog = env.FogEnabled;
            if (ImGui.Checkbox("Enabled##fog", ref fog)) edited = edited with { FogEnabled = fog };
            ImGui.BeginDisabled(!env.FogEnabled);
            var fogColor = ToNum(env.FogColor);
            if (ImGui.ColorEdit3("Fog Color", ref fogColor)) edited = edited with { FogColor = FromNum(fogColor) };
            ImGui.SameLine();
            if (ImGui.SmallButton("= Background")) edited = edited with { FogColor = env.Background };
            Tip("Make the fog the same colour as the background, so distant objects dissolve into the sky.");
            float start = env.FogStart, end = env.FogEnd, opacity = env.FogOpacity;
            if (ImGui.DragFloat("Fog Start", ref start, 0.2f, 0f, 500f)) edited = edited with { FogStart = MathF.Min(start, env.FogEnd - 0.1f) };
            HelpMarker("Distance from the camera where fog begins.");
            if (ImGui.DragFloat("Fog End", ref end, 0.2f, 0.1f, 1000f)) edited = edited with { FogEnd = MathF.Max(end, env.FogStart + 0.1f) };
            HelpMarker("Distance where objects are fully hidden by fog.");
            if (ImGui.SliderFloat("Fog Opacity", ref opacity, 0f, 1f, "%.2f")) edited = edited with { FogOpacity = opacity };
            ImGui.EndDisabled();
        }

                                  
        if (ImGui.CollapsingHeader("Post Processing", ImGuiTreeNodeFlags.DefaultOpen))
        {
            float exposure = env.Exposure, contrast = env.Contrast, saturation = env.Saturation;
            if (ImGui.SliderFloat("Exposure", ref exposure, 0.2f, 3f, "%.2f")) edited = edited with { Exposure = exposure };
            HelpMarker("Overall brightness of the final image. 1 = unchanged.\nThis is the scene's exposure; the Scene View toolbar has a separate editor-only Exposure.");
            if (ImGui.SliderFloat("Contrast", ref contrast, 0.5f, 1.8f, "%.2f")) edited = edited with { Contrast = contrast };
            if (ImGui.SliderFloat("Saturation", ref saturation, 0f, 2f, "%.2f")) edited = edited with { Saturation = saturation };
        }

                        
        if (ImGui.CollapsingHeader("Retro (PSX) look"))
        {
            float snap = env.VertexSnap, dither = env.DitherStrength, levels = env.DitherLevels;
            if (ImGui.SliderFloat("Vertex Snap", ref snap, 0f, 400f, snap <= 0f ? "off" : "%.0f")) edited = edited with { VertexSnap = snap };
            HelpMarker("PlayStation-style vertex wobble. 0 = off. Lower non-zero values = coarser wobble.\nVisible in Play Mode and the game; the Scene View shows it in Scene Lighting.");
            if (ImGui.SliderFloat("Dither Strength", ref dither, 0f, 1.5f, dither <= 0f ? "off" : "%.2f")) edited = edited with { DitherStrength = dither };
            HelpMarker("Ordered dithering of the final frame. Applied in the built game / Player.");
            if (ImGui.SliderFloat("Dither Levels", ref levels, 4f, 64f, "%.0f")) edited = edited with { DitherLevels = levels };
        }

                                                                             
        if (edited != env)
        {
            if (previewing) _envPreview = edited;
            else _level.Environment = edited;
        }

        ImGui.Separator();
        if (ImGui.Button("Reset to Default"))
        {
            if (previewing) { _envPreview = SceneEnvironment.Default; _envPreviewName = SceneEnvironment.DefaultPresetName; }
            else _level.Environment = SceneEnvironment.Default;
        }

        ImGui.Spacing();
        ImGui.SetNextItemWidth(180);
        ImGui.InputText("##presetName", ref _envPresetName, 48);
        ImGui.SameLine();
        if (ImGui.Button("Save as Preset") && !string.IsNullOrWhiteSpace(_envPresetName))
        {
            var name = _envPresetName.Trim();
            var current = _envPreview ?? _level.Environment;
            var list = UserPresets();
            list.RemoveAll(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            list.Add((name, current));
            SaveUserPresets();
            _statusMessage = $"Preset saved: {name} (environment_presets.json in the project)";
        }
        Tip("Save the current atmosphere as your own preset. Stored in the project, available in every scene.");

        ImGui.End();
    }
}
