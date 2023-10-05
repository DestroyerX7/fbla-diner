using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(KitchenObjectParent))]
public class Ingredient : KitchenObject, IPlaceable<Ingredient>
{
    [SerializeField] private IngredientSO _ingredientSO;

    public bool Place(Ingredient ingredient)
    {
        PlaceServerRpc(ingredient.GetNetworkObject());
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceServerRpc(NetworkObjectReference reference)
    {
        PlaceClientRpc(reference);
    }

    [ClientRpc]
    private void PlaceClientRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject networkObject);
        Ingredient kitchenObject = networkObject?.GetComponent<Ingredient>();
        kitchenObject.SetFollowTransform(NetworkObject);
    }

    public IngredientType GetIngredientType()
    {
        return _ingredientSO.IngredientType;
    }
}

