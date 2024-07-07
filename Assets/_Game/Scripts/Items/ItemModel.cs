using UnityEngine;

public class ItemModel
{
    public string Name { get; private set; }
    public bool CanDrop { get; private set; }
    
    public Sprite ItemSprite { get; private set; }
    public GameObject ItemPrefab { get; private set; }

    public ItemModel(string name, bool canDrop, Sprite itemSprite, GameObject itemPrefab)
    {
        Name = name;
        CanDrop = canDrop;
        ItemSprite = itemSprite;
        ItemPrefab = itemPrefab;
    }
}
