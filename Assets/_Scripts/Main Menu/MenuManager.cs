using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Menu[] _menus;

    public void OpenMenu(string menuName)
    {
        foreach (Menu menu in _menus)
        {
            if (menu.MenuName == menuName)
            {
                OpenMenu(menu);
                break;
            }
        }
    }

    public void OpenMenu(Menu menuToOpen)
    {
        foreach (Menu menu in _menus)
        {
            if (menu == menuToOpen)
            {
                menu.Open();
            }
            else
            {
                menu.Close();
            }
        }
    }

    public void CloseMenu(string menuName)
    {
        foreach (Menu menu in _menus)
        {
            if (menu.MenuName == menuName)
            {
                menu.Close();
                break;
            }
        }
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }

    public void CreateLobby(TMP_InputField inputField)
    {
        LobbyManager.Instance.CreateLobby(inputField);
    }

    public void SinglePlayer()
    {
        // NetworkManager.Singleton.GetComponent<UnityTransport>().Set
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(new());
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Game Setup", LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
