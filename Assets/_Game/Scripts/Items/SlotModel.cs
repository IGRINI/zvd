public class SlotModel
{
    public ItemModel Item { get; private set; }

    public void SetItem(ItemModel item)
    {
        Item = item;
    }
}
