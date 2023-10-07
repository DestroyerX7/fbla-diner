using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(KitchenObjectParent))]
public class DiningTable : NetworkBehaviour, IPlaceable<KitchenObject>
{
    private Plate _servedPlate;

    private Customer _currentCustomer;

    [SerializeField] private Transform _sitPos;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        RestaurantManager.Instance.AddDiningTable(this);
    }

    // Stuff may not sync over the network
    public bool Place(KitchenObject kitchenObject)
    {
        if (_currentCustomer != null && kitchenObject.TryGetComponent(out Plate plate))
        {
            plate.SetFollowTransform(NetworkObject);
            _servedPlate = plate;
            ServerToCustomer();
            return true;
        }

        return false;
    }

    private void ServerToCustomer()
    {
        _currentCustomer.Serve(_servedPlate);
    }

    public bool IsOpen()
    {
        return _currentCustomer == null;
    }

    public Vector3 SitPos()
    {
        return _sitPos.position;
    }

    public void SetCustomer(Customer customer)
    {
        _currentCustomer = customer;
    }

    public void LeaveTable()
    {
        _currentCustomer = null;
        _servedPlate.Trash();
        _servedPlate = null;
    }
}
