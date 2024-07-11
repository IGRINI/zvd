using UnityEngine;

public class ItemModel
{
    public string Name { get; private set; }
    
    public bool Droppable { get; private set; }
    
    public Sprite ItemSprite { get; private set; }
    public GameObject ItemPrefab { get; private set; }

    public ItemModel(string name, bool droppable, Sprite itemSprite, GameObject itemPrefab)
    {
        Name = name;
        Droppable = droppable;
        ItemSprite = itemSprite;
        ItemPrefab = itemPrefab;
    }
}
