using OpenTK.Mathematics;

namespace RainCore;

                                                                          
public readonly struct Frustum
{
    private readonly Vector4 _left;
    private readonly Vector4 _right;
    private readonly Vector4 _bottom;
    private readonly Vector4 _top;
    private readonly Vector4 _near;
    private readonly Vector4 _far;

    public Frustum(Matrix4 viewProjection)
    {
                                                                       
                                                                          
        var column0 = new Vector4(viewProjection.M11, viewProjection.M21, viewProjection.M31, viewProjection.M41);
        var column1 = new Vector4(viewProjection.M12, viewProjection.M22, viewProjection.M32, viewProjection.M42);
        var column2 = new Vector4(viewProjection.M13, viewProjection.M23, viewProjection.M33, viewProjection.M43);
        var column3 = new Vector4(viewProjection.M14, viewProjection.M24, viewProjection.M34, viewProjection.M44);

        _left = NormalizePlane(column3 + column0);
        _right = NormalizePlane(column3 - column0);
        _bottom = NormalizePlane(column3 + column1);
        _top = NormalizePlane(column3 - column1);
        _near = NormalizePlane(column3 + column2);
        _far = NormalizePlane(column3 - column2);
    }

    public bool IntersectsAabb(Vector3 min, Vector3 max)
    {
        return IntersectsPlane(_left, min, max)
            && IntersectsPlane(_right, min, max)
            && IntersectsPlane(_bottom, min, max)
            && IntersectsPlane(_top, min, max)
            && IntersectsPlane(_near, min, max)
            && IntersectsPlane(_far, min, max);
    }

    private static bool IntersectsPlane(Vector4 plane, Vector3 min, Vector3 max)
    {
        var positive = new Vector3(
            plane.X >= 0f ? max.X : min.X,
            plane.Y >= 0f ? max.Y : min.Y,
            plane.Z >= 0f ? max.Z : min.Z);

        return Vector3.Dot(new Vector3(plane.X, plane.Y, plane.Z), positive) + plane.W >= 0f;
    }

    private static Vector4 NormalizePlane(Vector4 plane)
    {
        float length = new Vector3(plane.X, plane.Y, plane.Z).Length;
        return length > 0f ? plane / length : plane;
    }
}