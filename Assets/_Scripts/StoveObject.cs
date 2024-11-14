using Unity.Netcode;
using UnityEngine;

public class StoveObject : Ingredient
{
    public CookState CookState { get; private set; }
    [field: SerializeField] public float CookTime { get; private set; }

    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Texture2D[] _cookStateTextures;

    private void Start()
    {
        _meshRenderer.material.mainTexture = _cookStateTextures[(int)CookState];
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
        _meshRenderer.material.mainTexture = _cookStateTextures[(int)CookState];
    }
}

public enum CookState
{
    Uncooked,
    Cooked,
    Burned
}
