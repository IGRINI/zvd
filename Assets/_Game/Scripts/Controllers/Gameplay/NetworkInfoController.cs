using System;
using System.Collections.Generic;
using Game.Entities;
using Game.Utils.PlayerCharInfo;
using Game.Views.Player;
using UnityEngine.Serialization;

namespace Game.Controllers.Gameplay
{
    public class NetworkInfoController
    {
        public static NetworkInfoController Singleton { get; private set; }

        public readonly MouseLookController.Settings MouseLookSettings;
        public readonly PlayerMoveController.Settings MoveSettings;
        public readonly Settings UnitsSettings;
        
        private readonly MouseLookController _mouseLookController;
        private readonly PlayerMoveController _playerMoveController;
        
        private readonly PlayerStatsContainer _playerStatsContainer;

        private NetworkInfoController(MouseLookController mouseLookController,
            PlayerMoveController playerMoveController,
            MouseLookController.Settings mouseLookSettings,
            PlayerMoveController.Settings moveSettings,
            Settings unitsSettings,
            PlayerStatsContainer playerStatsContainer)
        {
            Singleton = this;

            _mouseLookController = mouseLookController;
            _playerMoveController = playerMoveController;

            MouseLookSettings = mouseLookSettings;
            MoveSettings = moveSettings;
            UnitsSettings = unitsSettings;

            _playerStatsContainer = playerStatsContainer;
        }

        public void RegisterPlayer(PlayerView playerView, bool isOwner)
        {
            if (isOwner)
            {
                _mouseLookController.SetPlayerView(playerView);
                _playerMoveController.SetPlayerView(playerView);

                _playerStatsContainer.SetPlayer(playerView);
            }
        }

        public void UnregisterPlayer(PlayerView playerView, bool isOwner)
        {
            if (isOwner)
            {
                _mouseLookController.SetPlayerMoveActive(false);
                _playerMoveController.SetPlayerMoveActive(false);
                
                _playerStatsContainer.UnregisterPlayer(playerView);
            }
        }
        
        public bool IsFriendlyTeam(Team teamA, Team teamB)
        {
            return GetRelationType(teamA, teamB) == RelationType.Friendly;
        }

        public bool IsNeutralTeam(Team teamA, Team teamB)
        {
            return GetRelationType(teamA, teamB) == RelationType.Neutral;
        }

        public bool IsEnemyTeam(Team teamA, Team teamB)
        {
            return GetRelationType(teamA, teamB) == RelationType.Hostile;
        }

        private RelationType GetRelationType(Team teamA, Team teamB)
        {
            foreach (var relation in UnitsSettings.TeamRelations)
            {
                if ((relation.TeamA == teamA && relation.TeamB == teamB) || (relation.TeamA == teamB && relation.TeamB == teamA))
                {
                    return relation.Relation;
                }
            }
            return RelationType.Neutral;
        }
        
        [Serializable]
        public class Settings
        {
            public List<TeamRelation> TeamRelations;
            
            [Serializable]
            public class TeamRelation
            {
                public Team TeamA;
                public Team TeamB;
                public RelationType Relation;
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