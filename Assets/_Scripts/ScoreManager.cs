using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private NetworkVariable<float> _score = new();

    [SerializeField] private TextMeshProUGUI _scoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        _scoreText.text = _score.Value.ToString();
    }

    public void AddPoints(float points)
    {
        _score.Value += points;
    }
}
