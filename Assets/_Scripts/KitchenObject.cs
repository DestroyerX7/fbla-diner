using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour, ITrashable
{
    private Transform _followTransfrom;

    private void LateUpdate()
    {
        if (_followTransfrom == null)
        {
            return;
        }

        transform.SetPositionAndRotation(_followTransfrom.position, _followTransfrom.rotation);
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
        _followTransfrom = networkObject.GetComponent<KitchenObjectParent>().FollowPos;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public virtual void Trash()
    {
        TrashServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TrashServerRpc()
    {
        NetworkObject.Despawn();
    }
}
