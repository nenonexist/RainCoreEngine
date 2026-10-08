using ImGuiNET;
using OpenTK.Mathematics;

namespace RainCore.EditorApp;

             
                                                                                                                   
                                                                                                         
                                                                                                           
                                                                                    
                                                                                                              
              
public sealed partial class EditorWindow
{
    public const string PrefabSuffix = ".prefab.json";

    private bool _requestSavePrefabPopup;
    private string _prefabNameBuffer = "NewPrefab";

    private string PrefabsDir => Path.Combine(_projectPath, "Prefabs");

                                                                                                                   
    private static object? CloneInto(MeshRoomLevel target, object source, Vector3 delta)
    {
        switch (source)
        {
            case ModelInstance m:
            {
                var copy = target.AddModel(m.Model, m.ModelKey, m.Position + delta, m.RotationDeg, m.Scale);
                copy.IsSolid = m.IsSolid;
                copy.Visible = m.Visible;
                return copy;
            }
            case TriggerZone t:
            {
                var pages = t.Pages.Count > 0
                    ? t.Pages.Select(p => new EventPage(p.RequiredFlag, p.RequiredFlagValue, new List<EventCommand>(p.Commands))).ToList()
                    : null;
                var copy = new TriggerZone(t.Center + delta, t.Radius, t.Type, new List<EventCommand>(t.Commands),
                    t.RequiredFlag, t.RequiredFlagValue, t.RunOnce, t.ChancePerCheck, t.CheckIntervalSeconds,
                    pages: pages, markerPath: t.MarkerPath, markerScale: t.MarkerScale,
                    doorInteriorId: t.DoorInteriorId, doorWidth: t.DoorWidth, doorHeight: t.DoorHeight, doorDepth: t.DoorDepth,
                    movement: t.Movement, patrolPoints: t.PatrolPoints.ToArray());
                target.AddTrigger(copy);
                return copy;
            }
            case PosterInstance p:
            {
                var normal = Vector3.Cross(p.Up, p.Right);
                var copy = new PosterInstance(p.ImagePath, p.VideoPath, p.Position + delta, normal, p.Scale);
                target.AddPoster(copy);
                return copy;
            }
            case MoodZone z:
            {
                var copy = new MoodZone(z.Center + delta, z.Radius, z.TargetValue, z.PullRatePerSecond, z.ReleaseSeconds);
                target.AddMoodZone(copy);
                return copy;
            }
            case MoodEvent e:
            {
                var copy = new MoodEvent(e.Center + delta, e.Radius, e.Effect, e.CooldownSeconds, e.BaseChance, e.SoundName);
                target.AddMoodEvent(copy);
                return copy;
            }
        }
        return null;
    }

    private static string SanitizePrefabName(string raw)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(raw.Trim().Where(c => !invalid.Contains(c) && c != '.').ToArray());
    }

    private void SavePrefabFromSelection(string name)
    {
        var selected = AllSelected().ToList();
        if (selected.Count == 0) { _statusMessage = "Select something first, then save it as a prefab."; return; }

        try
        {
                                                                                                       
            var center = SelectionCenter();
            var temp = new MeshRoomLevel(name);
            foreach (var o in selected) CloneInto(temp, o, -center);

            Directory.CreateDirectory(PrefabsDir);
            File.WriteAllText(Path.Combine(PrefabsDir, name + PrefabSuffix), EditorSceneIO.SerializeToJson(temp, name, 1));
            _statusMessage = $"Prefab saved: Prefabs/{name}{PrefabSuffix} ({selected.Count} object(s))";
        }
        catch (Exception ex)
        {
            _statusMessage = $"Could not save prefab: {ex.Message}";
        }
    }

    private void InstantiatePrefabAt(string path, Vector3 point)
    {
        try
        {
            var json = File.ReadAllText(path);
            var temp = EditorSceneIO.DeserializeFromJson(json, Path.GetFileNameWithoutExtension(path), _projectPath, _modelCache, out _);

            var created = new List<object>();
            var sources = new List<object>();
            sources.AddRange(temp.Models);
            sources.AddRange(temp.Triggers);
            sources.AddRange(temp.Posters);
            sources.AddRange(temp.MoodZones);
            sources.AddRange(temp.MoodEvents);
            foreach (var o in sources)
                if (CloneInto(_level, o, point) is { } copy) created.Add(copy);

            if (created.Count == 0) { _statusMessage = "This prefab is empty."; return; }
            SelectAny(created[0]);
            _extraSelection.AddRange(created.Skip(1));
            _statusMessage = $"Placed prefab \"{Path.GetFileName(path).Replace(PrefabSuffix, string.Empty)}\" ({created.Count} object(s)).";
        }
        catch (Exception ex)
        {
            _statusMessage = $"Could not place prefab: {ex.Message}";
        }
    }

                                                                                                        
    private void DrawSavePrefabPopup()
    {
        if (_requestSavePrefabPopup) { ImGui.OpenPopup("SavePrefabPopup"); _requestSavePrefabPopup = false; }

        bool open = true;
        if (!ImGui.BeginPopupModal("SavePrefabPopup", ref open, ImGuiWindowFlags.AlwaysAutoResize)) return;

        ImGui.Text($"Save {SelectionCount} selected object(s) as a prefab.");
        ImGui.InputText("Name", ref _prefabNameBuffer, 64);
        var clean = SanitizePrefabName(_prefabNameBuffer);
        bool exists = clean.Length > 0 && File.Exists(Path.Combine(PrefabsDir, clean + PrefabSuffix));

        if (clean.Length == 0) ImGui.TextColored(new System.Numerics.Vector4(1f, 0.5f, 0.4f, 1f), "Enter a valid name.");
        else if (exists) ImGui.TextColored(new System.Numerics.Vector4(1f, 0.8f, 0.3f, 1f), "A prefab with this name exists - it will be replaced.");

        ImGui.BeginDisabled(clean.Length == 0);
        if (ImGui.Button("Save")) { SavePrefabFromSelection(clean); ImGui.CloseCurrentPopup(); }
        ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();
        ImGui.EndPopup();
    }
}
