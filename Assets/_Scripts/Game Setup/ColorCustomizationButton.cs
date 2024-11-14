using UnityEngine;
using UnityEngine.UI;

public class ColorCustomizationButton : MonoBehaviour
{
    [SerializeField] private int _index;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            PlayerCustomization.SetTextureIndex(_index);
            PlayerData copied = PlayerCustomization.PlayerData;
            copied.SpriteIndex = _index;
            PlayerCustomization.SetPlayerData(copied);
            GameSetupManager.Instance.UpdateLocalPlayerVisual();
        });
    }
}
