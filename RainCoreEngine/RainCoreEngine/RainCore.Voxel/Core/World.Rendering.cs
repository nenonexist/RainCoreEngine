using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using RainCoreGraphics;

namespace RainCore;

public partial class World
{
    public string MusicPath { get; internal set; } = string.Empty;

                                                                                 
                                                                                   
                                                                             
                                                                              
                                                                               
                                                                            
                                                        
    private readonly List<Chunk> _renderSnapshotBuffer = new();

    private readonly List<Chunk> _visibleChunkRenderBuffer = new();

                                                                                      
                                                                                      
                                                                                
                                                                                     
                                                                                        
                                                                                   
                                                                                 
                                                                                      
                                                                                    
                                                                                   
                                                                                 
                                                                                       
                     
                                                                                       
                                                                                    
                                                                                     
                                                                                     
                                                                                   
                                                                                                   
    private const int MeshesPerFrameBudget = 6;

                                                                                  
                                                                                  
                                                                                   
                                                                                     
                                                                                        
                                                                                    
    private const double MeshBudgetMs = 4.0;

                 
                                                                           
                                                                             
                                                                
                                                                                    
                                                                                     
                                                                                  
                                                                                      
                                                                                   
                                                                                 
                                                                  
                  
    public void Render(Vector3 cameraPosition, Vector3 cameraFront, float renderDistance = 160f, Matrix4? viewProjection = null)
    {
        float renderDistSq = renderDistance * renderDistance;
        var frustum = viewProjection.HasValue ? new Frustum(viewProjection.Value) : (Frustum?)null;

                                                                                  
                                                                                   
                                                                            
                                                                                    
                                                                                   
                                                                                
                                                                              
                                                                                   
                                                                                    
                                                                         
          
                                                                               
                                                                                
                                                                                    
                                                                                   
                                                                                 
                                             
        lock (_worldDataLock)
        {
            _renderSnapshotBuffer.Clear();
            _renderSnapshotBuffer.AddRange(_chunks.Values);
        }

        int meshedThisFrame = 0;
                                                                                     
                                                                               
        var meshStopwatch = System.Diagnostics.Stopwatch.StartNew();

        _visibleChunkRenderBuffer.Clear();
        foreach (var chunk in _renderSnapshotBuffer)
        {
            var closest = chunk.ClosestPointXZ(cameraPosition);
            float dx = closest.X - cameraPosition.X;
            float dz = closest.Z - cameraPosition.Z;
            float distSq = dx * dx + dz * dz;
            if (distSq > renderDistSq) continue;

            if (frustum.HasValue && !frustum.Value.IntersectsAabb(
                new Vector3(chunk.MinX, BedrockLevel, chunk.MinZ),
                new Vector3(chunk.MaxX, 128f, chunk.MaxZ))) continue;

            _visibleChunkRenderBuffer.Add(chunk);
        }

        foreach (var chunk in _visibleChunkRenderBuffer)
        {

                                                                                      
                                                                                      
              
                                                                                      
                                                                                     
                                                                                    
                                                                                   
                                                                                  
                                                                                  
                                                                                       
                                                                                        
                                                                                      
                                                                                    
                                                                                
                                                                                     
                                                                                  
                                     
              
                                                                                      
                                                                                       
                                                                                       
                                                                                     
                                                                                         
                                                                                        
                                                                                        
                                                                                        
                                                                                        
            if (chunk.NeedsMesh && meshedThisFrame < MeshesPerFrameBudget &&
                (meshedThisFrame == 0 || meshStopwatch.Elapsed.TotalMilliseconds < MeshBudgetMs))
            {
                long meshStart = System.Diagnostics.Stopwatch.GetTimestamp();
                chunk.EnsureMesh(GetBlock, _tint, GetLight, GetLiquidLevel, p => GetWaterColorAt(p.X, p.Z));
                Interlocked.Add(ref _meshBuildTicks, System.Diagnostics.Stopwatch.GetTimestamp() - meshStart);
                Interlocked.Increment(ref _meshBuildCount);
                meshedThisFrame++;
            }

            chunk.Render();
        }
    }

                 
                                                                                  
                                                                          
                                                                                 
                                                                                  
                                                                                
                                           
                  
    public void RenderLiquids(Vector3 cameraPosition, float renderDistance = 160f, Matrix4? viewProjection = null)
    {
                                                                            
        foreach (var chunk in _visibleChunkRenderBuffer)
        {
                                                                                    
                                                                                  
                                                                                   
                                                                        
            chunk.RenderLiquid();
        }
    }

                                                                                         
                                                                              
                                                                      
                                                                                
                                                                                  
                                      
    public void ReleaseGpuResources()
    {
        ShutdownBackgroundGeneration();

        List<Chunk> chunksSnapshot;
        lock (_worldDataLock) { chunksSnapshot = new List<Chunk>(_chunks.Values); }
        foreach (var chunk in chunksSnapshot)
            chunk.ReleaseGpu();
    }

    public ModelInstance AddModel(GlbModel model, string modelKey, Vector3 position, Vector3 rotationDeg = default, float scale = 1f)
    {
        var instance = new ModelInstance(model, modelKey, position, rotationDeg, scale);
        _entities.Models.Add(instance);
        _entities.Models.Sort(static (a, b) => string.CompareOrdinal(a.ModelKey, b.ModelKey));
        return instance;
    }

                 
                                                                                 
                                                                               
                                                                        
                                                                                
                                                                
                  
    public bool RemoveModel(ModelInstance model)
    {
        return _entities.Models.Remove(model);
    }

                                                                                         
                                                                                   
                                                                           
    private readonly List<ModelInstance> _visibleModelsBuffer = new();

                                                                                   
                                                                                     
                                                                               
                                                                     
    public void RenderModels(Shader shader, Vector3 cameraPosition, float renderDistance = 160f, Matrix4? viewProjection = null)
    {
        float renderDistSq = renderDistance * renderDistance;
        var frustum = viewProjection.HasValue ? new Frustum(viewProjection.Value) : (Frustum?)null;

        _visibleModelsBuffer.Clear();
        foreach (var model in _entities.Models)
        {
                                                                               
                                                                                          
                                                                                  
            if (Vector3.DistanceSquared(model.Position, cameraPosition) > renderDistSq) continue;

            if (frustum.HasValue)
            {
                model.GetWorldBounds(out var min, out var max);
                if (!frustum.Value.IntersectsAabb(min, max)) continue;
            }

            _visibleModelsBuffer.Add(model);
        }

        foreach (var model in _visibleModelsBuffer)
            model.Render(shader);
    }

                 
                                                                                
                                                                               
                                                                       
                                                                        
                                                                               
                                                                                
                  
                                                                                    
                                                                                         
                                                                                       
                                                                                              
    private const float NpcCullRadius = 1.5f;

    public void RenderNpcModels(Shader shader, IReadOnlyDictionary<string, GlbModel> modelCache, Vector3 cameraPosition, float renderDistance = 160f, Matrix4? viewProjection = null)
    {
        float renderDistSq = renderDistance * renderDistance;
        var frustum = viewProjection.HasValue ? new Frustum(viewProjection.Value) : (Frustum?)null;

        foreach (var npc in _entities.Npcs)
        {
            if (npc.Model == null && !string.IsNullOrEmpty(npc.ModelName)
                && modelCache.TryGetValue(npc.ModelName, out var found))
            {
                npc.Model = found;
            }

            if (npc.Model == null) continue;

            if (Vector3.DistanceSquared(npc.Position, cameraPosition) > renderDistSq) continue;

            if (frustum.HasValue)
            {
                var min = npc.Position - new Vector3(NpcCullRadius, 0f, NpcCullRadius);
                var max = npc.Position + new Vector3(NpcCullRadius, NpcCullRadius * 2f, NpcCullRadius);
                if (!frustum.Value.IntersectsAabb(min, max)) continue;
            }

            var matrix = Matrix4.CreateScale(npc.ModelScale)
                * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(npc.YawDegrees))
                * Matrix4.CreateTranslation(npc.Position);

            npc.Model.Render(shader, matrix);
        }
    }
}
