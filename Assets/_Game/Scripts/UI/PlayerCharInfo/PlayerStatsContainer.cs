using DG.Tweening;
using Game.Controllers;
using Game.Controllers.Gameplay;
using Game.Entities;
using Game.Views.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Utils.PlayerCharInfo
{
    public class PlayerStatsContainer : MonoBehaviour
    {
        private KeyboardController _keyboardController;
    
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image _healthBar;
        [SerializeField] private RawImage _faceCamera;
        [SerializeField] private Image _experienceImage;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _experienceText;
        [SerializeField] private TextMeshProUGUI _attackDamage;
        [SerializeField] private TextMeshProUGUI _strength;
        [SerializeField] private TextMeshProUGUI _agility;
        [SerializeField] private TextMeshProUGUI _intelligence;
        [SerializeField] private float _experienceAnimationTime = .5f;
        
        
        private PlayerView _player;

        private Tween _experienceTween;
        
        public void SetHealth(float health)
        {
            var healthPct = health / _player.GetMaxHealth();
            _healthBar.fillAmount = healthPct;
            _healthText.SetText($"{health} / {_player.GetMaxHealth()}");
        }

        public void SetPlayer(PlayerView playerView)
        {
            _player = playerView;

            _player.HealthChanged += SetHealth;
            _player.Level.OnValueChanged += OnLevelChanged;
            _player.CurrentExperience.OnValueChanged += OnExpirienceChanged;
            _player.CurrentAttributes.OnValueChanged += OnAttributesChanged;
            _player.StatsUpdated += StatsUpdated;
            SetHealth(_player.CurrentHealth);
            OnExpirienceChanged(_player.CurrentExperience.Value, _player.CurrentExperience.Value);
            OnLevelChanged(_player.Level.Value, _player.Level.Value);
            OnAttributesChanged(_player.CurrentAttributes.Value, _player.CurrentAttributes.Value);

            _player.FaceCamera.gameObject.SetActive(true);
            _player.VirtualCamera.gameObject.SetActive(true);
            _faceCamera.texture = _player.FaceCamera.targetTexture;
        }

        private void StatsUpdated()
        {
            SetHealth(_player.CurrentHealth);
            OnExpirienceChanged(_player.CurrentExperience.Value, _player.CurrentExperience.Value);
            OnLevelChanged(_player.Level.Value, _player.Level.Value);
            OnAttributesChanged(_player.CurrentAttributes.Value, _player.CurrentAttributes.Value);
        }

        private void OnAttributesChanged(BaseEntityModel.Attributes previousvalue, BaseEntityModel.Attributes attributes)
        {
            if(attributes == null) return;
            _strength.SetText(attributes.Strength.ToString());
            _agility.SetText(attributes.Agility.ToString());
            _intelligence.SetText(attributes.Intelligence.ToString());
            
            _attackDamage.SetText($"{_player.GetAttackDamage():F0}");
        }

        private void OnExpirienceChanged(int previousExperience, int experience)
        {
            _experienceTween?.Kill();
            if (Network.GetMaxLevel() <= _player.Level.Value)
            {
                _experienceText.SetText("Max level");
                _experienceImage.fillAmount = 1f;
                return;
            }

            var tween = DOTween.Sequence()
                .Join(_experienceText.DOTextValue(previousExperience, experience, _experienceAnimationTime,
                    $"{{0}}/{Network.Singleton.GetExperienceForLevel(_player.Level.Value)}"))
                .Join(_experienceImage.DOFillAmount((float)experience / Network.Singleton.GetExperienceForLevel(_player.Level.Value),
                    _experienceAnimationTime));
            _experienceTween = tween;
        }

        private void OnLevelChanged(int previousvalue, int level)
        {
            _levelText.SetText($"{level}");
        }

        public void UnregisterPlayer(PlayerView playerView)
        {
            if(_player != null)
            {
                SetHealth(0);
                playerView.HealthChanged -= SetHealth;
                _player.Level.OnValueChanged -= OnLevelChanged;
                _player.CurrentExperience.OnValueChanged -= OnExpirienceChanged;
                _player.CurrentAttributes.OnValueChanged -= OnAttributesChanged;
                
                _player.FaceCamera.gameObject.SetActive(false);
                _player.VirtualCamera.gameObject.SetActive(false);
                _faceCamera.texture = null;
                
                _player = null;
            }
        }
    }
}