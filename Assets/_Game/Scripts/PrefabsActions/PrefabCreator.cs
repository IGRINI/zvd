using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Game.PrefabsActions
{
    public class PrefabCreator
    {
        private readonly DiContainer _container;

        private PrefabCreator(DiContainer container)
        {
            _container = container;
        }
        
        public GameObject Create(Object prefab, Action<DiContainer> contextDecorator = null)
        {
            var subContainer = _container;
            if (contextDecorator != null)
            {
                subContainer = _container.CreateSubContainer();
                contextDecorator?.Invoke(subContainer);
            }
            return subContainer.InstantiatePrefab(prefab);
        }
        
        public GameObject Create(Object prefab, Transform parent, Action<DiContainer> contextDecorator = null)
        {
            var subContainer = _container;
            if (contextDecorator != null)
            {
                subContainer = _container.CreateSubContainer();
                contextDecorator?.Invoke(subContainer);
            }
            return subContainer.InstantiatePrefab(prefab, parent);
        }

        public T Create<T>(Object prefab, Action<DiContainer> contextDecorator = null)
        {
            var subContainer = _container;
            if (contextDecorator != null)
            {
                subContainer = _container.CreateSubContainer();
                contextDecorator?.Invoke(subContainer);
            }
            return subContainer.InstantiatePrefabForComponent<T>(prefab);
        }

        public T Create<T>(Object prefab, Transform parent, Action<DiContainer> contextDecorator = null)
        {
            var subContainer = _container;
            if (contextDecorator != null)
            {
                subContainer = _container.CreateSubContainer();
                contextDecorator?.Invoke(subContainer);
            }
            return subContainer.InstantiatePrefabForComponent<T>(prefab, parent);
        }

        public T Create<T>(Object prefab, Transform parent, IEnumerable<object> args, Action<DiContainer> contextDecorator = null)
        {
            var subContainer = _container;
            if (contextDecorator != null)
            {
                subContainer = _container.CreateSubContainer();
                contextDecorator?.Invoke(subContainer);
            }
            return subContainer.InstantiatePrefabForComponent<T>(prefab, parent, args);
        }
    }
}