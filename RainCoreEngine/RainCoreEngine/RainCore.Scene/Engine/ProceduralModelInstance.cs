using OpenTK.Mathematics;

using RainCoreGraphics;

namespace RainCoreScene;

                                                                                                                                    
public sealed class ProceduralModelInstance
{
    public ProceduralModel Model { get; }
    public string ModelKey { get; }
    public Vector3 Position;
    public Vector3 RotationDeg;
    public float Scale;
    public bool IsSolid { get; set; }
    public bool IsLiquid { get; set; }
    public bool IsIce { get; set; }
    public bool IgnoreFog { get; set; }
    public bool Visible { get; set; } = true;

    public ProceduralModelInstance(ProceduralModel model, string modelKey, Vector3 position, Vector3 rotationDeg = default, float scale = 1f)
    {
        Model = model;
        ModelKey = modelKey;
        Position = position;
        RotationDeg = rotationDeg;
        Scale = scale;
    }

    private Matrix4 GetModelMatrix() =>
        Matrix4.CreateScale(Scale)
        * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(RotationDeg.X))
        * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotationDeg.Y))
        * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(RotationDeg.Z))
        * Matrix4.CreateTranslation(Position);

    public void Render(Shader shader)
    {
        if (!Visible) return;
        Model.Render(shader, GetModelMatrix());
    }

                                                                                   
                                                                                                                                
    public void GetWorldBounds(out Vector3 min, out Vector3 max)
    {
        var a = Position + Model.LocalMin * Scale;
        var b = Position + Model.LocalMax * Scale;
        min = Vector3.ComponentMin(a, b);
        max = Vector3.ComponentMax(a, b);
    }
}

