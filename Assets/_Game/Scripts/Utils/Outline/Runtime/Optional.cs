using System;
using UnityEngine;

//  OutlineFx © NullTale - https://twitter.com/NullTale/
namespace Utils.Outline
{
    [Serializable]
    public sealed class Optional<T>
    {
        [SerializeField]
        internal bool enabled;

        [SerializeField]
        internal T value = default!;
    
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public T Value
        {
            get => value;
            set => this.value = value;
        }

        // =======================================================================
        public Optional(bool enabled)
        {
            this.enabled = enabled;
        }

        public Optional(T value, bool enabled)
        {
            this.enabled = enabled;
            this.value   = value;
        }

        public T GetValue(T disabledValue)
        {
            return enabled ? value : disabledValue;
        }
        
        public T GetValueOrDefault()
        {
            return enabled ? value : default;
        }
        
        public static implicit operator bool(Optional<T> opt)
        {
            return opt.enabled;
        }

        public static implicit operator T(Optional<T> opt)
        {
            return opt.value;
        }
    }
}