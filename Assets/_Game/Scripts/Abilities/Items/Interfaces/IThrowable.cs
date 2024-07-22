namespace Game.Abilities.Items.Interfaces
{
    public interface IThrowable
    {
        public string ProjectileName { get; }
        public float ProjectileSpeed { get; }
        
        public ProjectileTrajectory Trajectory { get; }
    }
    
    public enum ProjectileTrajectory
    {
        Linear,
        Ballistic
    }
}