using System;
using System.Collections.Generic;
using Game.Entities;
using Game.Items;
using Game.UI.Equipment;
using Game.Utils.PlayerCharInfo;
using Game.Views.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Object = UnityEngine.Object;

namespace Game.Controllers.Gameplay
{
    public class Network
    {
        public static Network Singleton { get; private set; }

        public readonly MouseLookController.Settings MouseLookSettings;
        public readonly PlayerMoveController.Settings MoveSettings;
        public readonly InteractionController.Settings InteractionSettings;
        public readonly Settings UnitsSettings;
        
        private readonly MouseLookController _mouseLookController;
        private readonly PlayerMoveController _playerMoveController;
        private readonly InteractionController _interactionController;
        
        private readonly PlayerStatsContainer _playerStatsContainer;
        private readonly PlayerInventoryContainer _playerInventoryContainer;
        private readonly EquipmentUiView _equipmentUiView;

        private readonly DroppedItemView _droppedItemPrefab;
        
        private readonly DiContainer Container;

        private PlayerView _playerView;

        public PlayerView PlayerView => _playerView;

        private Dictionary<ulong, PlayerView> _players = new();

        private Network(MouseLookController mouseLookController,
            PlayerMoveController playerMoveController,
            InteractionController interactionController,
            MouseLookController.Settings mouseLookSettings,
            PlayerMoveController.Settings moveSettings,
            InteractionController.Settings interactionSettings,
            Settings unitsSettings,
            PlayerStatsContainer playerStatsContainer,
            PlayerInventoryContainer playerInventoryContainer,
            DroppedItemView droppedItemPrefab,
            EquipmentUiView equipmentUiView,
            DiContainer container)
        {
            Singleton = this;

            _mouseLookController = mouseLookController;
            _interactionController = interactionController;
            _playerMoveController = playerMoveController;

            MouseLookSettings = mouseLookSettings;
            MoveSettings = moveSettings;
            InteractionSettings = interactionSettings;
            UnitsSettings = unitsSettings;

            _playerStatsContainer = playerStatsContainer;
            _playerInventoryContainer = playerInventoryContainer;
            _equipmentUiView = equipmentUiView;
            
            _droppedItemPrefab = droppedItemPrefab;
            
            Container = container;
        }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
        
        public void DespawnDroppedItem(DroppedItemView droppedItemView)
        {
            if(NetworkManager.Singleton.IsServer)
                droppedItemView.NetworkObject.Despawn();   
        }

        public void SpawnDroppedItem(Vector3 position, ItemModel itemModel)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var item = Object.Instantiate(_droppedItemPrefab, position, Quaternion.identity);
                item.SetItem(itemModel);
                item.NetworkObject.Spawn();
            }
        }

        public PlayerView GetPlayerById(ulong playerId)
        {
            return _players[playerId];
        }

        public void RegisterPlayer(PlayerView playerView, bool isOwner)
        {
            if (isOwner)
            {
                _mouseLookController.SetPlayerView(playerView);
                _playerMoveController.SetPlayerView(playerView);
                _interactionController.SetPlayerView(playerView);

                _playerStatsContainer.SetPlayer(playerView);
                _playerInventoryContainer.SetPlayerInventory(playerView.Inventory);
                _equipmentUiView.SetPlayerEquipment(playerView.Equipment);

                _playerView = playerView;
            }

            if (NetworkManager.Singleton.IsServer)
            {
                _players.Add(playerView.OwnerClientId, playerView);
            }
        }

        public void UnregisterPlayer(PlayerView playerView, bool isOwner)
        {
            if (isOwner)
            {
                _mouseLookController.SetPlayerMoveActive(false);
                _playerMoveController.SetPlayerMoveActive(false);
                
                _playerStatsContainer.UnregisterPlayer(playerView);
                _playerInventoryContainer.UnregisterInventory(playerView.Inventory);
                _equipmentUiView.UnregisterEquipment(playerView.Equipment);

                _playerView = null;
            }

            if (NetworkManager.Singleton.IsServer)
            {
                _players.Remove(playerView.OwnerClientId);
            }
        }
        
        public static bool IsFriendlyTeam(Team teamA, Team teamB)
        {
            return GetRelationType(teamA, teamB) == RelationType.Friendly;
        }

        public static bool IsNeutralTeam(Team teamA, Team teamB)
        {
            return GetRelationType(teamA, teamB) == RelationType.Neutral;
        }

        public static bool IsEnemyTeam(Team teamA, Team teamB)
        {
            return GetRelationType(teamA, teamB) == RelationType.Hostile;
        }

        public static int GetMaxLevel()
        {
            return Singleton.UnitsSettings.ExperienceTable.Count + 1;
        }

        public static RelationType GetRelationType(Team teamA, Team teamB)
        {
            if (teamA == teamB) return RelationType.Friendly;
            foreach (var relation in Singleton.UnitsSettings.TeamRelations)
            {
                if ((relation.TeamA == teamA && relation.TeamB == teamB) || (relation.TeamA == teamB && relation.TeamB == teamA))
                {
                    return relation.Relation;
                }
            }
            return RelationType.Neutral;
        }
        
        public int GetExperienceForLevel(int level)
        {
            if (level - 1 < UnitsSettings.ExperienceTable.Count)
            {
                return UnitsSettings.ExperienceTable[level - 1];
            }
            return UnitsSettings.ExperienceTable[^1];
        }
        
        [Serializable]
        public class Settings
        {
            public List<TeamRelation> TeamRelations;
            public List<HealthBarColor> HealthBarColors;
            public float ExpirienceRadius;
            public List<int> ExperienceTable;
            public float DamagePerStrength = .5f;
            
            [Serializable]
            public class TeamRelation
            {
                public Team TeamA;
                public Team TeamB;
                public RelationType Relation;
            }
            
            [Serializable]
            public class HealthBarColor
            {
                public RelationType RelationType;
                public Color HealthColor;
                public Color BgColor;
            }
        }
    }
    
    public enum RelationType
    {
        Friendly,
        Neutral,
        Hostile
    }
}