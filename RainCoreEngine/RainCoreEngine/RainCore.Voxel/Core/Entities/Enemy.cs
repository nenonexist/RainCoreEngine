using OpenTK.Mathematics;

namespace RainCore;

             
                                                                             
                                                                       
                                                                               
                                                                              
                                                                                
                                                                             
                                                                  
                                                                   
                                          
   
                                                                            
                                                                            
                                                                               
                                                                     
                                                       
              
public sealed class Enemy
{
    public const int MaxHealth = 12;
    public const float AggroRadius = 9f;
    public const float AttackRadius = 1.3f;
    public const int AttackDamage = 2;
    public const float AttackCooldownSeconds = 1.1f;
    private const float MoveSpeed = 1.6f;
    private const float TurnSpeedDegPerSec = 240f;

    public Vector3 Position { get; private set; }
    public int Health { get; private set; } = MaxHealth;
    public bool IsAlive => Health > 0;
    public float YawDegrees { get; private set; }

    private float _attackCooldownLeft;

    public Enemy(Vector3 spawnPosition)
    {
        Position = spawnPosition;
    }

                                                                               
                                                                               
                                                                                    
                                                  
    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;
        Health = Math.Max(0, Health - amount);
    }

                                                                                 
                                                                          
                                                                             
                                                                           
                                                                              
    public bool Update(float dt, World world, Vector3 playerGroundPosition)
    {
        if (!IsAlive) return false;
        if (_attackCooldownLeft > 0f) _attackCooldownLeft -= dt;

        var toPlayer = playerGroundPosition - Position;
        toPlayer.Y = 0;
        float dist = toPlayer.Length;

        if (dist > AggroRadius) return false;                                     

        if (dist <= AttackRadius)
        {
            if (dist > 0.01f) FaceDirection(Vector3.Normalize(toPlayer), dt);
            if (_attackCooldownLeft > 0f) return false;
            _attackCooldownLeft = AttackCooldownSeconds;
            return true;          
        }

                                                                            
        var dir = Vector3.Normalize(toPlayer);
        var next = Position + dir * MoveSpeed * dt;
        if (TryFindGroundY(world, next.X, Position.Y, next.Z, out var groundY))
            next.Y = groundY;
        Position = next;
        FaceDirection(dir, dt);
        return false;
    }

    private void FaceDirection(Vector3 dir, float dt)
    {
        float targetYaw = MathHelper.RadiansToDegrees(MathF.Atan2(dir.X, dir.Z));
        float diff = ((targetYaw - YawDegrees + 540f) % 360f) - 180f;                            
        YawDegrees += Math.Clamp(diff, -TurnSpeedDegPerSec * dt, TurnSpeedDegPerSec * dt);
    }

                                                                                         
                                                                               
                                                                                
                                                                              
    private static bool TryFindGroundY(World world, float x, float baseY, float z, out float groundY)
    {
        for (int dy = 2; dy >= -4; dy--)
        {
            var cell = new Vector3i((int)MathF.Floor(x), (int)MathF.Floor(baseY) + dy - 1, (int)MathF.Floor(z));
            if (world.IsSolidAt(cell))
            {
                groundY = cell.Y + 1;
                return true;
            }
        }
        groundY = baseY;
        return false;
    }
}
