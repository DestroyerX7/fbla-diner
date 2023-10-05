using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(KitchenObjectParent))]
public class DiningTable : NetworkBehaviour, IPlaceable<KitchenObject>
{
    // Temp
    [SerializeField] private RecipeSO _recipeSO;
    private Plate _servedPlate;

    public bool Place(KitchenObject kitchenObject)
    {
        if (kitchenObject.TryGetComponent(out Plate plate))
        {
            plate.SetFollowTransform(NetworkObject);
            _servedPlate = plate;
            CheckPlate();
            return true;
        }

        return false;
    }

    private float CheckPlate()
    {
        return _recipeSO.CheckRecipe(_servedPlate);
    }
}
