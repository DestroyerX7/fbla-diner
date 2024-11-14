using UnityEngine;

public class PlayerCustomizationManager : MonoBehaviour
{
    public static PlayerCustomizationManager Instance { get; private set; }

    [SerializeField] private Texture2D[] _playerTextures;
    [SerializeField] private Sprite[] _playerSprites;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Texture2D GetTextureByIndex(int index)
    {
        return _playerTextures[index];
    }

    public Sprite GetSpriteByIndex(int index)
    {
        return _playerSprites[index];
    }
}
