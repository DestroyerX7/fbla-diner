using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "FBLA Diner/Recipe")]
public class RecipeSO : ScriptableObject
{
    [field: SerializeField] public IngredientType[] Ingredients { get; private set; }

    [field: SerializeField] public string Name { get; private set; }

    [field: SerializeField] public Sprite RecipeImage { get; private set; }

    public float CheckRecipe(Plate plate)
    {
        Ingredient[] ingredients = new Ingredient[0];

        if (plate != null)
        {
            ingredients = plate.GetIngredients();
        }

        int currentScore = 0;

        foreach (IngredientType ingredientType in Ingredients)
        {
            Ingredient foundIngredient = ingredients.FirstOrDefault(i => i.GetIngredientType() == ingredientType);

            if (foundIngredient == null)
            {
                currentScore--;
            }
            else if (foundIngredient.TryGetComponent(out StoveObject stoveObject))
            {
                if (stoveObject.CookState != CookState.Cooked)
                {
                    currentScore--;
                }
                else
                {
                    currentScore++;
                }
            }
            else
            {
                currentScore++;
            }
        }

        return currentScore;
    }
}
