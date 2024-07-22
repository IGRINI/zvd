using Game.Controllers.Gameplay;
using Game.Items.Equipment;
using Game.UI.Inventory;
using UnityEngine;

namespace Game.UI.Equipment
{

    public class EquipmentSlotView : InventorySlotView
    {
        [SerializeField] private EquipmentSlotType _slotType;

        public EquipmentSlotType SlotType => _slotType;

        protected override void OnItemClick()
        {
            Network.Singleton.PlayerView.Equipment.UnequipRequestRpc(SlotType);
        }

        private void OnValidate()
        {
            gameObject.name = SlotType.ToString();
        }
    }

}