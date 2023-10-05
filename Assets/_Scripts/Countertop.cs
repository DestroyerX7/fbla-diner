using Unity.Netcode;
using UnityEngine;

public class Countertop : NetworkBehaviour, IPlaceable<KitchenObject>, IPickupable<KitchenObject>
{
    private KitchenObject _currentObject;

    public void Pickup(NetworkObject retunTo)
    {
        PickupServerRpc(retunTo);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickupServerRpc(NetworkObjectReference returnTo)
    {
        PickupClientRpc(returnTo);
    }

    [ClientRpc]
    public void PickupClientRpc(NetworkObjectReference returnTo)
    {
        returnTo.TryGet(out NetworkObject returnToNetworkObject);
        returnToNetworkObject.GetComponent<PlayerPickup>().SetCurrentObject(_currentObject);
        _currentObject.SetFollowTransform(returnToNetworkObject); // Could be moved out of rpc but may cause problems i.e object following player before they have the object
        _currentObject = null;
    }

    public bool Place(KitchenObject kitchenObject)
    {
        if (_currentObject != null && _currentObject.TryGetComponent(out IPlaceable<KitchenObject> placeable))
        {
            placeable.Place(kitchenObject);
            return true;
        }
        else if (_currentObject != null)
        {
            return false;
        }

        kitchenObject.SetFollowTransform(NetworkObject);
        PlaceServerRpc(kitchenObject.GetNetworkObject());
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceServerRpc(NetworkObjectReference kitchenObject)
    {
        PlaceClientRpc(kitchenObject);
    }

    [ClientRpc]
    private void PlaceClientRpc(NetworkObjectReference kitchenObject)
    {
        kitchenObject.TryGet(out NetworkObject kitchenNetworkObject);
        _currentObject = kitchenNetworkObject.GetComponent<KitchenObject>();
    }
}
