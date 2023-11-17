using UnityEngine;
using UnityEngine.UI;

public class ColorCustomizationButton : MonoBehaviour
{
    [SerializeField] private int _colorIndex;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            PlayerCustomization.SetColorIndex(_colorIndex);
            GameSetupManager.Instance.TempMethod();
            print(PlayerCustomization.ColorIndex);
        });
    }
}
