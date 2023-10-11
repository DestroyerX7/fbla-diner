using UnityEngine;

public class GameSetupManager : MonoBehaviour
{
    public void Leave()
    {
        LobbyManager.Instance.LeaveLobby();
    }

    public void StartGame()
    {
        LobbyManager.Instance.StartGame();
    }
}
