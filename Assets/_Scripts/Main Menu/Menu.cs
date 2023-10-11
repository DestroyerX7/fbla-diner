using UnityEngine;

public class Menu : MonoBehaviour
{
    [field: SerializeField] public string MenuName { get; private set; }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
