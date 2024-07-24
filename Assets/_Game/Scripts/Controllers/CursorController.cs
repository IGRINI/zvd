using System;
using UnityEngine;
using Zenject;

namespace Game.Controllers
{
    public class CursorController
    {
        private readonly Texture2D _normal;
        private readonly Texture2D _target;
        private readonly Texture2D _enemyTarget;

        private static CursorController _instance;
        
        
        private CursorController([Inject(Id = ECursorType.Normal)] Texture2D normal,
            [Inject(Id = ECursorType.Target)] Texture2D target,
            [Inject(Id = ECursorType.EnemyTarget)] Texture2D enemyTarget)
        {
            _normal = normal;
            _target = target;
            _enemyTarget = enemyTarget;

            _instance = this;
            
            SetCursor(ECursorType.Normal);
        }

        public static void SetCursor(ECursorType type)
        {
            Texture2D texture = null;
            Vector2 offsets = Vector2.zero;
            switch (type)
            {
                case ECursorType.Normal:
                    texture = _instance._normal;
                    offsets = new Vector2(17f, 0f);
                    break;
                case ECursorType.Target:
                    texture = _instance._target;
                    offsets = new Vector2(32f, 32f);
                    break;
                case ECursorType.EnemyTarget:
                    texture = _instance._enemyTarget;
                    offsets = new Vector2(32f, 32f);
                    break;
            }
            
            Cursor.SetCursor(texture, offsets, CursorMode.Auto);
        }
    }

    public enum ECursorType
    {
        Normal,
        Target,
        EnemyTarget
    }
}