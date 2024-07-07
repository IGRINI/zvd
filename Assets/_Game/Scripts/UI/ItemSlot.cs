using UnityEngine;

public abstract class ItemSlot : MonoBehaviour
{
    public ItemModel ItemModel { get; protected set; }
    
    public abstract void SetItem(ItemModel item);
    
    public abstract void RemoveItem();
}
