using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe List", menuName = "FBLA Diner/Recipe List")]
public class RecipeListSO : ScriptableObject
{
    [field: SerializeField] public RecipeSO[] RecipeList { get; private set; }

    public int /*RecipeSO*/ GetRandomRecipeIndex()
    {
        /*int rand =*/
        return Random.Range(0, RecipeList.Length);
        // return RecipeList[rand];
    }
}
