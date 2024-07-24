using Cinemachine;
using Game.Controllers.Gameplay;
using Game.Entities;
using Game.Entities.Modifiers;
using Game.PrefabsActions;
using Game.UI.Equipment;
using Game.UI.HealthBars;
using Game.Utils.PlayerCharInfo;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;
        [SerializeField] private Camera _camera;
        [SerializeField] private PlayerStatsContainer _playerStatsContainer;
        [SerializeField] private PlayerInventoryContainer _playerInventoryContainer;
        [SerializeField] private EquipmentUiView _equipmentUiView;
        [SerializeField] private HealthBar _healthBarPrefab;
        [SerializeField] private RectTransform _healthBarRoot;
        [SerializeField] private DroppedItemView _droppedItemViewPrefab;
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
            BindInstance(_playerStatsContainer);
            BindInstance(_playerInventoryContainer);
            BindInstance(_equipmentUiView);
            BindInstance(_canvas);
            BindInstance(_healthBarRoot);
            BindInstance(_droppedItemViewPrefab);
            
            BindSingle<PlayerMoveController>();
            BindSingle<MouseLookController>();
            BindSingle<Network>();
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

            EntityRegistry.Init(_camera);
        }
        
        private IfNotBoundBinder BindSingle<T>()
        {
            return Container.BindInterfacesAndSelfTo<T>().AsSingle().NonLazy();
        }

        private IfNotBoundBinder BindInstance<T>(T instance)
        {
            return Container.BindInstance(instance).AsSingle().NonLazy();
        }
    }
}