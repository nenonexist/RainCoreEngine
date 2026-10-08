using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RainCore.EditorApp;

public enum PaletteMode { Commands, Everything }

             
                                                                                                   
                                                                                                           
                                                                                  
                                                                
              
public sealed partial class EditorWindow
{
    private sealed record PaletteEntry(string Category, string Label, string Shortcut, Action Run, bool Enabled = true);

    private bool _paletteOpen;
    private bool _paletteJustOpened;
    private PaletteMode _paletteMode = PaletteMode.Commands;
    private string _paletteQuery = string.Empty;
    private int _paletteIndex;
    private bool _showShortcutsWindow;

    private void OpenPalette(PaletteMode mode)
    {
        if (!_projectLoaded) return;
        _paletteMode = mode;
        _paletteQuery = string.Empty;
        _paletteIndex = 0;
        _paletteOpen = true;
        _paletteJustOpened = true;
    }

    private (Vector3 Origin, Vector3 Direction) CameraRay() => (_flyCamera!.Camera.Position, _flyCamera.Camera.Front);

    private void PlaceFromCamera(Action<Vector3, Vector3> place)
    {
        var (o, d) = CameraRay();
        place(o, d);
    }

    private List<PaletteEntry> BuildPaletteEntries(bool everything)
    {
        bool edit = !_playMode;
        bool hasSel = GetSelectedPosition() != null && edit;
        var list = new List<PaletteEntry>
        {
            new("Scene", "Save Scene", "Ctrl+S", SaveCurrentScene, edit),
            new("Scene", "New Scene...", "", () => { _newSceneNameBuffer = "NewScene"; _requestNewScenePopup = true; }, edit),
            new("Scene", "Next Scene", "Ctrl+Tab", () => CycleScene(+1), _sceneTabs.Count > 1),
            new("Scene", "Previous Scene", "Ctrl+Shift+Tab", () => CycleScene(-1), _sceneTabs.Count > 1),
            new("Scene", "Open Scenes Panel", "", () => _showScenesPanel = true),
            new("Scene", "Open Environment", "", () => _showEnvironmentWindow = true),
            new("Scene", "Set Current Scene as Startup", "", () => SetStartupScene(_sceneName), edit),

            new("Create", "Add Floor", "", () => PlaceFromCamera((o, d) => PlaceAssetAt(EditorSceneIO.PrimitivePrefix + "floor", o, d)), edit),
            new("Create", "Add Wall", "", () => PlaceFromCamera((o, d) => PlaceAssetAt(EditorSceneIO.PrimitivePrefix + "wall", o, d)), edit),
            new("Create", "Add Ceiling", "", () => PlaceFromCamera((o, d) => PlaceAssetAt(EditorSceneIO.PrimitivePrefix + "ceiling", o, d)), edit),
            new("Create", "Add Point Event", "Shift+E", () => PlaceFromCamera(PlaceEventAtRay), edit),
            new("Create", "Add Trigger Zone", "Shift+T", () => PlaceFromCamera((o, d) => PlaceTriggerAtRay(o, d)), edit),
            new("Create", "Add Mood Zone", "Shift+M", () => PlaceFromCamera(PlaceMoodZoneAtRay), edit),
            new("Create", "Add Mood Event", "Shift+G", () => PlaceFromCamera(PlaceMoodEventAtRay), edit),
            new("Create", "Player Start Here", "", PlayerStartAtCamera, edit),
            new("Create", "New Script...", "", () => { _newScriptName = "NewScript"; _requestNewScriptPopup = true; }, edit),

            new("Edit", "Undo", "Ctrl+Z", () => DoUndo(), CanUndo),
            new("Edit", "Redo", "Ctrl+Y", () => DoRedo(), CanRedo),
            new("Edit", "Duplicate", "Ctrl+D", DuplicateSelected, hasSel),
            new("Edit", "Delete", "Del", DeleteSelected, hasSel),
            new("Edit", "Select All", "Ctrl+A", SelectAllObjects, edit),
            new("Edit", "Save Selection as Prefab...", "", () => { _prefabNameBuffer = "NewPrefab"; _requestSavePrefabPopup = true; }, hasSel),
            new("Edit", "Select Previous", "Alt+Left", () => SelectionHistoryStep(-1), CanSelectionHistoryBack),
            new("Edit", "Select Next", "Alt+Right", () => SelectionHistoryStep(+1), CanSelectionHistoryForward),

            new("View", "Focus Selected", "F", FocusSelected, hasSel),
            new("View", "Frame All", "Home", FrameScene),
            new("View", "Toggle Grid", "", () => _showGrid = !_showGrid),
            new("View", "Toggle Gizmos", "", () => _showGizmos = !_showGizmos),
            new("View", "Normalize View", "", NormalizeView),
            new("View", "Lighting: Scene Lighting", "", () => _lightMode = ViewportLighting.Scene),
            new("View", "Lighting: Editor Lighting", "", () => _lightMode = ViewportLighting.Editor),
            new("View", "Lighting: Unlit", "", () => _lightMode = ViewportLighting.Unlit),
            new("View", "Lighting: Wireframe", "", () => _lightMode = ViewportLighting.Wireframe),
            new("View", "Tool: Select / Move", "W", () => { _editMode = EditMode.Select; _gizmoMode = GizmoMode.Move; }),
            new("View", "Tool: Rotate", "E", () => { _editMode = EditMode.Select; _gizmoMode = GizmoMode.Rotate; }),
            new("View", "Tool: Scale", "R", () => { _editMode = EditMode.Select; _gizmoMode = GizmoMode.Scale; }),
            new("View", "Toggle Snap", "", () => _snapEnabled = !_snapEnabled),

            new("Play", "Play Scene", "", PlayScene, edit),
            new("Play", "Play From Here", "", PlayFromHere, edit),
            new("Play", "Play Project", "", PlayProject, edit),
            new("Play", "Stop", "Esc", StopPlayMode, _playMode),

            new("Window", "Open Diagnostics", "", () => _showDiagnosticsWindow = true),
            new("Window", "Open Console", "", () => _showConsoleWindow = true),
            new("Window", "Open History", "", () => _showHistoryWindow = true),
            new("Window", "Open Script Editor", "", () => _showScriptEditor = true),
            new("Window", "Open Database", "", () => _showDatabaseWindow = true),
            new("Window", "Open Settings", "", () => _showSettingsWindow = true),
            new("Window", "Build Game...", "", () => _showBuildWindow = true),
            new("Help", "Keyboard Shortcuts", "F1", () => _showShortcutsWindow = true),
        };

        for (int i = 0; i < 4; i++)
        {
            int slot = i;
            list.Add(new PaletteEntry("Camera", $"Go to Camera Bookmark {slot + 1}", $"Ctrl+{slot + 1}", () => GoToCameraBookmark(slot)));
            list.Add(new PaletteEntry("Camera", $"Save Camera Bookmark {slot + 1}", $"Ctrl+Shift+{slot + 1}", () => SaveCameraBookmark(slot)));
        }

        foreach (var (name, preset) in SceneEnvironment.BuiltInPresets)
        {
            var n = name; var p = preset;
            list.Add(new PaletteEntry("Atmosphere", $"Atmosphere: {n}", "", () => SetEnvironment(p, n), edit));
        }

        if (!everything) return list;

        foreach (var scene in AllSceneNames())
        {
            var s = scene;
            list.Add(new PaletteEntry("Scenes", $"Scene / {s}", "", () => SwitchToScene(s), edit));
        }

        for (int i = 0; i < _level.Models.Count; i++)
        {
            var m = _level.Models[i];
            list.Add(new PaletteEntry("Hierarchy", $"Object / {m.ModelKey} #{i}", "", () => { SelectModel(m); FocusSelected(); }, edit));
        }
        for (int i = 0; i < _level.Triggers.Count; i++)
        {
            var t = _level.Triggers[i];
            list.Add(new PaletteEntry("Hierarchy", $"Event / {t.Type} #{i}", "", () => { SelectTrigger(t); FocusSelected(); }, edit));
        }
        for (int i = 0; i < _level.Posters.Count; i++)
        {
            var p = _level.Posters[i];
            list.Add(new PaletteEntry("Hierarchy", $"Poster / {(p.IsVideo ? p.VideoPath : p.ImagePath)} #{i}", "", () => { SelectPoster(p); FocusSelected(); }, edit));
        }

        foreach (var folder in ProjectFolders)
        {
            var dir = Path.Combine(_projectPath, folder);
            if (!Directory.Exists(dir) || folder == SceneFolderName) continue;
            foreach (var file in Directory.EnumerateFiles(dir))
            {
                var f = file; var fd = folder;
                bool isScript = fd == "Scripts" && f.EndsWith(".lua", StringComparison.OrdinalIgnoreCase);
                list.Add(new PaletteEntry(isScript ? "Scripts" : "Assets", $"{fd} / {Path.GetFileName(f)}", "", () =>
                {
                    if (isScript) OpenScriptInEditor(f);
                    else { _selectedProjectFolder = fd; _selectedAsset = f; ClearSceneSelection(); }
                }));
            }
        }

                                                                                           
        list.Add(new PaletteEntry("Settings", "Environment / Fog", "", () => _showEnvironmentWindow = true));
        list.Add(new PaletteEntry("Settings", "Environment / Ambient", "", () => _showEnvironmentWindow = true));
        list.Add(new PaletteEntry("Settings", "Environment / Exposure", "", () => _showEnvironmentWindow = true));
        list.Add(new PaletteEntry("Help", "Help / Why is my scene dark?", "", () => { _lightMode = ViewportLighting.Editor; _showEnvironmentWindow = true; _statusMessage = "Editor Lighting enabled. To brighten the scene itself, raise Ambient Intensity in Environment."; }));
        return list;
    }

    private static bool Matches(string text, string[] words)
    {
        foreach (var w in words)
            if (!text.Contains(w, StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }

    private void DrawPalette()
    {
        if (!_paletteOpen) return;

        var viewport = ImGui.GetMainViewport();
        var width = MathF.Min(560f, viewport.WorkSize.X - 40f);
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(viewport.WorkPos.X + viewport.WorkSize.X * 0.5f, viewport.WorkPos.Y + 70f),
            ImGuiCond.Always, new System.Numerics.Vector2(0.5f, 0f));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(width, 0f));
        ImGui.SetNextWindowFocus();

        const ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.AlwaysAutoResize;
        if (!ImGui.Begin("##CommandPalette", flags)) { ImGui.End(); return; }

        ImGui.TextDisabled(_paletteMode == PaletteMode.Commands ? "Command Palette" : "Search everything (scenes, objects, assets, commands, settings)");

        if (_paletteJustOpened) { ImGui.SetKeyboardFocusHere(); _paletteJustOpened = false; }
        ImGui.SetNextItemWidth(-1);
        bool changed = ImGui.InputTextWithHint("##paletteQuery", "Type to search...", ref _paletteQuery, 128);
        if (changed) _paletteIndex = 0;

        var words = _paletteQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var results = BuildPaletteEntries(_paletteMode == PaletteMode.Everything)
            .Where(e => words.Length == 0 || Matches(e.Category + " " + e.Label, words))
            .Take(12).ToList();

        if (results.Count > 0) _paletteIndex = Math.Clamp(_paletteIndex, 0, results.Count - 1);
        if (ImGui.IsKeyPressed(ImGuiKey.DownArrow)) _paletteIndex = Math.Min(_paletteIndex + 1, Math.Max(results.Count - 1, 0));
        if (ImGui.IsKeyPressed(ImGuiKey.UpArrow)) _paletteIndex = Math.Max(_paletteIndex - 1, 0);

        PaletteEntry? toRun = null;
        ImGui.Separator();
        for (int i = 0; i < results.Count; i++)
        {
            var e = results[i];
            ImGui.BeginDisabled(!e.Enabled);
            if (ImGui.Selectable($"{e.Label}##pal{i}", i == _paletteIndex))
                toRun = e;
            ImGui.EndDisabled();
            if (e.Shortcut.Length > 0)
            {
                ImGui.SameLine(width - 120f);
                ImGui.TextDisabled(e.Shortcut);
            }
        }
        if (results.Count == 0) ImGui.TextDisabled("Nothing found. Try another word.");

        if (ImGui.IsKeyPressed(ImGuiKey.Enter) && results.Count > 0 && results[_paletteIndex].Enabled) toRun = results[_paletteIndex];

        bool close = ImGui.IsKeyPressed(ImGuiKey.Escape);
        ImGui.End();

        if (toRun != null)
        {
            _paletteOpen = false;
            toRun.Run();
        }
        else if (close) _paletteOpen = false;
    }

                                         

    private void HandleGlobalShortcuts()
    {
        var io = ImGui.GetIO();
        var kb = KeyboardState;
        bool ctrl = kb.IsKeyDown(Keys.LeftControl) || kb.IsKeyDown(Keys.RightControl);
        bool shift = kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift);
        bool alt = kb.IsKeyDown(Keys.LeftAlt) || kb.IsKeyDown(Keys.RightAlt);

                                                                      
        if (ctrl && shift && kb.IsKeyPressed(Keys.P)) { OpenPalette(PaletteMode.Commands); return; }
        if (ctrl && !shift && kb.IsKeyPressed(Keys.K)) { OpenPalette(PaletteMode.Everything); return; }
        if (_paletteOpen || io.WantTextInput) return;

        if (kb.IsKeyPressed(Keys.F1)) _showShortcutsWindow = !_showShortcutsWindow;

        if (ctrl && kb.IsKeyPressed(Keys.Tab) && !_playMode) CycleScene(shift ? -1 : +1);

                                                                                                      
        if (ctrl && !shift && !alt && kb.IsKeyPressed(Keys.S) && !_scriptEditorFocused && !_playMode)
            SaveCurrentScene();

        if (_playMode) return;

        if (ctrl && kb.IsKeyPressed(Keys.Z) && !shift) DoUndoShortcut();
        if (ctrl && (kb.IsKeyPressed(Keys.Y) || (shift && kb.IsKeyPressed(Keys.Z)))) DoRedo();

        if (kb.IsKeyPressed(Keys.Delete) && GetSelectedObject() != null) DeleteSelected();
        if (ctrl && !shift && kb.IsKeyPressed(Keys.A)) SelectAllObjects();

        if (alt && kb.IsKeyPressed(Keys.Left)) SelectionHistoryStep(-1);
        if (alt && kb.IsKeyPressed(Keys.Right)) SelectionHistoryStep(+1);

        if (kb.IsKeyPressed(Keys.F2) && _selectedProjectFolder == SceneFolderName) { _scenePrompt = ScenePromptKind.Rename; _scenePromptTarget = _sceneName; _scenePromptBuffer = _sceneName; _openScenePrompt = true; _showScenesPanel = true; }

        Keys[] digits = { Keys.D1, Keys.D2, Keys.D3, Keys.D4 };
        for (int i = 0; i < digits.Length; i++)
        {
            if (!ctrl || !kb.IsKeyPressed(digits[i])) continue;
            if (shift) SaveCameraBookmark(i); else GoToCameraBookmark(i);
        }
    }

    private void DrawShortcutsWindow()
    {
        if (!_showShortcutsWindow) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(480, 520), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Shortcuts", ref _showShortcutsWindow)) { ImGui.End(); return; }

        void Section(string title, (string Keys, string What)[] rows)
        {
            if (!ImGui.CollapsingHeader(title, ImGuiTreeNodeFlags.DefaultOpen)) return;
            foreach (var (k, w) in rows)
            {
                ImGui.TextColored(new System.Numerics.Vector4(0.7f, 0.85f, 1f, 1f), k);
                ImGui.SameLine(190f);
                ImGui.TextUnformatted(w);
            }
        }

        Section("Scene View - camera", new[]
        {
            ("RMB + mouse", "Look around"), ("RMB + W A S D", "Fly"), ("RMB + Q / E", "Down / up"),
            ("RMB + wheel", "Fly speed"), ("Shift / Ctrl while flying", "Faster / slower"),
            ("MMB drag", "Pan"), ("Wheel", "Zoom"), ("Alt + LMB", "Orbit around selection"),
            ("Alt + MMB / Alt + RMB", "Pan / zoom"), ("F", "Focus selected"), ("Home", "Frame whole scene"),
            ("Ctrl + 1..4", "Go to camera bookmark"), ("Ctrl + Shift + 1..4", "Save camera bookmark"),
        });
        Section("Scene View - tools", new[]
        {
            ("Q", "Select"), ("W", "Move"), ("E", "Rotate"), ("R", "Scale"),
            ("Hold Ctrl while dragging", "Snap (toggle Snap in the toolbar to snap always)"),
            ("Shift + P", "Place selected asset"), ("Shift + E", "Point event"), ("Shift + T", "Trigger zone"),
            ("Shift + M / Shift + G", "Mood zone / mood event"), ("Right-click", "Create menu"),
        });
        Section("Editor", new[]
        {
            ("Ctrl + S", "Save scene"), ("Ctrl + Z / Ctrl + Y", "Undo / Redo"), ("Ctrl + D", "Duplicate"),
            ("Delete", "Delete selected"), ("Alt + Left / Right", "Previous / next selection"),
            ("Ctrl + Tab (+ Shift)", "Next / previous scene"), ("Ctrl + K", "Search everything"),
            ("Ctrl + Shift + P", "Command Palette"), ("F1", "This window"), ("Esc", "Stop Play Mode / close palette"),
        });
        Section("Play Mode", new[]
        {
            ("W A S D", "Move"), ("RMB + mouse", "Look"), ("F / E", "Interact"), ("Esc", "Stop"),
        });

        ImGui.End();
    }
}
