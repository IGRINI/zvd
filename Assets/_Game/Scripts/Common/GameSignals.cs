using Game.Interactables;
using Game.Views.Player;
using UnityEngine;

namespace Game.Common
{
    public static class GameSignals
    {
        public class PlayerSpawnRequest
        {
            public Vector3 Position;
        }
        public class PlayerSpawned
        {
            public PlayerView Player;
        }
        public class PlayerMoveActive
        {
            public bool IsActive;
        }
        public class PlayerInteractiveActive
        {
            public bool IsActive;
        }
    }
}