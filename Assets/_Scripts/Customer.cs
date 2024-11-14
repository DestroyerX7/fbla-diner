using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Customer : NetworkBehaviour
{
    private RecipeSO _recipeSO;

    private DiningTable _currentTable;

    [SerializeField] private RecipeListSO _recipeListSO;

    [SerializeField] private GameObject _recipeImage;
    [SerializeField] private Image _foodImage;

    private NavMeshAgent _navMeshAgent;

    [SerializeField] private float _leaveTime = 30;
    private float _leaveTimer;

    public static List<Customer> _customers { get; private set; } = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _navMeshAgent = GetComponent<NavMeshAgent>();

        SetRecipeServerRpc();

        _leaveTimer = _leaveTime;

        _navMeshAgent.Warp(transform.position);

        _customers.Add(this);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _customers.Remove(this);
    }

    private void Update()
    {
        if (_leaveTimer <= 0 && _navMeshAgent.remainingDistance < 0.5)
        {
            DespawnCustomerServerRpc();
        }

        if (_leaveTimer <= 0 || _currentTable == null)
        {
            _recipeImage.SetActive(false);
        }

        if (_currentTable == null)
        {
            return;
        }

        if (_navMeshAgent.remainingDistance < 0.5)
        {
            _recipeImage.SetActive(true);
            _foodImage.sprite = _recipeSO.RecipeImage;
        }
        else
        {
            _recipeImage.SetActive(false);
        }

        _leaveTimer -= Time.deltaTime;
        if (_leaveTimer <= 0)
        {
            ScoreManager.Instance.AddPoints(_recipeSO.CheckRecipe(null));
            LeaveRestaurantServerRpc();
        }
    }

    public void Serve(Plate plate)
    {
        float score = CheckPlate(plate);
        _currentTable.LeaveTable();
        LeaveRestaurantServerRpc();
        ScoreManager.Instance.AddPoints(score);
    }

    private float CheckPlate(Plate plate)
    {
        return _recipeSO.CheckRecipe(plate);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetRecipeServerRpc()
    {
        int rand = FoodSelectionManager.Instance.GetRandomRecipeIndex();
        SetRecipeClientRpc(rand);
    }

    [ClientRpc]
    private void SetRecipeClientRpc(int randomRecipeIndex)
    {
        _recipeSO = FoodSelectionManager.Instance.RecipieList.RecipeList[randomRecipeIndex];
    }

    public void SetDiningTable(DiningTable diningTable)
    {
        SetDiningTableServerRpc(diningTable.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetDiningTableServerRpc(NetworkObjectReference reference)
    {
        SetDiningTableClientRpc(reference);
    }

    [ClientRpc]
    private void SetDiningTableClientRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject diningTable);
        _currentTable = diningTable.GetComponent<DiningTable>();

        Vector3 moveTo = _currentTable.SitPos();
        _navMeshAgent.SetDestination(moveTo);
    }

    public void WaitInLine()
    {
        WaitInLineServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void WaitInLineServerRpc()
    {
        WaitInLineClientRpc();
    }

    [ClientRpc]
    private void WaitInLineClientRpc()
    {
        Vector3 waitPos = RestaurantManager.Instance.GetWaitPos(this);
        _navMeshAgent.SetDestination(waitPos);
    }

    [ServerRpc(RequireOwnership = false)]
    private void LeaveRestaurantServerRpc()
    {
        Vector3 leavePos = RestaurantManager.Instance.GetCustomerSpawnPos();
        LeaveClientRpc(leavePos);
    }

    [ClientRpc]
    private void LeaveClientRpc(Vector3 leavePos)
    {
        _navMeshAgent.SetDestination(leavePos);
        _currentTable?.LeaveTable();
        _currentTable = null;
        _customers.Remove(this);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnCustomerServerRpc()
    {
        NetworkObject.Despawn();
    }

    public static void MakeAllCustomersLeave()
    {
        RestaurantManager.Instance.ClearCustomersLine();

        for (int i = _customers.Count - 1; i >= 0; i--)
        {
            Customer customer = _customers[i];
            customer.LeaveRestaurantServerRpc();
        }
    }
}
