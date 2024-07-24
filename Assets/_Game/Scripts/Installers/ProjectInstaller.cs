using Game.Controllers;
using Game.Controllers.Gameplay;
using Game.PrefabsActions;
using Game.Services;
using UnityEngine;
using Zenject;
using Game.Views.Player;
using TMPro;
using UnityEngine.InputSystem;

namespace Game.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private InputActionAsset _inputAsset;
        [SerializeField] private Texture2D _normal;
        [SerializeField] private Texture2D _enemy;
        [SerializeField] private Texture2D _neutral;
        [SerializeField] private Texture2D _friendly;
        [SerializeField] private Texture2D _normalTarget;
        [SerializeField] private Texture2D _enemyTarget;
        [SerializeField] private Texture2D _friendlyTarget;
        [SerializeField] private Texture2D _neutralTarget;
        
        public override void InstallBindings()
        {
            // Container.BindInterfacesAndSelfTo<PrefabCreator>().AsSingle().MoveIntoAllSubContainers().NonLazy();
            
            SignalBusInstaller.Install(Container);
            
            _inputAsset.Enable();
            BindInstance(_inputAsset);

            BindSingle<MouseController>();
            BindSingle<KeyboardController>();

            Container.BindInterfacesAndSelfTo<SteamService>().AsSingle().MoveIntoAllSubContainers().NonLazy();
            
            
            Container.BindInstance(_normal).WithId(ECursorId.Normal);
            
            Container.BindInstance(_neutral).WithId(ECursorId.Neutral);
            
            Container.BindInstance(_friendly).WithId(ECursorId.Friendly);
            
            Container.BindInstance(_enemy).WithId(ECursorId.Enemy);
            
            Container.BindInstance(_normalTarget).WithId(ECursorId.NormalTarget);
            
            Container.BindInstance(_neutralTarget).WithId(ECursorId.NeutralTarget);
            
            Container.BindInstance(_friendlyTarget).WithId(ECursorId.FriendlyTarget);
            
            Container.BindInstance(_enemyTarget).WithId(ECursorId.EnemyTarget);
            
            BindSingle<CursorController>();
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