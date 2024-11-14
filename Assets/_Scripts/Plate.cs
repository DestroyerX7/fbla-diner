using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(KitchenObjectParent))]
public class Plate : KitchenObject, IPlaceable<KitchenObject>, ITrashable
{
    private readonly Stack<Ingredient> _ingredients = new();

    public bool Place(KitchenObject kitchenObject)
    {
        if (kitchenObject.GetComponent<Ingredient>() == null)
        {
            return false;
        }

        PlaceServerRpc(kitchenObject.GetNetworkObject());
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
        Ingredient ingredient = networkObject.GetComponent<Ingredient>();
        // NetworkObject followObject = _ingredients.Count > 0 ? _ingredients.Peek().GetNetworkObject() : NetworkObject;

        if (_ingredients.Count > 0)
        {
            _ingredients.Peek().Place(ingredient);
        }
        else
        {
            ingredient.SetFollowTransform(NetworkObject);
        }

        // ingredient.SetFollowTransform(followObject);
        _ingredients.Push(ingredient);
    }

    public override void Trash()
    {
        while (_ingredients.Count > 0)
        {
            _ingredients.Pop().Trash();
        }

        base.Trash();
    }

    public Ingredient[] GetIngredients()
    {
        return _ingredients.Reverse().ToArray();
    }
}
