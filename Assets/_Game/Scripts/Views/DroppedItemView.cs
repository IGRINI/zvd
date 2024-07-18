using Game.Interactables;
using Game.Items;
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

    NetworkVariable<bool> IInteractable.CanInteract => _canInteract;
    
    private readonly NetworkVariable<bool> _canInteract = new();
    
    public NetworkVariable<ItemNetworkData> ItemNetworkData => _itemNetworkData;
    private readonly NetworkVariable<ItemNetworkData> _itemNetworkData = new();

    public ItemModel Item => _item;
    private ItemModel _item;
    
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
    
    public void SetItem(ItemModel item)
    {
        _item = item;
        _itemNetworkData.Value = _item.NetworkData;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _canInteract.Value = true;
        
        if (IsServer)
        {
            //TODO Test
            SetItem(ItemDatabase.CreateItemInstance("Healing Potion"));
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
