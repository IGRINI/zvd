using System;
using UnityEngine;

namespace Game.Utils
{
    public class AnimationEventHandler : MonoBehaviour
    {
        public event Action<string> OnAnimationEvent;
        
        public void AnimationEvent(string eventName)
        {
            OnAnimationEvent?.Invoke(eventName);
        }
    }
}