using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InventoryItem : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Image _itemBackground;
    [SerializeField] private Image _itemImage;

    [Inject]
    private void Constructor()
    {
        
    }
}
