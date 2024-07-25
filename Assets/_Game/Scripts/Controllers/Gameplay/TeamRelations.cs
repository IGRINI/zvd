using System;
using System.Collections.Generic;
using Game.Utils;
using Unity.Netcode;
using Zenject;

namespace Game.Controllers.Gameplay
{
    public class TeamRelations : NetworkBehaviour
    {
        private static TeamRelations _singleton;
        
        private NetworkVariable<NetworkRelations> _teamRelations = new();

        public static event Action TeamRelationsSpawned;

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            _teamRelations.Dispose();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _singleton = this;

            if (IsServer)
            {
                _teamRelations.Value = new NetworkRelations();

                foreach (var settingsTeamRelation in Network.Singleton.UnitsSettings.TeamRelations)
                {
                    _teamRelations.Value.TeamRelations.Add(new Network.Settings.TeamRelation()
                    {
                        Relation = settingsTeamRelation.Relation,
                        TeamA = settingsTeamRelation.TeamA,
                        TeamB = settingsTeamRelation.TeamB
                    });
                }
            }
            
            TeamRelationsSpawned?.Invoke();
        }

        public static NetworkVariable<NetworkRelations> GetTeamRelations()
        {
            return _singleton._teamRelations;
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

        public static RelationType GetRelationType(Team teamA, Team teamB)
        {
            if (teamA == teamB) return RelationType.Friendly;
            foreach (var relation in _singleton._teamRelations.Value.TeamRelations)
            {
                if ((relation.TeamA == teamA && relation.TeamB == teamB) || (relation.TeamA == teamB && relation.TeamB == teamA))
                {
                    return relation.Relation;
                }
            }
            return RelationType.Neutral;
        }

        public static void UpdateRelationType(Team teamA, Team teamB, RelationType relationType)
        {
            Network.Settings.TeamRelation teamRelation = null;
            foreach (var relation in _singleton._teamRelations.Value.TeamRelations)
            {
                if ((relation.TeamA == teamA && relation.TeamB == teamB) || (relation.TeamA == teamB && relation.TeamB == teamA))
                {
                    teamRelation = relation;
                    break;
                }
            }

            if (teamRelation == null)
            {
                _singleton._teamRelations.Value.TeamRelations.Add(new Network.Settings.TeamRelation()
                {
                    Relation = relationType,
                    TeamA = teamA,
                    TeamB = teamB
                });
            }
            else
            {
                teamRelation.Relation = relationType;
            }

            _singleton._teamRelations.ForceUpdate();
        }

        public class NetworkRelations : INetworkSerializable
        {
            public List<Network.Settings.TeamRelation> TeamRelations = new();
            
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                int count = TeamRelations.Count;
                serializer.SerializeValue(ref count);

                if (serializer.IsReader)
                {
                    TeamRelations.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        var relation = new Network.Settings.TeamRelation();
                        serializer.SerializeValue(ref relation);
                        TeamRelations.Add(relation);
                    }
                }
                else
                {
                    foreach (var relation in TeamRelations)
                    {
                        var tempRelation = relation;
                        serializer.SerializeValue(ref tempRelation);
                    }
                }
            }
        }
    }
    
    public enum Team : int
    {
        Villagers = 0,
        Zombies = 1,
        Animals = 2,
        Guards = 3,
        Robbers = 4 
    }
}