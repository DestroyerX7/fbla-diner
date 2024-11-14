using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class FoodSelectionManager : NetworkBehaviour
{
    [System.Serializable]
    private class IngredientToMachine
    {
        public IngredientType IngredientType;
        public GameObject[] Machines;
    }

    public static FoodSelectionManager Instance { get; private set; }

    [SerializeField] private List<Vector3> _spawnPositions = new();
    private HashSet<IngredientType> _ingredientsNeeded = new();

    [SerializeField] private List<IngredientToMachine> _ingredientsToMachines = new();
    [field: SerializeField] public RecipeListSO RecipieList { get; private set; } //TODO
    private NetworkList<int> _selectedRecipeIndexes;

    private void Awake()
    {
        _selectedRecipeIndexes = new();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddFood(RecipeSO recipeSO)
    {
        int index = System.Array.IndexOf(RecipieList.RecipeList.ToArray(), recipeSO);
        RecipieList.AddRecipe(recipeSO);
        _selectedRecipeIndexes.Add(index);
        AddIngredients(recipeSO.Ingredients);
        GameSetupManager.Instance.SelectedFood = true;
    }

    private void AddIngredients(IngredientType[] ingredients)
    {
        foreach (IngredientType ingredient in ingredients)
        {
            _ingredientsNeeded.Add(ingredient);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnMachinesServerRpc()
    {
        List<GameObject> machinesToSpawn = new();

        for (int i = 0; i < _ingredientsNeeded.Count; i++)
        {
            IngredientType currentIngredient = _ingredientsNeeded.ToList()[i];
            IngredientToMachine ingredientToMachine = _ingredientsToMachines.First(i => i.IngredientType == currentIngredient);

            foreach (GameObject machine in ingredientToMachine.Machines)
            {
                machinesToSpawn.Add(machine);
            }
        }

        for (int i = 0; i < machinesToSpawn.Count; i++)
        {
            GameObject machine = machinesToSpawn[i];
            GameObject spawnedMachine = Instantiate(machine, _spawnPositions[i], Quaternion.identity);
            spawnedMachine.GetComponent<NetworkObject>().Spawn();
        }
    }

    public int GetRandomRecipeIndex()
    {
        return _selectedRecipeIndexes[Random.Range(0, _selectedRecipeIndexes.Count)];
    }
}
