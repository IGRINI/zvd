using Game.Common;
using Game.Controllers.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Game.Installers
{
    [CreateAssetMenu(fileName = "SettingsInstaller", menuName = "Installers/SettingsInstaller")]
    public class SettingsInstaller : ScriptableObjectInstaller<SettingsInstaller>
    {
        [FormerlySerializedAs("_bootstrapSettings")] [SerializeField] public GameBootstrapper.Settings BootstrapSettings;
        [FormerlySerializedAs("_playerMoveSettings")] [SerializeField] public PlayerMoveController.Settings PlayerMoveSettings;
        [FormerlySerializedAs("_mouseLookSettings")] [SerializeField] public MouseLookController.Settings MouseLookSettings;
        [FormerlySerializedAs("_unitsSettings")] [SerializeField] public NetworkInfoController.Settings UnitsSettings;
        [SerializeField] public MouseObjectDetectionController.Settings MouseObjectDetectionSettings;
        
        public override void InstallBindings()
        {
            Container.Bind<GameBootstrapper.Settings>().FromInstance(BootstrapSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
            Container.Bind<PlayerMoveController.Settings>().FromInstance(PlayerMoveSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
            Container.Bind<MouseLookController.Settings>().FromInstance(MouseLookSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
            Container.Bind<NetworkInfoController.Settings>().FromInstance(UnitsSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
            Container.Bind<MouseObjectDetectionController.Settings>().FromInstance(MouseObjectDetectionSettings).AsSingle().CopyIntoAllSubContainers().NonLazy();
        }
    }
}