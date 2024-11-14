using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    private const string _relayJoinCodeKey = "RelayJoinCode";

    [SerializeField] private int _maxPlayers = 4;

    private string _playerId;

    private Lobby _joinedLobby;

    [SerializeField] private float _lobbyHeartbeatTime = 10;
    private float _lobbyHeartbeatTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitAndSignIn();
    }

    private void Start()
    {
        _lobbyHeartbeatTimer = _lobbyHeartbeatTime;
        NetworkManager.Singleton.OnClientStopped += LeaveLobby;
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    private async void InitAndSignIn()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                _playerId = AuthenticationService.Instance.PlayerId;
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (System.Exception)
        {

        }
    }

    public async void CreateLobby(TMP_InputField inputField)
    {
        string lobbyName = inputField.text;

        if (string.IsNullOrEmpty(lobbyName))
        {
            return;
        }

        try
        {
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            CreateLobbyOptions options = new()
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>()
                {
                    { _relayJoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            };

            _joinedLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, _maxPlayers, options);

            RelayServerData relayServerData = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            PlayerData playerData = new()
            {
                ClientId = NetworkManager.Singleton.LocalClientId,
                PlayerId = _playerId,
                SpriteIndex = NetworkManager.Singleton.ConnectedClients.Count,
            };
            PlayerCustomization.SetPlayerData(playerData);

            NetworkManager.Singleton.SceneManager.LoadScene("Game Setup", LoadSceneMode.Single);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    public async void JoinLobby(string id)
    {
        try
        {
            _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(id);

            string relayJoinCode = _joinedLobby.Data[_relayJoinCodeKey].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            RelayServerData relayServerData = new(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

            PlayerData playerData = new()
            {
                ClientId = NetworkManager.Singleton.LocalClientId,
                PlayerId = _playerId,
                SpriteIndex = _joinedLobby.Players.Count,
            };
            PlayerCustomization.SetPlayerData(playerData);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public async void LeaveLobby(bool isServer)
    {
        try
        {
            if (_joinedLobby != null)
            {
                await Lobbies.Instance.RemovePlayerAsync(_joinedLobby.Id, _playerId);
                _joinedLobby = null;
            }

            SceneManager.LoadScene(0);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (_joinedLobby == null || _playerId != _joinedLobby.HostId)
        {
            _lobbyHeartbeatTimer = _lobbyHeartbeatTime;
            return;
        }

        try
        {
            _lobbyHeartbeatTimer -= Time.deltaTime;
            if (_lobbyHeartbeatTimer <= 0)
            {
                _lobbyHeartbeatTimer = _lobbyHeartbeatTime;
                await Lobbies.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            // _maxPlayers - 1 since maxConnections does not include the host
            return await RelayService.Instance.CreateAllocationAsync(_maxPlayers - 1);
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
            return default;
        }
    }

    public void SinglePlayer()
    {
        NetworkManager.Singleton.StartHost();

        PlayerData playerData = new()
        {
            ClientId = NetworkManager.Singleton.LocalClientId,
            PlayerId = _playerId,
            SpriteIndex = 1,
        };
        PlayerCustomization.SetPlayerData(playerData);

        NetworkManager.Singleton.SceneManager.LoadScene("Game Setup", LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
