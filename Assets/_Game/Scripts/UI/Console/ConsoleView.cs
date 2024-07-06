using System;
using System.Reflection;
using DG.Tweening;
using Game.Controllers;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Utils.Console
{
    public class ConsoleView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _console;
        [SerializeField] private TMP_InputField _input;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private float _animationDuration = .1f;

        private float _hidePositionOffset;

        private bool _active;

        private Tween _showHideTween;

        [Inject]
        private void Constructor(KeyboardController keyboardController)
        {
            typeof(Dev).GetField("_consoleView", 
                BindingFlags.Static | 
                BindingFlags.NonPublic).SetValue(null, this);
            keyboardController.KeyPerformed += key =>
            {
                if(key != KeyAction.Console) return;
                if (_active) Hide();
                else Show();
            };
        }
        
        private void Awake()
        {
            _hidePositionOffset = _rectTransform.sizeDelta.y + 100f;
        }

        public void AddLine(object line, string prefix = "[DEBUG]")
        {
            _console.text = $"{_console.text}\n{DateTime.Now:T} {prefix}: {line}";
        }

        public void Show()
        {
            _showHideTween?.Kill();
            _active = true;
            _showHideTween = _rectTransform.DOAnchorPosY(0f, _animationDuration);
        }

        public void Hide()
        {
            _showHideTween?.Kill();
            _active = false;
            _showHideTween = _rectTransform.DOAnchorPosY(_hidePositionOffset, _animationDuration);
        }
    }
}