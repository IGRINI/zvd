using System;
using System.Collections.Generic;
using System.Linq;
using Game.PrefabsActions;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Game.Utils.Controllers
{
    public class ScreenController
    {
        public const string SCREENS_LIST = "SCREENS_LIST";

        private readonly DiContainer _container;
        private readonly List<Screen> _screenList;
        private readonly Canvas _canvas;

        public Screen Current { get; private set; }

        private ScreenController(
            DiContainer container,
            Canvas canvas,
            [Inject(Id = SCREENS_LIST)] List<Screen> screenList)
        {
            _container = container;
            _canvas = canvas;
            _screenList = screenList;
        }

        public void Show<T>(Action<DiContainer> contextDecorator = null) where T : Screen
        {
            var screen = _screenList.FirstOrDefault(x => x is T);
            if (screen != null)
            {
                var subContainer = _container.CreateSubContainer();
                contextDecorator?.Invoke(subContainer);
                var specificScreen = subContainer.InstantiatePrefabForComponent<T>(screen, _canvas.transform);
                specificScreen.Show();
                if(Current != null)
                    Object.Destroy(Current.gameObject);
                Current = specificScreen;
            }
            else
            {
                throw new ArgumentException($"Not found screen of type {typeof(T)}");
            }
        }
        
    }
}