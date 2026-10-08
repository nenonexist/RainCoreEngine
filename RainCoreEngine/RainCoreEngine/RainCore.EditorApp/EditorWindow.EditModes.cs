using ImGuiNET;

namespace RainCore.EditorApp;

             
                                                                        
                                                                             
                                                                          
                                                                     
                                                                    
                                                                      
                       
              
public enum EditMode { Select, Objects, Events }

                                                                      
                                                                             
                                                                           
                                                                             
public enum EventPlacementKind { Point, Zone }

                                                                                
                                                                         
                                                                       
                                                                              
                                                                   
                                                                          
                                                                            
                                                                           
                                                                              
                                                                               
public enum GizmoMode { Move, Rotate, Scale }

public sealed partial class EditorWindow
{
    private EditMode _editMode = EditMode.Select;
    private EventPlacementKind _eventPlacementKind = EventPlacementKind.Point;
    private GizmoMode _gizmoMode = GizmoMode.Move;

                                                                                                    
                                                                             
    private bool _snapEnabled;
    private float _snapMove = 0.5f;
    private float _snapRotate = 15f;
    private float _snapScale = 0.1f;

    private bool SnapActive()
    {
        bool ctrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
        return ctrl != _snapEnabled;
    }

                                                                              
                                                                            
                                                                     
                                                                             
    private void DrawEditModeToolbar()
    {
        DrawModeButton("Select", EditMode.Select);
        Tip("Select tool (Q)\nClick an object to select it. Drag a handle to move/rotate/scale.");
        ImGui.SameLine();
        DrawModeButton("Objects", EditMode.Objects);
        Tip("Objects tool\nPick an asset in Project, then left-click in the scene to place it.\nYou can also just drag the asset into the Scene View.");
        ImGui.SameLine();
        DrawModeButton("Events", EditMode.Events);
        Tip("Events tool\nLeft-click to place an event (Point = small radius, Zone = trigger area).");

        if (_editMode == EditMode.Objects)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(string.IsNullOrEmpty(_selectedAsset)
                ? "|  (select an asset in Project panel first)"
                : "|  LMB places the selected asset");
        }
        else if (_editMode == EditMode.Events)
        {
            ImGui.SameLine();
            ImGui.TextDisabled("|");
            ImGui.SameLine();
            if (ImGui.RadioButton("Point", _eventPlacementKind == EventPlacementKind.Point))
                _eventPlacementKind = EventPlacementKind.Point;
            ImGui.SameLine();
            if (ImGui.RadioButton("Zone", _eventPlacementKind == EventPlacementKind.Zone))
                _eventPlacementKind = EventPlacementKind.Zone;
        }
        else if (_editMode == EditMode.Select)
        {
                                                                                 
                                                                            
                                                                           
                                                         
            ImGui.SameLine();
            ImGui.TextDisabled("|");
            ImGui.SameLine();
            if (ImGui.RadioButton("Move (W)", _gizmoMode == GizmoMode.Move))
                _gizmoMode = GizmoMode.Move;
            Tip("Move gizmo. Shortcut: W");
            ImGui.SameLine();
            if (ImGui.RadioButton("Rotate (E)", _gizmoMode == GizmoMode.Rotate))
                _gizmoMode = GizmoMode.Rotate;
            Tip("Rotate gizmo (props only). Shortcut: E");
            ImGui.SameLine();
            if (ImGui.RadioButton("Scale (R)", _gizmoMode == GizmoMode.Scale))
                _gizmoMode = GizmoMode.Scale;
            Tip("Scale gizmo (props and posters). Shortcut: R");

            ImGui.SameLine();
            ImGui.Checkbox("Snap", ref _snapEnabled);
            Tip("Snap to the grid while dragging. Hold Ctrl to flip it temporarily.");
            if (_snapEnabled)
            {
                ImGui.SameLine(); ImGui.SetNextItemWidth(48); ImGui.DragFloat("##snapMove", ref _snapMove, 0.05f, 0.05f, 10f, "%.2f");
                Tip("Move step (world units)");
                ImGui.SameLine(); ImGui.SetNextItemWidth(42); ImGui.DragFloat("##snapRot", ref _snapRotate, 1f, 1f, 90f, "%.0f\u00b0");
                Tip("Rotate step (degrees)");
                ImGui.SameLine(); ImGui.SetNextItemWidth(48); ImGui.DragFloat("##snapScale", ref _snapScale, 0.01f, 0.01f, 2f, "%.2f");
                Tip("Scale step (multiplier)");
            }
        }

        ImGui.Separator();
    }

    private void DrawModeButton(string label, EditMode mode)
    {
        bool active = _editMode == mode;
        if (active) ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive]);
        if (ImGui.Button(label)) _editMode = mode;
        if (active) ImGui.PopStyleColor();
    }
}
