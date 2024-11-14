using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe List", menuName = "FBLA Diner/Recipe List")]
public class RecipeListSO : ScriptableObject
{
    [field: SerializeField] public List<RecipeSO> RecipeList;// { get; private set; }

    public int /*RecipeSO*/ GetRandomRecipeIndex()
    {
        /*int rand =*/
        return Random.Range(0, RecipeList.Count);
        // return RecipeList[rand];
    }

    public void AddRecipe(RecipeSO recipeSO)
    {
        RecipeList.Add(recipeSO);
    }
}
