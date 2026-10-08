using OpenTK.Mathematics;

namespace RainCore;

             
                                                                        
                                                                     
                                                                            
                                                                  
              
public class Prefab
{
    public readonly record struct BlockEntry(Vector3i Offset, BlockType Type, BlockShape Shape = BlockShape.Cube);
    public readonly record struct ModelEntry(string ModelName, Vector3 Offset, Vector3 RotationDeg, float Scale);

    private readonly List<BlockEntry> _blocks = new();
    private readonly List<ModelEntry> _models = new();

    public IReadOnlyList<BlockEntry> Blocks => _blocks;
    public IReadOnlyList<ModelEntry> Models => _models;

                                                                                       
                                                                                        
                                                                                         
                                                                             
                                                                                  
    public Vector3i? DoorOffset { get; private set; }

                                                                                                                                      
    public Prefab Block(int x, int y, int z, BlockType type, BlockShape shape = BlockShape.Cube)
    {
        _blocks.Add(new BlockEntry(new Vector3i(x, y, z), type, shape));
        return this;
    }

                                                                                 
    public Prefab Box(int x0, int y0, int z0, int x1, int y1, int z1, BlockType type)
    {
        for (int x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++)
            for (int y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++)
                for (int z = Math.Min(z0, z1); z <= Math.Max(z0, z1); z++)
                    _blocks.Add(new BlockEntry(new Vector3i(x, y, z), type));
        return this;
    }

                                                                                                
    public Prefab HollowBox(int x0, int y0, int z0, int x1, int y1, int z1, BlockType wallType)
    {
        for (int x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++)
            for (int y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++)
                for (int z = Math.Min(z0, z1); z <= Math.Max(z0, z1); z++)
                {
                    bool edge = x == x0 || x == x1 || y == y0 || y == y1 || z == z0 || z == z1;
                    if (edge) _blocks.Add(new BlockEntry(new Vector3i(x, y, z), wallType));
                }
        return this;
    }

                                                                                 
    public Prefab Model(string modelName, float x, float y, float z, Vector3 rotationDeg = default, float scale = 1f)
    {
        _models.Add(new ModelEntry(modelName, new Vector3(x, y, z), rotationDeg, scale));
        return this;
    }

                                                                                       
                                                                                         
                                                                                           
    public Prefab Door(int x, int y, int z)
    {
        DoorOffset = new Vector3i(x, y, z);
        return this;
    }

                 
                                                                          
                                                                                
                                                                           
                            
                       
         
                                                 
                                                                   
                                              
                  
    public static Prefab FromAscii(string[][] layers, Dictionary<char, BlockType> legend)
    {
        var prefab = new Prefab();
        for (int y = 0; y < layers.Length; y++)
            for (int z = 0; z < layers[y].Length; z++)
                for (int x = 0; x < layers[y][z].Length; x++)
                {
                    char c = layers[y][z][x];
                    if (c == '.' || c == ' ') continue;
                    if (legend.TryGetValue(c, out var type))
                        prefab.Block(x, y, z, type);
                }
        return prefab;
    }
}
