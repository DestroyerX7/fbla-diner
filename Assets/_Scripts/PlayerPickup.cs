using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(KitchenObjectParent))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerPickup : NetworkBehaviour
{
    private KitchenObject _currentObject;

    private PlayerInput _playerControls;
    private InputAction _pickupAction;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        base.OnNetworkSpawn();

        _playerControls = GetComponent<PlayerInput>();
        _pickupAction = _playerControls.actions["Pickup"];
    }

    private void Update()
    {
        if (_pickupAction.triggered)
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
