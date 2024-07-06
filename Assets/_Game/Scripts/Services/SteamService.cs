using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Utils;
using Steamworks;
using Steamworks.Data;
using UniRx;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Game.Services
{
    public class SteamService : IInitializable, ITickable, IDisposable
    {
        public static readonly AppId STEAM_APP_ID = 2629130;
        
        public static SteamId SteamID { get; private set; }

        private static AuthTicket _authTicket;
        
        private bool _initialized;
        private static readonly Dictionary<SteamId, Sprite> _cacheAvatars = new();

        public bool IsInLobby => _currentLobby != null;
        private Lobby? _currentLobby;
        
        public readonly ReactiveCommand PartyClosed = new();
        public readonly ReactiveCommand PartyEntered = new();
        public readonly ReactiveCommand<int> PartyMembersUpdated = new();
        public readonly List<Friend> PartyMembers = new();

        public async void Initialize()
        {
            if (_initialized)
                return;
            
            try
            {
                SteamClient.Init( STEAM_APP_ID );
            }
            catch ( Exception e )
            {
                Application.Quit();
            }
            
            try {
                if (SteamClient.RestartAppIfNecessary(STEAM_APP_ID)) {
                    Application.Quit();
                }
            }
            catch (DllNotFoundException e) {
                Application.Quit();
            }

            AcquireLaunchCommandLine();
            InitPartyCallbacks();
            SteamID = SteamClient.SteamId;
            _initialized = true;
            // _clientController.SetNickName(SteamClient.Name);
            
            
            SteamUser.OnValidateAuthTicketResponse += delegate(SteamId id, SteamId steamId, AuthResponse arg3)
            {
                Dev.Log($"{id} {steamId} {arg3}");
            };
        }

        private void InitPartyCallbacks()
        {
            SteamMatchmaking.OnLobbyEntered += OnLobbyEnter;
            SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
            SteamMatchmaking.OnLobbyMemberKicked += OnLobbyKicked;
            SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;

            // Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            // Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            // Callback<LobbyEnter_t>.Create(OnLobbyEnter);
            // Callback<LobbyInvite_t>.Create(OnLobbyInvite);
            // Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            // Callback<LobbyKicked_t>.Create(OnLobbyKicked);
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend joined)
        {
            try
            {
                if(PartyMembers.Contains(joined)) return;
                PurgeAvatarCacheFor(joined.Id);
                PartyMembers.Add(joined);
                PartyMembersUpdated.Execute(PartyMembers.Count);
                Dev.Log($"OnLobbyMemberJoined {joined.Name} {PartyMembers.Count}");
                var MyTeamName = "Додики";
                SteamFriends.SetRichPresence( "steam_player_group", MyTeamName );
                SteamFriends.SetRichPresence( "steam_player_group_size", PartyMembers.Count.ToString() );
            }
            catch (Exception e)
            {
                Dev.Log(e);
                throw;
            }
        }

        private void OnLobbyMemberLeave(Lobby lobby, Friend leaved)
        {
            try
            {
                if(!PartyMembers.Contains(leaved)) return;
                PurgeAvatarCacheFor(leaved.Id);
                PartyMembers.Remove(leaved);
                PartyMembersUpdated.Execute(PartyMembers.Count);
                Dev.Log($"OnLobbyMemberLeave {leaved.Name} {PartyMembers.Count}");
                var MyTeamName = "Додики";
                SteamFriends.SetRichPresence( "steam_player_group", MyTeamName );
                SteamFriends.SetRichPresence( "steam_player_group_size", PartyMembers.Count.ToString() );
            }
            catch (Exception e)
            {
                Dev.Log(e);
                throw;
            }
        }

        public string GetUserName(SteamId steamID = default)
        {
            // if (steamID == default)
            // {
            //     steamID = SteamID;
            // }

            // var info = SteamFriends.GetPersonaName();
            return SteamClient.Name;
        }

        public static async UniTask<AuthTicket> GetAuthTicket()
        {
            if(_authTicket != default)
                return _authTicket;
            _authTicket = await SteamUser.GetAuthTicketForWebApiAsync("MainGrimServer");
            return _authTicket;
        }

        public static async UniTask<string> GetAuthTicketForWebApi()
        {
            return BitConverter.ToString((await GetAuthTicket()).Data).Replace("-", string.Empty);
        }
        
        public static BeginAuthResult BeginAuthToken(byte[] data, SteamId steamId)
        {
            return SteamUser.BeginAuthSession(data, steamId);
        }

        private void OnLobbyKicked(Lobby lobby, Friend kicked, Friend whoKicked)
        {
            if (kicked.Id == SteamID)
            {
                _currentLobby = null;
                PartyClosed.Execute();
                foreach (var partyMember in PartyMembers)
                {
                    PurgeAvatarCacheFor(partyMember.Id);
                }
                PartyMembers.Clear();
                PartyMembersUpdated.Execute(PartyMembers.Count());
            }
            Dev.Log($"OnLobbyKicked {whoKicked.Name} KICKED {kicked.Name} from {lobby.Id}");
            var MyTeamName = "Додики";
            SteamFriends.SetRichPresence( "steam_player_group", MyTeamName );
            SteamFriends.SetRichPresence( "steam_player_group_size", PartyMembers.Count.ToString() );
        }

        private void AcquireLaunchCommandLine()
        {
            // if( SteamApps.GetLaunchParam( out var launchCmd, 260 ) > 0 )
            //     Dev.Log($"AcquireLaunchCommandLine {launchCmd}");
        }

        public void Tick()
        {
            if (!_initialized)
                return;
            
            SteamClient.RunCallbacks();
        }

        public void Dispose()
        {
            if (!_initialized)
                return;
            
            _initialized = false;
            SteamClient.Shutdown();
        }

        public async UniTask<bool> CreateLobbyOrInvite()
        {
            if (!_initialized)
                return false;
            
            if (IsInLobby)
            {
                SteamFriends.OpenGameInviteOverlay(_currentLobby.Value.Id);
                return true;
            }
            _currentLobby = await SteamMatchmaking.CreateLobbyAsync(3);
            _currentLobby.Value.SetJoinable(true);
            _currentLobby.Value.SetFriendsOnly();
            var MyTeamName = "Додики";
            var teamSize = 1;

            SteamFriends.SetRichPresence( "steam_player_group", MyTeamName );
            SteamFriends.SetRichPresence( "steam_player_group_size", teamSize.ToString() );
            return await CreateLobbyOrInvite();
        }

        private void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
        {
            SteamMatchmaking.JoinLobbyAsync(lobby.Id);
            Dev.Log($"OnLobbyInvite {lobby.Id} {steamId}");
        }

        private void OnLobbyEnter(Lobby lobby)
        {
            _currentLobby = lobby;

            PartyMembers.Clear();
            foreach (var lobbyMember in lobby.Members)
            {
                if(lobbyMember.Id != SteamID)
                    PartyMembers.Add(lobbyMember);
            }
            
            PartyEntered.Execute();
            PartyMembersUpdated.Execute(PartyMembers.Count);
            var MyTeamName = "Додики";
            SteamFriends.SetRichPresence( "steam_player_group", MyTeamName );
            SteamFriends.SetRichPresence( "steam_player_group_size", PartyMembers.Count.ToString() );
            Dev.Log($"OnLobbyEnter {lobby.Id} {PartyMembers.Count}");
        }

        private void OnLobbyInvite(Friend friend, Lobby lobby)
        {
            Dev.Log($"OnLobbyInvite {friend.Name} {lobby.Id}");
        }

        public void LeaveLobby()
        {
            if (!_initialized)
                return;
            if(!IsInLobby)
                return;
            
            _currentLobby.Value.Leave();

            foreach (var partyMember in PartyMembers)
            {
                PurgeAvatarCacheFor(partyMember.Id);
            }
            PartyMembers.Clear();
            PartyMembersUpdated.Execute(PartyMembers.Count);
            PartyClosed.Execute();
            Dev.Log($"LeaveLobby {_currentLobby.Value.Id}");
            _currentLobby = null;
            
        }
        
        public async UniTask<Sprite> GetAvatar(SteamId steamId = default)
        {
            try
            {
                if (!_initialized)
                    return null;
            
                if (steamId == default)
                    steamId = SteamID;
                if (_cacheAvatars.TryGetValue(steamId, out var avatar))
                {
                    return avatar;
                }
                var ret = await SteamFriends.GetLargeAvatarAsync(steamId);
                if (ret != null)
                {
                    var myAvatar = GetSteamImageAsTexture2D(ret.Value);
                    var sprite = Sprite.Create(myAvatar, new Rect(0, 0, myAvatar.width, myAvatar.height), new Vector2(.5f, .5f),
                        100f);
                    _cacheAvatars.Add(steamId, sprite);
                    return sprite;
                }

                return null;
            }
            catch ( Exception e )
            {
                Dev.Log( e );
                return null;
            }
        }

        private void PurgeAvatarCacheFor(SteamId steamId)
        {
            if (_cacheAvatars.TryGetValue(steamId, out var avatar))
            {
                Object.DestroyImmediate(avatar.texture);
                Object.DestroyImmediate(avatar);
                _cacheAvatars.Remove(steamId);
            }
        }

        private static Texture2D GetSteamImageAsTexture2D(Image image) {
            var avatar = new Texture2D( (int)image.Width, (int)image.Height, TextureFormat.ARGB32, false );
	
            avatar.filterMode = FilterMode.Trilinear;

            for ( int x = 0; x < image.Width; x++ )
            {
                for ( int y = 0; y < image.Height; y++ )
                {
                    var p = image.GetPixel( x, y );
                    avatar.SetPixel( x, (int)image.Height - y, new UnityEngine.Color( p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f ) );
                }
            }
	
            avatar.Apply();
            return avatar;
        }
    }
}