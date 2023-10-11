using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyList : MonoBehaviour
{
    [SerializeField] private float _lobbyQueryTime = 2;
    private float _lobbyQueryTimer;

    [SerializeField] private LobbyListItem _lobbyListItem;

    private void Update()
    {
        HandleLobbyQueries();
    }

    private void OnDisable()
    {
        _lobbyQueryTimer = _lobbyQueryTime;
    }

    private async void HandleLobbyQueries()
    {
        try
        {
            _lobbyQueryTimer -= Time.deltaTime;
            if (_lobbyQueryTimer <= 0)
            {
                _lobbyQueryTimer = _lobbyQueryTime;

                QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();
                List<Lobby> lobbies = response.Results;

                foreach (Transform oldLobby in transform)
                {
                    Destroy(oldLobby.gameObject);
                }

                foreach (Lobby lobby in lobbies)
                {
                    LobbyListItem lobbyListItem = Instantiate(_lobbyListItem, transform);
                    lobbyListItem.Setup(lobby);
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
}
