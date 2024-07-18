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
        private EAbilityBehaviour _abilityBehaviour;

        public string Name => _name;
        public bool Droppable => _droppable;
        public string ItemSpriteName => _itemSpriteName;
        public EAbilityBehaviour AbilityBehaviour => _abilityBehaviour;

        public ItemNetworkData() : this("ERROR_ITEM")
        {
        }

        public ItemNetworkData(string name, bool droppable = true, string itemSpriteName = "",
            EAbilityBehaviour abilityBehaviour = EAbilityBehaviour.Passive)
        {
            _name = name;
            _droppable = droppable;
            _itemSpriteName = itemSpriteName.IsNullOrWhitespace() ? name : itemSpriteName;
            _abilityBehaviour = abilityBehaviour;
            _abilityBehaviour |= EAbilityBehaviour.Item;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _name);
            serializer.SerializeValue(ref _droppable);
            serializer.SerializeValue(ref _itemSpriteName);
            serializer.SerializeValue(ref _abilityBehaviour);
        }
    }
}