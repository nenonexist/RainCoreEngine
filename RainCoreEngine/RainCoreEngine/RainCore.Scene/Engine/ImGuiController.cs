using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using NumericsVector2 = System.Numerics.Vector2;

namespace RainCoreScene;

public sealed class ImGuiController : IDisposable
{
    private int _vertexArray;
    private int _vertexBuffer;
    private int _indexBuffer;
    private int _fontTexture;
    private int _shader;
    private int _vertexShader;
    private int _fragmentShader;
    private int _projectionLocation;
    private int _textureLocation;
    private bool _frameStarted;

    public ImGuiController()
    {
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        ImGui.StyleColorsDark();
        CreateDeviceResources();
    }

    public void Update(float deltaSeconds, Vector2i size, MouseState mouse, KeyboardState keyboard)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new NumericsVector2(size.X, size.Y);
        io.DisplayFramebufferScale = NumericsVector2.One;
        io.DeltaTime = deltaSeconds > 0f ? deltaSeconds : 1f / 60f;
        var mousePosition = mouse.Position;
        io.AddMousePosEvent(mousePosition.X, mousePosition.Y);
        io.AddMouseButtonEvent(0, mouse.IsButtonDown(MouseButton.Left));
        io.AddMouseButtonEvent(1, mouse.IsButtonDown(MouseButton.Right));
        io.AddMouseButtonEvent(2, mouse.IsButtonDown(MouseButton.Middle));
        io.AddMouseWheelEvent(0f, mouse.ScrollDelta.Y);
        ImGui.NewFrame();
        _frameStarted = true;
    }

    public void Render()
    {
        if (!_frameStarted) return;
        _frameStarted = false;
        ImGui.Render();
        RenderDrawData(ImGui.GetDrawData());
    }

    private void CreateDeviceResources()
    {
        _vertexArray = GL.GenVertexArray();
        _vertexBuffer = GL.GenBuffer();
        _indexBuffer = GL.GenBuffer();
        GL.BindVertexArray(_vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
        int stride = System.Runtime.InteropServices.Marshal.SizeOf<ImDrawVert>();
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

        _vertexShader = CompileShader(ShaderType.VertexShader, "#version 330 core\nlayout(location=0) in vec2 aPos; layout(location=1) in vec2 aUv; layout(location=2) in vec4 aColor; uniform mat4 projection; out vec2 uv; out vec4 color; void main(){ uv=aUv; color=aColor; gl_Position=projection*vec4(aPos,0,1); }");
        _fragmentShader = CompileShader(ShaderType.FragmentShader, "#version 330 core\nin vec2 uv; in vec4 color; uniform sampler2D texture0; out vec4 FragColor; void main(){ FragColor=color*texture(texture0,uv); }");
        _shader = GL.CreateProgram();
        GL.AttachShader(_shader, _vertexShader);
        GL.AttachShader(_shader, _fragmentShader);
        GL.LinkProgram(_shader);
        _projectionLocation = GL.GetUniformLocation(_shader, "projection");
        _textureLocation = GL.GetUniformLocation(_shader, "texture0");

        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);
        _fontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        io.Fonts.SetTexID((IntPtr)_fontTexture);
        io.Fonts.ClearTexData();
    }

    private static int CompileShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        return shader;
    }

    private void RenderDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0) return;
        GL.Enable(EnableCap.Blend);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.ScissorTest);
        GL.UseProgram(_shader);
        GL.BindVertexArray(_vertexArray);
        var projection = Matrix4.CreateOrthographicOffCenter(0f, drawData.DisplaySize.X, drawData.DisplaySize.Y, 0f, -1f, 1f);
        GL.UniformMatrix4(_projectionLocation, false, ref projection);
        GL.Uniform1(_textureLocation, 0);

        for (int listIndex = 0; listIndex < drawData.CmdListsCount; listIndex++)
        {
            var commandList = drawData.CmdLists[listIndex];
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, commandList.VtxBuffer.Size * System.Runtime.InteropServices.Marshal.SizeOf<ImDrawVert>(), commandList.VtxBuffer.Data, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, commandList.IdxBuffer.Size * sizeof(ushort), commandList.IdxBuffer.Data, BufferUsageHint.StreamDraw);
            int indexOffset = 0;
            for (int commandIndex = 0; commandIndex < commandList.CmdBuffer.Size; commandIndex++)
            {
                var command = commandList.CmdBuffer[commandIndex];
                GL.BindTexture(TextureTarget.Texture2D, (int)command.TextureId);
                var clip = command.ClipRect;
                GL.Scissor((int)clip.X, (int)(drawData.DisplaySize.Y - clip.W), (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)command.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(indexOffset * sizeof(ushort)), (int)command.VtxOffset);
                indexOffset += (int)command.ElemCount;
            }
        }
        GL.Disable(EnableCap.ScissorTest);
        GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.Blend);
    }

    public void Dispose()
    {
        GL.DeleteTexture(_fontTexture);
        GL.DeleteBuffer(_vertexBuffer);
        GL.DeleteBuffer(_indexBuffer);
        GL.DeleteVertexArray(_vertexArray);
        GL.DeleteProgram(_shader);
        GL.DeleteShader(_vertexShader);
        GL.DeleteShader(_fragmentShader);
        ImGui.DestroyContext();
    }
}
