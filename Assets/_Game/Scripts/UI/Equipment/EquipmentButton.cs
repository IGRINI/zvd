using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.UI.Equipment
{
    public class EquipmentButton : MonoBehaviour
    {
        [Inject] private readonly EquipmentUiView _equipmentUiView;
        
        [SerializeField] private Button _button;

        private bool _opened = true;
        
        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void Start()
        {
            OnClick();
        }

        private void OnClick()
        {
            if(_opened)
                _equipmentUiView.Close();
            else
                _equipmentUiView.Open();
            _opened = !_opened;
        }
    }
}