using Game.Abilities.Items.Interfaces;

namespace Game.Abilities.Items
{
    public class GrenadeItem : BaseItemAbility, IThrowable
    {
        public override EAbilityBehaviour AbilityBehaviour { get; protected set; } = EAbilityBehaviour.PointTarget;

        string IThrowable.ProjectileName => "GrenadeProjectile";
        float IThrowable.ProjectileSpeed => 1.2f;
        ProjectileTrajectory IThrowable.Trajectory => ProjectileTrajectory.Ballistic;

        protected override void OnSpellStart()
        {
            IThrowable throwable = this;
            ProjectileManager.ThrowObjectRpc(throwable.ProjectileName, GetCaster().transform.position, GetTargetPoint().Value, throwable.Trajectory, throwable.ProjectileSpeed);
        }
    }
}

