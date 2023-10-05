using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

[RequireComponent(typeof(KitchenObjectParent))]
public class Stove : NetworkBehaviour, IPickupable<KitchenObject>, IPlaceable<KitchenObject>
{
    private StoveObject _currentObject;

    [Range(1, 10)]
    [SerializeField] private float _cookSpeedMultiplier = 1;

    private float _cookTimer;

    [SerializeField] private GameObject _timerPopup;
    [SerializeField] private Image _timerImage;

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
            if (IsServer)
            {
                _currentObject.Cook();
            }
            _cookTimer = 0;
        }

        float percentCooked = _cookTimer / totalCookSpeed;
        _timerImage.fillAmount = percentCooked;
    }

    /// <summary>
    /// Places item passed on the stove if the stove does not have a current item
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

    public void Pickup(NetworkObject returnTo)
    {
        PickupServerRpc(returnTo);
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
}
