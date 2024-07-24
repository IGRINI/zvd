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
        [SerializeField] private Texture2D _target;
        [SerializeField] private Texture2D _enemyTarget;
        
        public override void InstallBindings()
        {
            // Container.BindInterfacesAndSelfTo<PrefabCreator>().AsSingle().MoveIntoAllSubContainers().NonLazy();
            
            SignalBusInstaller.Install(Container);
            
            _inputAsset.Enable();
            BindInstance(_inputAsset);

            BindSingle<MouseController>();
            BindSingle<KeyboardController>();

            Container.BindInterfacesAndSelfTo<SteamService>().AsSingle().MoveIntoAllSubContainers().NonLazy();
            
            
            Container.BindInstance(_normal)
                .WithId(ECursorType.Normal);
            Container.BindInstance(_target)
                .WithId(ECursorType.Target);
            Container.BindInstance(_enemyTarget)
                .WithId(ECursorType.EnemyTarget);

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