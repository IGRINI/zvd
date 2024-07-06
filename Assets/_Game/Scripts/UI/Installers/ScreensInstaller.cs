using System.Collections.Generic;
using Game.Utils.Controllers;
using UnityEngine;
using Zenject;

namespace Game.Utils.Installers
{
    public class ScreensInstaller : MonoInstaller
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private List<Screen> _screensList;
        
        
        public override void InstallBindings()
        {
            BindController<ScreenController>();

            Container.BindInstance(_canvas)
                .AsSingle()
                .NonLazy();
            Container.BindInstance(_screensList)
                .WithId(ScreenController.SCREENS_LIST)
                .WhenInjectedInto<ScreenController>()
                .NonLazy();
        }

        private IfNotBoundBinder BindController<T>()
        {
            return Container.BindInterfacesAndSelfTo<T>().AsSingle().NonLazy();
        }
    }
}