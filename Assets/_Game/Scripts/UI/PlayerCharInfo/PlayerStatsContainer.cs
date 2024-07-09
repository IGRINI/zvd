using Game.Views.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Utils.PlayerCharInfo
{
    public class PlayerStatsContainer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image _healthBar;

        private PlayerView _player;
        
        public void SetHealth(float health)
        {
            var healthPct = health / _player.MaxHealth;
            _healthBar.fillAmount = healthPct;
            _healthText.SetText($"{health} / {_player.MaxHealth}");
        }

        public void SetPlayer(PlayerView playerView)
        {
            _player = playerView;

            _player.HealthChanged += SetHealth;
            SetHealth(_player.CurrentHealth);
        }

        public void UnregisterPlayer(PlayerView playerView)
        {
            if(_player != null)
            {
                SetHealth(0);
                playerView.HealthChanged -= SetHealth;
                _player = null;
            }
        }
    }
}