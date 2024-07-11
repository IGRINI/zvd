public class SlotModel
{
    public ItemModel ItemModel { get; private set; }

    public void SetItem(ItemModel itemModel)
    {
        ItemModel = itemModel;
    }
}
