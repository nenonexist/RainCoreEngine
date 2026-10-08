using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RainCore.EditorApp;

                                                                                                                             
public enum ViewportLighting { Scene, Editor, Unlit, Wireframe }

             
                                                                                      
                                                                                                         
                                                                                          
                                                                       
              
public sealed partial class EditorWindow
{
    private ViewportLighting _lightMode = ViewportLighting.Scene;
    private float _viewExposureEv;
    private bool _showGrid = true;
    private bool _showGizmos = true;

    private bool _sceneIsDark;
    private bool _darkWarningDismissed;
    private float _darkCheckTimer;

    private static readonly Vector3 EditorBackground = new(0.10f, 0.10f, 0.12f);

                                   

    private void DrawSceneViewHeader()
    {
        DrawSceneTabs();
        DrawViewportToolbar();
        DrawEditModeToolbar();
    }

    private void DrawViewportToolbar()
    {
        DrawUndoRedoButtons();

        ImGui.SameLine();
        ImGui.TextDisabled("|");
        ImGui.SameLine();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo("##viewLighting", LightingLabel(_lightMode)))
        {
            foreach (ViewportLighting mode in Enum.GetValues<ViewportLighting>())
            {
                if (ImGui.Selectable(LightingLabel(mode), mode == _lightMode)) _lightMode = mode;
                Tip(LightingHelp(mode));
            }
            ImGui.EndCombo();
        }
        Tip("Scene View Lighting - changes only how the editor shows the scene, never the scene itself.");

        ImGui.SameLine();
        ImGui.SetNextItemWidth(90);
        ImGui.SliderFloat("##viewExposure", ref _viewExposureEv, -2f, 2f, "Exp %.1f");
        Tip("View Exposure (-2..+2 stops). Brightens or darkens the Scene View only - the scene's own exposure is not touched.\nDouble-click to type a value.");

        ImGui.SameLine();
        ImGui.Checkbox("Grid", ref _showGrid);
        Tip("Show the floor grid.");
        ImGui.SameLine();
        ImGui.Checkbox("Gizmos", ref _showGizmos);
        Tip("Show event markers, trigger radii and transform handles.");

        ImGui.SameLine();
        if (ImGui.Button("Normalize View")) NormalizeView();
        Tip("Reset the Scene View to a safe state: Editor Lighting, normal exposure, grid and gizmos on.");

        ImGui.SameLine();
        if (ImGui.Button("Frame All")) FrameScene();
        Tip("Fly the camera so the whole scene is visible.\nShortcut: Home");

        ImGui.Separator();
    }

    private static string LightingLabel(ViewportLighting mode) => mode switch
    {
        ViewportLighting.Scene => "Scene Lighting",
        ViewportLighting.Editor => "Editor Lighting",
        ViewportLighting.Unlit => "Unlit",
        _ => "Wireframe",
    };

    private static string LightingHelp(ViewportLighting mode) => mode switch
    {
        ViewportLighting.Scene => "The real lighting and atmosphere of the scene, exactly as the player sees it.",
        ViewportLighting.Editor => "Temporary editor-only light. Never exported to the game. Use it when the scene is dark or has no lights yet.",
        ViewportLighting.Unlit => "Materials without any lighting - flat colors and textures.",
        _ => "Technical mode: triangle edges only.",
    };

    private void NormalizeView()
    {
        _lightMode = ViewportLighting.Editor;
        _viewExposureEv = 0f;
        _showGrid = true;
        _showGizmos = true;
        _darkWarningDismissed = false;
        _statusMessage = "Scene View normalized.";
    }

                                                               

    private Vector3 CurrentViewportBackground()
    {
        if (_playMode || _lightMode == ViewportLighting.Scene) return _level.Environment.Background;
        return EditorBackground;
    }

                                                                                                             
                                                                                                   
    private void PrepareModelShaderForViewport()
    {
        var shader = _modelShader!;
        shader.Use();

        bool showEnvironment = _playMode || _lightMode == ViewportLighting.Scene;
        (showEnvironment ? EffectiveEnvironment() : SceneEnvironment.Default).ApplyTo(shader);

        int mode = _playMode ? 0 : _lightMode switch
        {
            ViewportLighting.Editor => 1,
            ViewportLighting.Unlit => 2,
            _ => 0,
        };
        shader.SetInt("viewMode", mode);
        shader.SetFloat("viewExposureEv", _playMode ? 0f : _viewExposureEv);

        if (!_playMode && _lightMode == ViewportLighting.Wireframe)
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
    }

    private void FinishModelShaderForViewport()
    {
        if (!_playMode && _lightMode == ViewportLighting.Wireframe)
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    }

                                                                                                             
                                                                                                       
    private void SampleViewportBrightness()
    {
        _darkCheckTimer -= (float)_lastFrameDelta;
        if (_darkCheckTimer > 0f) return;
        _darkCheckTimer = 0.5f;

        if (_playMode || _sceneFramebuffer == null || _level.Models.Count == 0 || _lightMode == ViewportLighting.Wireframe)
        {
            _sceneIsDark = false;
            return;
        }

        int w = _sceneFramebuffer.Width, h = _sceneFramebuffer.Height;
        if (w < 16 || h < 16) return;

        var bg = CurrentViewportBackground();
        var px = new byte[4];
        int hits = 0;
        float sum = 0f;
        for (int gy = 1; gy <= 6; gy++)
        {
            for (int gx = 1; gx <= 6; gx++)
            {
                GL.ReadPixels(w * gx / 7, h * gy / 7, 1, 1,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte, px);
                float r = px[0] / 255f, g = px[1] / 255f, b = px[2] / 255f;
                if (MathF.Abs(r - bg.X) + MathF.Abs(g - bg.Y) + MathF.Abs(b - bg.Z) < 0.03f) continue;             
                hits++;
                sum += 0.299f * r + 0.587f * g + 0.114f * b;
            }
        }

        _sceneIsDark = hits >= 3 && sum / hits < 0.07f;
        if (!_sceneIsDark) _darkWarningDismissed = false;
    }

                                      

    private void DrawViewportOverlays(System.Numerics.Vector2 imageMin, System.Numerics.Vector2 size)
    {
        var drawList = ImGui.GetWindowDrawList();

        if (_playMode)
        {
            var max = new System.Numerics.Vector2(imageMin.X + size.X, imageMin.Y + size.Y);
            drawList.AddRect(imageMin, max, ImGui.GetColorU32(new System.Numerics.Vector4(0.85f, 0.25f, 0.2f, 1f)), 0f, ImDrawFlags.None, 3f);

            const string label = "PLAY MODE";
            var textSize = ImGui.CalcTextSize(label);
            var boxMin = new System.Numerics.Vector2(imageMin.X + size.X * 0.5f - textSize.X * 0.5f - 10f, imageMin.Y + 6f);
            var boxMax = new System.Numerics.Vector2(boxMin.X + textSize.X + 20f, boxMin.Y + textSize.Y + 8f);
            drawList.AddRectFilled(boxMin, boxMax, ImGui.GetColorU32(new System.Numerics.Vector4(0.75f, 0.18f, 0.15f, 0.92f)), 4f);
            drawList.AddText(new System.Numerics.Vector2(boxMin.X + 10f, boxMin.Y + 4f), ImGui.GetColorU32(new System.Numerics.Vector4(1f, 1f, 1f, 1f)), label);
            return;
        }

                                                                                                             
        if (_lightMode != ViewportLighting.Scene || MathF.Abs(_viewExposureEv) > 0.01f)
        {
            var info = $"{LightingLabel(_lightMode)}  Exp {_viewExposureEv:+0.0;-0.0;0.0}";
            var textSize = ImGui.CalcTextSize(info);
            var pos = new System.Numerics.Vector2(imageMin.X + size.X - textSize.X - 14f, imageMin.Y + 6f);
            drawList.AddRectFilled(new System.Numerics.Vector2(pos.X - 6f, pos.Y - 3f),
                new System.Numerics.Vector2(pos.X + textSize.X + 6f, pos.Y + textSize.Y + 3f),
                ImGui.GetColorU32(new System.Numerics.Vector4(0f, 0f, 0f, 0.55f)), 3f);
            drawList.AddText(pos, ImGui.GetColorU32(new System.Numerics.Vector4(0.9f, 0.9f, 0.9f, 1f)), info);
        }

        if (_sceneIsDark && !_darkWarningDismissed)
            DrawDarkSceneWarning(imageMin);
        else if (_level.Models.Count == 0 && _level.Triggers.Count == 0 && _level.Posters.Count == 0)
            DrawEmptySceneHint(imageMin, size);
    }

    private void DrawDarkSceneWarning(System.Numerics.Vector2 imageMin)
    {
        ImGui.SetCursorScreenPos(new System.Numerics.Vector2(imageMin.X + 12f, imageMin.Y + 34f));
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new System.Numerics.Vector4(0.12f, 0.1f, 0.05f, 0.92f));
        if (ImGui.BeginChild("##darkWarning", new System.Numerics.Vector2(330f, 128f), ImGuiChildFlags.Border))
        {
            ImGui.TextColored(new System.Numerics.Vector4(1f, 0.8f, 0.3f, 1f), "Scene is very dark.");
            ImGui.TextDisabled("Try:");
            if (ImGui.SmallButton("Enable Editor Lighting")) _lightMode = ViewportLighting.Editor;
            ImGui.SameLine();
            if (ImGui.SmallButton("Exposure +1")) _viewExposureEv = MathF.Min(_viewExposureEv + 1f, 2f);
            if (ImGui.SmallButton("Switch to Unlit")) _lightMode = ViewportLighting.Unlit;
            ImGui.SameLine();
            if (ImGui.SmallButton("Open Environment")) _showEnvironmentWindow = true;
            ImGui.SameLine();
            if (ImGui.SmallButton("Dismiss")) _darkWarningDismissed = true;
        }
        ImGui.EndChild();
        ImGui.PopStyleColor();
    }

    private void DrawEmptySceneHint(System.Numerics.Vector2 imageMin, System.Numerics.Vector2 size)
    {
        ImGui.SetCursorScreenPos(new System.Numerics.Vector2(imageMin.X + size.X * 0.5f - 170f, imageMin.Y + size.Y * 0.5f - 60f));
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new System.Numerics.Vector4(0.08f, 0.08f, 0.1f, 0.85f));
        if (ImGui.BeginChild("##emptyScene", new System.Numerics.Vector2(340f, 130f), ImGuiChildFlags.Border))
        {
            ImGui.Text("Empty Scene");
            ImGui.TextDisabled("Nothing here yet.");
            if (ImGui.SmallButton("+ Floor")) PlaceAssetAt(EditorSceneIO.PrimitivePrefix + "floor", _flyCamera!.Camera.Position, _flyCamera.Camera.Front);
            ImGui.SameLine();
            if (ImGui.SmallButton("+ Wall")) PlaceAssetAt(EditorSceneIO.PrimitivePrefix + "wall", _flyCamera!.Camera.Position, _flyCamera.Camera.Front);
            ImGui.SameLine();
            if (ImGui.SmallButton("+ Event")) PlaceEventAtRay(_flyCamera!.Camera.Position, _flyCamera.Camera.Front);
            ImGui.SameLine();
            if (ImGui.SmallButton("Player Start")) PlayerStartAtCamera();
            ImGui.Spacing();
            ImGui.TextWrapped("Tip: drag a model (.glb) from the Project panel into the Scene View.");
        }
        ImGui.EndChild();
        ImGui.PopStyleColor();
    }

    private void PlayerStartAtCamera()
    {
        if (_flyCamera == null) return;
        if (TryResolvePlacementPoint(_flyCamera.Camera.Position, _flyCamera.Camera.Front, out var point))
        {
            _level.PlayerStartPosition = point;
            _level.PlayerStartYawDegrees = 0f;
            _statusMessage = "Player Start placed.";
        }
    }

                                                 

    private string _dragAssetKey = string.Empty;

    private void BeginAssetDragSource(string assetKey, string label)
    {
        if (!ImGui.BeginDragDropSource()) return;
        _dragAssetKey = assetKey;
        ImGui.SetDragDropPayload("RC_ASSET", IntPtr.Zero, 0);
        ImGui.Text(label);
        ImGui.TextDisabled("Drop into the Scene View");
        ImGui.EndDragDropSource();
    }

    private unsafe void DrawSceneViewDropTarget()
    {
        if (_playMode || !ImGui.BeginDragDropTarget()) return;

        var payload = ImGui.AcceptDragDropPayload("RC_ASSET");
        if (payload.NativePtr != null && !string.IsNullOrEmpty(_dragAssetKey) && _flyCamera != null
            && ViewportRay.TryCompute(_flyCamera.Camera, MouseState.Position, _viewportOrigin, _viewportSize,
                out var rayOrigin, out var rayDirection))
        {
            _selectedAsset = _dragAssetKey;
            PlaceAssetAt(_dragAssetKey, rayOrigin, rayDirection);
            _dragAssetKey = string.Empty;
        }

        ImGui.EndDragDropTarget();
    }

                                    

    private void ResetViewportForNewScene()
    {
        _sceneIsDark = false;
        _darkWarningDismissed = false;
        _darkCheckTimer = 0f;
        _flyCamera?.CancelFocus();
    }

    private void FocusSelected()
    {
        if (_flyCamera == null) return;

        if (_selectedModel is { } model)
        {
            model.GetWorldBounds(out var min, out var max);
            _flyCamera.FocusOn((min + max) * 0.5f, (max - min).Length * 0.5f);
        }
        else if (_selectedTrigger is { } trigger) _flyCamera.FocusOn(trigger.Center, MathF.Max(trigger.Radius, 1.5f));
        else if (_selectedMoodZone is { } zone) _flyCamera.FocusOn(zone.Center, MathF.Max(zone.Radius * 0.6f, 2f));
        else if (_selectedMoodEvent is { } moodEvent) _flyCamera.FocusOn(moodEvent.Center, MathF.Max(moodEvent.Radius * 0.6f, 2f));
        else if (_selectedPoster is { } poster) _flyCamera.FocusOn(poster.Position, MathF.Max(poster.Scale * 1.5f, 1.5f));
        else FrameScene();
    }

    private void FrameScene()
    {
        if (_flyCamera == null) return;

        if (_level.Models.Count == 0)
        {
            _flyCamera.FocusOn(_level.PlayerStartPosition ?? Vector3.Zero, 6f);
            return;
        }

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        foreach (var model in _level.Models)
        {
            model.GetWorldBounds(out var mn, out var mx);
            min = Vector3.ComponentMin(min, mn);
            max = Vector3.ComponentMax(max, mx);
        }
        _flyCamera.FrameBounds(min, max);
    }

                                                     

    private void HandleViewportHotkeys(bool hovered)
    {
        if (_playMode || !hovered || _flyCamera == null) return;
        if (ImGui.GetIO().WantTextInput) return;

        var kb = KeyboardState;
        bool ctrl = kb.IsKeyDown(Keys.LeftControl) || kb.IsKeyDown(Keys.RightControl);
        bool alt = kb.IsKeyDown(Keys.LeftAlt) || kb.IsKeyDown(Keys.RightAlt);
        bool shift = kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift);
        if (ctrl || alt || shift || MouseState.IsButtonDown(MouseButton.Right)) return;                                         

        if (kb.IsKeyPressed(Keys.Q)) _editMode = EditMode.Select;
        if (kb.IsKeyPressed(Keys.W)) { _editMode = EditMode.Select; _gizmoMode = GizmoMode.Move; }
        if (kb.IsKeyPressed(Keys.E)) { _editMode = EditMode.Select; _gizmoMode = GizmoMode.Rotate; }
        if (kb.IsKeyPressed(Keys.R)) { _editMode = EditMode.Select; _gizmoMode = GizmoMode.Scale; }
        if (kb.IsKeyPressed(Keys.F)) FocusSelected();
        if (kb.IsKeyPressed(Keys.Home)) FrameScene();
    }

                              

    private void SaveCameraBookmark(int slot)
    {
        _flyCamera?.SaveBookmark(slot);
        _statusMessage = $"Camera bookmark {slot + 1} saved.";
    }

    private void GoToCameraBookmark(int slot)
    {
        if (_flyCamera == null) return;
        _statusMessage = _flyCamera.RestoreBookmark(slot) ? $"Camera bookmark {slot + 1}." : $"Camera bookmark {slot + 1} is empty - save one first.";
    }

                                    

                                                                                                             
    private static void Tip(string text)
    {
        if (!ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled)) return;
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 30f);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }

                                                                             
    private static void HelpMarker(string text)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        Tip(text);
    }
}
