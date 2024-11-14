using Unity.Netcode;
using UnityEngine;

public class Bread : Ingredient
{
    [SerializeField] private GameObject _bottonBun;
    [SerializeField] private GameObject _topBun;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override bool Place(Ingredient ingredient)
    {
        SetAsBottomBunServerRpc();
        return base.Place(ingredient);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAsBottomBunServerRpc()
    {
        SetAsBottomBunClientRpc();
    }

    [ClientRpc]
    private void SetAsBottomBunClientRpc()
    {
        _topBun.SetActive(false);
        _bottonBun.SetActive(true);
    }
}
