using System;
using UnityEngine;

namespace Game.Utils
{
    public class AnimationEventHandler : MonoBehaviour
    {
        public event Action<string> OnAnimationEvent;
        public event Action<string> OnAnimationEnded;
        
        public void AnimationEvent(string eventName)
        {
            OnAnimationEvent?.Invoke(eventName);
        }
        
        public void AnimationEnded(string eventName)
        {
            OnAnimationEnded?.Invoke(eventName);
        }
    }
}