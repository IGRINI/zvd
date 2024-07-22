using Game.Abilities.Items.Interfaces;
using Unity.Netcode;
using UnityEngine;

public class ProjectileManager : NetworkBehaviour
{
    public static ProjectileManager Singleton { get; private set; }
    
    private void Awake()
    {
        Singleton = this;
    }
    
    private static void CreateLocalProjectile(string projectileName, Vector3 fromPosition, Vector3 toPosition,
        ProjectileTrajectory trajectory, float speed)
    {
        var projectilePrefab = Resources.Load<ProjectileView>($"Projectiles/{projectileName}");
        if (projectilePrefab != null)
        {
            var projectile = Instantiate(projectilePrefab);
            projectile.Initialize(fromPosition, toPosition, speed, trajectory);
        }
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    public void ThrowObjectRpc(string projectileName, Vector3 fromPosition, Vector3 toPosition,
        ProjectileTrajectory trajectory, float speed)
    {
        CreateLocalProjectileRpc(projectileName, fromPosition, toPosition, trajectory, speed);
    }
    
    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    public void CreateLocalProjectileRpc(string projectileName, Vector3 fromPosition, Vector3 toPosition,
        ProjectileTrajectory trajectory, float speed)
    {
        CreateLocalProjectile(projectileName, fromPosition, toPosition, trajectory, speed);
    }
}
