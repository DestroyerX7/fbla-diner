using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "FBLA Diner/Recipe")]
public class RecipeSO : ScriptableObject
{
    [field: SerializeField] public IngredientType[] Ingredients { get; private set; }

    [field: SerializeField] public string Name { get; private set; }

    [field: SerializeField] public Sprite RecipeImage { get; private set; }

    public float CheckRecipe(Plate plate)
    {
        Ingredient[] ingredients = plate.GetIngredients();

        float sum = 0;

        for (int i = 0; i < Ingredients.Length; i++)
        {
            IngredientType ingredientType = Ingredients[i];

            if (ingredients.Length <= i || ingredients[i].GetIngredientType() != ingredientType)
            {
                sum--;
            }
            else if (ingredients[i].TryGetComponent(out StoveObject stoveObject))
            {
                if (stoveObject.CookState != CookState.Cooked)
                {
                    sum--;
                }
                else
                {
                    sum++;
                }
            }
            else
            {
                sum++;
            }
        }

        return sum;
    }
}
