#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec3 aColor;
layout (location = 3) in float aUnlit;
layout (location = 4) in vec2 aUV;
layout (location = 5) in float aLight;                                                                                

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float snapGrid;                                                                 

out vec3 vColor;
out vec3 vNormal;
out vec2 vUV;
out float vUnlit;
out float vDist;
out float vWorldY;
out float vLight;

void main()
{
    vec4 worldPos = vec4(aPosition, 1.0) * model;
    vWorldY = worldPos.y;                                                                               

    vec4 viewPos = worldPos * view;
    vDist = length(viewPos.xyz);                                                       

    vec4 clip = viewPos * projection;

                                                                   
                                                                           
                                                                     
    if (snapGrid > 0.0)
    {
        clip.xy = floor(clip.xy / clip.w * snapGrid) / snapGrid * clip.w;
    }

    gl_Position = clip;
    vColor = aColor;
    vNormal = aNormal;
    vUnlit = aUnlit;
    vUV = aUV;
    vLight = aLight;
}
