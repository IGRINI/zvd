using System;
using Game.Services;
using Game.Utils.Common;
using TMPro;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Game.Utils.Screens
{
    public class GameScreen : Screen
    {
        [Inject] private readonly SteamService _steamService;
        
        [SerializeField] private InvitesView _invitesView;
        [SerializeField] private PlayerInfoView _selfPlayerInfoView;

        protected override void OnInitialize(Action onComplete = null)
        {
            base.OnInitialize(onComplete);

            _selfPlayerInfoView.Initialize(_steamService.GetUserName(), Random.Range(1, 100));
        }
    }
}