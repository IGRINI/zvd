using System;
using Game.Abilities;
using Sirenix.Utilities;
using Unity.Netcode;

namespace Game.Items
{
    [Serializable]
    public class ItemNetworkData : INetworkSerializable
    {
        private string _name;
        private bool _droppable;
        private string _itemSpriteName;
        private int _charges;
        private int _maxCharges;
        private bool _isStackable;
        private bool _isConsumable;
        private bool _hasCharges;
        private EAbilityBehaviour _abilityBehaviour;

        public string Name => _name;
        public bool Droppable => _droppable;
        public string ItemSpriteName => _itemSpriteName;
        
        public int Charges => _charges;
        public int MaxCharges => _maxCharges;
        public bool HasCharges => _hasCharges;
        public bool IsConsumable => _isConsumable;
        public bool IsStackable => _isStackable;
        
        public EAbilityBehaviour AbilityBehaviour => _abilityBehaviour;

        public EItemChargeResult SpendCharge()
        {
            if (_isConsumable && !HasCharges)
            {
                return EItemChargeResult.ItemBroken;
            }

            if (!HasCharges || _charges <= 0)
            {
                return EItemChargeResult.NoChargesLeft;
            }

            _charges--;

            if (_charges == 0 && _isConsumable)
            {
                return EItemChargeResult.ItemBroken;
            }

            return EItemChargeResult.ChargeUsed;
        }

        
        public void UpdateCharges(int charges)
        {
            _charges = charges;
        }
        

        public ItemNetworkData() : this("ERROR_ITEM")
        {
        }

        public ItemNetworkData(string name, bool droppable = true, string itemSpriteName = "",
            EAbilityBehaviour abilityBehaviour = EAbilityBehaviour.Passive,
            bool hasCharges = false, bool isConsumable = false, bool isStackable = false, int charges = -1, int maxCharges = -1)
        {
            _name = name;
            _droppable = droppable;
            _itemSpriteName = itemSpriteName.IsNullOrWhitespace() ? name : itemSpriteName;
            _abilityBehaviour = abilityBehaviour;
            _abilityBehaviour |= EAbilityBehaviour.Item;

            _hasCharges = hasCharges;
            _isConsumable = isConsumable;
            _isStackable = isStackable;
            _charges = charges;
            _maxCharges = maxCharges;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _name);
            serializer.SerializeValue(ref _droppable);
            serializer.SerializeValue(ref _itemSpriteName);
            serializer.SerializeValue(ref _abilityBehaviour);
            serializer.SerializeValue(ref _hasCharges);
            serializer.SerializeValue(ref _charges);
            serializer.SerializeValue(ref _maxCharges);
        }
    }
}