using Game.Utils.PlayerCharInfo;
using Game.Views.Player;

namespace Game.Controllers.Gameplay
{
    public class NetworkInfoController
    {
        public static NetworkInfoController Singleton { get; private set; }

        public readonly MouseLookController.Settings MouseLookSettings;
        public readonly PlayerMoveController.Settings MoveSettings;
        
        private readonly MouseLookController _mouseLookController;
        private readonly PlayerMoveController _playerMoveController;
        
        private readonly PlayerStatsContainer _playerStatsContainer;

        private NetworkInfoController(MouseLookController mouseLookController,
            PlayerMoveController playerMoveController,
            MouseLookController.Settings mouseLookSettings,
            PlayerMoveController.Settings moveSettings,
            PlayerStatsContainer playerStatsContainer)
        {
            Singleton = this;

            _mouseLookController = mouseLookController;
            _playerMoveController = playerMoveController;

            MouseLookSettings = mouseLookSettings;
            MoveSettings = moveSettings;

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
    }
}