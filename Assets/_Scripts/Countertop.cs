using Unity.Netcode;
using UnityEngine;

public class Countertop : NetworkBehaviour, IPlaceable<KitchenObject>, IPickupable<KitchenObject>
{
    private KitchenObject _currentObject;

    public /*KitchenObject*/ void Pickup(NetworkObject retunTo)
    {
        KitchenObject kitchenObject = _currentObject;
        PickupServerRpc(retunTo);
        // return kitchenObject;
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
        _currentObject.SetFollowTransform(returnToNetworkObject);
        _currentObject = null;
    }

    public bool Place(KitchenObject kitchenObject)
    {
        if (_currentObject != null)
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
