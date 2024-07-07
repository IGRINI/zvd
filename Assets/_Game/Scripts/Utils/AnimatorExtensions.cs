using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Game.Utils
{
    public static class AnimatorExtensions
    {
        public static TweenerCore<float, float, FloatOptions> DOLayerWeight(this Animator animator, int layerIndex, float targetWeight, float duration)
        {
            var currentWeight = animator.GetLayerWeight(layerIndex);
            return DOTween.To(() => currentWeight, x => 
            {
                currentWeight = x;
                animator.SetLayerWeight(layerIndex, currentWeight);
            }, targetWeight, duration);
        }
    }
}