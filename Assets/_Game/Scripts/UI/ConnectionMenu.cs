using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Utils
{
    public class ConnectionMenu : MonoBehaviour
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private UnityTransport _networkTransport;
        [SerializeField] private Button _connect;
        [SerializeField] private TMP_Text _connectText;
        [SerializeField] private TMP_InputField _ip;
        [SerializeField] private TMP_InputField _port;

        private void Awake()
        {
            _connect.onClick.AddListener(Connect);
            _connectText = _connect.GetComponentInChildren<TMP_Text>();
            
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            _networkManager.OnClientStopped += OnClientStopped;
        }

        private void OnClientStopped(bool obj)
        {
            OnClientDisconnected(0);
        }

        private void OnClientConnected(ulong obj)
        {
            _connect.onClick.RemoveListener(Connect);
            _connect.onClick.AddListener(Disconnect);
            _connectText.text = "Disconnect";
        }

        private void OnClientDisconnected(ulong obj)
        {
            _connect.onClick.AddListener(Connect);
            _connect.onClick.RemoveListener(Disconnect);
            _connectText.text = "Connect";
        }

        private void Connect()
        {
            _networkTransport.ConnectionData.Address = _ip.text;
            _networkTransport.ConnectionData.Port = ushort.Parse(_port.text);
            _ip.interactable = _port.interactable = false;
            _networkManager.StartClient();
            _connect.onClick.RemoveListener(Connect);
        }

        private void Disconnect()
        {
            _ip.interactable = _port.interactable = true;
            _networkManager.Shutdown();
            _connect.onClick.RemoveListener(Disconnect);
        }
    }
}