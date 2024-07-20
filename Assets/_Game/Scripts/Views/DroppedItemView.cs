using Game.Controllers.Gameplay;
using Game.Interactables;
using Game.Items;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class DroppedItemView : NetworkBehaviour, IHoverable, IInteractable, IPoolable
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
            NetworkInfoController.Singleton.DespawnDroppedItem(this);
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
        
        if (IsClient)
        {
            _isOutlineActive = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        
        if (IsClient)
        {
            _isOutlineActive = false;
            _outlineHandler.DisableOutline();
        }
        
    }

    public void OnDespawned()
    {
        NetworkObject.Despawn();   
    }

    public void OnSpawned()
    {
        NetworkObject.Spawn();
    }
    
    public class Pool : MonoMemoryPool<Transform, Vector3, ItemModel, DroppedItemView>
    {
        protected override void Reinitialize(Transform parent, Vector3 position, ItemModel data, DroppedItemView item)
        {
            item.transform.SetParent(parent);
            item.transform.position = position;
            item.SetItem(data);
        }
    }
}
