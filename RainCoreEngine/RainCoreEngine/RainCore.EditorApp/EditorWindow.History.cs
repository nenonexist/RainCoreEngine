using ImGuiNET;
using OpenTK.Mathematics;

namespace RainCore.EditorApp;

             
                                                                            
   
                                                                                                
                                                                                              
                                                                                          
                                                                                           
                                                                                                    
                                                                                    
                                                                                            
                                                                              
              
public sealed partial class EditorWindow
{
    private EditorHistory _history = new();
    private bool _historyWasInteracting;
    private int[] _historyCounts = new int[6];
    private SceneEnvironment? _historyEnv;
    private bool _showHistoryWindow;

    private static readonly string[] ObjectKindNames = { "Object", "Event", "Poster", "Mood Zone", "Mood Event", "Point Event" };

    private int[] CountObjects() => new[]
    {
        _level.Models.Count, _level.Triggers.Count, _level.Posters.Count,
        _level.MoodZones.Count, _level.MoodEvents.Count, _level.Interactables.Count,
    };

    private string? DescribeSelection()
    {
        if (_selectedModel is { } m)
            return m.ModelKey.StartsWith(EditorSceneIO.PrimitivePrefix, StringComparison.Ordinal)
                ? m.ModelKey[EditorSceneIO.PrimitivePrefix.Length..]
                : m.ModelKey;
        if (_selectedTrigger != null) return "Event";
        if (_selectedPoster != null) return "Poster";
        if (_selectedMoodZone != null) return "Mood Zone";
        if (_selectedMoodEvent != null) return "Mood Event";
        return null;
    }

    private string DescribeChange()
    {
        var now = CountObjects();
        for (int i = 0; i < now.Length; i++)
            if (now[i] > _historyCounts[i]) return $"Create {ObjectKindNames[i]}";
        for (int i = 0; i < now.Length; i++)
            if (now[i] < _historyCounts[i]) return $"Delete {ObjectKindNames[i]}";
        if (_historyEnv != null && _level.Environment != _historyEnv) return "Change Environment";
        var selection = DescribeSelection();
        return selection != null ? $"Edit {selection}" : "Edit Scene";
    }

                                                                                                 
    private void HistoryReset()
    {
        _history.Reset(EditorSceneIO.SerializeToJson(_level, _sceneName, _nextEventNumber));
        _historyCounts = CountObjects();
        _historyEnv = _level.Environment;
        _historyWasInteracting = false;
    }

                                                                                                     
                                                                                                      
    private bool HistoryCommitNow(string? label = null)
    {
        if (_playMode || !_projectLoaded || _level == null) return false;

        var json = EditorSceneIO.SerializeToJson(_level, _sceneName, _nextEventNumber);
        if (!_history.HasBaseline)
        {
            _history.Reset(json);
            _historyCounts = CountObjects();
            _historyEnv = _level.Environment;
            return false;
        }

        if (json == _history.Current) return false;

        bool committed = _history.Commit(json, label ?? DescribeChange());
        _historyCounts = CountObjects();
        _historyEnv = _level.Environment;
        return committed;
    }

                                                                                                
    private void HistoryTick()
    {
        if (_playMode)
        {
            _historyWasInteracting = false;
            return;
        }

        bool interacting = MouseState.IsAnyButtonDown || KeyboardState.IsAnyKeyDown
            || ImGui.IsAnyItemActive() || _gizmoDragAxis != null || _isDraggingSelection || _isDraggingPlayerStart;

        if (_historyWasInteracting && !interacting)
            HistoryCommitNow();
        else if (!interacting && !Enumerable.SequenceEqual(_historyCounts, CountObjects()))
            HistoryCommitNow();                                                                                      

        _historyWasInteracting = interacting;
    }

                                                                                                

    private static int IndexOfRef<T>(IReadOnlyList<T> list, T item) where T : class
    {
        for (int i = 0; i < list.Count; i++)
            if (ReferenceEquals(list[i], item)) return i;
        return -1;
    }

    private (int Kind, int Index) CaptureSelection()
    {
        if (_selectedModel != null) return (1, IndexOfRef(_level.Models, _selectedModel));
        if (_selectedTrigger != null) return (2, IndexOfRef(_level.Triggers, _selectedTrigger));
        if (_selectedPoster != null) return (3, IndexOfRef(_level.Posters, _selectedPoster));
        if (_selectedMoodZone != null) return (4, IndexOfRef(_level.MoodZones, _selectedMoodZone));
        if (_selectedMoodEvent != null) return (5, IndexOfRef(_level.MoodEvents, _selectedMoodEvent));
        return (0, -1);
    }

    private void ClearSceneSelection()
    {
        _extraSelection.Clear();
        _selectedModel = null;
        _selectedTrigger = null;
        _selectedPoster = null;
        _selectedMoodZone = null;
        _selectedMoodEvent = null;
        _gizmoDragAxis = null;
        _gizmoHoverAxis = null;
        _isDraggingSelection = false;
        _isDraggingPlayerStart = false;
    }

    private void RestoreSelection((int Kind, int Index) s)
    {
        if (s.Index < 0) return;
        switch (s.Kind)
        {
            case 1 when s.Index < _level.Models.Count: SelectModel(_level.Models[s.Index]); break;
            case 2 when s.Index < _level.Triggers.Count: SelectTrigger(_level.Triggers[s.Index]); break;
            case 3 when s.Index < _level.Posters.Count: SelectPoster(_level.Posters[s.Index]); break;
            case 4 when s.Index < _level.MoodZones.Count: SelectMoodZone(_level.MoodZones[s.Index]); break;
            case 5 when s.Index < _level.MoodEvents.Count: SelectMoodEvent(_level.MoodEvents[s.Index]); break;
        }
    }

    private void ApplyHistorySnapshot(string json)
    {
        var selection = CaptureSelection();
        _level = EditorSceneIO.DeserializeFromJson(json, _sceneName, _projectPath, _modelCache, out _nextEventNumber);
        ClearSceneSelection();
        _commandUndo.Clear();
        RestoreSelection(selection);
        _historyCounts = CountObjects();
        _historyEnv = _level.Environment;
        _historyWasInteracting = false;
    }

    private bool CanUndo => !_playMode && _history.UndoCount > 0;
    private bool CanRedo => !_playMode && _history.RedoCount > 0;

    private void DoUndo(int steps = 1)
    {
        if (_playMode) return;
        HistoryCommitNow();                               

        var label = _history.NextUndoLabel;
        var json = _history.Undo(steps);
        if (json == null) { _statusMessage = "Nothing to undo."; return; }

        ApplyHistorySnapshot(json);
        _statusMessage = steps == 1 ? $"Undo: {label}" : $"Undo: {steps} steps";
    }

    private void DoRedo(int steps = 1)
    {
        if (_playMode) return;
        HistoryCommitNow();

        var label = _history.NextRedoLabel;
        var json = _history.Redo(steps);
        if (json == null) { _statusMessage = "Nothing to redo."; return; }

        ApplyHistorySnapshot(json);
        _statusMessage = steps == 1 ? $"Redo: {label}" : $"Redo: {steps} steps";
    }

                                                                                                       
                                                                                  
    private void DoUndoShortcut()
    {
        if (_selectedTrigger != null && _commandUndo.Count > 0)
            UndoCommandChange(_triggerCommandsBuffer);
        else
            DoUndo();
    }

                 

                                                                                                                        
    private void DrawUndoRedoButtons()
    {
        ImGui.BeginDisabled(!CanUndo);
        if (ImGui.Button("Undo")) DoUndo();
        ImGui.EndDisabled();
        Tip(CanUndo ? $"Undo: {_history.NextUndoLabel}\nShortcut: Ctrl+Z" : "Nothing to undo.\nShortcut: Ctrl+Z");

        ImGui.SameLine();
        ImGui.BeginDisabled(!CanRedo);
        if (ImGui.Button("Redo")) DoRedo();
        ImGui.EndDisabled();
        Tip(CanRedo ? $"Redo: {_history.NextRedoLabel}\nShortcut: Ctrl+Y" : "Nothing to redo.\nShortcut: Ctrl+Y");
    }

    private void DrawEditMenu()
    {
        if (!ImGui.BeginMenu("Edit")) return;

        if (ImGui.MenuItem(CanUndo ? $"Undo {_history.NextUndoLabel}" : "Undo", "Ctrl+Z", false, CanUndo)) DoUndo();
        if (ImGui.MenuItem(CanRedo ? $"Redo {_history.NextRedoLabel}" : "Redo", "Ctrl+Y", false, CanRedo)) DoRedo();
        if (ImGui.MenuItem("History...")) _showHistoryWindow = true;

        ImGui.Separator();
        bool hasSelection = GetSelectedPosition() != null;
        if (ImGui.MenuItem("Duplicate", "Ctrl+D", false, hasSelection && !_playMode)) DuplicateSelected();
        if (ImGui.MenuItem("Delete", "Del", false, hasSelection && !_playMode)) DeleteSelected();
        if (ImGui.MenuItem("Select All", "Ctrl+A", false, !_playMode)) SelectAllObjects();
        if (ImGui.MenuItem("Save Selection as Prefab...", null, false, hasSelection && !_playMode)) { _prefabNameBuffer = "NewPrefab"; _requestSavePrefabPopup = true; }
        if (ImGui.MenuItem("Focus Selected", "F", false, hasSelection)) FocusSelected();
        if (ImGui.MenuItem("Select Previous", "Alt+Left", false, CanSelectionHistoryBack)) SelectionHistoryStep(-1);
        if (ImGui.MenuItem("Select Next", "Alt+Right", false, CanSelectionHistoryForward)) SelectionHistoryStep(+1);

        ImGui.Separator();
        if (ImGui.MenuItem("Command Palette...", "Ctrl+Shift+P")) OpenPalette(PaletteMode.Commands);
        if (ImGui.MenuItem("Search Everything...", "Ctrl+K")) OpenPalette(PaletteMode.Everything);
        if (ImGui.MenuItem("Shortcuts...", "F1")) _showShortcutsWindow = true;

        ImGui.EndMenu();
    }

    private void DrawHistoryWindow()
    {
        if (!_showHistoryWindow) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(320, 360), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("History", ref _showHistoryWindow)) { ImGui.End(); return; }

        if (_history.UndoCount == 0 && _history.RedoCount == 0)
        {
            ImGui.TextWrapped("No actions yet. Every change you make to the scene appears here and can be undone - experiment freely.");
            ImGui.End();
            return;
        }

        ImGui.TextDisabled("Click a step to jump back to the state before it.");
        ImGui.Separator();

        int redoTotal = _history.RedoCount;
        int redoIndex = redoTotal;
        foreach (var label in _history.RedoLabels())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.6f, 0.6f, 0.65f, 1f));
            if (ImGui.Selectable($"{label}##redo{redoIndex}")) { DoRedo(redoIndex); ImGui.PopStyleColor(); break; }
            ImGui.PopStyleColor();
            redoIndex--;
        }

        ImGui.TextColored(new System.Numerics.Vector4(0.4f, 0.85f, 0.4f, 1f), "-- current state --");

        int step = 1;
        foreach (var label in _history.UndoLabels())
        {
            if (ImGui.Selectable($"{label}##undo{step}")) { DoUndo(step); break; }
            step++;
        }

        ImGui.End();
    }

                                                                  

    private readonly List<object> _selectionHistory = new();
    private int _selectionHistoryPos = -1;
    private object? _lastTrackedSelection;
    private bool _selectionHistoryJump;

    private object? GetSelectedObject()
    {
        if (_selectedModel != null) return _selectedModel;
        if (_selectedTrigger != null) return _selectedTrigger;
        if (_selectedPoster != null) return _selectedPoster;
        if (_selectedMoodZone != null) return _selectedMoodZone;
        if (_selectedMoodEvent != null) return _selectedMoodEvent;
        return null;
    }

    private bool IsInScene(object o) => o switch
    {
        ModelInstance m => _level.Models.Contains(m),
        TriggerZone t => _level.Triggers.Contains(t),
        PosterInstance p => _level.Posters.Contains(p),
        MoodZone z => _level.MoodZones.Contains(z),
        MoodEvent e => _level.MoodEvents.Contains(e),
        _ => false,
    };

    private void SelectAny(object o)
    {
        switch (o)
        {
            case ModelInstance m: SelectModel(m); break;
            case TriggerZone t: SelectTrigger(t); break;
            case PosterInstance p: SelectPoster(p); break;
            case MoodZone z: SelectMoodZone(z); break;
            case MoodEvent e: SelectMoodEvent(e); break;
        }
    }

    private void TrackSelectionHistory()
    {
        NormalizeSelection();
        var current = GetSelectedObject();
        if (ReferenceEquals(current, _lastTrackedSelection)) return;
        _lastTrackedSelection = current;
        if (current == null) return;

        if (_selectionHistoryJump) { _selectionHistoryJump = false; return; }

                                                                    
        if (_selectionHistoryPos < _selectionHistory.Count - 1)
            _selectionHistory.RemoveRange(_selectionHistoryPos + 1, _selectionHistory.Count - _selectionHistoryPos - 1);
        _selectionHistory.Add(current);
        if (_selectionHistory.Count > 50) _selectionHistory.RemoveAt(0);
        _selectionHistoryPos = _selectionHistory.Count - 1;
    }

    private bool CanSelectionHistoryBack => FindSelectionHistoryTarget(-1) >= 0;
    private bool CanSelectionHistoryForward => FindSelectionHistoryTarget(+1) >= 0;

    private int FindSelectionHistoryTarget(int direction)
    {
        for (int i = _selectionHistoryPos + direction; i >= 0 && i < _selectionHistory.Count; i += direction)
            if (IsInScene(_selectionHistory[i])) return i;
        return -1;
    }

    private void SelectionHistoryStep(int direction)
    {
        int target = FindSelectionHistoryTarget(direction);
        if (target < 0) return;
        _selectionHistoryPos = target;
        _selectionHistoryJump = true;
        SelectAny(_selectionHistory[target]);
    }
}
