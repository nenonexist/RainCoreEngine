using ImGuiNET;
using OpenTK.Mathematics;

namespace RainCore.EditorApp;

             
                                                                         
                                                                             
                                                                                   
                                                                         
                                                                                 
                                                                       
              
public sealed partial class EditorWindow
{
    private const string ViewportCreateMenuId = "ViewportCreateMenu";

    private Vector2 _rightClickStartPos;
    private Vector3 _pendingCreateRayOrigin;
    private Vector3 _pendingCreateRayDirection;

                                                                
                                                                        
                                                                          
                                                                    
    private void DrawViewportCreateMenu()
    {
        if (!ImGui.BeginPopup(ViewportCreateMenuId)) return;
        DrawCreateMenuItems(_pendingCreateRayOrigin, _pendingCreateRayDirection);
        ImGui.EndPopup();
    }

                                                                          
                                                                           
                                                                          
                                                                       
                                                                             
                                                                             
                                                                               
    private void DrawHierarchyCreateMenu()
    {
        if (!ImGui.BeginPopupContextWindow("HierarchyCreateMenu")) return;
        DrawCreateMenuItems(_flyCamera!.Camera.Position, _flyCamera.Camera.Front);
        ImGui.EndPopup();
    }

    private void DrawCreateMenuItems(Vector3 origin, Vector3 direction)
    {
        if (ImGui.BeginMenu("Primitive"))
        {
            foreach (var (key, label) in Primitives)
                if (ImGui.MenuItem(label)) PlaceAssetAt(key, origin, direction);
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Model"))
        {
            var modelsDir = Path.Combine(_projectPath, "Models");
            var files = Directory.Exists(modelsDir) ? Directory.GetFiles(modelsDir, "*.glb") : Array.Empty<string>();
            if (files.Length == 0) ImGui.MenuItem("(no .glb files in Models/)", enabled: false);
            foreach (var file in files)
                if (ImGui.MenuItem(Path.GetFileName(file))) PlaceAssetAt(file, origin, direction);
            ImGui.EndMenu();
        }

        ImGui.Separator();
        if (ImGui.MenuItem("Point Event")) PlaceEventAtRay(origin, direction);
        if (ImGui.MenuItem("Trigger Zone")) PlaceTriggerAtRay(origin, direction);

        ImGui.Separator();
                                                                        
                                                                     
                                                                          
                                                                              
                                                                   
                                                                           
                                                                             
                                                                           
        if (ImGui.MenuItem("Player Start Here") && TryResolvePlacementPoint(origin, direction, out var startPoint))
        {
            _level.PlayerStartPosition = startPoint;
            _level.PlayerStartYawDegrees = 0f;
        }
    }
}
