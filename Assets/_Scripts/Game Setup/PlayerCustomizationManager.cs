using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCustomizationManager : MonoBehaviour
{
    public static PlayerCustomizationManager Instance { get; private set; }

    [SerializeField] private Color32[] _colors;

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

    public Color32 GetColorByIndex(int index)
    {
        return _colors[index];
    }
}
