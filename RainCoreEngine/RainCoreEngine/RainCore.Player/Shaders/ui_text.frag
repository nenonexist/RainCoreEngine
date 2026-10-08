#version 330 core
in vec2 vUV;
out vec4 FragColor;

uniform sampler2D uTex;
                                                                     
                                                                    
                                                                       
                          
uniform int uUseTexture;
uniform vec4 uColor;

void main()
{
    if (uUseTexture == 1)
        FragColor = texture(uTex, vUV);
    else
        FragColor = uColor;
}
