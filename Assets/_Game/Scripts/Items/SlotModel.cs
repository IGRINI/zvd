using Unity.Netcode;

public class SlotModel
{
    public NetworkVariable<ItemModel> Item => _item;
    private readonly NetworkVariable<ItemModel> _item = new();

    public void SetItem(ItemModel itemModel)
    {
        _item.Value = itemModel;
    }
}
