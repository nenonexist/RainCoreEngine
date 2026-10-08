using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace RainCore;

public sealed class UiButton : IUiElement
{
    private readonly TextPanel _panel = new();
    private readonly TextPanel _selectedPanel = new();
    private Shader? _shader;
    private string _lastText = "";

    public string Text { get; set; } = "";
    public bool Selected { get; set; }
    public bool Enabled { get; set; } = true;
    public bool UseBackground { get; set; } = true;
    public Vector3 NormalColor { get; set; } = new(0.10f, 0.08f, 0.16f);
    public Vector3 SelectedColor { get; set; } = new(0.35f, 0.16f, 0.55f);

                                                                      
                                                                        
                                                                    
                                                                    
                                                                          
                                         
    private const float SourceFontSizePx = 42f;

    public void Build(Shader shader)
    {
        _shader = shader;
        _panel.Build(shader, -1f, 1f, -1f, 1f,
            panelColor: NormalColor, panelAlpha: 0.96f,
            usePanelBackground: UseBackground, useMonospace: false,
            panelTextureFileName: UseBackground ? "UiButton.png" : null,
            fontFamilyName: "Segoe UI", fontSizePx: SourceFontSizePx);
        _selectedPanel.Build(shader, -1f, 1f, -1f, 1f,
            panelColor: SelectedColor, panelAlpha: 0.98f,
            usePanelBackground: UseBackground,
            panelTextureFileName: UseBackground ? "UiButtonCover.png" : null,
            fontFamilyName: "Segoe UI", fontSizePx: SourceFontSizePx);
    }

    public void Render(UiRect rect)
    {
        if (_shader == null) return;
        string displayText = Selected ? $"> {Text}" : Text;
        if (string.IsNullOrEmpty(displayText)) displayText = " ";
        if (_lastText != displayText)
        {
            _panel.SetText(displayText);
            _selectedPanel.SetText(displayText);
            _lastText = displayText;
        }
        var panel = Selected ? _selectedPanel : _panel;

                                                                                 
                                                                              
                                                                          
                                                                                  
                                                                                
                                                                                  
                                                                                 
                                                                                  
                                                                              
                                                                          
                                                                              
                                                                                 
                                                                                
                                                      
        var viewport = new int[4];
        GL.GetInteger(GetPName.Viewport, viewport);
        float vpWidth = viewport[2] > 0 ? viewport[2] : 1024f;
        float vpHeight = viewport[3] > 0 ? viewport[3] : 480f;

        float textNdcW = panel.TextPixelWidth / vpWidth * 2f;
        float textNdcH = panel.TextPixelHeight / vpHeight * 2f;

                                                                              
                                                                      
                                                                          
                                                                           
                                                                             
                                                                         
                                                                           
                                                                                
        float bgInsetX = UseBackground ? rect.Width * 0.02f : 0f;
        float bgInsetY = UseBackground ? rect.Height * 0.06f : 0f;
        float bgLeft = rect.Left + bgInsetX;
        float bgRight = rect.Right - bgInsetX;
        float bgBottom = rect.Bottom + bgInsetY;
        float bgTop = rect.Top - bgInsetY;

                                                                            
                                                                            
                                                                          
        float maxW = (bgRight - bgLeft) * 0.94f;
        float maxH = (bgTop - bgBottom) * 0.86f;
        float scale = MathF.Min(1f, MathF.Min(maxW / textNdcW, maxH / textNdcH));
        textNdcW *= scale;
        textNdcH *= scale;

                                                                       
                                   
        float padding = (bgRight - bgLeft) * 0.03f;
        float left = bgLeft + padding;
        float right = left + textNdcW;
        float centerY = (bgTop + bgBottom) * 0.5f;
        float bottom = centerY - textNdcH * 0.5f;
        float top = centerY + textNdcH * 0.5f;

        if (UseBackground)
            panel.Render(left, right, bottom, top, bgLeft, bgRight, bgBottom, bgTop);
        else
            panel.Render(left, right, bottom, top);
    }
}