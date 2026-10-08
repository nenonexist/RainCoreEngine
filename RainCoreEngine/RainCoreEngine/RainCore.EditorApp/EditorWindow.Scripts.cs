using ImGuiNET;

namespace RainCore.EditorApp;

             
                                                                                                               
                                                                                                     
                                                                                            
                                                                                                         
                                                                                                    
              
public sealed partial class EditorWindow
{
    private bool _showScriptEditor;
    private bool _scriptEditorFocused;
    private string _scriptPath = string.Empty;
    private string _scriptText = string.Empty;
    private string _scriptSavedText = string.Empty;
    private string _scriptStatus = string.Empty;
    private bool _scriptStatusIsError;
    private int _scriptErrorLine;
    private string _scriptFind = string.Empty;
    private int _scriptGoToLine = 1;

    private bool _requestNewScriptPopup;
    private string _newScriptName = "NewScript";
    private int _newScriptTemplate;

    private static readonly (string Name, string Description, string Body)[] ScriptTemplates =
    {
        ("Empty", "A single function you can call from a Run Script event command.",
@"-- Called by the event command ""Run Script"" (script file + function name).
-- Args arrive as strings.
function on_run(arg)
    Game.Log(""on_run called"")
end
"),
        ("Interactable", "Toggle a flag and play a sound each time the player interacts.",
@"-- Interactable: use with an OnInteract trigger -> Run Script -> on_interact
local FLAG = ""door_open""

function on_interact()
    local open = not Game.GetFlag(FLAG)
    Game.SetFlag(FLAG, open)
    Game.PlaySound(""door"")      -- file name from the Sound folder
    Game.Log(open and ""opened"" or ""closed"")
end
"),
        ("Trigger", "Runs once: counts how many times it fired.",
@"-- Trigger: use with an OnEnter trigger -> Run Script -> on_trigger
function on_trigger()
    local n = Game.GetVar(""trigger_count"") + 1
    Game.SetVar(""trigger_count"", n)
    Game.Log(""triggered "" .. n .. "" time(s)"")
end
"),
        ("Teleporter", "Moves the player to fixed coordinates.",
@"-- Teleporter: Run Script -> on_teleport
function on_teleport()
    Game.Teleport(0, 1, 0)       -- x, y, z
end
"),
        ("Random Event", "Does something only some of the time.",
@"-- Random Event: Run Script -> on_random
function on_random()
    if Game.Random(1, 100) <= 30 then
        Game.Log(""something happened"")
        Game.SetWeather(true)
    end
end
"),
    };

    private string ScriptsDir => Path.Combine(_projectPath, "Scripts");

    private static string SanitizeScriptName(string raw)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var name = new string(raw.Trim().Where(c => !invalid.Contains(c)).ToArray());
        if (name.EndsWith(".lua", StringComparison.OrdinalIgnoreCase)) name = name[..^4];
        return name;
    }

    private bool ScriptDirty => _scriptPath.Length > 0 && _scriptText != _scriptSavedText;

    private void OpenScriptInEditor(string path, int line = 0)
    {
        if (ScriptDirty && !path.Equals(_scriptPath, StringComparison.OrdinalIgnoreCase)) SaveScript();                    

        try
        {
            if (!path.Equals(_scriptPath, StringComparison.OrdinalIgnoreCase) || !_showScriptEditor)
            {
                _scriptText = File.ReadAllText(path);
                _scriptSavedText = _scriptText;
                _scriptPath = path;
                _scriptStatus = string.Empty;
                _scriptErrorLine = 0;
            }
            _showScriptEditor = true;
            if (line > 0) _scriptGoToLine = line;
        }
        catch (Exception ex)
        {
            _statusMessage = $"Could not open script: {ex.Message}";
        }
    }

                                                                        
    private bool TryOpenScriptByName(string fileName, int line = 0)
    {
        var path = Path.Combine(ScriptsDir, fileName);
        if (!File.Exists(path)) return false;
        OpenScriptInEditor(path, line);
        return true;
    }

    private void CheckScriptSyntax()
    {
        _scriptStatus = "Compiling...";
        if (ScriptHost.CheckSyntax(_scriptText, out var message, out var line))
        {
            _scriptStatus = "OK - no syntax errors";
            _scriptStatusIsError = false;
            _scriptErrorLine = 0;
        }
        else
        {
            _scriptStatus = message;
            _scriptStatusIsError = true;
            _scriptErrorLine = line;
            _statusMessage = $"[Script] {Path.GetFileName(_scriptPath)}:{(line > 0 ? line : 0)} - {message}";
        }
    }

    private void SaveScript()
    {
        if (_scriptPath.Length == 0) return;
        try
        {
            File.WriteAllText(_scriptPath, _scriptText);
            _scriptSavedText = _scriptText;
            CheckScriptSyntax();
            if (!_scriptStatusIsError) _statusMessage = $"Saved script: {Path.GetFileName(_scriptPath)}";
        }
        catch (Exception ex)
        {
            _scriptStatus = $"Could not save: {ex.Message}";
            _scriptStatusIsError = true;
        }
    }

    private string LineText(int oneBased)
    {
        if (oneBased < 1) return string.Empty;
        var lines = _scriptText.Split('\n');
        return oneBased <= lines.Length ? lines[oneBased - 1].TrimEnd('\r') : string.Empty;
    }

    private void DrawScriptEditor()
    {
        _scriptEditorFocused = false;
        if (!_showScriptEditor) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(640, 520), ImGuiCond.FirstUseEver);
        var title = $"Script Editor - {(_scriptPath.Length > 0 ? Path.GetFileName(_scriptPath) : "no file")}{(ScriptDirty ? " *" : "")}###ScriptEditor";
        if (!ImGui.Begin(title, ref _showScriptEditor)) { ImGui.End(); return; }
        _scriptEditorFocused = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);

        if (_scriptPath.Length == 0)
        {
            ImGui.TextWrapped("No script open. Double-click a .lua file in Project -> Scripts, or create one with Create -> Script.");
            if (ImGui.Button("New Script...")) { _newScriptName = "NewScript"; _requestNewScriptPopup = true; }
            ImGui.End();
            return;
        }

        if (ImGui.Button("Save")) SaveScript();
        Tip("Save and check syntax.\nShortcut: Ctrl+S");
        ImGui.SameLine();
        if (ImGui.Button("Check")) CheckScriptSyntax();
        Tip("Check Lua syntax without running the script.");
        ImGui.SameLine();
        if (ImGui.Button("Reload")) { try { _scriptText = File.ReadAllText(_scriptPath); _scriptSavedText = _scriptText; _scriptStatus = string.Empty; } catch { } }
        Tip("Discard your edits and re-read the file from disk.");
        ImGui.SameLine();
        if (ImGui.Button("Open in external editor"))
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_scriptPath) { UseShellExecute = true }); }
            catch (Exception ex) { _statusMessage = $"Could not open editor: {ex.Message}"; }
        }

        ImGui.SetNextItemWidth(180);
        ImGui.InputTextWithHint("##scriptFind", "Find...", ref _scriptFind, 128);
        if (_scriptFind.Length > 0)
        {
            int count = 0, firstLine = 0, line = 1;
            foreach (var l in _scriptText.Split('\n'))
            {
                if (l.Contains(_scriptFind, StringComparison.OrdinalIgnoreCase)) { count++; if (firstLine == 0) firstLine = line; }
                line++;
            }
            ImGui.SameLine();
            ImGui.TextDisabled(count == 0 ? "no matches" : $"{count} line(s), first at {firstLine}");
            if (count > 0) { ImGui.SameLine(); if (ImGui.SmallButton("Go")) _scriptGoToLine = firstLine; }
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(70);
        ImGui.InputInt("##goToLine", ref _scriptGoToLine, 0);
        ImGui.SameLine();
        ImGui.TextDisabled("line");

                                                                                       
        if (_scriptStatus.Length > 0)
        {
            var color = _scriptStatusIsError ? new System.Numerics.Vector4(1f, 0.45f, 0.4f, 1f) : new System.Numerics.Vector4(0.45f, 0.9f, 0.5f, 1f);
            ImGui.TextColored(color, _scriptStatusIsError ? "x 1 error" : "OK");
            ImGui.SameLine();
            if (_scriptStatusIsError && _scriptErrorLine > 0)
            {
                if (ImGui.Selectable($"{_scriptStatus}##scriptErr")) _scriptGoToLine = _scriptErrorLine;
                Tip("Click to jump to the line.");
            }
            else ImGui.TextWrapped(_scriptStatus);
        }

                                                                                                          
                                                                                                         
        if (_scriptGoToLine > 0)
        {
            ImGui.Separator();
            for (int l = Math.Max(1, _scriptGoToLine - 1); l <= _scriptGoToLine + 1; l++)
            {
                var text = LineText(l);
                if (l > _scriptText.Split('\n').Length) break;
                bool hot = l == _scriptGoToLine;
                ImGui.TextColored(hot ? new System.Numerics.Vector4(1f, 0.9f, 0.4f, 1f) : new System.Numerics.Vector4(0.6f, 0.6f, 0.65f, 1f), $"{l,4} | {text}");
            }
        }
        ImGui.Separator();

        var size = new System.Numerics.Vector2(-1, -1);
        ImGui.InputTextMultiline("##scriptText", ref _scriptText, 1 << 20, size, ImGuiInputTextFlags.AllowTabInput);

        if (_scriptEditorFocused && ImGui.GetIO().KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.S, false))
            SaveScript();

        ImGui.End();
    }

                                                                                                          
    private void DrawNewScriptPopup()
    {
        if (_requestNewScriptPopup) { ImGui.OpenPopup("NewScriptPopup"); _requestNewScriptPopup = false; }

        bool open = true;
        if (!ImGui.BeginPopupModal("NewScriptPopup", ref open, ImGuiWindowFlags.AlwaysAutoResize)) return;

        ImGui.InputText("Name", ref _newScriptName, 64);
        var clean = SanitizeScriptName(_newScriptName);
        var path = Path.Combine(ScriptsDir, clean + ".lua");
        bool exists = clean.Length > 0 && File.Exists(path);

        ImGui.TextUnformatted("Template");
        for (int i = 0; i < ScriptTemplates.Length; i++)
        {
            if (ImGui.RadioButton(ScriptTemplates[i].Name, _newScriptTemplate == i)) _newScriptTemplate = i;
            ImGui.SameLine();
            ImGui.TextDisabled(ScriptTemplates[i].Description);
        }

        if (clean.Length == 0) ImGui.TextColored(new System.Numerics.Vector4(1f, 0.5f, 0.4f, 1f), "Enter a valid name.");
        else if (exists) ImGui.TextColored(new System.Numerics.Vector4(1f, 0.5f, 0.4f, 1f), "A script with this name already exists.");

        ImGui.BeginDisabled(clean.Length == 0 || exists);
        if (ImGui.Button("Create"))
        {
            try
            {
                Directory.CreateDirectory(ScriptsDir);
                File.WriteAllText(path, ScriptTemplates[_newScriptTemplate].Body);
                _selectedProjectFolder = "Scripts";
                OpenScriptInEditor(path);
                _statusMessage = $"Created script: Scripts/{clean}.lua";
            }
            catch (Exception ex) { _statusMessage = $"Could not create script: {ex.Message}"; }
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();
        ImGui.EndPopup();
    }
}
