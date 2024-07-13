using System;
using Game.Interactables;
using UnityEngine;
using Zenject;

namespace Game.Controllers.Gameplay
{
    public class MouseObjectDetectionController : IFixedTickable
    {
        public IHoverable HoveredObject { get; private set; }
        
        private readonly MouseController _mouseController;
        private readonly Camera _camera;
        private readonly Settings _settings;

        public MouseObjectDetectionController(MouseController mouseController,
            Camera camera,
            Settings settings)
        {
            _mouseController = mouseController;
            _camera = camera;
            _settings = settings;

        }

        public void FixedTick()
        {
            var ray = _camera.ScreenPointToRay(_mouseController.MousePosition);

            // if (Physics.SphereCast(ray.origin, _settings.Mouse.InteractiveRayRadius, _camera.transform.forward, out var hit, float.PositiveInfinity, _settings.Mouse.InteractiveSphereLayerMask)
            //     &&
            //     hit.collider.TryGetComponent<IHoverable>(out var hoverable))
            if (Physics.Raycast(ray, out var hit, float.PositiveInfinity, _settings.Mouse.InteractiveSphereLayerMask, QueryTriggerInteraction.Ignore)
                &&
                hit.collider.TryGetComponent<IHoverable>(out var hoverable) && hoverable.IsActive)
            {
                DisableCurrentHoveredObject();
                
                HoveredObject = hoverable;
                HoveredObject.OnHoverStart();
                HoveredObject.OutlineHandler.EnableOutline();
            }
            else
            {
                DisableCurrentHoveredObject();
            }
            
            
        }

        private void DisableCurrentHoveredObject()
        {
            if (HoveredObject is { IsActive: true })
            {
                HoveredObject.OnHoverStop();
                HoveredObject.OutlineHandler.DisableOutline();
                HoveredObject = null;
            }
        }

        [Serializable]
        public class Settings
        {
            public MouseSettings Mouse;

            [Serializable]
            public class MouseSettings
            {
                public float InteractiveRayRadius;
                public LayerMask InteractiveSphereLayerMask;
            }
        }
    }
}

