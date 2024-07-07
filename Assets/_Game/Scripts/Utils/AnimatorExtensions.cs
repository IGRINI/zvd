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
        
        public static float GetAnimationClipLength(this Animator animator, string clipName)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == clipName)
                {
                    return clip.length;
                }
            }
            Debug.LogWarning($"Animation clip '{clipName}' not found in the animator.");
            return 0f;
        }
    }
}