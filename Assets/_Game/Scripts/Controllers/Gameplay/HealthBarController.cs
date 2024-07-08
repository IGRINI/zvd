using System.Collections.Generic;
using System.Linq;
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
            
            foreach (var entity in _healthBars.Keys.ToList())
            {
                if (!entitiesOnScreen.Contains(entity) || entity.IsDied)
                {
                    _healthBarPool.Despawn(_healthBars[entity]);
                    _healthBars.Remove(entity);
                }
            }
            
            foreach (var entity in entitiesOnScreen)
            {
                if (!_healthBars.ContainsKey(entity) && !entity.IsDied)
                {
                    _healthBars.Add(entity, _healthBarPool.Spawn(entity));
                }
                else if (entity.IsDied)
                {
                    _healthBarPool.Despawn(_healthBars[entity]);
                    _healthBars.Remove(entity);
                    continue;
                }
                
                var healthBarComponent = _healthBars[entity];
                var screenPosition = _camera.WorldToScreenPoint(entity.transform.position + Vector3.up * entity.HealthBarOffset);
                healthBarComponent.transform.position = screenPosition;
            }
        }
    }
}