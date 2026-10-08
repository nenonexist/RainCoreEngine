using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RainCore.EditorApp;

             
                                                                              
                                                                              
                                                                             
                                                                               
                                                                      
   
                                                                                
                                                                             
                                                                            
                                                                         
                               
              
public sealed class ImGuiController : IDisposable
{
    private int _windowWidth;
    private int _windowHeight;

    private int _vertexArray;
    private int _vertexBuffer;
    private int _vertexBufferSize;
    private int _indexBuffer;
    private int _indexBufferSize;
    private int _fontTexture;
    private int _shader;
    private int _shaderProjectionMatrixLocation;
    private int _shaderFontTextureLocation;

    private readonly List<char> _pressedChars = new();
    private string _iniFilePath;

                                                                                 
                                                                             
                                                                                
                                                                             
                                                                                              
    private IntPtr _iniFilenamePtr;

                 
                                                                                 
                                                                                 
                                                                                 
                                                                                 
                                                                                 
                                                                     
                                                                                   
                                                                         
                                                                       
                                                                               
                                                                              
                                                                             
                                                           
                  
    public unsafe ImGuiController(GameWindow window, int width, int height, string iniFilePath)
    {
        _windowWidth = width;
        _windowHeight = height;
        _iniFilePath = iniFilePath;

        IntPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);

        var io = ImGui.GetIO();
        LoadFont(io);
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ApplyFluentStyle();

                                                                                
                                                                             
                                                                              
                                                                               
                                                                               
                                                                              
                                                                              
                                                                      
        _iniFilenamePtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(_iniFilePath);
        io.NativePtr->IniFilename = (byte*)_iniFilenamePtr;

        CreateDeviceResources();
        SetKeyMappings();

        window.TextInput += e => _pressedChars.Add((char)e.Unicode);
        window.MouseWheel += e => { };                                                

        SetPerFrameImGuiData(1f / 60f);
        ImGui.NewFrame();
    }

    public void WindowResized(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    private void CreateDeviceResources()
    {
        _vertexBufferSize = 10000;
        _indexBufferSize = 2000;

        int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
        int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

        _vertexArray = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArray);

        _vertexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        _indexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        RecreateFontDeviceTexture();

        const string vertexSource = @"#version 330 core
uniform mat4 projection_matrix;
layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;
out vec4 color;
out vec2 texCoord;
void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
        const string fragmentSource = @"#version 330 core
uniform sampler2D in_fontTexture;
in vec4 color;
in vec2 texCoord;
out vec4 outputColor;
void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";
        _shader = CreateProgram(vertexSource, fragmentSource);
        _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
        _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");

        int stride = Unsafe.SizeOf<ImDrawVert>();
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

        GL.BindVertexArray(prevVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
    }

    private void RecreateFontDeviceTexture()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);

        int prevTexture = GL.GetInteger(GetPName.TextureBinding2D);
        _fontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
            PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

        io.Fonts.SetTexID(_fontTexture);
        io.Fonts.ClearTexData();
        GL.BindTexture(TextureTarget.Texture2D, prevTexture);
    }

                                                                                  
                                                                                     
    public void Update(GameWindow window, float deltaSeconds)
    {
        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput(window);
        ImGui.NewFrame();
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(_windowWidth, _windowHeight);
        io.DisplayFramebufferScale = System.Numerics.Vector2.One;
        io.DeltaTime = deltaSeconds > 0f ? deltaSeconds : 1f / 60f;
    }

    private void UpdateImGuiInput(GameWindow window)
    {
        var io = ImGui.GetIO();
        var mouse = window.MouseState;
        var keyboard = window.KeyboardState;

        io.MouseDown[0] = mouse.IsButtonDown(MouseButton.Left);
        io.MouseDown[1] = mouse.IsButtonDown(MouseButton.Right);
        io.MouseDown[2] = mouse.IsButtonDown(MouseButton.Middle);
        io.MousePos = new System.Numerics.Vector2(mouse.X, mouse.Y);
        io.MouseWheel = mouse.ScrollDelta.Y;
        io.MouseWheelH = mouse.ScrollDelta.X;

        foreach (char c in _pressedChars) io.AddInputCharacter(c);
        _pressedChars.Clear();

        io.KeyCtrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
        io.KeyAlt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
        io.KeyShift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
        io.KeySuper = keyboard.IsKeyDown(Keys.LeftSuper) || keyboard.IsKeyDown(Keys.RightSuper);
    }

    private static readonly Dictionary<Keys, ImGuiKey> KeyMap = new();

    private void SetKeyMappings()
    {
                                                                               
                                                                           
                                                                              
        KeyMap[Keys.Tab] = ImGuiKey.Tab;
        KeyMap[Keys.Left] = ImGuiKey.LeftArrow;
        KeyMap[Keys.Right] = ImGuiKey.RightArrow;
        KeyMap[Keys.Up] = ImGuiKey.UpArrow;
        KeyMap[Keys.Down] = ImGuiKey.DownArrow;
        KeyMap[Keys.Enter] = ImGuiKey.Enter;
        KeyMap[Keys.Escape] = ImGuiKey.Escape;
        KeyMap[Keys.Backspace] = ImGuiKey.Backspace;
        KeyMap[Keys.Delete] = ImGuiKey.Delete;
    }

                                                                                     
                                                                               
                                                                                     
    public void Render()
    {
        ImGui.Render();
        RenderDrawData(ImGui.GetDrawData());
    }

    private void RenderDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0) return;

        int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
        int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
        bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
        bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
        bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
        bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);

        GL.Enable(EnableCap.Blend);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.ScissorTest);

        drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        GL.Viewport(0, 0, _windowWidth, _windowHeight);
        Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(0f, _windowWidth, _windowHeight, 0f, -1f, 1f);

        GL.UseProgram(_shader);
        GL.UniformMatrix4(_shaderProjectionMatrixLocation, false, ref mvp);
        GL.Uniform1(_shaderFontTextureLocation, 0);
        GL.ActiveTexture(TextureUnit.Texture0);

        GL.BindVertexArray(_vertexArray);

        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
                                                                                
                                                                           
                                                                              
                                                                
            var cmdList = drawData.CmdLists[n];

            int vertexSize = cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _vertexBufferSize)
            {
                _vertexBufferSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }

            int indexSize = cmdList.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _indexBufferSize)
            {
                _indexBufferSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
                GL.BufferData(BufferTarget.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertexSize, cmdList.VtxBuffer.Data);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, indexSize, cmdList.IdxBuffer.Data);

            for (int cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex++)
            {
                var cmd = cmdList.CmdBuffer[cmdIndex];
                GL.BindTexture(TextureTarget.Texture2D, (int)cmd.TextureId);

                var clip = cmd.ClipRect;
                GL.Scissor((int)clip.X, (int)(_windowHeight - clip.W), (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));

                GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)cmd.ElemCount,
                    DrawElementsType.UnsignedShort, (IntPtr)(cmd.IdxOffset * sizeof(ushort)), (int)cmd.VtxOffset);
            }
        }

        GL.Disable(EnableCap.ScissorTest);
        if (prevBlendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
        if (prevDepthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
        if (prevCullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
        if (prevScissorTestEnabled) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
        GL.BindVertexArray(prevVao);
        GL.UseProgram(prevProgram);
    }

    private static int CreateProgram(string vertexSource, string fragmentSource)
    {
        int vertex = CompileShader(ShaderType.VertexShader, vertexSource);
        int fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
            throw new Exception($"Editor: ImGui shader link error: {GL.GetProgramInfoLog(program)}");

        GL.DetachShader(program, vertex);
        GL.DetachShader(program, fragment);
        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
        return program;
    }

    private static int CompileShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
            throw new Exception($"Editor: {type} compile error: {GL.GetShaderInfoLog(shader)}");
        return shader;
    }

                                                                                      
                                                                                 
                                                                                      
                                                                                    
                                                                                              
    public void SaveLayout()
    {
        try { ImGui.SaveIniSettingsToDisk(_iniFilePath); }
        catch (Exception ex) { Console.WriteLine($"[Editor] Failed to save panel layout: {ex.Message}"); }
    }

                                                                                     
                                                                                     
                                                                                 
                                                                            
                                                                                  
                                                                            
                                                                                
                                                                      
    public unsafe void SwitchIniFile(string newIniFilePath)
    {
        var io = ImGui.GetIO();

        if (_iniFilenamePtr != IntPtr.Zero)
            System.Runtime.InteropServices.Marshal.FreeHGlobal(_iniFilenamePtr);

        _iniFilePath = newIniFilePath;
        _iniFilenamePtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(newIniFilePath);
        io.NativePtr->IniFilename = (byte*)_iniFilenamePtr;

        if (File.Exists(newIniFilePath))
            ImGui.LoadIniSettingsFromDisk(newIniFilePath);
    }

                                                                         
                                                                       
                                                                            
                                                                        
                                                                           
                                                                             
                                                                      
                                                                              
                                                                
       
                                                                      
                                                                            
                                                                         
                                                                                     
    private static void ApplyFluentStyle()
    {
        static System.Numerics.Vector4 Col(float r, float g, float b, float a) => new(r, g, b, a);

        var style = ImGui.GetStyle();

        style.WindowRounding = 8f;
        style.ChildRounding = 6f;
        style.FrameRounding = 5f;
        style.PopupRounding = 8f;
        style.ScrollbarRounding = 8f;
        style.GrabRounding = 5f;
        style.TabRounding = 6f;

        style.WindowBorderSize = 1f;
        style.FrameBorderSize = 0f;
        style.PopupBorderSize = 1f;

        style.WindowPadding = new System.Numerics.Vector2(10f, 10f);
        style.FramePadding = new System.Numerics.Vector2(8f, 5f);
        style.ItemSpacing = new System.Numerics.Vector2(8f, 6f);
        style.ItemInnerSpacing = new System.Numerics.Vector2(6f, 5f);
        style.IndentSpacing = 18f;
        style.ScrollbarSize = 14f;
        style.GrabMinSize = 10f;

        const float accentR = 0.0f, accentG = 0.47f, accentB = 0.83f;           

        var colors = style.Colors;
        colors[(int)ImGuiCol.Text] = Col(0.92f, 0.92f, 0.94f, 1f);
        colors[(int)ImGuiCol.TextDisabled] = Col(0.55f, 0.55f, 0.58f, 1f);
        colors[(int)ImGuiCol.WindowBg] = Col(0.11f, 0.11f, 0.13f, 1f);
        colors[(int)ImGuiCol.ChildBg] = Col(0.11f, 0.11f, 0.13f, 0f);
        colors[(int)ImGuiCol.PopupBg] = Col(0.13f, 0.13f, 0.15f, 0.98f);
        colors[(int)ImGuiCol.Border] = Col(0.25f, 0.25f, 0.28f, 0.5f);
        colors[(int)ImGuiCol.BorderShadow] = Col(0f, 0f, 0f, 0f);
        colors[(int)ImGuiCol.FrameBg] = Col(0.16f, 0.16f, 0.19f, 1f);
        colors[(int)ImGuiCol.FrameBgHovered] = Col(0.20f, 0.20f, 0.24f, 1f);
        colors[(int)ImGuiCol.FrameBgActive] = Col(accentR, accentG, accentB, 0.4f);
        colors[(int)ImGuiCol.TitleBg] = Col(0.09f, 0.09f, 0.11f, 1f);
        colors[(int)ImGuiCol.TitleBgActive] = Col(0.09f, 0.09f, 0.11f, 1f);
        colors[(int)ImGuiCol.TitleBgCollapsed] = Col(0.09f, 0.09f, 0.11f, 0.8f);
        colors[(int)ImGuiCol.MenuBarBg] = Col(0.13f, 0.13f, 0.15f, 1f);
        colors[(int)ImGuiCol.ScrollbarBg] = Col(0.11f, 0.11f, 0.13f, 0f);
        colors[(int)ImGuiCol.ScrollbarGrab] = Col(0.30f, 0.30f, 0.33f, 1f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = Col(0.38f, 0.38f, 0.41f, 1f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = Col(accentR, accentG, accentB, 1f);
        colors[(int)ImGuiCol.CheckMark] = Col(accentR, accentG, accentB, 1f);
        colors[(int)ImGuiCol.SliderGrab] = Col(accentR, accentG, accentB, 0.8f);
        colors[(int)ImGuiCol.SliderGrabActive] = Col(accentR, accentG, accentB, 1f);
        colors[(int)ImGuiCol.Button] = Col(0.18f, 0.18f, 0.21f, 1f);
        colors[(int)ImGuiCol.ButtonHovered] = Col(accentR, accentG, accentB, 0.55f);
        colors[(int)ImGuiCol.ButtonActive] = Col(accentR, accentG, accentB, 1f);
        colors[(int)ImGuiCol.Header] = Col(accentR, accentG, accentB, 0.35f);
        colors[(int)ImGuiCol.HeaderHovered] = Col(accentR, accentG, accentB, 0.55f);
        colors[(int)ImGuiCol.HeaderActive] = Col(accentR, accentG, accentB, 0.75f);
        colors[(int)ImGuiCol.Separator] = Col(0.25f, 0.25f, 0.28f, 1f);
        colors[(int)ImGuiCol.SeparatorHovered] = Col(accentR, accentG, accentB, 0.7f);
        colors[(int)ImGuiCol.SeparatorActive] = Col(accentR, accentG, accentB, 1f);
        colors[(int)ImGuiCol.ResizeGrip] = Col(accentR, accentG, accentB, 0.25f);
        colors[(int)ImGuiCol.ResizeGripHovered] = Col(accentR, accentG, accentB, 0.6f);
        colors[(int)ImGuiCol.ResizeGripActive] = Col(accentR, accentG, accentB, 0.9f);
        colors[(int)ImGuiCol.Tab] = Col(0.14f, 0.14f, 0.17f, 1f);
        colors[(int)ImGuiCol.TabHovered] = Col(accentR, accentG, accentB, 0.6f);
                                                                                
                                                                          
                                                                      
        colors[(int)ImGuiCol.TabSelected] = Col(0.18f, 0.18f, 0.22f, 1f);
        colors[(int)ImGuiCol.TabDimmed] = Col(0.11f, 0.11f, 0.13f, 1f);
        colors[(int)ImGuiCol.TabDimmedSelected] = Col(0.15f, 0.15f, 0.18f, 1f);
        colors[(int)ImGuiCol.DockingPreview] = Col(accentR, accentG, accentB, 0.6f);
        colors[(int)ImGuiCol.DockingEmptyBg] = Col(0.09f, 0.09f, 0.11f, 1f);
        colors[(int)ImGuiCol.PlotLines] = Col(accentR, accentG, accentB, 1f);
        colors[(int)ImGuiCol.PlotHistogram] = Col(accentR, accentG, accentB, 1f);
        colors[(int)ImGuiCol.TextSelectedBg] = Col(accentR, accentG, accentB, 0.4f);
        colors[(int)ImGuiCol.NavHighlight] = Col(accentR, accentG, accentB, 1f);
    }

                                                                            
                                                                           
                                                                             
                                                                             
                                                                                 
                                                                              
                                                                               
                                                                             
                                                                          
                                                                     
                                                                     
                                                        
       
                                                                             
                                                                                
                                                                             
                                                                      
                                                                             
                             
    private static void LoadFont(ImGuiIOPtr io)
    {
        const string fontPath = @"C:\Windows\Fonts\segoeui.ttf";
        try
        {
            if (File.Exists(fontPath))
            {
                io.Fonts.AddFontFromFileTTF(fontPath, 18f, null, io.Fonts.GetGlyphRangesCyrillic());
                return;
            }
            Console.WriteLine($"[Editor] Segoe UI не найден ({fontPath}) - использую дефолтный шрифт ImGui.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Editor] Не удалось загрузить Segoe UI: {ex.Message} - использую дефолтный шрифт ImGui.");
        }

        io.Fonts.AddFontDefault();
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_vertexArray);
        GL.DeleteBuffer(_vertexBuffer);
        GL.DeleteBuffer(_indexBuffer);
        GL.DeleteTexture(_fontTexture);
        GL.DeleteProgram(_shader);

        if (_iniFilenamePtr != IntPtr.Zero)
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(_iniFilenamePtr);
            _iniFilenamePtr = IntPtr.Zero;
        }
    }
}
