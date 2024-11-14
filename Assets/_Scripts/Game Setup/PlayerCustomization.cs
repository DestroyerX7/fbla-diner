using UnityEngine;

public class PlayerCustomization
{
    public static int TextureIndex { get; private set; }
    public static PlayerData PlayerData { get; private set; }

    public static void SetTextureIndex(int textureIndex)
    {
        TextureIndex = textureIndex;
    }

    public static void SetPlayerData(PlayerData playerData)
    {
        PlayerData = playerData;
    }
}
