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

public class LobbyManager : MonoBehaviour
{
    private const string _relayJoinCodeKey = "RelayJoinCode";

    [SerializeField] private int _maxPlayers = 4;

    private string _playerId;

    private Lobby _joinedLobby;

    [SerializeField] private float _lobbyHeartbeatTime = 10;
    private float _lobbyHeartbeatTimer;

    private void Awake()
    {
        InitAndSignIn();
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    private async void InitAndSignIn()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            _playerId = AuthenticationService.Instance.PlayerId;
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateLobby()
    {
        try
        {
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            CreateLobbyOptions options = new()
            {
                IsPrivate = false,
                Data = new System.Collections.Generic.Dictionary<string, DataObject>()
                {
                    { _relayJoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            };

            _joinedLobby = await Lobbies.Instance.CreateLobbyAsync("Test Lobby Name", _maxPlayers, options);

            RelayServerData relayServerData = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
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
            // _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(id);
            _joinedLobby = await Lobbies.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = _joinedLobby.Data[_relayJoinCodeKey].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            RelayServerData relayServerData = new(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    public async void LeaveLobby()
    {
        if (_joinedLobby == null)
        {
            return;
        }

        try
        {
            await Lobbies.Instance.RemovePlayerAsync(_joinedLobby.Id, _playerId);
            _joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    // Not ready yet, other players will not recieve that they have been kicked
    public async void KickPlayer(Player player)
    {
        throw new System.NotImplementedException();

        if (_joinedLobby == null || _playerId != _joinedLobby.Id)
        {
            return;
        }

        await Lobbies.Instance.RemovePlayerAsync(_joinedLobby.Id, player.Id);
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
                await Lobbies.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
                _lobbyHeartbeatTimer = _lobbyHeartbeatTime;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    public async void StartGame()
    {
        throw new System.NotImplementedException();

        await Lobbies.Instance.DeleteLobbyAsync(_joinedLobby.Id);
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
}
