using UnityEngine;

public class Trash : MonoBehaviour, IPlaceable<KitchenObject>
{
    public bool Place(KitchenObject kitchenObject)
    {
        if (kitchenObject.TryGetComponent(out ITrashable trashable))
        {
            trashable.Trash();
            return true;
        }

        return false;
    }
}
