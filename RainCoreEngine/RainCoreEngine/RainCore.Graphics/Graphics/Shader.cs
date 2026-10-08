using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace RainCoreGraphics;

             
                                                                      
                                                                          
                                                                       
                                                                       
                                                      
              
public class Shader : IDisposable
{
    public readonly int Handle;

    public Shader(string vertPath, string fragPath)
    {
        string vertSrc = File.ReadAllText(vertPath);
        string fragSrc = File.ReadAllText(fragPath);

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertSrc);
        CompileAndCheck(vertexShader, "VERTEX");

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragSrc);
        CompileAndCheck(fragmentShader, "FRAGMENT");

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);
        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            var infoLog = GL.GetProgramInfoLog(Handle);
            throw new Exception($"Shader program link error: {infoLog}");
        }

        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    private static void CompileAndCheck(int shader, string name)
    {
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"{name} shader compile error: {infoLog}");
        }
    }

    public void Use() => GL.UseProgram(Handle);

    public void SetMatrix4(string name, Matrix4 matrix)
    {
        int location = GL.GetUniformLocation(Handle, name);
                                                                          
                                                                          
                                                                           
                                                                           
                                                                 
                                                                     
                              
        GL.UniformMatrix4(location, true, ref matrix);
    }

    public void SetVector2(string name, Vector2 value) => GL.Uniform2(GL.GetUniformLocation(Handle, name), value);
    public void SetVector3(string name, Vector3 value) => GL.Uniform3(GL.GetUniformLocation(Handle, name), value);
    public void SetVector4(string name, Vector4 value) => GL.Uniform4(GL.GetUniformLocation(Handle, name), value);
    public void SetInt(string name, int value) => GL.Uniform1(GL.GetUniformLocation(Handle, name), value);
    public void SetFloat(string name, float value) => GL.Uniform1(GL.GetUniformLocation(Handle, name), value);

    public void Dispose()
    {
        GL.DeleteProgram(Handle);
        GC.SuppressFinalize(this);
    }
}
