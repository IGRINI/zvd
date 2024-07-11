using Game.Interactables;
using Unity.Netcode;

public class DroppedItemView : NetworkBehaviour, IHoverable, IInteractable
{
    public NetworkVariable<ItemModel> Item => _item;
    
    private readonly NetworkVariable<ItemModel> _item = new();

    
    private OutlineHandler _outlineHandler;

    OutlineHandler IHoverable.OutlineHandler
    {
        get => _outlineHandler;
        set => _outlineHandler = value;
    }

    private void Awake()
    {
        _outlineHandler = GetComponent<OutlineHandler>();
    }

    public void Interact()
    {
        
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            
        }
        if (IsClient)
        {
            
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        if (IsServer)
        {
            
        }
        if (IsClient)
        {
            
        }
        
    }
}
