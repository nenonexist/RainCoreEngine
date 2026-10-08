using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace RainCoreScene;

public sealed partial class ProceduralModel
{
                                                                                           
    public static ProceduralModel CreateBox(Vector3 size, Vector3 color)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        float hx = size.X * 0.5f, hz = size.Z * 0.5f, y1 = size.Y;
        AddBox(vertexData, indices, ref vi, new Vector3(-hx, 0, -hz), new Vector3(hx, y1, hz), color);
        return FinalizeMesh(vertexData, indices, new Vector3(-hx, 0, -hz), new Vector3(hx, y1, hz));
    }

    public static ProceduralModel CreateWireBox(Vector3 size, Vector3 color, float thickness = 0.035f)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var min = -size * 0.5f;
        var max = size * 0.5f;
        AddBox(vertexData, indices, ref vi, new Vector3(min.X, min.Y, min.Z), new Vector3(max.X, min.Y + thickness, min.Z + thickness), color, 1f);
        AddBox(vertexData, indices, ref vi, new Vector3(min.X, max.Y - thickness, min.Z), new Vector3(max.X, max.Y, min.Z + thickness), color, 1f);
        AddBox(vertexData, indices, ref vi, new Vector3(min.X, min.Y, max.Z - thickness), new Vector3(max.X, min.Y + thickness, max.Z), color, 1f);
        AddBox(vertexData, indices, ref vi, new Vector3(min.X, max.Y - thickness, max.Z - thickness), new Vector3(max.X, max.Y, max.Z), color, 1f);
        AddBox(vertexData, indices, ref vi, new Vector3(min.X, min.Y, min.Z), new Vector3(min.X + thickness, max.Y, min.Z + thickness), color, 1f);
        AddBox(vertexData, indices, ref vi, new Vector3(max.X - thickness, min.Y, min.Z), new Vector3(max.X, max.Y, min.Z + thickness), color, 1f);
        AddBox(vertexData, indices, ref vi, new Vector3(min.X, min.Y, max.Z - thickness), new Vector3(min.X + thickness, max.Y, max.Z), color, 1f);
        AddBox(vertexData, indices, ref vi, new Vector3(max.X - thickness, min.Y, max.Z - thickness), new Vector3(max.X, max.Y, max.Z), color, 1f);
        return FinalizeMesh(vertexData, indices, min, max);
    }

                                                                                 
                                                                                
                                                                                  
                                                                                   
                                         
    public static ProceduralModel CreateBeacon(float poleHeight, float poleRadius, float headSize, Vector3 poleColor, Vector3 glowColor)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;

        AddBox(vertexData, indices, ref vi, new Vector3(-poleRadius, 0, -poleRadius), new Vector3(poleRadius, poleHeight, poleRadius), poleColor);

        float half = headSize * 0.5f;
        var headMin = new Vector3(-half, poleHeight - half * 0.3f, -half);
        var headMax = new Vector3(half, poleHeight + half * 1.4f, half);
        AddBox(vertexData, indices, ref vi, headMin, headMax, glowColor, unlit: 1f);

        return FinalizeMesh(vertexData, indices, new Vector3(-half, 0, -half), new Vector3(half, poleHeight + half * 1.4f, half));
    }

    public static ProceduralModel CreateGabledHouse(float width, float depth, float wallHeight, float roofHeight,
        Vector3 wallColor, Vector3 roofColor, Vector3 snowColor)
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        float hx = width * 0.5f;
        float hz = depth * 0.5f;
        AddBox(vertexData, indices, ref vi, new Vector3(-hx, 0f, -hz), new Vector3(hx, wallHeight, hz), wallColor);

        var leftFront = new Vector3(-hx, wallHeight, -hz);
        var rightFront = new Vector3(hx, wallHeight, -hz);
        var ridgeFront = new Vector3(0f, wallHeight + roofHeight, -hz);
        var leftBack = new Vector3(-hx, wallHeight, hz);
        var rightBack = new Vector3(hx, wallHeight, hz);
        var ridgeBack = new Vector3(0f, wallHeight + roofHeight, hz);
        AddQuad(vertexData, indices, ref vi, Vector3.Normalize(Vector3.Cross(ridgeFront - leftFront, leftBack - leftFront)), leftFront, ridgeFront, ridgeBack, leftBack, roofColor);
        AddQuad(vertexData, indices, ref vi, Vector3.Normalize(Vector3.Cross(rightBack - rightFront, ridgeFront - rightFront)), rightFront, rightBack, ridgeBack, ridgeFront, roofColor);
        AddTri(vertexData, indices, ref vi, Vector3.UnitZ, leftFront, rightFront, ridgeFront, wallColor);
        AddTri(vertexData, indices, ref vi, -Vector3.UnitZ, rightBack, leftBack, ridgeBack, wallColor);
        AddBox(vertexData, indices, ref vi, new Vector3(-0.16f, wallHeight + roofHeight - 0.05f, -0.18f),
            new Vector3(0.16f, wallHeight + roofHeight + 0.12f, 0.18f), snowColor);
        return FinalizeMesh(vertexData, indices, new Vector3(-hx, 0f, -hz), new Vector3(hx, wallHeight + roofHeight, hz));
    }

    public static ProceduralModel CreateRuinedHut()
    {
        var vertexData = new List<float>();
        var indices = new List<uint>();
        uint vi = 0;
        var wall = new Vector3(0.25f, 0.23f, 0.20f);
        var sideWall = new Vector3(0.22f, 0.21f, 0.18f);
        var beam = new Vector3(0.16f, 0.15f, 0.13f);
        AddBox(vertexData, indices, ref vi, new Vector3(-2.9f, 0f, 2.16f), new Vector3(2.9f, 2.8f, 2.44f), wall);
        AddBox(vertexData, indices, ref vi, new Vector3(-3.04f, 0f, -2.3f), new Vector3(-2.76f, 2.8f, 2.3f), sideWall);
        AddBox(vertexData, indices, ref vi, new Vector3(0.4f, 0f, -2.45f), new Vector3(2.5f, 1.45f, -2.17f), sideWall);
        AddBox(vertexData, indices, ref vi, new Vector3(-2.2f, 0.65f, -1.6f), new Vector3(4.2f, 0.83f, 1.0f), beam);
        AddBox(vertexData, indices, ref vi, new Vector3(2.25f, 0f, 0.6f), new Vector3(2.48f, 3.5f, 0.83f), beam);
        return FinalizeMesh(vertexData, indices, new Vector3(-3.2f, 0f, -2.6f), new Vector3(4.3f, 3.6f, 2.6f));
    }
}
