using UnityEngine;

//  OutlineFx © NullTale - https://twitter.com/NullTale/
namespace Utils.Outline
{
    [ExecuteAlways] [DisallowMultipleComponent]
    public class OutlineFx : MonoBehaviour
    {
        [SerializeField]
        private Color _color = Color.white;
        internal Renderer _renderer;

        public Color Color
        {
            get => _color;
            set => _color = value;
        }
        
        public float Alpha
        {
            get => _color.a;
            set => _color.a = value;
        }

        // =======================================================================
        private void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void OnWillRenderObject()
        {
#if UNITY_EDITOR
            if (Application.isEditor && Equals(_renderer, null) == false)
            {
                if (TryGetComponent(out _renderer) == false)
                    return;
            }
#endif
            
            OutlineFxFeature.Render(this);
        }
    }
}