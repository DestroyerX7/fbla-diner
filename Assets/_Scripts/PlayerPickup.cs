using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(KitchenObjectParent))]
public class PlayerPickup : NetworkBehaviour
{
    private KitchenObject _currentObject;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_currentObject == null)
            {
                TryPickup();
            }
            else
            {
                TryPlace();
            }
        }
    }

    private void TryPlace()
    {
        IPlaceable<KitchenObject> placeable = Physics.OverlapSphere(transform.position, 2).OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).FirstOrDefault(c => c.gameObject != gameObject && c.gameObject != _currentObject.gameObject && c.GetComponent<IPlaceable<KitchenObject>>() != null)?.GetComponent<IPlaceable<KitchenObject>>();

        if (placeable != null && placeable.Place(_currentObject))
        {
            _currentObject = null;
        }
    }

    public void TryPickup()
    {
        IPickupable<KitchenObject> pickupable = Physics.OverlapSphere(transform.position, 2).OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).FirstOrDefault(c => c.gameObject != gameObject && c.GetComponent<IPickupable<KitchenObject>>() != null)?.GetComponent<IPickupable<KitchenObject>>();
        pickupable?.Pickup(NetworkObject);
    }

    public void SetCurrentObject(KitchenObject kitchenObject)
    {
        _currentObject = kitchenObject;
    }
}
