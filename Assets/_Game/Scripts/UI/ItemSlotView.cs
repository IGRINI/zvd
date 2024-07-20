using Game.Items;
using UnityEngine;

public abstract class ItemSlotView : MonoBehaviour
{
    public RectTransform ItemObjectTransform => _itemObjectTransform;
    
    [SerializeField] protected RectTransform _itemObjectTransform;
    
    public ItemNetworkData ItemNetworkData { get; protected set; }
    
    public abstract void SetItem(ItemNetworkData itemNetworkData);
    
    public abstract void RemoveItem();
}
