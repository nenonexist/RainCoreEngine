#version 330 core
in vec2 vUV;
uniform sampler2D uScene;
uniform vec2 uScreenSize;                                                                                                                               
uniform float uDitherStrength;                                                     
uniform float uDitherLevels;                                                                              

out vec4 FragColor;

const float bayer4x4[16] = float[](
     0.0,  8.0,  2.0, 10.0,
    12.0,  4.0, 14.0,  6.0,
     3.0, 11.0,  1.0,  9.0,
    15.0,  7.0, 13.0,  5.0
);

void main()
{
    vec3 color = texture(uScene, vUV).rgb;

                                                                         
                                                                         
                                                                           
                                                                            
                                                         
    ivec2 pixel = ivec2(mod(vUV * uScreenSize, 4.0));
    float threshold = (bayer4x4[pixel.y * 4 + pixel.x] / 16.0 - 0.5) * uDitherStrength;

    vec3 dithered = color + threshold / uDitherLevels;
    vec3 quantized = floor(dithered * uDitherLevels + 0.5) / uDitherLevels;

    FragColor = vec4(clamp(quantized, 0.0, 1.0), 1.0);
}
