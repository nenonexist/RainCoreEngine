using OpenTK.Mathematics;

namespace RainCore;

                                                                
                                                                  
                                                                  
                                                 

public partial class World
{

                                                                   
                                             
                                                                   

    public void FlattenArea(int centerX, int centerZ, int radius, int groundY, int blendMargin, BlockType groundBlock)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dz = -radius; dz <= radius; dz++)
            {
                float edgeDist = MathF.Max(MathF.Abs(dx), MathF.Abs(dz));
                if (edgeDist > radius) continue;

                int x = centerX + dx, z = centerZ + dz;
                int naturalHeight = ComputeHeightSmoothed(x, z, _noiseOffsetX, _noiseOffsetZ);

                int targetHeight;
                float blendStart = radius - blendMargin;
                if (edgeDist <= blendStart)
                {
                    targetHeight = groundY;
                }
                else
                {
                    float t = Clamp01((edgeDist - blendStart) / blendMargin);
                    t = t * t * (3f - 2f * t);
                    targetHeight = (int)MathF.Round(Lerp(groundY, naturalHeight, t));
                }

                int clearTop = Math.Max(groundY, naturalHeight) + 20;
                for (int y = BedrockLevel; y <= clearTop; y++)
                    SetBlockSilent(new Vector3i(x, y, z), BlockType.Air);

                SetBlockSilent(new Vector3i(x, targetHeight, z), groundBlock);
                for (int y = targetHeight - 1; y >= targetHeight - 3; y--)
                    SetBlockSilent(new Vector3i(x, y, z), BlockType.Dirt);
                for (int y = targetHeight - 4; y >= BedrockLevel; y--)
                    SetBlockSilent(new Vector3i(x, y, z), BlockType.Stone);
            }
        }
    }


                                                                                  
                                                                             
                                                                                 
                                                                             
                                                                                
                                                                         
                                                                              
                                                                                
    private readonly List<(string ModelName, Vector3 Position, Vector3 RotationDeg, float Scale)> _pendingPrefabModels = new();
    public IReadOnlyList<(string ModelName, Vector3 Position, Vector3 RotationDeg, float Scale)> PendingPrefabModels
    {
        get { lock (_worldDataLock) return _pendingPrefabModels.ToList(); }
    }


                 
                                                                                
                                                                            
                                                                            
                                                                             
                     
                  
    public List<(string ModelName, Vector3 Position, Vector3 RotationDeg, float Scale)> ConsumePendingPrefabModels()
    {
        lock (_worldDataLock)
        {
            var result = new List<(string ModelName, Vector3 Position, Vector3 RotationDeg, float Scale)>(_pendingPrefabModels);
            _pendingPrefabModels.Clear();
            return result;
        }
    }


                                                                                                                   
    public void PlacePrefab(Prefab prefab, Vector3i origin, int rotationSteps = 0)
    {
        foreach (var entry in prefab.Blocks)
        {
            var pos = origin + RotateY(entry.Offset, rotationSteps);
            SetBlockSilent(pos, entry.Type);
            if (_streaming) _structureBlocks.Add(pos);
            if (entry.Shape != BlockShape.Cube)
                SetBlockShape(pos, RotateShape(entry.Shape, rotationSteps));                                                              
        }

        foreach (var m in prefab.Models)
        {
            var worldPos = new Vector3(origin.X, origin.Y, origin.Z) + RotateY(m.Offset, rotationSteps);
            var rotationDeg = m.RotationDeg + new Vector3(0, rotationSteps * 90f, 0);
            lock (_worldDataLock) { _pendingPrefabModels.Add((m.ModelName, worldPos, rotationDeg, m.Scale)); }
        }
    }


                 
                                                                                 
                                                                               
                                                                                
                                                                               
                                                                            
                                                                          
                                                                          
                                                                          
                                                                             
                  
    public void ScatterRandomPrefabs(Random rng, int centerX, int centerZ, int radius, int groundY,
        (Func<Prefab> Factory, float Chance)[] candidates, int cellSize = 12)
    {
        for (int x = -radius; x <= radius; x += cellSize)
        {
            for (int z = -radius; z <= radius; z += cellSize)
            {
                if (x * x + z * z > radius * radius) continue;

                var origin = new Vector3i(centerX + x, groundY, centerZ + z);
                if (GetBlock(origin + new Vector3i(0, 1, 0)) != BlockType.Air)
                    continue;                                                            

                foreach (var (factory, chance) in candidates)
                {
                    if (rng.NextDouble() > chance) continue;

                    int jitter = cellSize / 3;
                    int jx = jitter > 0 ? rng.Next(-jitter, jitter + 1) : 0;
                    int jz = jitter > 0 ? rng.Next(-jitter, jitter + 1) : 0;

                    PlacePrefab(factory(), new Vector3i(origin.X + jx, groundY, origin.Z + jz), rng.Next(4));
                    break;                                     
                }
            }
        }
    }


                                                                                                                   
    public static Vector3i RotateY(Vector3i v, int steps)
    {
        steps = ((steps % 4) + 4) % 4;
        for (int i = 0; i < steps; i++) v = new Vector3i(v.Z, v.Y, -v.X);
        return v;
    }


    public static Vector3 RotateY(Vector3 v, int steps)
    {
        steps = ((steps % 4) + 4) % 4;
        for (int i = 0; i < steps; i++) v = new Vector3(v.Z, v.Y, -v.X);
        return v;
    }


                                                                               
                                                                              
                                                                               
                                                                                  
                                                                              
                                               
    private static readonly Dictionary<Vector3i, BlockShape> AscentToSlope = new()
    {
        { new Vector3i(1, 0, 0), BlockShape.SlopeMinX },
        { new Vector3i(-1, 0, 0), BlockShape.SlopeMaxX },
        { new Vector3i(0, 0, 1), BlockShape.SlopeMinZ },
        { new Vector3i(0, 0, -1), BlockShape.SlopeMaxZ },
    };


    private static readonly Dictionary<BlockShape, Vector3i> SlopeToAscent = new()
    {
        { BlockShape.SlopeMinX, new Vector3i(1, 0, 0) },
        { BlockShape.SlopeMaxX, new Vector3i(-1, 0, 0) },
        { BlockShape.SlopeMinZ, new Vector3i(0, 0, 1) },
        { BlockShape.SlopeMaxZ, new Vector3i(0, 0, -1) },
    };


                                                                                                                                 
    public static BlockShape RotateShape(BlockShape shape, int steps)
    {
        return SlopeToAscent.TryGetValue(shape, out var ascent) ? AscentToSlope[RotateY(ascent, steps)] : shape;
    }


                                                                                                                                                                           
    public void PlaceBlock(Vector3i pos, BlockType type)
    {
        SetBlockSilent(pos, type);
    }
}
