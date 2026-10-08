using ImGuiNET;
using OpenTK.Mathematics;

namespace RainCore.EditorApp;

             
                                                                                                               
                                                                                                               
                                                                                                 
                                                                                                  
                                                            
              
public sealed partial class EditorWindow
{
    private readonly List<object> _extraSelection = new();

    private IEnumerable<object> AllSelected()
    {
        var primary = GetSelectedObject();
        if (primary != null) yield return primary;
        foreach (var o in _extraSelection) yield return o;
    }

    private bool IsMultiSelection => _extraSelection.Count > 0 && GetSelectedObject() != null;
    private int SelectionCount => AllSelected().Count();

    private bool IsSelected(object o) => ReferenceEquals(GetSelectedObject(), o) || _extraSelection.Contains(o);

                                                                                                   
    private void NormalizeSelection()
    {
        if (GetSelectedObject() == null) { _extraSelection.Clear(); return; }
        _extraSelection.RemoveAll(o => !IsInScene(o));
    }

    private void ToggleInSelection(object o)
    {
        var primary = GetSelectedObject();
        if (primary == null) { SelectAny(o); return; }

        if (ReferenceEquals(primary, o))
        {
            if (_extraSelection.Count == 0) return;
            var next = _extraSelection[0];
            var rest = _extraSelection.Skip(1).ToList();
            SelectAny(next);                                     
            _extraSelection.AddRange(rest);
        }
        else if (!_extraSelection.Remove(o))
        {
            _extraSelection.Add(o);
        }
    }

                                                         
    private void ClickInHierarchy(object o)
    {
        bool additive = ImGui.GetIO().KeyCtrl || ImGui.GetIO().KeyShift;
        if (additive) ToggleInSelection(o); else SelectAny(o);
    }

                                           

    private static Vector3? PositionOf(object o) => o switch
    {
        ModelInstance m => m.Position,
        TriggerZone t => t.Center,
        PosterInstance p => p.Position,
        MoodZone z => z.Center,
        MoodEvent e => e.Center,
        _ => null,
    };

    private static void SetPositionOf(object o, Vector3 pos)
    {
        switch (o)
        {
            case ModelInstance m: m.Position = pos; break;
            case TriggerZone t: t.SetCenter(pos); break;
            case PosterInstance p: p.SetPosition(pos); break;
            case MoodZone z: z.SetCenter(pos); break;
            case MoodEvent e: e.SetCenter(pos); break;
        }
    }

                                                                                     
    private void MoveExtrasBy(Vector3 delta)
    {
        if (delta == Vector3.Zero) return;
        foreach (var o in _extraSelection)
            if (PositionOf(o) is { } p) SetPositionOf(o, p + delta);
    }

    private void ScaleExtrasBy(float ratio)
    {
        foreach (var o in _extraSelection)
        {
            if (o is ModelInstance m) m.Scale = MathF.Max(m.Scale * ratio, 0.05f);
            else if (o is PosterInstance p) p.Scale = MathF.Max(p.Scale * ratio, 0.05f);
        }
    }

    private void RotateExtrasBy(Vector3 deltaDeg)
    {
        foreach (var o in _extraSelection)
            if (o is ModelInstance m) m.RotationDeg += deltaDeg;
    }

    private Vector3 SelectionCenter()
    {
        var sum = Vector3.Zero; int n = 0;
        foreach (var o in AllSelected())
            if (PositionOf(o) is { } p) { sum += p; n++; }
        return n > 0 ? sum / n : Vector3.Zero;
    }

    private void AlignSelection(int axis)
    {
        var primary = GetSelectedObject();
        if (primary == null || PositionOf(primary) is not { } target) return;
        foreach (var o in _extraSelection)
        {
            if (PositionOf(o) is not { } p) continue;
            if (axis == 0) p.X = target.X; else if (axis == 1) p.Y = target.Y; else p.Z = target.Z;
            SetPositionOf(o, p);
        }
    }

    private void DistributeSelection(int axis)
    {
        var items = AllSelected().Where(o => PositionOf(o) != null).OrderBy(o =>
        {
            var p = PositionOf(o)!.Value;
            return axis == 0 ? p.X : axis == 1 ? p.Y : p.Z;
        }).ToList();
        if (items.Count < 3) { _statusMessage = "Distribute needs at least 3 objects."; return; }

        float Get(object o) { var p = PositionOf(o)!.Value; return axis == 0 ? p.X : axis == 1 ? p.Y : p.Z; }
        float first = Get(items[0]), last = Get(items[^1]);
        for (int i = 1; i < items.Count - 1; i++)
        {
            var p = PositionOf(items[i])!.Value;
            float v = first + (last - first) * i / (items.Count - 1);
            if (axis == 0) p.X = v; else if (axis == 1) p.Y = v; else p.Z = v;
            SetPositionOf(items[i], p);
        }
    }

    private void RemoveObject(object o)
    {
        switch (o)
        {
            case ModelInstance m: _level.RemoveModel(m); break;
            case TriggerZone t: _level.RemoveTrigger(t); break;
            case PosterInstance p: _level.RemovePoster(p); break;
            case MoodZone z: _level.RemoveMoodZone(z); break;
            case MoodEvent e: _level.RemoveMoodEvent(e); break;
        }
    }

    private void DeleteSelected()
    {
        var all = AllSelected().ToList();
        foreach (var o in all) RemoveObject(o);
        ClearSceneSelection();
        _extraSelection.Clear();
        if (all.Count > 1) _statusMessage = $"Deleted {all.Count} objects.";
    }

    private void DuplicateSelected()
    {
        var originals = AllSelected().ToList();
        if (originals.Count == 0) return;

        var copies = new List<object>();
        foreach (var o in originals)
        {
            SelectAny(o);
            DuplicateSingleSelected();
            if (GetSelectedObject() is { } copy && !originals.Contains(copy)) copies.Add(copy);
        }

        if (copies.Count == 0) return;
        SelectAny(copies[0]);
        _extraSelection.AddRange(copies.Skip(1));
        if (copies.Count > 1) _statusMessage = $"Duplicated {copies.Count} objects.";
    }

                                   

    private void DrawMultiInspector()
    {
        var all = AllSelected().ToList();
        ImGui.Text($"{all.Count} objects selected");
        ImGui.TextDisabled("Ctrl/Shift+click adds or removes an object. Changes apply to all of them.");
        ImGui.Separator();

        int models = all.Count(o => o is ModelInstance);
        int events = all.Count(o => o is TriggerZone);
        int posters = all.Count(o => o is PosterInstance);
        int moods = all.Count(o => o is MoodZone or MoodEvent);
        ImGui.TextUnformatted($"Objects: {models}   Events: {events}   Posters: {posters}   Mood: {moods}");

        var center = SelectionCenter();
        ImGui.TextDisabled($"Center: {center.X:0.##}, {center.Y:0.##}, {center.Z:0.##}");

        if (ImGui.CollapsingHeader("Transform (group)", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var offset = System.Numerics.Vector3.Zero;
            if (ImGui.DragFloat3("Move by", ref offset, 0.05f))
            {
                var d = new Vector3(offset.X, offset.Y, offset.Z);
                foreach (var o in all) if (PositionOf(o) is { } p) SetPositionOf(o, p + d);
            }
            HelpMarker("Drag to shift the whole group. Values reset each time - it is a relative move.");

            if (models + posters > 0)
            {
                float ratio = 1f;
                if (ImGui.DragFloat("Scale x", ref ratio, 0.01f, 0.1f, 4f, "%.2f"))
                    if (MathF.Abs(ratio - 1f) > 0.0001f)
                    {
                        foreach (var o in all)
                        {
                            if (o is ModelInstance m) m.Scale = MathF.Max(m.Scale * ratio, 0.05f);
                            else if (o is PosterInstance p) p.Scale = MathF.Max(p.Scale * ratio, 0.05f);
                        }
                    }
                HelpMarker("Multiplies the size of every object/poster in the group.");
            }

            ImGui.TextUnformatted("Align to main:");
            ImGui.SameLine(); if (ImGui.SmallButton("X")) AlignSelection(0);
            ImGui.SameLine(); if (ImGui.SmallButton("Y")) AlignSelection(1);
            ImGui.SameLine(); if (ImGui.SmallButton("Z")) AlignSelection(2);
            Tip("Puts all other selected objects on the same X / Y / Z as the main (first) selected object.");

            ImGui.TextUnformatted("Distribute:");
            ImGui.SameLine(); if (ImGui.SmallButton("X##d")) DistributeSelection(0);
            ImGui.SameLine(); if (ImGui.SmallButton("Y##d")) DistributeSelection(1);
            ImGui.SameLine(); if (ImGui.SmallButton("Z##d")) DistributeSelection(2);
            Tip("Spaces objects evenly between the two outermost ones on that axis (3+ objects).");
        }

        if (models > 0 && ImGui.CollapsingHeader("Rendering & Physics (objects)", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var ms = all.OfType<ModelInstance>().ToList();
            bool allVisible = ms.All(m => m.Visible), anyVisible = ms.Any(m => m.Visible);
            bool visible = allVisible;
            if (ImGui.Checkbox(anyVisible && !allVisible ? "Visible (mixed)" : "Visible", ref visible))
                foreach (var m in ms) m.Visible = visible;

            bool allSolid = ms.All(m => m.IsSolid), anySolid = ms.Any(m => m.IsSolid);
            bool solid = allSolid;
            if (ImGui.Checkbox(anySolid && !allSolid ? "Solid (mixed)" : "Solid", ref solid))
                foreach (var m in ms) m.IsSolid = solid;
        }

        ImGui.Separator();
        if (ImGui.Button("Duplicate")) DuplicateSelected();
        ImGui.SameLine();
        if (ImGui.Button("Delete all")) DeleteSelected();
        ImGui.SameLine();
        if (ImGui.Button("Select only main")) { var p = GetSelectedObject(); if (p != null) SelectAny(p); }
        ImGui.SameLine();
        if (ImGui.Button("Focus")) FocusSelectedGroup();
    }

    private void SelectAllObjects()
    {
        var all = new List<object>();
        all.AddRange(_level.Models);
        all.AddRange(_level.Triggers);
        all.AddRange(_level.Posters);
        all.AddRange(_level.MoodZones);
        all.AddRange(_level.MoodEvents);
        if (all.Count == 0) { _statusMessage = "Nothing to select."; return; }
        SelectAny(all[0]);
        _extraSelection.AddRange(all.Skip(1));
        _statusMessage = $"Selected {all.Count} objects.";
    }

    private void FocusSelectedGroup()
    {
        if (_flyCamera == null) return;
        var pts = AllSelected().Select(PositionOf).Where(p => p != null).Select(p => p!.Value).ToList();
        if (pts.Count == 0) return;
        var min = pts.Aggregate(Vector3.ComponentMin);
        var max = pts.Aggregate(Vector3.ComponentMax);
        _flyCamera.FrameBounds(min - new Vector3(1f), max + new Vector3(1f));
    }
}
