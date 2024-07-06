using System;
using Game.Utils;
using Game.Utils.Common;
using Game.Utils.Controllers;
using Game.Utils.Screens;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Game.UI.Navigator
{
    public class NavigationView : MonoBehaviour
    {
        [Inject] private readonly ScreenController _screenController;
        
        [SerializeField] private SelectedLight _selectLight;
        [SerializeField] private SelectButton _gameScreenButton;
        [SerializeField] private SelectButton _profileScreenButton;
        [SerializeField] private SelectButton _tavernScreenButton;
        [SerializeField] private SelectButton _shelterScreenButton;
        [SerializeField] private SelectButton _marketScreenButton;
        [SerializeField] private UIButton _closeButton;

        [SerializeField] private ConnectionLoading _connectionView;
        
        private void Start()
        {
            _gameScreenButton.OnClick.AddListener(() => _screenController.Show<GameScreen>());
            _profileScreenButton.OnClick.AddListener(() => _screenController.Show<ProfileScreen>());
            _tavernScreenButton.OnClick.AddListener(() => _screenController.Show<TavernScreen>());
            _shelterScreenButton.OnClick.AddListener(() => _screenController.Show<ShelterScreen>());
            _marketScreenButton.OnClick.AddListener(() => _screenController.Show<MarketScreen>());
            
            _gameScreenButton.OnClick.AddListener(() => SetSelected(_gameScreenButton));
            _profileScreenButton.OnClick.AddListener(() => SetSelected(_profileScreenButton));
            _tavernScreenButton.OnClick.AddListener(() => SetSelected(_tavernScreenButton));
            _shelterScreenButton.OnClick.AddListener(() => SetSelected(_shelterScreenButton));
            _marketScreenButton.OnClick.AddListener(() => SetSelected(_marketScreenButton));

            _closeButton.OnClick.AddListener(Application.Quit);

            TryToConnect();
            _connectionView.TryAgain.AddListener(TryToConnect);

        }

        private void TryToConnect()
        {
            _connectionView.ShowLoading();
        }

        private void SetSelected(UIButton button)
        {
            _gameScreenButton.MakeDeselected();
            _profileScreenButton.MakeDeselected();
            _tavernScreenButton.MakeDeselected();
            _shelterScreenButton.MakeDeselected();
            _marketScreenButton.MakeDeselected();
            
            _selectLight.SelectElement(button.RectTransform);
        }
    }
 
}
