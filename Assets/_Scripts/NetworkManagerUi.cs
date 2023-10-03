using UnityEngine;
using Unity.Netcode;

public class NetworkManagerUi : MonoBehaviour
{
    public void Host()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void Client()
    {
        NetworkManager.Singleton.StartClient();
    }
}
