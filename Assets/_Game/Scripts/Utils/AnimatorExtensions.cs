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
            var runtimeAnimatorController = animator.runtimeAnimatorController;
            if (runtimeAnimatorController == null) 
                throw new System.NullReferenceException("Animator does not have a runtime animator controller");

            var clips = runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == clipName)
                {
                    return clip.length;
                }
            }
            throw new System.ArgumentException($"Clip '{clipName}' not found in AnimatorController");
        }
    }
}