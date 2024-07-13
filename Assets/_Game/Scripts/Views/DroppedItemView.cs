using Game.Interactables;
using Unity.Netcode;
using UnityEngine;

public class DroppedItemView : NetworkBehaviour, IHoverable, IInteractable
{
    OutlineHandler IHoverable.OutlineHandler
    {
        get => _outlineHandler;
        set => _outlineHandler = value;
    }

    bool IHoverable.IsActive
    {
        get => _isOutlineActive;
        set => _isOutlineActive = value;
    }
    
    public NetworkVariable<ItemModel> Item => _item;
    private readonly NetworkVariable<ItemModel> _item = new();

    
    private OutlineHandler _outlineHandler;
    private bool _isOutlineActive;

    private void Awake()
    {
        _outlineHandler = GetComponent<OutlineHandler>();
    }

    public void Interact()
    {
        
    }
    
    public void InitializeItem(ItemModel itemModel)
    {
        _item.Value = itemModel;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        //TODO Test
        InitializeItem(new ItemModel()
        {
            Droppable =  true,
            Name = "TEST ITEM",
            ItemSpriteLink = "/"
        });
        
        if (IsServer)
        {
            
        }
        if (IsClient)
        {
            _isOutlineActive = true;
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
            _isOutlineActive = false;
            _outlineHandler.DisableOutline();
        }
        
    }
}
