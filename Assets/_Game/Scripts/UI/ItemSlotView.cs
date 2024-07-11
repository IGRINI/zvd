using UnityEngine;

public abstract class ItemSlotView : MonoBehaviour
{
    public ItemModel ItemModel { get; protected set; }
    
    public abstract void SetItem(ItemModel itemModel);
    
    public abstract void RemoveItem();
}
