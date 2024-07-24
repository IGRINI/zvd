using System;
using UnityEngine;
using Zenject;

namespace Game.Controllers
{
    public class CursorController
    {
        private static Texture2D _normal;
        private static Texture2D _neutral;
        private static Texture2D _friendly;
        private static Texture2D _enemy;
        private static Texture2D _normalTarget;
        private static Texture2D _neutralTarget;
        private static Texture2D _friendlyTarget;
        private static Texture2D _enemyTarget;

        private static ECursorType _cursorType = ECursorType.Normal;
        private static ECursorHover _cursorHover = ECursorHover.None;

        private CursorController([Inject(Id = ECursorId.Normal)] Texture2D normal,
            [Inject(Id = ECursorId.Neutral)] Texture2D neutral,
            [Inject(Id = ECursorId.Friendly)] Texture2D friendly,
            [Inject(Id = ECursorId.Enemy)] Texture2D enemy,
            [Inject(Id = ECursorId.NormalTarget)] Texture2D normalTarget,
            [Inject(Id = ECursorId.NeutralTarget)] Texture2D neutralTarget,
            [Inject(Id = ECursorId.FriendlyTarget)] Texture2D friendlyTarget,
            [Inject(Id = ECursorId.EnemyTarget)] Texture2D enemyTarget)
        {
            _normal = normal;
            _neutral = neutral;
            _friendly = friendly;
            _enemy = enemy;
            _normalTarget = normalTarget;
            _neutralTarget = neutralTarget;
            _friendlyTarget = friendlyTarget;
            _enemyTarget = enemyTarget;
            
            SetCursor(ECursorType.Normal);
        }

        public static void SetCursor(ECursorType type)
        {
            Texture2D texture = null;
            Vector2 offsets = Vector2.zero;
            _cursorType = type;
            
            switch (type)
            {
                case ECursorType.Normal:
                    texture = _normal;
                    offsets = new Vector2(17f, 0f);
                    break;
                case ECursorType.Target:
                    texture = _normalTarget;
                    offsets = new Vector2(32f, 32f);
                    break;
            }

            if (type == ECursorType.Normal)
            {
                texture = _cursorHover switch
                {
                    ECursorHover.None => _normal,
                    ECursorHover.Enemy => _enemy,
                    ECursorHover.Friendly => _friendly,
                    ECursorHover.Neutral => _neutral,
                    _ => texture
                };
            }
            else
            {
                texture = _cursorHover switch
                {
                    ECursorHover.None => _normalTarget,
                    ECursorHover.Enemy => _enemyTarget,
                    ECursorHover.Friendly => _friendlyTarget,
                    ECursorHover.Neutral => _neutralTarget,
                    _ => texture
                };
            }
            
            Cursor.SetCursor(texture, offsets, CursorMode.Auto);
        }

        public static void SetCursorHover(ECursorHover hoverType)
        {
            _cursorHover = hoverType;
            SetCursor(_cursorType);
        }
    }

    public enum ECursorType
    {
        Normal,
        Target
    }

    public enum ECursorHover
    {
        None,
        Enemy,
        Friendly,
        Neutral
    }

    public enum ECursorId
    {
        Normal,
        Friendly,
        Enemy,
        Neutral,
        NormalTarget,
        EnemyTarget,
        FriendlyTarget,
        NeutralTarget,
    }
}