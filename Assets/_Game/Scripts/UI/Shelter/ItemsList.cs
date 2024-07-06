using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ItemsList : MonoBehaviour
{
   [SerializeField] private RectTransform _content;

   private float _cellSize = 45f;

   private Vector2 _inventorySize;

   private void Awake()
   {
      
   }

   public void Initialize(Vector2 inventorySize)
   {
      _content.sizeDelta = new Vector2(inventorySize.x * _cellSize -1f, inventorySize.y * _cellSize - 1f);
   }
}
