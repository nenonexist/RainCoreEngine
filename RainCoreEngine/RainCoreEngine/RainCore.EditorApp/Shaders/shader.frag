#version 330 core
in vec3 vColor;
in vec3 vNormal;
in float vUnlit;
in float vDist;
in vec2 vUV;
in float vWorldY;
in float vLight;

uniform sampler2D modelTexture;
uniform int useTexture;
uniform vec3 fogColor;
uniform float fogStart;
uniform float fogEnd;
uniform float fogMaxOpacity;
uniform float fogHeightRef;                                                                                    
uniform float fogHeightFalloff;                                                           
uniform float fogHeightStrength;                                                                         

                                                                                                  
                                                                                                 
uniform float envAmbientOffset;                                           
uniform float exposureEv;                                                           
uniform float contrastDelta;                               
uniform float saturationDelta;                               

                                                                                                   
uniform int viewMode;                                                                 
uniform float viewExposureEv;                                                             

out vec4 FragColor;

void main()
{
                                                                            
                                                                            
                                                                            
                                                                         
                                                                    
    vec4 texel = useTexture == 1 ? texture(modelTexture, vUV) : vec4(1.0);

                                                                       
                                                                    
                                                                        
    if (texel.a < 0.5) discard;

    vec3 texturedColor = vColor * texel.rgb;
    vec3 baseColor;

    if (vUnlit > 0.5 || viewMode == 2)
    {
        baseColor = texturedColor;
    }
    else if (viewMode == 1)
    {
                                                                                                 
                                                                                           
        vec3 lightDir = normalize(vec3(0.4, 1.0, 0.3));
        float diff = max(dot(normalize(vNormal), lightDir), 0.0);
        float hemi = 0.55 + 0.45 * normalize(vNormal).y;
        baseColor = texturedColor * clamp(hemi * 0.7 + diff * 0.5, 0.0, 1.2);
    }
    else
    {
        vec3 lightDir = normalize(vec3(0.4, 1.0, 0.3));
        float diff = max(dot(normalize(vNormal), lightDir), 0.0);
        float ambient = max(0.45 + envAmbientOffset, 0.0);
        float sceneLight = ambient + diff * 0.55;

                                                                    
                                                                              
                                                                       
                                                                           
                                                                              
                                                                       
                                              
        float light = max(sceneLight, vLight);
        baseColor = texturedColor * light;
    }

                                                            
    float distFactor = smoothstep(fogStart, fogEnd, vDist);

                                                                          
                                                                           
                                                                  
                                                                        
    float heightAbove = max(vWorldY - fogHeightRef, 0.0);
    float heightDensity = exp(-heightAbove * fogHeightFalloff);
    float density = mix(1.0, heightDensity, fogHeightStrength);

    float fogFactor = clamp(distFactor * fogMaxOpacity * density, 0.0, 1.0);
    vec3 finalColor = mix(baseColor, fogColor, fogFactor);

                                                                                        
    finalColor *= exp2(exposureEv + viewExposureEv);
    finalColor = (finalColor - 0.5) * (1.0 + contrastDelta) + 0.5;
    float luma = dot(finalColor, vec3(0.299, 0.587, 0.114));
    finalColor = mix(vec3(luma), finalColor, 1.0 + saturationDelta);

    FragColor = vec4(max(finalColor, 0.0), 1.0);
}
