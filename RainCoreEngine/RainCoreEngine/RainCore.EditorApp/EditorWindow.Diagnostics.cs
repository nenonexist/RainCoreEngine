using ImGuiNET;

namespace RainCore.EditorApp;

             
                                                                                             
                                                                                                              
                                                                                                          
                                                                                                              
              
public sealed partial class EditorWindow
{
    private enum DiagSeverity { Info, Warning, Error }

    private sealed record DiagItem(DiagSeverity Severity, string Text, string Why, Action? Locate = null, string FixLabel = "", Action? Fix = null);

    private bool _showDiagnosticsWindow;
    private List<DiagItem> _diagnostics = new();
    private double _diagnosticsAge = 999;
    private List<(string File, string Error, int Line)> _scriptErrorsCache = new();
    private double _scriptScanAge = 999;

    public int DiagnosticsWarningCount => _diagnostics.Count(d => d.Severity != DiagSeverity.Info);

    private void RefreshDiagnostics(bool force = false)
    {
        _diagnosticsAge += _lastFrameDelta;
        _scriptScanAge += _lastFrameDelta;
        if (!force && _diagnosticsAge < 0.75) return;
        _diagnosticsAge = 0;

        var list = new List<DiagItem>();
        var camPos = _flyCamera!.Camera.Position;
        var camFront = _flyCamera.Camera.Front;

        if (_level.Models.Count == 0 && _level.Posters.Count == 0)
            list.Add(new DiagItem(DiagSeverity.Warning, "Scene has no geometry",
                "Without floors, walls or props there is nothing to see or stand on.",
                null, "Add Floor", () => PlaceAssetAt(EditorSceneIO.PrimitivePrefix + "floor", camPos, camFront)));

        if (_level.PlayerStartPosition == null)
            list.Add(new DiagItem(DiagSeverity.Warning, "Player Start is not set",
                "Play Mode and the built game will use a default spawn point instead of the one you want.",
                null, "Set at camera", PlayerStartAtCamera));

        if (_currentProjectDescriptor != null && !AllSceneNames().Contains(_currentProjectDescriptor.StartScene, StringComparer.OrdinalIgnoreCase))
            list.Add(new DiagItem(DiagSeverity.Error, $"Startup scene \"{_currentProjectDescriptor.StartScene}\" does not exist",
                "Play Project and the built game start from this scene.",
                () => _showScenesPanel = true, "Use current scene", () => SetStartupScene(_sceneName)));

        foreach (var trig in _level.Triggers)
        {
            var t = trig;
            if (t.Commands.Count == 0 && t.Pages.Count == 0)
                list.Add(new DiagItem(DiagSeverity.Info, "An event has no commands",
                    "The trigger exists but does nothing when activated.", () => { SelectTrigger(t); FocusSelected(); }));

            if (!string.IsNullOrWhiteSpace(t.MarkerPath)
                && !t.MarkerPath.StartsWith(EditorSceneIO.PrimitivePrefix, StringComparison.Ordinal)
                && !File.Exists(Path.Combine(_projectPath, "Models", t.MarkerPath + ".glb")))
                list.Add(new DiagItem(DiagSeverity.Warning, $"Event model \"{t.MarkerPath}\" not found in Models/",
                    "The event is invisible in the scene because its .glb file is missing or was renamed.",
                    () => { SelectTrigger(t); FocusSelected(); }));
        }

        foreach (var poster in _level.Posters)
        {
            var p = poster;
            var file = p.IsVideo ? p.VideoPath : p.ImagePath;
            var folder = p.IsVideo ? "Videos" : "Images";
            if (!string.IsNullOrEmpty(file) && !File.Exists(Path.Combine(_projectPath, folder, file)))
                list.Add(new DiagItem(DiagSeverity.Warning, $"Poster file \"{file}\" not found in {folder}/",
                    "The poster will not display. Restore the file or delete the poster.",
                    () => { SelectPoster(p); FocusSelected(); }, "Delete poster", () => { _level.RemovePoster(p); if (ReferenceEquals(_selectedPoster, p)) _selectedPoster = null; }));
        }

        if (_sceneIsDark && _lightMode == ViewportLighting.Scene)
            list.Add(new DiagItem(DiagSeverity.Warning, "Scene is very dark in Scene Lighting",
                "The scene's own light/ambient is too low to see anything. Raise Ambient in Environment or preview with Editor Lighting.",
                () => _showEnvironmentWindow = true, "Editor Lighting", () => _lightMode = ViewportLighting.Editor));

        if (_scriptScanAge > 3.0)
        {
            _scriptScanAge = 0;
            _scriptErrorsCache = ScanScriptErrors();
        }
        foreach (var (file, error, line) in _scriptErrorsCache)
        {
            var f = file; var l = line;
            list.Add(new DiagItem(DiagSeverity.Error, $"Script error in {f}{(l > 0 ? $":{l}" : "")}", error,
                () => TryOpenScriptByName(f, l)));
        }

        _diagnostics = list;
    }

    private List<(string File, string Error, int Line)> ScanScriptErrors()
    {
        var result = new List<(string, string, int)>();
        try
        {
            if (!Directory.Exists(ScriptsDir)) return result;
            foreach (var path in Directory.EnumerateFiles(ScriptsDir, "*.lua", SearchOption.AllDirectories))
            {
                if (ScriptHost.CheckSyntax(File.ReadAllText(path), out var message, out var line)) continue;
                result.Add((Path.GetRelativePath(ScriptsDir, path), message, line));
            }
        }
        catch {                                                                  }
        return result;
    }

    private void DrawDiagnosticsWindow()
    {
        if (!_showDiagnosticsWindow) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(460, 320), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Diagnostics", ref _showDiagnosticsWindow)) { ImGui.End(); return; }

        if (ImGui.Button("Refresh")) { _scriptScanAge = 999; RefreshDiagnostics(force: true); }
        ImGui.SameLine();
        ImGui.TextDisabled($"Scene \"{_sceneName}\"");
        ImGui.Separator();

        if (_diagnostics.Count == 0)
        {
            ImGui.TextColored(new System.Numerics.Vector4(0.45f, 0.9f, 0.5f, 1f), "No problems found.");
            ImGui.End();
            return;
        }

        int i = 0;
        foreach (var item in _diagnostics.ToList())
        {
            ImGui.PushID(i++);
            var (icon, color) = item.Severity switch
            {
                DiagSeverity.Error => ("[x]", new System.Numerics.Vector4(1f, 0.45f, 0.4f, 1f)),
                DiagSeverity.Warning => ("[!]", new System.Numerics.Vector4(1f, 0.8f, 0.3f, 1f)),
                _ => ("[i]", new System.Numerics.Vector4(0.6f, 0.75f, 1f, 1f)),
            };
            ImGui.TextColored(color, icon);
            ImGui.SameLine();
            ImGui.TextWrapped(item.Text);
            ImGui.TextDisabled(item.Why);
            if (item.Locate != null && ImGui.SmallButton("Locate")) item.Locate();
            if (item.Fix != null)
            {
                if (item.Locate != null) ImGui.SameLine();
                if (ImGui.SmallButton(item.FixLabel)) { item.Fix(); RefreshDiagnostics(force: true); }
            }
            ImGui.Separator();
            ImGui.PopID();
        }

        ImGui.End();
    }

                      

    private enum ConsoleLevel { Info, Warning, Error }

    private readonly record struct ConsoleLine(DateTime Time, ConsoleLevel Level, string Text);

    private readonly List<ConsoleLine> _console = new();
    private string _lastLoggedStatus = string.Empty;
    private bool _showConsoleWindow;
    private bool _consoleShowInfo = true, _consoleShowWarning = true, _consoleShowError = true;
    private bool _consoleAutoScroll = true;
    private int _lastPlayLogCount;
    private string _lastPlayLogLine = string.Empty;

    private static ConsoleLevel ClassifyMessage(string text)
    {
        var lower = text.ToLowerInvariant();
        if (lower.Contains("could not") || lower.Contains("failed") || lower.Contains("error") || lower.Contains("exception") || text.StartsWith("[Script]", StringComparison.Ordinal))
            return ConsoleLevel.Error;
        if (lower.Contains("missing") || lower.Contains("not found") || lower.Contains("nothing to") || lower.Contains("empty") || lower.Contains("stop play mode"))
            return ConsoleLevel.Warning;
        return ConsoleLevel.Info;
    }

    private void LogToConsole(string text, ConsoleLevel? level = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        _console.Add(new ConsoleLine(DateTime.Now, level ?? ClassifyMessage(text), text));
        if (_console.Count > 1000) _console.RemoveRange(0, _console.Count - 1000);
    }

                                                                                                                         
    private void ConsoleTick()
    {
        if (_statusMessage != _lastLoggedStatus)
        {
            _lastLoggedStatus = _statusMessage;
            LogToConsole(_statusMessage);
        }

        if (_playMode && _playLog.Count > 0)
        {
                                                                                             
            var last = _playLog[^1];
            if (last != _lastPlayLogLine || _playLog.Count != _lastPlayLogCount)
            {
                _lastPlayLogLine = last;
                LogToConsole("[Play] " + last, ConsoleLevel.Info);
            }
            _lastPlayLogCount = _playLog.Count;
        }
        else if (!_playMode)
        {
            _lastPlayLogCount = 0;
            _lastPlayLogLine = string.Empty;
        }
    }

    private void DrawConsoleWindow()
    {
        if (!_showConsoleWindow) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(560, 240), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Console", ref _showConsoleWindow)) { ImGui.End(); return; }

        int errors = _console.Count(c => c.Level == ConsoleLevel.Error);
        int warnings = _console.Count(c => c.Level == ConsoleLevel.Warning);
        int infos = _console.Count - errors - warnings;

        ImGui.Checkbox($"Info ({infos})", ref _consoleShowInfo);
        ImGui.SameLine();
        ImGui.Checkbox($"Warning ({warnings})", ref _consoleShowWarning);
        ImGui.SameLine();
        ImGui.Checkbox($"Error ({errors})", ref _consoleShowError);
        ImGui.SameLine();
        if (ImGui.Button("Clear")) _console.Clear();
        ImGui.SameLine();
        ImGui.Checkbox("Auto-scroll", ref _consoleAutoScroll);
        ImGui.Separator();

        if (ImGui.BeginChild("##consoleScroll", System.Numerics.Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar))
        {
            if (_console.Count == 0)
                ImGui.TextDisabled("No messages.");

            int shown = 0;
            for (int i = 0; i < _console.Count; i++)
            {
                var line = _console[i];
                if (line.Level == ConsoleLevel.Info && !_consoleShowInfo) continue;
                if (line.Level == ConsoleLevel.Warning && !_consoleShowWarning) continue;
                if (line.Level == ConsoleLevel.Error && !_consoleShowError) continue;
                shown++;

                var color = line.Level switch
                {
                    ConsoleLevel.Error => new System.Numerics.Vector4(1f, 0.5f, 0.45f, 1f),
                    ConsoleLevel.Warning => new System.Numerics.Vector4(1f, 0.85f, 0.4f, 1f),
                    _ => new System.Numerics.Vector4(0.85f, 0.85f, 0.88f, 1f),
                };
                ImGui.PushStyleColor(ImGuiCol.Text, color);
                if (ImGui.Selectable($"{line.Time:HH:mm:ss}  {line.Text}##log{i}"))
                    OpenSourceOfLogLine(line.Text);
                ImGui.PopStyleColor();
            }

            if (_console.Count > 0 && shown == 0) ImGui.TextDisabled("All messages are hidden by the filters above.");
            if (_consoleAutoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - 4f) ImGui.SetScrollHereY(1f);
        }
        ImGui.EndChild();

        ImGui.End();
    }

                                                                                                                   
    private void OpenSourceOfLogLine(string text)
    {
        int dot = text.IndexOf(".lua", StringComparison.OrdinalIgnoreCase);
        if (dot < 0) return;

        int start = dot;
        while (start > 0 && !char.IsWhiteSpace(text[start - 1]) && text[start - 1] != ']' && text[start - 1] != ':') start--;
        var file = text[start..(dot + 4)];

        int line = 0;
        int after = dot + 4;
        if (after < text.Length && text[after] == ':')
        {
            int end = after + 1;
            while (end < text.Length && char.IsDigit(text[end])) end++;
            int.TryParse(text.AsSpan(after + 1, end - after - 1), out line);
        }

        if (!TryOpenScriptByName(file, line)) _statusMessage = $"Script \"{file}\" not found in Scripts/.";
    }
}
