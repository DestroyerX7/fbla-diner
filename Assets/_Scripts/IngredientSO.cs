using UnityEngine;

[CreateAssetMenu(menuName = "FBLA Diner/Ingredient", fileName = "New Ingredient")]
public class IngredientSO : ScriptableObject
{
    [field: SerializeField] public IngredientType IngredientType { get; private set; }
}

public enum IngredientType
{
    Bread,
    BurgerPatty,
    Lettuce,
    Bacon,
    Cheese,
    Tomatoes,
    Steak,
}
