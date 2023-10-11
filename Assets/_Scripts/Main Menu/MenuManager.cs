using UnityEngine;

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
}
