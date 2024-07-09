using System.Collections.Generic;
using Game.Entities;
using Game.Utils.HealthBars;
using UnityEngine;
using Zenject;

namespace Game.Controllers.Gameplay
{
    public class HealthBarController : ITickable
    {
        private readonly Canvas _canvas;
        private readonly HealthBar.Pool _healthBarPool;
        private readonly Camera _camera;

        private readonly Dictionary<BaseEntityModel, HealthBar> _healthBars = new();

        private HealthBarController(Canvas canvas, HealthBar.Pool healthBarPool, Camera camera)
        {
            _canvas = canvas;
            _healthBarPool = healthBarPool;
            _camera = camera;
        }

        public void Tick()
        {
            var entitiesOnScreen = EntityRegistry.GetEntitiesOnScreen();

            var entitiesToRemove = new List<BaseEntityModel>();

            foreach (var entity in _healthBars.Keys)
            {
                if (!entitiesOnScreen.Contains(entity) || entity.IsDied)
                {
                    _healthBarPool.Despawn(_healthBars[entity]);
                    entitiesToRemove.Add(entity);
                }
                else
                {
                    UpdateHealthBarPosition(entity, _healthBars[entity]);
                }
            }

            foreach (var entity in entitiesToRemove)
            {
                _healthBars.Remove(entity);
            }

            foreach (var entity in entitiesOnScreen)
            {
                if (entity.IsDied)
                {
                    if (_healthBars.ContainsKey(entity))
                    {
                        _healthBarPool.Despawn(_healthBars[entity]);
                        _healthBars.Remove(entity);
                    }
                    continue;
                }

                if (!_healthBars.ContainsKey(entity))
                {
                    _healthBars.Add(entity, _healthBarPool.Spawn(entity));
                    UpdateHealthBarPosition(entity, _healthBars[entity]);
                }
            }
        }

        private void UpdateHealthBarPosition(BaseEntityModel entity, HealthBar healthBar)
        {
            Vector3 worldPosition = entity.transform.position + Vector3.up * entity.HealthBarOffset;
            Vector3 screenPosition = _camera.WorldToScreenPoint(worldPosition);

            // Check if the entity is in front of the camera
            if (screenPosition.z > 0)
            {
                RectTransform canvasRectTransform = _canvas.GetComponent<RectTransform>();
                RectTransform healthBarRectTransform = healthBar.GetComponent<RectTransform>();

                // Convert screen position to Canvas/RectTransform position
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, null, out localPoint))
                {
                    healthBarRectTransform.anchoredPosition = localPoint;
                }
            }
            else
            {
                // Remove the health bar from the dictionary and despawn it
                if (_healthBars.ContainsKey(entity))
                {
                    _healthBarPool.Despawn(_healthBars[entity]);
                    _healthBars.Remove(entity);
                }
            }
        }
    }
}
