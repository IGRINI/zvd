using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI.Navigator
{
    public class ConnectionLoading : MonoBehaviour
    {
        public UnityEvent TryAgain => _tryAgain.onClick;
        
        [SerializeField] private TextMeshProUGUI _connecting;
        [SerializeField] private CanvasGroup _buttons;
        [SerializeField] private Button _exit;
        [SerializeField] private Button _tryAgain;

        private string _connectingText;
        private Coroutine _currentCoroutine;
        
        private void Awake()
        {
            _connectingText = _connecting.text;
            
            _exit.onClick.AddListener(Application.Quit);
        }

        public void HideScreen()
        {
            gameObject.SetActive(false);
            StopCoroutine(_currentCoroutine);
        }

        public void ShowLoading()
        {
            _buttons.interactable = _buttons.blocksRaycasts = false;
            _buttons.DOFade(0f, .2f);
            _currentCoroutine = StartCoroutine(ConnectingAnimation());
        }

        public void ShowTryAgain()
        {
            gameObject.SetActive(true);
            StopCoroutine(_currentCoroutine);
            _connecting.text = "Can't reach server...";
            _buttons.interactable = _buttons.blocksRaycasts = true;
            _buttons.DOFade(1f, .2f);
        }

        private IEnumerator ConnectingAnimation()
        {
            var dots = "";
            while (true)
            {
                _connecting.text = string.Format(_connectingText, dots);
                dots += ".";
                if (dots == "....")
                    dots = "";
                yield return new WaitForSeconds(1f);
            }
        }
    }
}