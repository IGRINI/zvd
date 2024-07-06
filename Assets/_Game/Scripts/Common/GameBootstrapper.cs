using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Common
{
    public class GameBootstrapper : MonoBehaviour
    {
        private const string MenuSceneName = "Menu";

        [Inject] private readonly Settings _settings;

        private void Awake()
        {
            Application.targetFrameRate = _settings.StartFrameRate;
            SceneManager.LoadSceneAsync(MenuSceneName, LoadSceneMode.Single).allowSceneActivation = true;
        }
        
        
        [Serializable]
        public class Settings
        {
            public int StartFrameRate = 240;
        }
    }
}