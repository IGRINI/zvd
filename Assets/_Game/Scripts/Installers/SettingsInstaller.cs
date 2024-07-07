using Game.Common;
using Game.Controllers.Gameplay;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    [CreateAssetMenu(fileName = "SettingsInstaller", menuName = "Installers/SettingsInstaller")]
    public class SettingsInstaller : ScriptableObjectInstaller<SettingsInstaller>
    {
        [SerializeField] private GameBootstrapper.Settings _bootstrapSettings;
        [SerializeField] private InteractionController.Settings _interactiveSettings;
        [SerializeField] private PlayerMoveController.Settings _playerMoveSettings;
        [SerializeField] private MouseLookController.Settings _mouseLookSettings;
        [SerializeField] private NetworkInfoController.Settings _unitsSettings;
        [SerializeField] private HandsController.Settings _handsSettings; 
        
        public override void InstallBindings()
        {
            Container.Bind<GameBootstrapper.Settings>().FromInstance(_bootstrapSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
            Container.Bind<InteractionController.Settings>().FromInstance(_interactiveSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
            Container.Bind<PlayerMoveController.Settings>().FromInstance(_playerMoveSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
            Container.Bind<MouseLookController.Settings>().FromInstance(_mouseLookSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
            Container.Bind<NetworkInfoController.Settings>().FromInstance(_unitsSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
            Container.Bind<HandsController.Settings>().FromInstance(_handsSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
        }
    }
}