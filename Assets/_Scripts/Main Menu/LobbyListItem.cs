using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyListItem : MonoBehaviour
{
    private Lobby _lobby;

    [SerializeField] private TextMeshProUGUI _lobbyName;
    [SerializeField] private TextMeshProUGUI _playerCount;

    public void Setup(Lobby lobby)
    {
        _lobby = lobby;
        _lobbyName.text = _lobby.Name;
        _playerCount.text = _lobby.Players.Count + "/" + _lobby.MaxPlayers;
    }

    public void JoinLobby()
    {
        LobbyManager.Instance.JoinLobby(_lobby.Id);
    }
}
