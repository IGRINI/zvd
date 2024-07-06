using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace Game.Utils
{
    public static class DOTweenExtensions
    {
        public static TweenerCore<Color, Color, ColorOptions> DOColor(
            this TMP_Text target,
            Color endValue,
            float duration)
        {
            var t = DOTween.To(() => target.color, x => target.color = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }
    }
}