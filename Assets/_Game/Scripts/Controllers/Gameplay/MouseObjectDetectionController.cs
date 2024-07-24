using System;
using Game.Entities;
using Game.Interactables;
using UnityEngine;
using Zenject;

namespace Game.Controllers.Gameplay
{
    public class MouseObjectDetectionController : IFixedTickable
    {
        public IHoverable HoveredObject { get; private set; }
        public Vector3 PointerPosition { get; private set; }
        
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

            if (Physics.Raycast(ray, out var hit, float.PositiveInfinity, _settings.Mouse.InteractiveSphereLayerMask, QueryTriggerInteraction.Ignore)
                &&
                hit.collider.TryGetComponent<IHoverable>(out var hoverable) && hoverable.CanHover)
            {
                DisableCurrentHoveredObject();
                
                HoveredObject = hoverable;
                HoveredObject.OnHoverStart();
                HoveredObject.OutlineHandler.EnableOutline();
                PointerPosition = hit.point;

                if (hoverable is BaseEntityModel entity)
                {
                    var cursorHover = Network.GetRelationType(Network.Singleton.PlayerView.TeamNumber.Value, entity.TeamNumber.Value) switch
                    {
                        RelationType.Friendly => ECursorHover.Friendly,
                        RelationType.Hostile => ECursorHover.Enemy,
                        RelationType.Neutral => ECursorHover.Neutral,
                    };
                    
                    CursorController.SetCursorHover(cursorHover);
                }
                else
                {
                    CursorController.SetCursorHover(ECursorHover.None);
                }
            }
            else
            {
                DisableCurrentHoveredObject();

                Physics.Raycast(ray, out var pointerHit, float.PositiveInfinity,
                    _settings.Mouse.NonInteractionLayerMask, QueryTriggerInteraction.Ignore);

                PointerPosition = pointerHit.point;
                
                CursorController.SetCursorHover(ECursorHover.None);
            }
            
            
        }

        public void DisableCurrentHoveredObject()
        {
            if (HoveredObject is { CanHover: true })
            {
                HoveredObject.OnHoverStop();
                HoveredObject.OutlineHandler.DisableOutline();
            }
            HoveredObject = null;
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
                public LayerMask NonInteractionLayerMask;
            }
        }
    }
}

