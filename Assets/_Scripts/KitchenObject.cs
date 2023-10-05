using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    private Transform _followTransfrom;

    private void LateUpdate()
    {
        if (_followTransfrom == null)
        {
            return;
        }

        transform.SetPositionAndRotation(_followTransfrom.position + transform.forward, _followTransfrom.rotation);
    }

    public void SetFollowTransform(NetworkObject followTransfrom)
    {
        SetFollowTransfromServerRpc(followTransfrom);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetFollowTransfromServerRpc(NetworkObjectReference reference)
    {
        SetFollowTransfromClientRpc(reference);
    }

    [ClientRpc]
    private void SetFollowTransfromClientRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject networkObject);
        _followTransfrom = networkObject.transform;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
