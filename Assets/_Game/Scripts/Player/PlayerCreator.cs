using Game.Common;
using Game.PrefabsActions;
using Game.Views.Player;
using UnityEngine;
using Zenject;

namespace Game.Player
{
    public class PlayerCreator
    {
        private readonly PlayerView _playerViewPrefab;
        private readonly PrefabCreator _prefabCreator;
        private readonly SignalBus _signalBus;
        
        private PlayerCreator(PlayerView playerViewPrefab, PrefabCreator prefabCreator, SignalBus signalBus)
        {
            _playerViewPrefab = playerViewPrefab;
            _prefabCreator = prefabCreator;
            _signalBus = signalBus;
            
            _signalBus.Subscribe<GameSignals.PlayerSpawnRequest>(CreatePlayerForRequest);
        }

        private void CreatePlayerForRequest(GameSignals.PlayerSpawnRequest playerSpawnRequest)
        {
            var player = _prefabCreator.Create<PlayerView>(_playerViewPrefab);
            player.transform.position = playerSpawnRequest.Position;
            
            _signalBus.Fire(new GameSignals.PlayerSpawned() { Player = player });
            _signalBus.Fire(new GameSignals.PlayerMoveActive() { IsActive = true });
            _signalBus.Fire(new GameSignals.PlayerInteractiveActive() { IsActive = true });
        }
    }
}