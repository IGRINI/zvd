using Game.Abilities.Items.Interfaces;
using Unity.Netcode;
using UnityEngine;

public class ProjectileManager
{
    
    private static void CreateLocalProjectile(string projectileName, Vector3 fromPosition, Vector3 toPosition,
        ProjectileTrajectory trajectory, float speed)
    {
        // var projectile = Resources.Load<ProjectileView>($"Projectiles/{projectileName}.prefab");
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    public static void ThrowObjectRpc(string projectileName, Vector3 fromPosition, Vector3 toPosition,
        ProjectileTrajectory trajectory, float speed)
    {
        CreateLocalProjectileRpc(projectileName, fromPosition, toPosition, trajectory, speed);
    }
    
    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Reliable)]
    public static void CreateLocalProjectileRpc(string projectileName, Vector3 fromPosition, Vector3 toPosition,
        ProjectileTrajectory trajectory, float speed)
    {
        CreateLocalProjectile(projectileName, fromPosition, toPosition, trajectory, speed);
    }
}
