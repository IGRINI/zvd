using System.Configuration;
using DG.Tweening;
using UnityEngine;

namespace Game.Utils.Common
{
    public class SelectedLight : MonoBehaviour
    {
        [SerializeField] private RectTransform _transform;
        [SerializeField] private RectTransform _border;

        [Header("Settings")] 
        [SerializeField] private float _additionalSize = 20f;
        
        public void SelectElement(RectTransform element)
        {
            _transform.DOMoveX(element.position.x, .2f);
            _border.DOSizeDelta(new Vector2(element.sizeDelta.x + _additionalSize, _border.sizeDelta.y), .2f);
        }
    }
}