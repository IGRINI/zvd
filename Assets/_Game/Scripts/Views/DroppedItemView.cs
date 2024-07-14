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

    bool IHoverable.CanHover
    {
        get => _isOutlineActive;
        set => _isOutlineActive = value;
    }
    
    bool IInteractable.CanInteract
    {
        get => _canInteract;
        set => _canInteract = value;
    }
    
    public NetworkVariable<ItemModel> Item => _item;
    private readonly NetworkVariable<ItemModel> _item = new();

    
    private OutlineHandler _outlineHandler;
    private bool _isOutlineActive;
    private bool _canInteract;

    private void Awake()
    {
        _outlineHandler = GetComponent<OutlineHandler>();
    }

    public void OnBeforeNetworkInteract()
    {
        _canInteract = false;
    }

    public void OnSuccessfulInteract()
    {
        NetworkObject.Despawn();
        Destroy(gameObject);
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
