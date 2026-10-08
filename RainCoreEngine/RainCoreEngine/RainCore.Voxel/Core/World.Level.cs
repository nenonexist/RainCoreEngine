using OpenTK.Mathematics;

namespace RainCore;

             
                                                                          
                                                                       
                                                                         
                                                                        
                                                                     
                                                                         
                                      
              
public partial class World : ILevel
{
    string ILevel.Name => DreamName;

                                                                                               
                                                                                      
                                                                               
    bool ILevel.IsSolidAt(Vector3i pos) => IsSolidAt(pos);

    int ILevel.GetSurfaceHeight(int x, int z) => GetSurfaceHeight(x, z);

    bool ILevel.Raycast(Vector3 origin, Vector3 direction, float maxDistance, out Vector3 hitPoint, out Vector3 hitNormal)
    {
        if (!Raycast(origin, direction, maxDistance, out var hitBlock, out var placeBlock, out var exactHitPoint))
        {
            hitPoint = default;
            hitNormal = default;
            return false;
        }

        hitPoint = exactHitPoint;
        var delta = placeBlock - hitBlock;
        hitNormal = delta == Vector3i.Zero
            ? Vector3.UnitY
            : Vector3.Normalize(new Vector3(delta.X, delta.Y, delta.Z));
        return true;
    }

                                                                                     
                                                                                  
                                                                                 
                                                                              
                                                                              
                                                         
    bool ILevel.RemovePoster(PosterInstance poster)
    {
        if (!_posters.Remove(poster)) return false;
        poster.Dispose();
        return true;
    }

    void ILevel.Save()
    {
        SaveEdits();
        SaveInteractableStates();
        SaveItemCatalog();
        SaveModelPlacements();
        SavePosterPlacements();
        SaveWorldSettings();
    }
}
