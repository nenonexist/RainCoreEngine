using OpenTK.Mathematics;

namespace RainCore;

             
                                                                            
                                                                            
                                                                        
                                                                           
                                                                         
                       
   
                                                          
                                                                      
                                                                            
                                                                         
                                                                          
                                                                     
                                                                      
                                                                      
              
public readonly struct BlockDefinition
{
    public readonly byte Id;
    public readonly string Name;
    public readonly int AtlasCol;
    public readonly int AtlasRow;
    public readonly bool IsSolid;
    public readonly bool IsOpaque;
    public readonly bool IsUnlit;
    public readonly Vector3 Color;
                                                                                                                                                  
    public readonly int Luminosity;
                 
                                                                            
                                                                            
                                                                           
                                                                          
                                                                        
                                                          
                  
    public readonly BlockShape DefaultShape;

    public BlockDefinition(byte id, string name, int atlasCol, int atlasRow, bool isSolid, bool isOpaque, bool isUnlit, Vector3 color, int luminosity = 0, BlockShape defaultShape = BlockShape.Cube)
    {
        Id = id;
        Name = name;
        AtlasCol = atlasCol;
        AtlasRow = atlasRow;
        IsSolid = isSolid;
        IsOpaque = isOpaque;
        IsUnlit = isUnlit;
        Color = color;
        Luminosity = luminosity;
        DefaultShape = defaultShape;
    }
}

             
                                                                             
                                                                              
                                                                             
                                                                               
                                                                             
                     
   
                                                                         
                                                                             
                                                                               
                                                              
              
public static class BlockRegistry
{
                 
                                                                         
                                                                              
                                                                            
                                                                            
                                          
                  
    public const byte CustomBlockRangeStart = 50;

    private static readonly Dictionary<byte, BlockDefinition> ById = new();
    private static readonly Dictionary<string, byte> IdByName = new(StringComparer.OrdinalIgnoreCase);
    private static byte _nextId = CustomBlockRangeStart;

                                                                                                                                 
    public static IReadOnlyCollection<BlockDefinition> AllCustomBlocks => ById.Values;

                 
                                                                            
                                                                              
                                                                             
                                                                            
                                                                              
                                         
       
                                                                               
                                                                      
                                                                              
                                                                  
                                                                             
                                                
                  
    static BlockRegistry()
    {
        Register("Brick", color: new Vector3(0.55f, 0.25f, 0.20f), atlasCol: 6, atlasRow: 0);
        Register("GlowCrystal", isUnlit: true, color: new Vector3(0.55f, 0.85f, 1.0f), luminosity: 14, atlasCol: 7, atlasRow: 0);
        Register("RedTulip", shape: BlockShape.Cross, isSolid: false, color: new Vector3(0.8f, 0.2f, 0.3f));
        Register("WildBush", shape: BlockShape.Cross, isSolid: false, color: new Vector3(0.25f, 0.5f, 0.2f));
        Register("TallGrass", shape: BlockShape.Cross, isSolid: false, color: new Vector3(0.25f, 0.65f, 0.18f));
        Register("Fern", shape: BlockShape.Cross, isSolid: false, color: new Vector3(0.16f, 0.48f, 0.22f));
        Register("BlueFlower", shape: BlockShape.Cross, isSolid: false, color: new Vector3(0.18f, 0.35f, 0.85f));
        Register("OakLeaves", isOpaque: false, color: new Vector3(0.16f, 0.48f, 0.16f));
        Register("AspenLeaves", isOpaque: false, color: new Vector3(0.92f, 0.62f, 0.12f));
        Register("MapleLeaves", isOpaque: false, color: new Vector3(0.78f, 0.18f, 0.08f));
        Register("AspenWood", color: new Vector3(0.68f, 0.54f, 0.34f));
        Register("MapleWood", color: new Vector3(0.42f, 0.16f, 0.10f));

                                                                               
                                                                            
                                                                                
        Register("CoalOre", color: new Vector3(0.25f, 0.24f, 0.23f), atlasCol: 6, atlasRow: 1);
        Register("IronOre", color: new Vector3(0.72f, 0.55f, 0.42f), atlasCol: 6, atlasRow: 2);
        Register("CopperOre", color: new Vector3(0.75f, 0.45f, 0.25f), atlasCol: 6, atlasRow: 3);
                                                                              
                                                                               
                                                                            
                                                                              
                                                    
        Register("GoldOre", color: new Vector3(0.85f, 0.72f, 0.20f), atlasCol: 8, atlasRow: 0);

                                                                                  
                                                                                   
                                                                         
                                                                          
                                                        
        Register("CopperBlock", color: new Vector3(0.80f, 0.50f, 0.28f), atlasCol: 8, atlasRow: 1);
        Register("IronBlock", color: new Vector3(0.78f, 0.78f, 0.80f), atlasCol: 8, atlasRow: 2);
        Register("GoldBlock", color: new Vector3(0.95f, 0.82f, 0.25f), atlasCol: 8, atlasRow: 3);

                                                                              
                                                                            
                                                                            
                                                  
        Register("WoodPlank", color: new Vector3(0.62f, 0.46f, 0.28f), atlasCol: 8, atlasRow: 4);

                                                                                
                                                                            
        Register("Permafrost", color: new Vector3(0.42f, 0.46f, 0.5f), atlasCol: 6, atlasRow: 4);                                 
        Register("PeatSoil", color: new Vector3(0.22f, 0.18f, 0.13f), atlasCol: 6, atlasRow: 5);                    
        Register("ForestLoam", color: new Vector3(0.3f, 0.2f, 0.11f), atlasCol: 6, atlasRow: 6);                                        
        Register("MeadowSoil", color: new Vector3(0.5f, 0.4f, 0.27f), atlasCol: 6, atlasRow: 7);                                 

                                                                                    
                                                                               
        Register("BerryBush", shape: BlockShape.Cross, isSolid: false, color: new Vector3(0.55f, 0.18f, 0.2f), atlasCol: 7, atlasRow: 3);
        Register("DeadBush", shape: BlockShape.Cross, isSolid: false, color: new Vector3(0.42f, 0.36f, 0.28f), atlasCol: 7, atlasRow: 4);
        Register("SnowyBush", shape: BlockShape.Cross, isSolid: false, color: new Vector3(0.75f, 0.82f, 0.85f), atlasCol: 7, atlasRow: 5);

                                                                     
                                                                            
                                                                          
                                             
        Register("PineNeedles", color: new Vector3(0.13f, 0.33f, 0.2f), atlasCol: 7, atlasRow: 6);
        Register("BirchWood", color: new Vector3(0.82f, 0.79f, 0.72f), atlasCol: 7, atlasRow: 7);

                                                                             
                                                                              
        Register("ConcreteWhite", color: new Vector3(0.92f, 0.92f, 0.90f), atlasCol: 8, atlasRow: 5);
        Register("ConcreteRed", color: new Vector3(0.78f, 0.08f, 0.10f), atlasCol: 8, atlasRow: 6);
        Register("ConcreteBlack", color: new Vector3(0.06f, 0.06f, 0.07f), atlasCol: 8, atlasRow: 7);
    }

                 
                                                                           
                                                                               
                                                                            
                                                                         
                            
                  
    private static void Register(string name, bool? isSolid = null, bool? isOpaque = null, bool isUnlit = false,
        Vector3 color = default, int luminosity = 0, BlockShape shape = BlockShape.Cube, int atlasCol = 0, int atlasRow = 0)
    {
        if (IdByName.ContainsKey(name))
        {
            Console.WriteLine($"[Блоки] Дубликат имени \"{name}\" в BlockRegistry - вторая регистрация пропущена.");
            return;
        }
        if (_nextId == 0)                                                         
        {
            Console.WriteLine($"[Блоки] Достигнут предел id (255) - \"{name}\" не зарегистрирован.");
            return;
        }

        bool defaultSolid = shape != BlockShape.Cross;
        bool defaultOpaque = shape != BlockShape.Cross;

        var def = new BlockDefinition(
            _nextId, name, atlasCol, atlasRow,
            isSolid ?? defaultSolid, isOpaque ?? defaultOpaque, isUnlit,
            color, Math.Clamp(luminosity, 0, 15), shape);

        ById[_nextId] = def;
        IdByName[name] = _nextId;

                                                                                      
                                                                               
                                                                                       
        if (_nextId == byte.MaxValue) { _nextId = 0; return; }
        _nextId++;
    }

    public static bool TryGetDefinition(BlockType type, out BlockDefinition def) =>
        ById.TryGetValue((byte)type, out def);

                                                                                                                            
    public static bool TryGetByName(string name, out BlockType type)
    {
        if (IdByName.TryGetValue(name, out var id))
        {
            type = (BlockType)id;
            return true;
        }
        type = default;
        return false;
    }

                                                                                                                                          
    public static string NameFor(BlockType type) =>
        TryGetDefinition(type, out var def) ? def.Name : type.ToString();
}
