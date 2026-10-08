using OpenTK.Mathematics;

namespace RainCore;

             
                                                                          
                                                       
              
public class Camera
{
    public Vector3 Position;
    public float Yaw = -90f;                      
    public float Pitch = 0f;                       

    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right { get; private set; } = Vector3.UnitX;

    public float Fov = 70f;
    public float AspectRatio { get; set; } = 16f / 9f;

                                                                               
                                                                                                  
                                                                                    
                                                                              
                                               
    public Vector3 BobOffset = Vector3.Zero;

    public Camera(Vector3 position, float yaw = -90f, float pitch = 0f)
    {
        Position = position;
        Yaw = yaw;
        Pitch = pitch;
        UpdateVectors();
    }

    public void Rotate(float deltaYaw, float deltaPitch)
    {
        Yaw += deltaYaw;
        Pitch += deltaPitch;
        Pitch = MathHelper.Clamp(Pitch, -89f, 89f);
        UpdateVectors();
    }

    public void SetPose(Vector3 position, float yaw, float pitch)
    {
        Position = position;
        Yaw = yaw;
        Pitch = MathHelper.Clamp(pitch, -89f, 89f);
        UpdateVectors();
    }

    private void UpdateVectors()
    {
        var yawRad = MathHelper.DegreesToRadians(Yaw);
        var pitchRad = MathHelper.DegreesToRadians(Pitch);

        Front = Vector3.Normalize(new Vector3(
            MathF.Cos(pitchRad) * MathF.Cos(yawRad),
            MathF.Sin(pitchRad),
            MathF.Cos(pitchRad) * MathF.Sin(yawRad)
        ));

        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }

    public Matrix4 GetViewMatrix()
    {
        var eye = Position + BobOffset;
        return Matrix4.LookAt(eye, eye + Front, Up);
    }


    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), AspectRatio, 0.05f, 500f);
    }

}
