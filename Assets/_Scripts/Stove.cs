using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

// [RequireComponent(typeof(KitchenItemParent))]
public class Stove : NetworkBehaviour, IPickupable<KitchenObject>, IPlaceable<KitchenObject>
{
    private StoveObject _currentObject;

    [Range(1, 10)]
    [SerializeField] private float _cookSpeedMultiplier = 1;

    private float _cookTimer;

    [SerializeField] private GameObject _timerPopup;
    [SerializeField] private Image _timerImage;

    // private KitchenItemParent _placePos;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // _placePos = GetComponent<KitchenItemParent>();
    }

    private void Update()
    {
        if (_currentObject == null)
        {
            return;
        }

        _cookTimer += Time.deltaTime;
        float totalCookSpeed = _currentObject.CookTime / _cookSpeedMultiplier;
        if (_cookTimer >= totalCookSpeed)
        {
            _currentObject.Cook();
            _cookTimer = 0;
        }

        float percentCooked = _cookTimer / totalCookSpeed;
        _timerImage.fillAmount = percentCooked;
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
        StoveObject kitchenObject = networkObject?.GetComponent<StoveObject>();
        _currentObject = kitchenObject;
        kitchenObject.SetFollowTransform(NetworkObject);
        _timerPopup.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickupServerRpc(NetworkObjectReference returnTo)
    {
        PickupClientRpc(returnTo);
    }

    [ClientRpc]
    private void PickupClientRpc(NetworkObjectReference returnTo)
    {
        returnTo.TryGet(out NetworkObject returnToNetworkObject);
        returnToNetworkObject.GetComponent<PlayerPickup>().SetCurrentObject(_currentObject);
        _currentObject.SetFollowTransform(returnToNetworkObject);
        Reset();
    }

    private void Reset()
    {
        _currentObject = null;
        _cookTimer = 0;
        _timerPopup.SetActive(false);
    }

    /// <summary>
    /// Plaecs item passes on the stove if the stove does not have a current item
    /// </summary>
    /// <param name="item">Item to be put on the stove</param>
    /// <returns>Wheater the item was placed on the stove or not</returns>
    public bool Place(KitchenObject item)
    {
        if (_currentObject != null || item.GetComponent<StoveObject>() == null)
        {
            return false;
        }

        PlaceServerRpc(item.GetNetworkObject());
        return true;
    }

    // /// <summary>
    // /// Set the curretn item on the stove to null and return the item
    // /// </summary>
    // /// <returns>The item that was removed from the stove</returns>
    public /*KitchenObject*/ void Pickup(NetworkObject returnTo)
    {
        // if (_currentItem == null)
        // {
        //     return null;
        // }

        // KitchenItem kitchenItem = _currentItem;
        PickupServerRpc(returnTo);
        // return kitchenItem;
    }
}
