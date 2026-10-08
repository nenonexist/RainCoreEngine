using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace RainCore;

             
                                                                         
                                                                            
                                                         
              
public class Label3D
{
    private string? _currentText;
    private int _textureHandle;
    private float _worldWidth;
    private float _worldHeight;

                                                                           
    public bool HasText => _textureHandle != 0;

                 
                                                                        
                                                                           
                               
                  
    public void SetText(string? text, Color color)
    {
        if (text == _currentText) return;
        _currentText = text;

        if (_textureHandle != 0)
        {
            GL.DeleteTexture(_textureHandle);
            _textureHandle = 0;
        }

        if (string.IsNullOrEmpty(text)) return;

        (_textureHandle, _worldWidth, _worldHeight) = TextTexture.Create(text, color);
    }

    public (int handle, float width, float height) Get()
    {
        return (_textureHandle, _worldWidth, _worldHeight);
    }

}
