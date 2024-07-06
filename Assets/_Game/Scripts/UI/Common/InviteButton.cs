using Game.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Utils.Common
{
    public class InviteButton : UIButton
    {
        [Inject] private readonly SteamService _steamService;
        
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Image _plusIcon;
        
        private Sprite _defaultSprite;

        protected override void Awake()
        {
            base.Awake();
            _defaultSprite = GetComponent<Image>().sprite;
        }

        public void SetButtonInfo(Sprite sprite = null)
        {
            if(sprite == null)
            {
                _plusIcon.enabled = true;
                _avatarImage.sprite = _defaultSprite;
                return;
            }
            _avatarImage.sprite = sprite;
            _plusIcon.enabled = false;
        }
    }
}