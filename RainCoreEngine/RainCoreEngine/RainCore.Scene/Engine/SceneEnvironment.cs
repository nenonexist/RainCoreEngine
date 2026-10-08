using OpenTK.Mathematics;
using RainCoreGraphics;

namespace RainCore;

             
                                                                                          
                                                                                
                                                                                     
                                                                        
                                                                          
                                                    
                                                                     
                                                                         
              
public sealed record SceneEnvironment
{
    public const string DefaultPresetName = "Default";

    public Vector3 Background { get; init; } = new(0.05f, 0.05f, 0.07f);

                                                                                                     
    public float AmbientIntensity { get; init; } = 1f;

    public bool FogEnabled { get; init; }
    public Vector3 FogColor { get; init; } = new(0.05f, 0.05f, 0.07f);
    public float FogStart { get; init; } = 10f;
    public float FogEnd { get; init; } = 60f;
    public float FogOpacity { get; init; } = 1f;

                                                         
    public float Exposure { get; init; } = 1f;
    public float Contrast { get; init; } = 1f;
    public float Saturation { get; init; } = 1f;

                                                                                                             
    public float VertexSnap { get; init; }

                                                                                                
    public float DitherStrength { get; init; }
    public float DitherLevels { get; init; } = 32f;

    public static SceneEnvironment Default { get; } = new();

                                                                                                     
    public void ApplyTo(Shader shader)
    {
        shader.SetVector3("fogColor", FogColor);
        shader.SetFloat("fogStart", FogStart);
        shader.SetFloat("fogEnd", MathF.Max(FogEnd, FogStart + 0.01f));
        shader.SetFloat("fogMaxOpacity", FogEnabled ? Math.Clamp(FogOpacity, 0f, 1f) : 0f);
        shader.SetFloat("envAmbientOffset", (AmbientIntensity - 1f) * 0.45f);
        shader.SetFloat("exposureEv", MathF.Log2(MathF.Max(Exposure, 0.001f)));
        shader.SetFloat("contrastDelta", Contrast - 1f);
        shader.SetFloat("saturationDelta", Saturation - 1f);
        shader.SetFloat("snapGrid", VertexSnap);
    }

                                                                               
    public static IReadOnlyList<(string Name, SceneEnvironment Env)> BuiltInPresets { get; } = new (string, SceneEnvironment)[]
    {
        (DefaultPresetName, new SceneEnvironment()),
        ("Day", new SceneEnvironment
        {
            Background = new(0.55f, 0.72f, 0.92f), AmbientIntensity = 1.35f,
            FogEnabled = true, FogColor = new(0.70f, 0.82f, 0.95f), FogStart = 25f, FogEnd = 120f, FogOpacity = 0.7f,
            Exposure = 1.1f, Saturation = 1.1f,
        }),
        ("Night", new SceneEnvironment
        {
            Background = new(0.015f, 0.02f, 0.05f), AmbientIntensity = 0.45f,
            FogEnabled = true, FogColor = new(0.02f, 0.03f, 0.07f), FogStart = 6f, FogEnd = 45f, FogOpacity = 0.9f,
            Exposure = 0.9f, Contrast = 1.1f, Saturation = 0.8f,
        }),
        ("Sunset", new SceneEnvironment
        {
            Background = new(0.95f, 0.50f, 0.28f), AmbientIntensity = 1.1f,
            FogEnabled = true, FogColor = new(0.90f, 0.45f, 0.30f), FogStart = 15f, FogEnd = 90f, FogOpacity = 0.75f,
            Exposure = 1.05f, Contrast = 1.1f, Saturation = 1.25f,
        }),
        ("Foggy", new SceneEnvironment
        {
            Background = new(0.62f, 0.64f, 0.66f), AmbientIntensity = 1.1f,
            FogEnabled = true, FogColor = new(0.62f, 0.64f, 0.66f), FogStart = 2f, FogEnd = 28f, FogOpacity = 1f,
            Saturation = 0.7f,
        }),
        ("Cold", new SceneEnvironment
        {
            Background = new(0.35f, 0.48f, 0.62f), AmbientIntensity = 0.95f,
            FogEnabled = true, FogColor = new(0.40f, 0.52f, 0.66f), FogStart = 8f, FogEnd = 70f, FogOpacity = 0.8f,
            Saturation = 0.85f,
        }),
        ("Warm", new SceneEnvironment
        {
            Background = new(0.60f, 0.42f, 0.28f), AmbientIntensity = 1.2f,
            FogEnabled = true, FogColor = new(0.65f, 0.45f, 0.30f), FogStart = 12f, FogEnd = 80f, FogOpacity = 0.6f,
            Saturation = 1.15f,
        }),
        ("Horror", new SceneEnvironment
        {
            Background = new(0.01f, 0.01f, 0.012f), AmbientIntensity = 0.35f,
            FogEnabled = true, FogColor = new(0.03f, 0.02f, 0.02f), FogStart = 3f, FogEnd = 22f, FogOpacity = 1f,
            Exposure = 0.9f, Contrast = 1.3f, Saturation = 0.6f, VertexSnap = 160f, DitherStrength = 0.6f, DitherLevels = 24f,
        }),
        ("Dream", new SceneEnvironment
        {
            Background = new(0.78f, 0.70f, 0.88f), AmbientIntensity = 1.4f,
            FogEnabled = true, FogColor = new(0.80f, 0.72f, 0.92f), FogStart = 4f, FogEnd = 50f, FogOpacity = 0.85f,
            Exposure = 1.1f, Contrast = 0.9f, Saturation = 1.2f, DitherStrength = 0.3f, DitherLevels = 48f,
        }),
        ("Liminal", new SceneEnvironment
        {
            Background = new(0.80f, 0.78f, 0.62f), AmbientIntensity = 1.5f,
            FogEnabled = true, FogColor = new(0.82f, 0.80f, 0.64f), FogStart = 8f, FogEnd = 55f, FogOpacity = 0.55f,
            Exposure = 1.05f, Contrast = 0.85f, Saturation = 0.75f, DitherStrength = 0.4f, DitherLevels = 32f,
        }),
        ("Cinematic", new SceneEnvironment
        {
            Background = new(0.04f, 0.045f, 0.06f), AmbientIntensity = 0.8f,
            FogEnabled = true, FogColor = new(0.06f, 0.07f, 0.09f), FogStart = 10f, FogEnd = 80f, FogOpacity = 0.6f,
            Exposure = 1f, Contrast = 1.25f, Saturation = 0.9f,
        }),
    };
}
