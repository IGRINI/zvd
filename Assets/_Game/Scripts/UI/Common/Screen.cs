using System;
using UnityEngine;
using Zenject;

namespace Game.Utils
{
    public class Screen : MonoBehaviour
    {
        protected virtual void Awake()
        {
            gameObject.SetActive(false);
        }

        public virtual void Show(Action onInitialize = null, Action onAppear = null)
        {
            OnInitialize(onInitialize);
            gameObject.SetActive(true);
            OnAppear(onAppear);
        }

        protected virtual void OnInitialize(Action onComplete = null) => onComplete?.Invoke();
        protected virtual void OnAppear(Action onComplete = null) => onComplete?.Invoke();
        protected virtual void OnDisappear(Action onComplete = null) => onComplete?.Invoke();
        
        
    }
}