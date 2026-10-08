using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RainCore.EditorApp;

             
                                                                              
                                                                                                 
                                          
                                                             
                                                                                                      
                                         
                                                                                       
                                                                                              
                                                                     
              
public sealed class EditorFlyCamera
{
    public readonly Camera Camera;
    public float MoveSpeed = 4f;
    public float MouseSensitivity = 0.15f;

                                                                                                                           
    public Vector3? Pivot;

                                                                                                  
    public float OrbitDistance = 8f;

                                                                                          
                                                                                     
    public bool IsNavigating { get; private set; }

                                                                                                
    public bool AltHeld { get; private set; }

    private Vector2 _lastMousePos;
    private bool _wasNavigating;
    private Vector3? _focusTarget;

    public readonly record struct Bookmark(Vector3 Position, float Yaw, float Pitch);
    public readonly Bookmark?[] Bookmarks = new Bookmark?[4];

    public EditorFlyCamera(Vector3 startPosition)
    {
        Camera = new Camera(startPosition);
    }

                                                                                                     
                                                                                    
    public void Update(float dt, GameWindow window, bool viewportHovered)
    {
        var mouse = window.MouseState;
        var kb = window.KeyboardState;
        bool alt = kb.IsKeyDown(Keys.LeftAlt) || kb.IsKeyDown(Keys.RightAlt);
        bool ctrl = kb.IsKeyDown(Keys.LeftControl) || kb.IsKeyDown(Keys.RightControl);
        AltHeld = alt && viewportHovered;

        bool rmb = mouse.IsButtonDown(MouseButton.Right);
        bool mmb = mouse.IsButtonDown(MouseButton.Middle);
        bool lmb = mouse.IsButtonDown(MouseButton.Left);

        bool flying = viewportHovered && rmb && !alt;
        bool orbiting = viewportHovered && alt && lmb;
        bool panning = viewportHovered && mmb;
        bool dollying = viewportHovered && alt && rmb;
        bool navigating = flying || orbiting || panning || dollying || (_wasNavigating && (rmb || mmb || (alt && lmb)));

        var pos = mouse.Position;
        var delta = _wasNavigating ? new Vector2(pos.X - _lastMousePos.X, pos.Y - _lastMousePos.Y) : Vector2.Zero;
        _lastMousePos = pos;

        float pivotDistance = Pivot.HasValue ? MathF.Max(Vector3.Distance(Camera.Position, Pivot.Value), 0.5f) : OrbitDistance;

        if (flying)
        {
            CancelFocus();
            if (_wasNavigating) Camera.Rotate(delta.X * MouseSensitivity, -delta.Y * MouseSensitivity);

                                                                                      
            if (mouse.ScrollDelta.Y != 0f)
                MoveSpeed = Math.Clamp(MoveSpeed * MathF.Pow(1.2f, mouse.ScrollDelta.Y), 0.5f, 120f);

            float mult = kb.IsKeyDown(Keys.LeftShift) ? 3f : ctrl ? 0.25f : 1f;
            float speed = MoveSpeed * mult * dt;
            if (kb.IsKeyDown(Keys.W)) Camera.Position += Camera.Front * speed;
            if (kb.IsKeyDown(Keys.S)) Camera.Position -= Camera.Front * speed;
            if (kb.IsKeyDown(Keys.A)) Camera.Position -= Camera.Right * speed;
            if (kb.IsKeyDown(Keys.D)) Camera.Position += Camera.Right * speed;
            if (kb.IsKeyDown(Keys.E)) Camera.Position += Vector3.UnitY * speed;
            if (kb.IsKeyDown(Keys.Q)) Camera.Position -= Vector3.UnitY * speed;
        }
        else if (orbiting)
        {
            CancelFocus();
            if (_wasNavigating)
            {
                var center = Pivot ?? (Camera.Position + Camera.Front * OrbitDistance);
                float dist = MathF.Max(Vector3.Distance(Camera.Position, center), 0.5f);
                Camera.Rotate(delta.X * MouseSensitivity, -delta.Y * MouseSensitivity);
                Camera.Position = center - Camera.Front * dist;
            }
        }
        else if (panning)
        {
            CancelFocus();
            float k = pivotDistance * 0.0018f;
            Camera.Position += (-Camera.Right * delta.X + Camera.Up * delta.Y) * k;
        }
        else if (dollying)
        {
            CancelFocus();
            Camera.Position -= Camera.Front * (delta.Y * pivotDistance * 0.004f);
        }
        else if (viewportHovered && mouse.ScrollDelta.Y != 0f)
        {
                                                                                                   
            CancelFocus();
            float step = MathF.Max(pivotDistance * 0.12f, 0.35f);
            if (kb.IsKeyDown(Keys.LeftShift)) step *= 3f;
            if (ctrl) step *= 0.25f;
            Camera.Position += Camera.Front * (mouse.ScrollDelta.Y * step);
            OrbitDistance = Math.Clamp(OrbitDistance - mouse.ScrollDelta.Y * step, 0.5f, 400f);
        }

                                                   
        if (_focusTarget is { } target && !navigating)
        {
            float t = 1f - MathF.Exp(-12f * dt);
            Camera.Position = Vector3.Lerp(Camera.Position, target, t);
            if (Vector3.DistanceSquared(Camera.Position, target) < 0.0004f)
            {
                Camera.Position = target;
                _focusTarget = null;
            }
        }

        IsNavigating = flying || orbiting || panning || dollying;
        _wasNavigating = IsNavigating;
    }

                                                                                                         
    public void FocusOn(Vector3 center, float radius)
    {
        radius = MathF.Max(radius, 0.25f);
        float halfFov = MathHelper.DegreesToRadians(Camera.Fov * 0.5f);
        float distance = radius / MathF.Sin(MathF.Min(halfFov, 1.2f)) * 1.15f;
        _focusTarget = center - Camera.Front * distance;
        OrbitDistance = distance;
    }

                                                               
    public void FrameBounds(Vector3 min, Vector3 max)
    {
        var center = (min + max) * 0.5f;
        FocusOn(center, (max - min).Length * 0.5f);
    }

    public void CancelFocus() => _focusTarget = null;

    public void SaveBookmark(int slot)
    {
        if ((uint)slot >= (uint)Bookmarks.Length) return;
        Bookmarks[slot] = new Bookmark(Camera.Position, Camera.Yaw, Camera.Pitch);
    }

    public bool RestoreBookmark(int slot)
    {
        if ((uint)slot >= (uint)Bookmarks.Length || Bookmarks[slot] is not { } b) return false;
        CancelFocus();
        Camera.Position = b.Position;
                                                                                                       
        Camera.Rotate(b.Yaw - Camera.Yaw, b.Pitch - Camera.Pitch);
        return true;
    }
}
