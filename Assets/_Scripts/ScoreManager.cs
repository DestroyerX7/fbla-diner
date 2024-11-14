using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private NetworkVariable<float> _score = new();
    public float Score => _score.Value;

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
        _scoreText.text = "Cash : " + _score.Value;
    }

    public void AddPoints(float points)
    {
        AddPointsServerRpc(points);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPointsServerRpc(float points)
    {
        _score.Value += points;
        _score.Value = Math.Max(0, _score.Value);
    }
}
