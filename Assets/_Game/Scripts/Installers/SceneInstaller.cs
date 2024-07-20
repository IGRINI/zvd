using Cinemachine;
using Game.Controllers.Gameplay;
using Game.Entities;
using Game.Entities.Modifiers;
using Game.PrefabsActions;
using Game.Utils.HealthBars;
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
        [SerializeField] private HealthBar _healthBarPrefab;
        [SerializeField] private RectTransform _healthBarRoot;
        [SerializeField] private DroppedItemView _droppedItemViewPrefab;
        [SerializeField] private Transform _deactivatedDroppedItems;
        [SerializeField] private Canvas _canvas;
        
        
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
            BindInstance(_canvas);
            BindInstance(_healthBarRoot);
            BindSingle<PlayerMoveController>();
            BindSingle<MouseLookController>();
            BindSingle<NetworkInfoController>();
            BindSingle<HealthBarController>();
            BindSingle<MouseObjectDetectionController>();
            BindSingle<InteractionController>();
            BindSingle<ModifiersManager>();
            // BindSingle<HandsController>();
            //
            // Container.Bind<PlayerCreator>().AsSingle().NonLazy();
            //
            // Container.BindInterfacesAndSelfTo<CannonController>().AsSingle().NonLazy();

            Container.BindMemoryPool<HealthBar, HealthBar.Pool>()
                .FromComponentInNewPrefab(_healthBarPrefab)
                .UnderTransform(_healthBarRoot);
            
            Container.BindMemoryPool<DroppedItemView, DroppedItemView.Pool>()
                .FromComponentInNewPrefab(_droppedItemViewPrefab)
                .UnderTransform(_deactivatedDroppedItems);

            EntityRegistry.Init(_camera);
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