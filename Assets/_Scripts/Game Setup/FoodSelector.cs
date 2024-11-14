using UnityEngine;

public class FoodSelector : MonoBehaviour
{
    [SerializeField] private RecipeSO _recipieSo;

    public void SelectFood()
    {
        FoodSelectionManager.Instance.AddFood(_recipieSo);
    }
}
