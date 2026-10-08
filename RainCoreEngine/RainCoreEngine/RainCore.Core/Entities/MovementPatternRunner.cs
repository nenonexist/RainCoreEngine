using OpenTK.Mathematics;

namespace RainCore;

             
                                                                              
                                                                               
                                                                         
                                                                        
                                                                            
                                                                     
                                                                         
                                                                          
                                                                            
                                                                              
                                                                                
              
public sealed class MovementPatternRunner
{
    private const float WanderRadius = 4f;
    private const float WanderSpeed = 1.2f;
    private const double WanderPauseSeconds = 2.0;
    private const float PatrolSpeed = 1.5f;
    private const float PatrolPointReachDist = 0.3f;
    private const float FaceCameraRadius = 6f;
    private const float TurnSpeedDegPerSec = 240f;

    private readonly Vector3 _spawnOrigin;
    private readonly Random _rng = new();
    private Vector3 _wanderTarget;
    private double _wanderPauseLeft;
    private int _patrolIndex;

    public MovementPatternRunner(Vector3 spawnOrigin)
    {
        _spawnOrigin = spawnOrigin;
        _wanderTarget = spawnOrigin;
    }

                                                                                 
                                                                              
                                                                               
                                                                              
                              
    public void Update(double dt, MovementPattern pattern, IReadOnlyList<Vector3> patrolPoints,
        ILevel level, Vector3 playerPos, ref Vector3 position, ref float yawDegrees)
    {
        switch (pattern)
        {
            case MovementPattern.RandomWander:
                UpdateWander(dt, level, ref position, ref yawDegrees);
                break;
            case MovementPattern.PatrolRoute:
                UpdatePatrol(dt, patrolPoints, level, ref position, ref yawDegrees);
                break;
            case MovementPattern.FaceCamera:
                UpdateFacePlayer(dt, playerPos, ref position, ref yawDegrees);
                break;
            case MovementPattern.Static:
            default:
                break;
        }
    }

    private void UpdateWander(double dt, ILevel level, ref Vector3 position, ref float yawDegrees)
    {
        if (_wanderPauseLeft > 0)
        {
            _wanderPauseLeft -= dt;
            return;
        }

        var toTarget = _wanderTarget - position;
        toTarget.Y = 0;
        if (toTarget.LengthSquared < 0.04f)
        {
            PickNewWanderTarget(level, position.Y);
            _wanderPauseLeft = WanderPauseSeconds;
            return;
        }

        var dir = Vector3.Normalize(toTarget);
        var next = position + dir * WanderSpeed * (float)dt;
        if (TryFindGroundY(level, next.X, position.Y, next.Z, out var groundY))
            next.Y = groundY;

        position = next;
        FaceDirection(dir, dt, ref yawDegrees);
    }

    private void PickNewWanderTarget(ILevel level, float baseY)
    {
        for (int attempt = 0; attempt < 6; attempt++)
        {
            float angle = (float)(_rng.NextDouble() * Math.PI * 2);
            float dist = (float)(_rng.NextDouble() * WanderRadius);
            var candidate = _spawnOrigin + new Vector3(MathF.Cos(angle) * dist, 0, MathF.Sin(angle) * dist);
            candidate = level.Bounds.Clamp(candidate, 1f);
            if (TryFindGroundY(level, candidate.X, baseY, candidate.Z, out var groundY))
            {
                candidate.Y = groundY;
                _wanderTarget = candidate;
                return;
            }
        }
        _wanderTarget = _spawnOrigin;                                      
    }

    private void UpdatePatrol(double dt, IReadOnlyList<Vector3> patrolPoints, ILevel level,
        ref Vector3 position, ref float yawDegrees)
    {
        if (patrolPoints.Count == 0) return;

        var target = patrolPoints[_patrolIndex % patrolPoints.Count];
        var toTarget = target - position;
        toTarget.Y = 0;

        if (toTarget.LengthSquared < PatrolPointReachDist * PatrolPointReachDist)
        {
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Count;
            return;
        }

        var dir = Vector3.Normalize(toTarget);
        var next = position + dir * PatrolSpeed * (float)dt;
        if (TryFindGroundY(level, next.X, position.Y, next.Z, out var groundY))
            next.Y = groundY;

        position = next;
        FaceDirection(dir, dt, ref yawDegrees);
    }

    private void UpdateFacePlayer(double dt, Vector3 playerPos, ref Vector3 position, ref float yawDegrees)
    {
        var toPlayer = playerPos - position;
        toPlayer.Y = 0;
        if (toPlayer.LengthSquared < 0.01f) return;
        if (toPlayer.Length > FaceCameraRadius) return;
        FaceDirection(Vector3.Normalize(toPlayer), dt, ref yawDegrees);
    }

    private static void FaceDirection(Vector3 dir, double dt, ref float yawDegrees)
    {
        float targetYaw = MathHelper.RadiansToDegrees(MathF.Atan2(dir.X, dir.Z));
        float diff = ((targetYaw - yawDegrees + 540f) % 360f) - 180f;                            
        float maxStep = TurnSpeedDegPerSec * (float)dt;
        yawDegrees += Math.Clamp(diff, -maxStep, maxStep);
    }

                                                                                  
                                                                        
                                                                             
                                                                 
    private static bool TryFindGroundY(ILevel level, float x, float baseY, float z, out float groundY)
    {
        for (int dy = 2; dy >= -4; dy--)
        {
            var cell = new Vector3i((int)MathF.Floor(x), (int)MathF.Floor(baseY) + dy - 1, (int)MathF.Floor(z));
            if (level.IsSolidAt(cell))
            {
                groundY = cell.Y + 1;
                return true;
            }
        }
        groundY = baseY;
        return false;
    }
}
