using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace Game.Utils
{
    public static class TextExtensions
    { 
        public static TweenerCore<float, float, FloatOptions> DOTextValue(this TextMeshProUGUI text, float startValue, float endValue, float duration, string pattern = "{0}")
        {
            return DOTween.To(() => startValue, x => 
            {
                startValue = x;
                text.SetText(string.Format(pattern, Mathf.RoundToInt(startValue)));
            }, endValue, duration);
        }
    }
}