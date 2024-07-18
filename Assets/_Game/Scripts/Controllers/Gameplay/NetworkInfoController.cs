using System;
using System.Collections.Generic;
using Game.Entities;
using Game.Utils.PlayerCharInfo;
using Game.Views.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Controllers.Gameplay
{
    public class NetworkInfoController
    {
        public static NetworkInfoController Singleton { get; private set; }

        public readonly MouseLookController.Settings MouseLookSettings;
        public readonly PlayerMoveController.Settings MoveSettings;
        public readonly InteractionController.Settings InteractionSettings;
        public readonly Settings UnitsSettings;
        
        private readonly MouseLookController _mouseLookController;
        private readonly PlayerMoveController _playerMoveController;
        private readonly InteractionController _interactionController;
        
        private readonly PlayerStatsContainer _playerStatsContainer;
        private readonly PlayerInventoryContainer _playerInventoryContainer;

        private PlayerView _playerView;

        public PlayerView PlayerView => _playerView;

        private Dictionary<ulong, PlayerView> _players = new();

        private NetworkInfoController(MouseLookController mouseLookController,
            PlayerMoveController playerMoveController,
            InteractionController interactionController,
            MouseLookController.Settings mouseLookSettings,
            PlayerMoveController.Settings moveSettings,
            InteractionController.Settings interactionSettings,
            Settings unitsSettings,
            PlayerStatsContainer playerStatsContainer,
            PlayerInventoryContainer playerInventoryContainer)
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
        }

        public PlayerView GetPlayerById(ulong playerId)
        {
            return _players[playerId];
        }

        public void RegisterInventory(EntityInventory entityInventory, bool isOwner)
        {
            if (isOwner)
            {
                _playerInventoryContainer.SetPlayerInventory(entityInventory);
            }
        }

        public void UnregisterInventory(EntityInventory entityInventory, bool isOwner)
        {
            if (isOwner)
            {
                _playerInventoryContainer.UnregisterInventory(entityInventory);
            }
        }

        public void RegisterPlayer(PlayerView playerView, bool isOwner)
        {
            if (isOwner)
            {
                _mouseLookController.SetPlayerView(playerView);
                _playerMoveController.SetPlayerView(playerView);
                _interactionController.SetPlayerView(playerView);

                _playerStatsContainer.SetPlayer(playerView);

                _playerView = playerView;
            }

            if (NetworkManager.Singleton.IsServer)
            {
                _players.Add(playerView.OwnerClientId, _playerView);
            }
        }

        public void UnregisterPlayer(PlayerView playerView, bool isOwner)
        {
            if (isOwner)
            {
                _mouseLookController.SetPlayerMoveActive(false);
                _playerMoveController.SetPlayerMoveActive(false);
                
                _playerStatsContainer.UnregisterPlayer(playerView);

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