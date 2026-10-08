using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace RainCore;

             
                                                                      
                                                                         
                                                                           
                                                                            
                                                                            
                                                         
              
public class TextPanel
{
    private Shader _panelShader = null!;
    private Shader _textShader = null!;
    private int _panelVao, _panelVbo;
    private int _backgroundVao, _backgroundVbo;
    private int _highlightVao, _highlightVbo;
    private int _textVao, _textVbo;
    private int _textTexture;
    private int _backgroundTexture;
    private float _textPixelWidth, _textPixelHeight;
    private string? _cachedText;
    private int _highlightedLineIndex = -1;
    private int _lineCount = 1;

    public float TextPixelWidth => _textPixelWidth;
    public float TextPixelHeight => _textPixelHeight;
    public bool HasTexture => _textTexture != 0;
                                      
    public OpenTK.Mathematics.Vector3 PanelColor { get; private set; } = new OpenTK.Mathematics.Vector3(0.04f, 0.07f, 0.12f);
    public float PanelAlpha { get; private set; } = 0.9f;
    public bool UsePanelBackground { get; private set; } = true;

                                                                                                                   
    public bool UseMonospace { get; private set; }

                                                                                                                                       
    public string? FontFamilyName { get; private set; }
                                                                                                                
    public float? FontSizePx { get; private set; }

                                                                                             
                                                                                
                                                                              
                                                                               
                                                                         
                                                       
    public Vector3 IconTintColor { get; set; } = Vector3.One;
    public float IconTintAlpha { get; set; } = 1f;

    public void Build(Shader panelShader, float left, float right, float bottom, float top,
        OpenTK.Mathematics.Vector3? panelColor = null, float panelAlpha = 0.9f, bool usePanelBackground = true,
        bool useMonospace = false, string? panelTextureFileName = null,
        string? fontFamilyName = null, float? fontSizePx = null)
    {
        _panelShader = panelShader;

        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _textShader = new Shader(Path.Combine(shadersDir, "ui.vert"), Path.Combine(shadersDir, "ui.frag"));

                                                                      
        PanelColor = panelColor ?? PanelColor;
        PanelAlpha = panelAlpha;
        UsePanelBackground = usePanelBackground;
        UseMonospace = useMonospace;
        FontFamilyName = fontFamilyName;
        FontSizePx = fontSizePx;
        var initialPanelVerts = BuildPanelQuad(left, right, bottom, top);

        _panelVao = GL.GenVertexArray();
        _panelVbo = GL.GenBuffer();
        GL.BindVertexArray(_panelVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _panelVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, initialPanelVerts.Length * sizeof(float), initialPanelVerts, BufferUsageHint.DynamicDraw);

        _highlightVao = GL.GenVertexArray();
        _highlightVbo = GL.GenBuffer();
        GL.BindVertexArray(_highlightVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _highlightVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 6 * 10 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 10 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 10 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 10 * sizeof(float), 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, 10 * sizeof(float), 9 * sizeof(float));
        GL.EnableVertexAttribArray(3);

        _textVao = GL.GenVertexArray();
        _textVbo = GL.GenBuffer();
        GL.BindVertexArray(_textVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _textVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        if (!string.IsNullOrEmpty(panelTextureFileName))
        {
            _backgroundTexture = LoadTextureFromUiFolder(panelTextureFileName);
            if (_backgroundTexture != 0)
            {
                _backgroundVao = GL.GenVertexArray();
                _backgroundVbo = GL.GenBuffer();
                GL.BindVertexArray(_backgroundVao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _backgroundVbo);
                GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                                                                                     
                                                                                        
                                                                                     
                                                        
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
                GL.EnableVertexAttribArray(1);
            }
        }
    }

    private float[] BuildPanelQuad(float left, float right, float bottom, float top)
    {
        float borderSize = 0.01f;
        float innerLeft = left + borderSize;
        float innerRight = right - borderSize;
        float innerBottom = bottom + borderSize;
        float innerTop = top - borderSize;

        var verts = new List<float>();
        void AddQuad(float l, float r, float b, float t, Vector3 color, float alpha)
        {
            float[] xs = { l, r, r, l };
            float[] ys = { b, b, t, t };
            int[] order = { 0, 1, 2, 0, 2, 3 };
            foreach (var i in order)
            {
                verts.Add(xs[i]); verts.Add(ys[i]); verts.Add(0f);
                verts.Add(0f); verts.Add(0f); verts.Add(1f);
                verts.Add(color.X); verts.Add(color.Y); verts.Add(color.Z);
                verts.Add(alpha);
            }
        }

                                                                                          
                                                                                
                                                                               
                                                                             
                                                                  
        AddQuad(left, right, bottom, top, new Vector3(0.02f, 0.03f, 0.05f), UsePanelBackground ? 0.8f : 0f);
        AddQuad(innerLeft, innerRight, innerBottom, innerTop, PanelColor, UsePanelBackground ? PanelAlpha : 0f);
        return verts.ToArray();
    }

                                                                                                                                   
    public void SetText(string text, int highlightedLine = -1)
    {
        if (text == _cachedText && highlightedLine == _highlightedLineIndex) return;
        _cachedText = text;
        _highlightedLineIndex = highlightedLine;
        _lineCount = Math.Max(1, text.Split('\n').Length);
        if (_textTexture != 0) GL.DeleteTexture(_textTexture);
        (_textTexture, _textPixelWidth, _textPixelHeight) =
            TextTexture.Create(text, System.Drawing.Color.White, pixelsPerWorldUnit: 1f, monospace: UseMonospace,
                fontFamilyName: FontFamilyName, fontSizePx: FontSizePx);
    }

                                                                             
                                                                            
                                                                            
    public void Render(float textLeft, float textRight, float textBottom, float textTop)
    {
        if (_textTexture == 0) return;
        float panelMarginX = MathF.Max((textRight - textLeft) * 0.08f, 0.02f);
        float panelMarginY = MathF.Max((textTop - textBottom) * 0.08f, 0.02f);
        RenderInternal(textLeft, textRight, textBottom, textTop,
            textLeft - panelMarginX, textRight + panelMarginX, textBottom - panelMarginY, textTop + panelMarginY);
    }

                                                                                
                                                                               
                                                                           
                                                                         
                                                                              
    public void Render(float textLeft, float textRight, float textBottom, float textTop,
        float panelLeft, float panelRight, float panelBottom, float panelTop)
    {
        if (_textTexture == 0) return;
        RenderInternal(textLeft, textRight, textBottom, textTop, panelLeft, panelRight, panelBottom, panelTop);
    }

    private void RenderInternal(float textLeft, float textRight, float textBottom, float textTop,
        float panelLeft, float panelRight, float panelBottom, float panelTop)
    {
                                                            
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _panelShader.Use();
        _panelShader.SetMatrix4("model", Matrix4.Identity);
        _panelShader.SetMatrix4("view", Matrix4.Identity);
        _panelShader.SetMatrix4("projection", Matrix4.Identity);

        var panelVerts = BuildPanelQuad(panelLeft, panelRight, panelBottom, panelTop);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _panelVbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, panelVerts.Length * sizeof(float), panelVerts);
        GL.BindVertexArray(_panelVao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 12);

        if (_highlightedLineIndex >= 0 && _highlightedLineIndex < _lineCount)
        {
            var highlightVerts = BuildHighlightQuad(textLeft, textRight, textBottom, textTop);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _highlightVbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, highlightVerts.Length * sizeof(float), highlightVerts);
            GL.BindVertexArray(_highlightVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        if (_backgroundTexture != 0)
        {
                                                                                                                                                              
            var backgroundVerts = BuildTextureQuad(panelLeft, panelRight, panelBottom, panelTop);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _backgroundVbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, backgroundVerts.Length * sizeof(float), backgroundVerts);
            _textShader.Use();
            _textShader.SetInt("uText", 0);
            _textShader.SetVector4("uTint", new Vector4(IconTintColor, IconTintAlpha));
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _backgroundTexture);
            GL.BindVertexArray(_backgroundVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            _textShader.SetVector4("uTint", Vector4.One);                                                                                     
        }

                                                                        
                                                                                
                                                                                 
                                                                                 
                                                                           
                                                                                
                                                                                   
                                                                                   
                                                                             
                                                                                
                                                                               
        var (fitLeft, fitRight, fitBottom, fitTop) =
            FitTextQuad(textLeft, textRight, textBottom, textTop);

        float[] verts =
        {
            fitLeft, fitBottom, 0f, 1f,
            fitRight, fitBottom, 1f, 1f,
            fitRight, fitTop, 1f, 0f,
            fitLeft, fitBottom, 0f, 1f,
            fitRight, fitTop, 1f, 0f,
            fitLeft, fitTop, 0f, 0f,
        };
        GL.BindBuffer(BufferTarget.ArrayBuffer, _textVbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, verts.Length * sizeof(float), verts);

        _textShader.Use();
        _textShader.SetInt("uText", 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _textTexture);
        GL.BindVertexArray(_textVao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        GL.Disable(EnableCap.Blend);
    }

                 
                                                                       
                                                                                
                                                                             
                                                                           
                                                                              
                                                                                
                                                 
                  
    private (float left, float right, float bottom, float top) FitTextQuad(
        float boxLeft, float boxRight, float boxBottom, float boxTop)
    {
        if (_textPixelWidth <= 0f || _textPixelHeight <= 0f)
            return (boxLeft, boxRight, boxBottom, boxTop);

        var viewport = new int[4];
        GL.GetInteger(GetPName.Viewport, viewport);
        int vpWidth = viewport[2];
        int vpHeight = viewport[3];
        if (vpWidth <= 0 || vpHeight <= 0)
            return (boxLeft, boxRight, boxBottom, boxTop);

        float boxWidthPx = (boxRight - boxLeft) * 0.5f * vpWidth;
        float boxHeightPx = (boxTop - boxBottom) * 0.5f * vpHeight;
        if (boxWidthPx <= 0f || boxHeightPx <= 0f)
            return (boxLeft, boxRight, boxBottom, boxTop);

        float texAspect = _textPixelWidth / _textPixelHeight;
        float boxAspect = boxWidthPx / boxHeightPx;

        float fitWidthPx, fitHeightPx;
        if (boxAspect > texAspect)
        {
                                                                       
            fitHeightPx = boxHeightPx;
            fitWidthPx = fitHeightPx * texAspect;
        }
        else
        {
                                                                       
            fitWidthPx = boxWidthPx;
            fitHeightPx = fitWidthPx / texAspect;
        }

        float fitWidthNdc = fitWidthPx / (0.5f * vpWidth);
        float fitHeightNdc = fitHeightPx / (0.5f * vpHeight);

        float left = boxLeft;
        float right = boxLeft + fitWidthNdc;
        float centerY = (boxBottom + boxTop) * 0.5f;
        float bottom = centerY - fitHeightNdc * 0.5f;
        float top = centerY + fitHeightNdc * 0.5f;

        return (left, right, bottom, top);
    }

    private float[] BuildTextureQuad(float left, float right, float bottom, float top)
    {
        return new float[]
        {
            left, bottom, 0f, 1f,
            right, bottom, 1f, 1f,
            right, top, 1f, 0f,
            left, bottom, 0f, 1f,
            right, top, 1f, 0f,
            left, top, 0f, 0f,
        };
    }

    private int LoadTextureFromUiFolder(string fileName)
    {
        var candidateDirs = new[]
        {
            AppContext.BaseDirectory,
            Directory.GetCurrentDirectory(),
            Path.Combine(AppContext.BaseDirectory, "..", "..", ".."),
            Path.Combine(AppContext.BaseDirectory, "..", ".."),
        };

        string? foundPath = null;
        foreach (var dir in candidateDirs)
        {
            if (dir == null) continue;
            var path = Path.GetFullPath(Path.Combine(dir, "ui", fileName));
            if (File.Exists(path))
            {
                foundPath = path;
                break;
            }
        }

        if (foundPath == null)
            return 0;

        using var bmp = new Bitmap(foundPath);
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        int handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
            PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        bmp.UnlockBits(data);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        return handle;
    }

    private float[] BuildHighlightQuad(float left, float right, float bottom, float top)
    {
        float totalHeight = top - bottom;
        float rowHeight = totalHeight / Math.Max(1, _lineCount);
        float rowTop = top - _highlightedLineIndex * rowHeight;
        float rowBottom = rowTop - rowHeight;
        float insetX = (right - left) * 0.02f;
        float insetY = rowHeight * 0.08f;
        float hlLeft = left + insetX;
        float hlRight = right - insetX;
        float hlTop = rowTop - insetY;
        float hlBottom = rowBottom + insetY;
        var color = new OpenTK.Mathematics.Vector3(0.18f, 0.28f, 0.45f);
        var alpha = 0.65f;

        return new float[]
        {
            hlLeft, hlBottom, 0f, 0f, 0f, 1f, color.X, color.Y, color.Z, alpha,
            hlRight, hlBottom, 0f, 0f, 0f, 1f, color.X, color.Y, color.Z, alpha,
            hlRight, hlTop, 0f, 0f, 0f, 1f, color.X, color.Y, color.Z, alpha,
            hlLeft, hlBottom, 0f, 0f, 0f, 1f, color.X, color.Y, color.Z, alpha,
            hlRight, hlTop, 0f, 0f, 0f, 1f, color.X, color.Y, color.Z, alpha,
            hlLeft, hlTop, 0f, 0f, 0f, 1f, color.X, color.Y, color.Z, alpha,
        };
    }
}
