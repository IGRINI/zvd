using System;
using Game.Interactables;
using UnityEngine;
using Zenject;

namespace Game.Controllers.Gameplay
{
    public class MouseObjectDetectionController: IFixedTickable
    {
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
            if (Physics.SphereCast(_camera.transform.position, _settings.Mouse.InteractiveRayRadius, _camera.transform.forward, out var hit, _settings.Mouse.InteractiveRayDistance, _settings.Mouse.InteractiveSphereLayerMask)
                &&
                hit.collider.TryGetComponent<IHoverable>(out var hoverable))
            {
                
            }
            else
            {
                           
            }
        }

        [Serializable]
        public class Settings
        {
            public MouseSettings Mouse;

            [Serializable]
            public class MouseSettings
            {
                public float InteractiveRayDistance;
                public float InteractiveRayRadius;
                public LayerMask InteractiveSphereLayerMask;
            }
        }
    }
}

