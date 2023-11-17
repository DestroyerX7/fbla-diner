using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class GameSetupManager : NetworkBehaviour
{
    [SerializeField] private PlayerVisual[] _playerVisuals;
    private NetworkList<PlayerData> _playerDataList;

    private NetworkVariable<int> _numPlayersReady = new();
    private bool _isPlayerReady;

    [SerializeField] private GameObject _startButton;

    public PlayerVisual PlayerVisual { get; private set; }

    [SerializeField] private PlayerController _player;

    public void Leave()
    {
        LobbyManager.Instance.LeaveLobby();
    }

    public void StartGame()
    {
        if (_numPlayersReady.Value != NetworkManager.Singleton.ConnectedClients.Count)
        {
            return;
        }

        if (!IsHost)
        {
            return;
        }

        foreach (PlayerData playerData in _playerDataList)
        {
            PlayerController player = Instantiate(_player);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerData.ClientId);
        }

        LobbyManager.Instance.StartGame();
    }

    public void Ready(Image readyButtonImage)
    {
        ReadyServerRpc(_isPlayerReady);

        if (_isPlayerReady)
        {
            _isPlayerReady = false;
            readyButtonImage.color = Color.red;
        }
        else
        {
            _isPlayerReady = true;
            readyButtonImage.color = Color.green;
        }
    }

    private void Awake()
    {
        _playerDataList = new();
        Instance = this;

    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            _startButton.SetActive(true);
        }

        _playerDataList.OnListChanged += UpdatePlayerVisuals;

        PlayerVisual = _playerVisuals[_playerDataList.Count];

        if (!IsServer)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += UpdatePlayerData;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerData;
        UpdatePlayerData(NetworkManager.Singleton.ConnectedClientsIds[0]);
    }

    private void UpdatePlayerData(ulong clientId)
    {
        PlayerData playerData = new()
        {
            ClientId = clientId,
        };

        _playerDataList.Add(playerData);
    }

    private void RemovePlayerData(ulong clientId)
    {
        foreach (PlayerData playerData in _playerDataList)
        {
            if (playerData.ClientId == clientId)
            {
                _playerDataList.Remove(playerData);
                break;
            }
        }
    }

    private void UpdatePlayerVisuals(NetworkListEvent<PlayerData> changeEvent)
    {
        for (int i = 0; i < _playerVisuals.Length; i++)
        {
            if (i < _playerDataList.Count)
            {
                _playerVisuals[i].Enable();
            }
            else
            {
                _playerVisuals[i].Disable();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReadyServerRpc(bool isPlayerReady)
    {
        if (isPlayerReady)
        {
            _numPlayersReady.Value--;
        }
        else
        {
            _numPlayersReady.Value++;
        }
    }

    public void TempMethod()
    {
        // foreach (PlayerVisual playerisual in _playerVisuals)
        // {
        //     playerisual.GetComponent<MeshRenderer>().materials[0].color = PlayerCustomizationManager.Instance.GetColorByIndex(PlayerCustomization.ColorIndex);
        // }
        // print(PlayerVisual);
        PlayerVisual.SetColor(PlayerCustomizationManager.Instance.GetColorByIndex(PlayerCustomization.ColorIndex));
    }

    public static GameSetupManager Instance { get; private set; }
}
