using Unity.Netcode;
using UnityEngine;

public class Dispenser : NetworkBehaviour, IPickupable<KitchenObject>
{
    [SerializeField] private KitchenObject _dispenseObject;

    // Seems like a sketchy solution since it forces every IPickupable
    // to call the SetCurrentObject method on the PlayerPickup
    public void Pickup(NetworkObject returnTo)
    {
        DispenseServerRpc(returnTo);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DispenseServerRpc(NetworkObjectReference returnTo)
    {
        KitchenObject dispensedObject = Instantiate(_dispenseObject);
        NetworkObject dispensedNetworkObject = dispensedObject.GetNetworkObject();
        dispensedNetworkObject.Spawn(true);

        DispenseClientRpc(dispensedNetworkObject, returnTo);
    }

    [ClientRpc]
    private void DispenseClientRpc(NetworkObjectReference dispensedObject, NetworkObjectReference returnTo)
    {
        dispensedObject.TryGet(out NetworkObject dispensedNetworkObject);
        returnTo.TryGet(out NetworkObject returnToNetworkObject);

        KitchenObject kitchenObject = dispensedNetworkObject.GetComponent<KitchenObject>();
        returnToNetworkObject.GetComponent<PlayerPickup>().SetCurrentObject(kitchenObject);
        kitchenObject.SetFollowTransform(returnToNetworkObject);
    }
}
