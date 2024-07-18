using Game.Items;
using UnityEngine;

public abstract class ItemSlotView : MonoBehaviour
{
    public ItemNetworkData ItemNetworkData { get; protected set; }
    
    public abstract void SetItem(ItemNetworkData itemNetworkData);
    
    public abstract void RemoveItem();
}
