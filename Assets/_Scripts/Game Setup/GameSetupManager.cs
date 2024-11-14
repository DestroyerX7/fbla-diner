using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class GameSetupManager : NetworkBehaviour
{
    public static GameSetupManager Instance { get; private set; }

    [SerializeField] private PlayerVisual[] _playerVisuals;
    private NetworkList<PlayerData> _playerDataList;

    private NetworkVariable<int> _numPlayersReady = new();
    private bool _isPlayerReady;

    [SerializeField] private GameObject _startButton;
    [SerializeField] private GameObject _foodSelection;

    public PlayerVisual PlayerVisual { get; private set; }

    [SerializeField] private PlayerController _player;

    public bool SelectedFood = false;

    private void Awake()
    {
        _playerDataList = new();
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            ServerInit();
        }

        _playerDataList.OnListChanged += UpdatePlayerVisuals;

        if (IsServer)
        {
            PlayerVisual = _playerVisuals[_playerDataList.Count - 1];
        }
        else
        {
            PlayerVisual = _playerVisuals[_playerDataList.Count];
        }

        PlayerCustomization.SetTextureIndex(_playerDataList.Count);
        UpdateLocalPlayerVisual();
    }

    private void ServerInit()
    {
        _startButton.SetActive(true);
        _foodSelection.SetActive(true);

        NetworkManager.Singleton.OnClientConnectedCallback += UpdatePlayerData;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerData;
        NetworkManager.Singleton.OnClientStopped += OnShutdown;
        UpdatePlayerData(NetworkManager.Singleton.LocalClientId);
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

        if (!SelectedFood)
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

    public void Leave()
    {
        NetworkManager.Singleton.Shutdown();
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

    private void UpdatePlayerData(ulong clientId)
    {
        PlayerData playerData = new()
        {
            ClientId = clientId,
            // PlayerId = LobbyManager.Instance.
            SpriteIndex = _playerDataList.Count + 1
        };

        _playerDataList.Add(playerData);
        _playerVisuals[_playerDataList.Count - 1].SetPlayerData(playerData);
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

    public void UpdateLocalPlayerVisual()
    {
        PlayerData playerData = new()
        {
            ClientId = NetworkManager.Singleton.LocalClientId,
            SpriteIndex = PlayerCustomization.TextureIndex
        };

        PlayerVisual.SetPlayerData(PlayerCustomization.PlayerData);
    }

    private void OnShutdown(bool isCurrentClient)
    {
        if (isCurrentClient)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= UpdatePlayerData;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayerData;
        }
    }
}
