using System;
using System.Linq;
using Game.Controllers.Gameplay;
using Game.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Utils.HealthBars
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private Image _healthBg;
        [SerializeField] private TextMeshProUGUI _healthText;

        private BaseEntityModel _entity;

        public BaseEntityModel Entity => _entity;

        public void SetHealth(float currentHealth, float maxHealth)
        {
            var fillAmount = currentHealth / maxHealth;
            
            _healthFillImage.fillAmount = fillAmount;
            _healthText.SetText($"{currentHealth} / {maxHealth}");
        }

        public void SetHealthBarColors(Color color, Color bgColor)
        {
            _healthFillImage.color = color;
            _healthBg.color = bgColor;
        }

        private void OnDisable()
        {
            if(_entity != null)
                _entity.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(float obj)
        {
            SetHealth(obj, _entity.MaxHealth);
        }

        public class Pool : MonoMemoryPool<BaseEntityModel, HealthBar>
        {
            protected override void Reinitialize(BaseEntityModel entity, HealthBar item)
            {
                item._entity = entity;
                item._entity.HealthChanged += item.OnHealthChanged;
                item.SetHealth(entity.CurrentHealth, entity.MaxHealth);
                
                var colors = NetworkInfoController.Singleton.UnitsSettings.HealthBarColors.First(x =>
                    x.RelationType == NetworkInfoController.GetRelationType(
                        NetworkInfoController.Singleton.PlayerView.TeamNumber.Value, entity.TeamNumber.Value));

                item.SetHealthBarColors(colors.HealthColor, colors.BgColor);
            }
        }
    }
}