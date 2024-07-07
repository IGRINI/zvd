using Cinemachine;
using Game.Controllers.Gameplay;
using Game.PrefabsActions;
using Game.Utils.PlayerCharInfo;
using Game.Views.Player;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Game.Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;
        [SerializeField] private Camera _camera;
        [FormerlySerializedAs("_playerContainer")] [SerializeField] private PlayerStatsContainer playerStatsContainer;
        [SerializeField] private PlayerInventoryContainer _playerInventoryContainer;
        
        public override void InstallBindings()
        {
            // Container.BindInterfacesTo<OldChatUi>()
            //     .FromInstance(oldChatUi)
            //     .AsSingle()
            //     .NonLazy();


            BindSingle<PrefabCreator>();

            BindInstance(_cinemachineVirtualCamera);
            BindInstance(_camera);
            BindInstance(playerStatsContainer);
            BindInstance(_playerInventoryContainer);
            BindSingle<PlayerMoveController>();
            BindSingle<MouseLookController>();
            BindSingle<NetworkInfoController>();
            // BindSingle<InteractionController>();
            // BindSingle<HandsController>();
            //
            // Container.Bind<PlayerCreator>().AsSingle().NonLazy();
            //
            // Container.BindInterfacesAndSelfTo<CannonController>().AsSingle().NonLazy();
        }
        
        private IfNotBoundBinder BindSingle<T>()
        {
            return Container.BindInterfacesAndSelfTo<T>().AsSingle().NonLazy();
        }

        private IfNotBoundBinder BindInstance<T>(T instance)
        {
            return Container.Bind<T>().FromInstance(instance).AsSingle().NonLazy();
        }
    }
}