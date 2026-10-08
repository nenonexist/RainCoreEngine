using OpenTK.Mathematics;

namespace RainCore;

             
                                                                         
                                                                            
                                                                           
                             
              
public static class GeometryUtils
{
                                                                               
                                                                                              
    public static bool RayIntersectsAabb(Vector3 origin, Vector3 dir, Vector3 min, Vector3 max, float maxT, out float tHit, out Vector3 normal)
    {
        float tMin = 0f, tMax = maxT;
        var enterAxis = 0;
        var enterSign = -1f;

        for (int axis = 0; axis < 3; axis++)
        {
            float o = axis == 0 ? origin.X : axis == 1 ? origin.Y : origin.Z;
            float d = axis == 0 ? dir.X : axis == 1 ? dir.Y : dir.Z;
            float lo = axis == 0 ? min.X : axis == 1 ? min.Y : min.Z;
            float hi = axis == 0 ? max.X : axis == 1 ? max.Y : max.Z;

            if (MathF.Abs(d) < 1e-8f)
            {
                if (o < lo || o > hi) { tHit = 0; normal = default; return false; }
                continue;
            }

            float t1 = (lo - o) / d;
            float t2 = (hi - o) / d;
            float sign = -1f;
            if (t1 > t2) { (t1, t2) = (t2, t1); sign = 1f; }

            if (t1 > tMin) { tMin = t1; enterAxis = axis; enterSign = sign; }
            tMax = MathF.Min(tMax, t2);
            if (tMin > tMax) { tHit = 0; normal = default; return false; }
        }

        if (tMin < 0f || tMin > maxT) { tHit = 0; normal = default; return false; }

        tHit = tMin;
        normal = enterAxis == 0 ? new Vector3(enterSign, 0, 0)
               : enterAxis == 1 ? new Vector3(0, enterSign, 0)
               : new Vector3(0, 0, enterSign);
        return true;
    }
}
