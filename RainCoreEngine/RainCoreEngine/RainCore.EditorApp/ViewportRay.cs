using OpenTK.Mathematics;

namespace RainCore.EditorApp;

             
                                                                        
                                                                           
                                                                   
                                                                           
                                                                  
                                                                            
                                                                  
                                                             
   
                                                                      
                                                                          
                                                           
   
                                                                            
                                               
              
public static class ViewportRay
{
                 
                                                                             
                                                                              
                                                                           
                                                                    
                                                                             
                                                                  
       
                                                                          
                                                                            
                                                                             
                                                       
                  
    public static bool TryCompute(Camera camera, Vector2 mouseScreenPos,
        Vector2 viewportOrigin, Vector2 viewportSize, out Vector3 origin, out Vector3 direction)
    {
        origin = camera.Position;
        direction = camera.Front;

        if (viewportSize.X <= 0f || viewportSize.Y <= 0f) return false;

        float localX = mouseScreenPos.X - viewportOrigin.X;
        float localY = mouseScreenPos.Y - viewportOrigin.Y;
        if (localX < 0f || localY < 0f || localX > viewportSize.X || localY > viewportSize.Y)
            return false;

                                                                             
                                           
        float ndcX = localX / viewportSize.X * 2f - 1f;
        float ndcY = 1f - localY / viewportSize.Y * 2f;

                                                                     
                                                                            
                                                                        
                                                                           
                                    
        var invProjection = Matrix4.Invert(camera.GetProjectionMatrix());
        var invView = Matrix4.Invert(camera.GetViewMatrix());

        var nearWorld = Unproject(new Vector4(ndcX, ndcY, -1f, 1f), invProjection, invView);
        var farWorld = Unproject(new Vector4(ndcX, ndcY, 1f, 1f), invProjection, invView);

        var dir = farWorld - nearWorld;
        if (dir.LengthSquared < 1e-10f) return false;

        origin = nearWorld;
        direction = Vector3.Normalize(dir);
        return true;
    }

    private static Vector3 Unproject(Vector4 clip, Matrix4 invProjection, Matrix4 invView)
    {
        var view = clip * invProjection;
        if (MathF.Abs(view.W) > 1e-8f) view /= view.W;
        var world = view * invView;
        return world.Xyz;
    }

                                                                                
                                                                       
                                                                        
                                                                             
                                                                         
                                                                            
                                                                                
                                                                
    public static bool TryWorldToScreen(Camera camera, Vector3 worldPos,
        Vector2 viewportOrigin, Vector2 viewportSize, out Vector2 screenPos)
    {
        screenPos = default;
        if (viewportSize.X <= 0f || viewportSize.Y <= 0f) return false;

        var clip = new Vector4(worldPos, 1f) * camera.GetViewMatrix() * camera.GetProjectionMatrix();
        if (clip.W <= 1e-5f) return false;

        float ndcX = clip.X / clip.W;
        float ndcY = clip.Y / clip.W;

        float localX = (ndcX * 0.5f + 0.5f) * viewportSize.X;
        float localY = (1f - (ndcY * 0.5f + 0.5f)) * viewportSize.Y;

        screenPos = new Vector2(viewportOrigin.X + localX, viewportOrigin.Y + localY);
        return true;
    }

                                                                                  
                                                                               
                                                                                 
    public static float DistancePointToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float lengthSquared = ab.LengthSquared;
        if (lengthSquared < 1e-6f) return (point - a).Length;

        float t = Vector2.Dot(point - a, ab) / lengthSquared;
        t = Math.Clamp(t, 0f, 1f);
        var closest = a + ab * t;
        return (point - closest).Length;
    }

                                                                                     
                                                                                     
                                                                                 
                                                                                   
                                                                              
                                                                                
                                                                            
                                                                              
                                                                       
    public static bool TryClosestPointOnLineToRay(Vector3 lineOrigin, Vector3 lineDirection,
        Vector3 rayOrigin, Vector3 rayDirection, out Vector3 closestOnLine)
    {
        closestOnLine = lineOrigin;

        var r = rayOrigin - lineOrigin;
        float a = Vector3.Dot(rayDirection, rayDirection);
        float b = Vector3.Dot(rayDirection, lineDirection);
        float c = Vector3.Dot(rayDirection, r);
        float e = Vector3.Dot(lineDirection, lineDirection);
        float f = Vector3.Dot(lineDirection, r);

        float denom = a * e - b * b;
        if (MathF.Abs(denom) < 1e-6f) return false;

        float t = (a * f - b * c) / denom;                                
        closestOnLine = lineOrigin + lineDirection * t;
        return true;
    }
}
