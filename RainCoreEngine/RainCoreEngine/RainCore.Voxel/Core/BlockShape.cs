using OpenTK.Mathematics;

namespace RainCore;

             
                                                                       
                                                                      
                                                                     
                                                                           
                                                                        
                                                                           
                        
   
                                                                                 
                                                                            
                                                                              
                                           
              
public enum BlockShape : byte
{
    Cube = 0,
    HalfBottom,
    HalfTop,
    SlopeMinX,                                                             
    SlopeMaxX,                                                   
    SlopeMinZ,                                        
    SlopeMaxZ,                                                   
    Post,
    Sprite,
    Torch,
    Lantern,
    SnowLayer,
    SnowLayer2,
    SnowLayer3,
    SnowLayer4,
    SnowLayer5,
    SnowLayer6,
    SnowLayer7,
    SnowLayer8,
                 
                                                                              
                                                                        
                                                                              
                                                                     
                                                                             
                                                                        
                                                                         
                                                                                  
                                                                              
                                                                       
                  
    Cross,
}

             
                                                                           
                                     
              
public readonly struct ShapeFace
{
    public readonly Vector3 Normal;
    public readonly Vector3[] Corners;                                                                            
                 
                                                                             
                                                                           
                                                                      
                                                                         
                                                                                
                                                                     
                                                                          
                  
    public readonly Vector3i? CullNormal;

    public ShapeFace(Vector3 normal, Vector3[] corners, Vector3i? cullNormal)
    {
        Normal = normal;
        Corners = corners;
        CullNormal = cullNormal;
    }
}

             
                                                                            
                                                                        
                                           
              
public static class BlockShapeGeometry
{
    private static readonly Dictionary<BlockShape, ShapeFace[]> Faces = Build();

    public static IReadOnlyList<ShapeFace> FacesFor(BlockShape shape) => Faces[shape];

    private static Dictionary<BlockShape, ShapeFace[]> Build()
    {
        var map = new Dictionary<BlockShape, ShapeFace[]>();
        const float d = 0.70710678f;                                                   

                                                                 
        map[BlockShape.HalfBottom] = new[]
        {
            new ShapeFace(new Vector3(0, -1, 0), new[] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,0,0), new Vector3(0,0,0) }, new Vector3i(0,-1,0)),
            new ShapeFace(new Vector3(0, 1, 0),  new[] { new Vector3(0,0.5f,0), new Vector3(1,0.5f,0), new Vector3(1,0.5f,1), new Vector3(0,0.5f,1) }, null),
            new ShapeFace(new Vector3(0, 0, -1), new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0.5f,0), new Vector3(0,0.5f,0) }, new Vector3i(0,0,-1)),
            new ShapeFace(new Vector3(0, 0, 1),  new[] { new Vector3(1,0,1), new Vector3(0,0,1), new Vector3(0,0.5f,1), new Vector3(1,0.5f,1) }, new Vector3i(0,0,1)),
            new ShapeFace(new Vector3(-1, 0, 0), new[] { new Vector3(0,0,1), new Vector3(0,0,0), new Vector3(0,0.5f,0), new Vector3(0,0.5f,1) }, new Vector3i(-1,0,0)),
            new ShapeFace(new Vector3(1, 0, 0),  new[] { new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(1,0.5f,1), new Vector3(1,0.5f,0) }, new Vector3i(1,0,0)),
        };

                                                
        map[BlockShape.HalfTop] = new[]
        {
            new ShapeFace(new Vector3(0, 1, 0),  new[] { new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(0,1,1) }, new Vector3i(0,1,0)),
            new ShapeFace(new Vector3(0, -1, 0), new[] { new Vector3(0,0.5f,1), new Vector3(1,0.5f,1), new Vector3(1,0.5f,0), new Vector3(0,0.5f,0) }, null),
            new ShapeFace(new Vector3(0, 0, -1), new[] { new Vector3(0,0.5f,0), new Vector3(1,0.5f,0), new Vector3(1,1,0), new Vector3(0,1,0) }, new Vector3i(0,0,-1)),
            new ShapeFace(new Vector3(0, 0, 1),  new[] { new Vector3(1,0.5f,1), new Vector3(0,0.5f,1), new Vector3(0,1,1), new Vector3(1,1,1) }, new Vector3i(0,0,1)),
            new ShapeFace(new Vector3(-1, 0, 0), new[] { new Vector3(0,0.5f,1), new Vector3(0,0.5f,0), new Vector3(0,1,0), new Vector3(0,1,1) }, new Vector3i(-1,0,0)),
            new ShapeFace(new Vector3(1, 0, 0),  new[] { new Vector3(1,0.5f,0), new Vector3(1,0.5f,1), new Vector3(1,1,1), new Vector3(1,1,0) }, new Vector3i(1,0,0)),
        };

                                                                              
        map[BlockShape.SlopeMinX] = new[]
        {
            new ShapeFace(new Vector3(0, -1, 0), new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) }, new Vector3i(0,-1,0)),
            new ShapeFace(new Vector3(1, 0, 0),  new[] { new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(1,1,0) }, new Vector3i(1,0,0)),
            new ShapeFace(new Vector3(0, 0, -1), new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,1,0) }, new Vector3i(0,0,-1)),
            new ShapeFace(new Vector3(0, 0, 1),  new[] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1) }, new Vector3i(0,0,1)),
            new ShapeFace(new Vector3(-d, d, 0), new[] { new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(1,1,1), new Vector3(1,1,0) }, null),
        };

                                                   
        map[BlockShape.SlopeMaxX] = new[]
        {
            new ShapeFace(new Vector3(0, -1, 0), new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) }, new Vector3i(0,-1,0)),
            new ShapeFace(new Vector3(-1, 0, 0), new[] { new Vector3(0,0,1), new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(0,1,1) }, new Vector3i(-1,0,0)),
            new ShapeFace(new Vector3(0, 0, -1), new[] { new Vector3(1,0,0), new Vector3(0,0,0), new Vector3(0,1,0) }, new Vector3i(0,0,-1)),
            new ShapeFace(new Vector3(0, 0, 1),  new[] { new Vector3(1,0,1), new Vector3(0,0,1), new Vector3(0,1,1) }, new Vector3i(0,0,1)),
            new ShapeFace(new Vector3(d, d, 0),  new[] { new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,1,1), new Vector3(0,1,0) }, null),
        };

                                                        
        map[BlockShape.SlopeMinZ] = new[]
        {
            new ShapeFace(new Vector3(0, -1, 0), new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) }, new Vector3i(0,-1,0)),
            new ShapeFace(new Vector3(0, 0, 1),  new[] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1) }, new Vector3i(0,0,1)),
            new ShapeFace(new Vector3(-1, 0, 0), new[] { new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(0,1,1) }, new Vector3i(-1,0,0)),
            new ShapeFace(new Vector3(1, 0, 0),  new[] { new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(1,1,1) }, new Vector3i(1,0,0)),
            new ShapeFace(new Vector3(0, d, -d), new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,1,1), new Vector3(0,1,1) }, null),
        };

                                                   
        map[BlockShape.SlopeMaxZ] = new[]
        {
            new ShapeFace(new Vector3(0, -1, 0), new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) }, new Vector3i(0,-1,0)),
            new ShapeFace(new Vector3(0, 0, -1), new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0) }, new Vector3i(0,0,-1)),
            new ShapeFace(new Vector3(-1, 0, 0), new[] { new Vector3(0,0,1), new Vector3(0,0,0), new Vector3(0,1,0) }, new Vector3i(-1,0,0)),
            new ShapeFace(new Vector3(1, 0, 0),  new[] { new Vector3(1,0,1), new Vector3(1,0,0), new Vector3(1,1,0) }, new Vector3i(1,0,0)),
            new ShapeFace(new Vector3(0, d, d),  new[] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,0), new Vector3(0,1,0) }, null),
        };

                                                                           
                                                                            
                                                                             
                                                                              
                                                         
                                                                            
                                                                              
                                                                          
                                                                           
                                                                              
                                                                            
                                                                          
                                                                            
                                                                            
                                                                               
                                                                               
                                                   
        map[BlockShape.Cross] = new[]
        {
            new ShapeFace(new Vector3(-d, 0, d),  new[] { new Vector3(0,1,0), new Vector3(1,1,1), new Vector3(1,0,1), new Vector3(0,0,0) }, null),
            new ShapeFace(new Vector3(d, 0, d),   new[] { new Vector3(1,1,0), new Vector3(0,1,1), new Vector3(0,0,1), new Vector3(1,0,0) }, null),
        };

        map[BlockShape.Post] = new[]
        {
            new ShapeFace(new Vector3(0, -1, 0), new[] { new Vector3(.32f,0,.32f), new Vector3(.68f,0,.32f), new Vector3(.68f,0,.68f), new Vector3(.32f,0,.68f) }, new Vector3i(0,-1,0)),
            new ShapeFace(new Vector3(0, 1, 0), new[] { new Vector3(.32f,.82f,.32f), new Vector3(.68f,.82f,.32f), new Vector3(.68f,.82f,.68f), new Vector3(.32f,.82f,.68f) }, null),
            new ShapeFace(new Vector3(0, 0, -1), new[] { new Vector3(.32f,0,.32f), new Vector3(.68f,0,.32f), new Vector3(.68f,.82f,.32f), new Vector3(.32f,.82f,.32f) }, null),
            new ShapeFace(new Vector3(0, 0, 1), new[] { new Vector3(.68f,0,.68f), new Vector3(.32f,0,.68f), new Vector3(.32f,.82f,.68f), new Vector3(.68f,.82f,.68f) }, null),
            new ShapeFace(new Vector3(-1, 0, 0), new[] { new Vector3(.32f,0,.68f), new Vector3(.32f,0,.32f), new Vector3(.32f,.82f,.32f), new Vector3(.32f,.82f,.68f) }, null),
            new ShapeFace(new Vector3(1, 0, 0), new[] { new Vector3(.68f,0,.32f), new Vector3(.68f,0,.68f), new Vector3(.68f,.82f,.68f), new Vector3(.68f,.82f,.32f) }, null),
        };

        map[BlockShape.Sprite] = new[]
        {
            new ShapeFace(new Vector3(-d, 0, d), new[] { new Vector3(.25f,.9f,.25f), new Vector3(.75f,.9f,.75f), new Vector3(.75f,0,.75f), new Vector3(.25f,0,.25f) }, null),
            new ShapeFace(new Vector3(d, 0, d), new[] { new Vector3(.75f,.9f,.25f), new Vector3(.25f,.9f,.75f), new Vector3(.25f,0,.75f), new Vector3(.75f,0,.25f) }, null),
        };

        map[BlockShape.Torch] = BuildPost(.43f, .57f, .78f);
        map[BlockShape.Lantern] = BuildPost(.28f, .72f, .82f);
        map[BlockShape.SnowLayer] = BuildLayer(0.125f);
        map[BlockShape.SnowLayer2] = BuildLayer(0.25f);
        map[BlockShape.SnowLayer3] = BuildLayer(0.375f);
        map[BlockShape.SnowLayer4] = BuildLayer(0.5f);
        map[BlockShape.SnowLayer5] = BuildLayer(0.625f);
        map[BlockShape.SnowLayer6] = BuildLayer(0.75f);
        map[BlockShape.SnowLayer7] = BuildLayer(0.875f);
        map[BlockShape.SnowLayer8] = BuildLayer(1f);

        return map;
    }

    private static ShapeFace[] BuildPost(float min, float max, float height)
    {
        return new[]
        {
            new ShapeFace(new Vector3(0, -1, 0), new[] { new Vector3(min,0,min), new Vector3(max,0,min), new Vector3(max,0,max), new Vector3(min,0,max) }, new Vector3i(0,-1,0)),
            new ShapeFace(new Vector3(0, 1, 0), new[] { new Vector3(min,height,min), new Vector3(max,height,min), new Vector3(max,height,max), new Vector3(min,height,max) }, null),
            new ShapeFace(new Vector3(0, 0, -1), new[] { new Vector3(min,0,min), new Vector3(max,0,min), new Vector3(max,height,min), new Vector3(min,height,min) }, null),
            new ShapeFace(new Vector3(0, 0, 1), new[] { new Vector3(max,0,max), new Vector3(min,0,max), new Vector3(min,height,max), new Vector3(max,height,max) }, null),
            new ShapeFace(new Vector3(-1, 0, 0), new[] { new Vector3(min,0,max), new Vector3(min,0,min), new Vector3(min,height,min), new Vector3(min,height,max) }, null),
            new ShapeFace(new Vector3(1, 0, 0), new[] { new Vector3(max,0,min), new Vector3(max,0,max), new Vector3(max,height,max), new Vector3(max,height,min) }, null),
        };
    }

    private static ShapeFace[] BuildLayer(float height)
    {
        return new[]
        {
            new ShapeFace(new Vector3(0, -1, 0), new[] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,0,0), new Vector3(0,0,0) }, new Vector3i(0,-1,0)),
            new ShapeFace(new Vector3(0, 1, 0), new[] { new Vector3(0,height,0), new Vector3(1,height,0), new Vector3(1,height,1), new Vector3(0,height,1) }, new Vector3i(0,1,0)),
            new ShapeFace(new Vector3(0, 0, -1), new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,height,0), new Vector3(0,height,0) }, new Vector3i(0,0,-1)),
            new ShapeFace(new Vector3(0, 0, 1), new[] { new Vector3(1,0,1), new Vector3(0,0,1), new Vector3(0,height,1), new Vector3(1,height,1) }, new Vector3i(0,0,1)),
            new ShapeFace(new Vector3(-1, 0, 0), new[] { new Vector3(0,0,1), new Vector3(0,0,0), new Vector3(0,height,0), new Vector3(0,height,1) }, new Vector3i(-1,0,0)),
            new ShapeFace(new Vector3(1, 0, 0), new[] { new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(1,height,1), new Vector3(1,height,0) }, new Vector3i(1,0,0)),
        };
    }
}
