using Unity.Netcode;
using UnityEngine;

public class StoveObject : KitchenObject
{
    public CookState CookState { get; private set; }
    [field: SerializeField] public float CookTime { get; private set; }

    private MeshRenderer _meshRenderer;
    [SerializeField] private Color32[] _cookStateColors;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material.color = _cookStateColors[(int)CookState];
    }

    public void Cook()
    {
        CookServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CookServerRpc()
    {
        CookClientRpc();
    }

    [ClientRpc]
    private void CookClientRpc()
    {
        CookState = CookState != CookState.Burned ? ++CookState : CookState;
        _meshRenderer.material.color = _cookStateColors[(int)CookState];
    }
}

public enum CookState
{
    Uncooked,
    Cooked,
    Burned
}
