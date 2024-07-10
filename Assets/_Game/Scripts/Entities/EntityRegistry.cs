using System.Collections.Generic;
using Game.Utils;
using UnityEngine;

namespace Game.Entities
{
    public static class EntityRegistry
    {
        public static IReadOnlyList<BaseEntityModel> AllEntities => _entities;

        private static readonly List<BaseEntityModel> _entities = new();
        private static Camera _camera;

        public static void Init(Camera camera)
        {
            _camera = camera;
        }

        public static void RegisterEntity(BaseEntityModel entity)
        {
            if (!_entities.Contains(entity))
            {
                _entities.Add(entity);
            }
        }

        public static void UnregisterEntity(BaseEntityModel entity)
        {
            _entities.Remove(entity);
        }

        public static List<BaseEntityModel> GetEntitiesOnScreen()
        {
            var visibleEntities = new List<BaseEntityModel>();
            var planes = GeometryUtility.CalculateFrustumPlanes(_camera);

            foreach (var entity in AllEntities)
            {
                if (IsEntityVisible(entity.GetComponent<Collider>(), planes))
                {
                    visibleEntities.Add(entity);
                }
            }

            return visibleEntities;
        }

        public static List<BaseEntityModel> GetEntitiesInRange(Vector3 position, float range)
        {
            var entitiesInRange = new List<BaseEntityModel>();

            foreach (var entity in _entities)
            {
                if (position.CheckDistanceTo(entity.transform.position, range))
                {
                    entitiesInRange.Add(entity);
                }
            }

            return entitiesInRange;
        }

        private static bool IsEntityVisible(Collider entityCollider, Plane[] planes)
        {
            return GeometryUtility.TestPlanesAABB(planes, entityCollider.bounds);
        }
    }
}