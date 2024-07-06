using System;
using Game.Utils.Controllers;
using Game.Utils.Screens;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Utils
{
    public class TestAddScreen : MonoBehaviour
    {
        [Inject] private ScreenController _screenController;
        [SerializeField] private Button _button;

        private void Awake()
        {
            _button.onClick.AddListener(() => _screenController.Show<GameScreen>());
        }
    }
}