using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;

namespace RainCore.Player;

             
                                                                             
                                                                       
                                                                       
                                                                                
                                                                             
                                                                   
                                                              
   
                                                                         
                                                                          
                                                                            
                                   
              
public sealed class DialogueOverlay
{
    private const int CanvasWidth = 1024;
    private const int CanvasHeight = 480;

    private readonly Shader _shader;
    private readonly int _vao;
    private readonly int _vbo;

                                                                         
                                                                             
                                                                         
                                 
    private string? _cachedBodyText;
    private int _bodyTextureHandle;
    private float _bodyTexW, _bodyTexH;

    private readonly List<(string text, int handle, float w, float h)> _choiceTextures = new();
    private string? _cachedChoicePrompt;
    private int _choicePromptHandle;
    private float _choicePromptW, _choicePromptH;

    public DialogueOverlay(string shadersDir)
    {
        _shader = new Shader(Path.Combine(shadersDir, "ui_text.vert"), Path.Combine(shadersDir, "ui_text.frag"));

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                                                                           
                                                                               
        GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        int stride = 4 * sizeof(float);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }

                                                                             
                                                                      
    public void Draw(DialogueState dlg)
    {
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader.Use();
        GL.BindVertexArray(_vao);

        const float boxW = 620f, boxH = 110f;
        float boxX = (CanvasWidth - boxW) / 2f;
        float boxY = CanvasHeight - boxH - 16f;

        if (dlg.HasDialogue)
        {
            DrawPanel(boxX, boxY, boxW, boxH, new Vector4(0.05f, 0.05f, 0.08f, 0.82f));
            var body = string.IsNullOrEmpty(dlg.Speaker) ? dlg.Text! : $"{dlg.Speaker}\n{dlg.Text}";
            EnsureBodyTexture(body);
            DrawTextCentered(boxX + 16f, boxY + 12f, boxW - 32f, boxH - 24f);
        }
        else if (dlg.HasNote)
        {
            DrawPanel(boxX, boxY, boxW, boxH, new Vector4(0.05f, 0.06f, 0.1f, 0.82f));
            EnsureBodyTexture($"[{dlg.NoteTitle}]\n{dlg.NoteText}");
            DrawTextCentered(boxX + 16f, boxY + 12f, boxW - 32f, boxH - 24f);
        }

        if (dlg.IsChoicePending)
        {
            var options = dlg.ChoiceOptions!;
            float choiceH = 40f + options.Count * 30f;
            float choiceY = boxY - choiceH - 10f;
            DrawPanel(boxX, choiceY, boxW, choiceH, new Vector4(0.05f, 0.05f, 0.08f, 0.9f));

            EnsureChoiceTextures(dlg.ChoicePrompt ?? string.Empty, options);

            float y = choiceY + 8f;
            if (_choicePromptHandle != 0)
            {
                DrawTexturedQuad(_choicePromptHandle, boxX + 16f, y, MathF.Min(_choicePromptW, boxW - 32f), _choicePromptH);
                y += _choicePromptH + 6f;
            }
            foreach (var (_, handle, w, h) in _choiceTextures)
            {
                if (handle == 0) continue;
                DrawTexturedQuad(handle, boxX + 24f, y, MathF.Min(w, boxW - 48f), h);
                y += h + 4f;
            }
        }

        GL.Disable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);
    }

    private void EnsureBodyTexture(string text)
    {
        if (text == _cachedBodyText) return;
        _cachedBodyText = text;
        if (_bodyTextureHandle != 0) GL.DeleteTexture(_bodyTextureHandle);
                                                                                 
                                                                         
                                                                             
                                                           
        (_bodyTextureHandle, _bodyTexW, _bodyTexH) = TextTexture.Create(text, Color.White, pixelsPerWorldUnit: 1f, fontSizePx: 22f);
    }

    private void EnsureChoiceTextures(string prompt, IReadOnlyList<string> options)
    {
        if (prompt != _cachedChoicePrompt)
        {
            _cachedChoicePrompt = prompt;
            if (_choicePromptHandle != 0) GL.DeleteTexture(_choicePromptHandle);
            (_choicePromptHandle, _choicePromptW, _choicePromptH) = string.IsNullOrEmpty(prompt)
                ? (0, 0f, 0f)
                : TextTexture.Create(prompt, Color.White, pixelsPerWorldUnit: 1f, fontSizePx: 20f);
        }

                                                                               
                                                                              
                                               
        bool same = _choiceTextures.Count == options.Count;
        for (int i = 0; same && i < options.Count; i++)
            same &= _choiceTextures[i].text == options[i];
        if (same) return;

        foreach (var (_, handle, _, _) in _choiceTextures)
            if (handle != 0) GL.DeleteTexture(handle);
        _choiceTextures.Clear();

        for (int i = 0; i < options.Count; i++)
        {
            var (handle, w, h) = TextTexture.Create($"{i + 1}. {options[i]}", Color.FromArgb(255, 230, 220, 140),
                pixelsPerWorldUnit: 1f, fontSizePx: 20f);
            _choiceTextures.Add((options[i], handle, w, h));
        }
    }

    private void DrawTextCentered(float x, float y, float maxW, float maxH)
    {
        if (_bodyTextureHandle == 0) return;
        DrawTexturedQuad(_bodyTextureHandle, x, y, MathF.Min(_bodyTexW, maxW), MathF.Min(_bodyTexH, maxH));
    }

    private void DrawTexturedQuad(int handle, float x, float y, float w, float h)
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, handle);
        _shader.SetInt("uUseTexture", 1);
        DrawQuadNdc(x, y, w, h);
    }

    private void DrawPanel(float x, float y, float w, float h, Vector4 color)
    {
        _shader.SetInt("uUseTexture", 0);
        _shader.SetVector4("uColor", color);
        DrawQuadNdc(x, y, w, h);
    }

                                                                            
                                                                            
                                                                            
                                                                            
                                                                            
                                                                              
                                                                   
    private void DrawQuadNdc(float x, float y, float w, float h)
    {
        float ndcX0 = x / CanvasWidth * 2f - 1f;
        float ndcX1 = (x + w) / CanvasWidth * 2f - 1f;
        float ndcY0 = 1f - y / CanvasHeight * 2f;
        float ndcY1 = 1f - (y + h) / CanvasHeight * 2f;

        float[] verts =
        {
            ndcX0, ndcY1,   0f, 1f,
            ndcX1, ndcY1,   1f, 1f,
            ndcX1, ndcY0,   1f, 0f,
            ndcX0, ndcY1,   0f, 1f,
            ndcX1, ndcY0,   1f, 0f,
            ndcX0, ndcY0,   0f, 0f,
        };
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, verts.Length * sizeof(float), verts);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
}
