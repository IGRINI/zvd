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
    
    NetworkVariable<bool> IInteractable.CanInteract
    {
        get => _canInteract;
    }
    private readonly NetworkVariable<bool> _canInteract = new();
    
    public NetworkVariable<ItemModel> Item => _item;
    private readonly NetworkVariable<ItemModel> _item = new();

    
    private OutlineHandler _outlineHandler;
    private bool _isOutlineActive;
    
    
    private void Awake()
    {
        _outlineHandler = GetComponent<OutlineHandler>();
    }

    public void OnBeforeNetworkInteract()
    {
        
    }

    public void OnSuccessfulInteract()
    {
        if (IsServer)
        {
            NetworkObject.Despawn();
            Destroy(gameObject);
        }
    }
    
    public void InitializeItem(ItemModel itemModel)
    {
        _item.Value = itemModel;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _canInteract.Value = true;
        
        if (IsServer)
        {
            //TODO Test
            InitializeItem(new ItemModel()
            {
                Droppable =  true,
                Name = "TEST ITEM",
                ItemSpriteLink = "/"
            });
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
