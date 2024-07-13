namespace Game.Interactables
{
    public interface IHoverable
    {
        public OutlineHandler OutlineHandler { get; protected set; }
        
        public bool IsActive { get; protected set; }
        
        public void OnHoverStart()
        {
            
        }
        
        public void OnHoverStop()
        {
            
        }
    }
}