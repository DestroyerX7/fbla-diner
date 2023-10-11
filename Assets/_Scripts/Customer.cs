using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Customer : NetworkBehaviour
{
    private RecipeSO _recipeSO;

    private DiningTable _currentTable;

    [SerializeField] private RecipeListSO _recipeListSO;

    [SerializeField] private GameObject _recipeImage;
    [SerializeField] private TMP_Text _recipeText;

    private NavMeshAgent _navMeshAgent;

    [SerializeField] private float _leaveTime = 60;
    private float _leaveTimer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _navMeshAgent = GetComponent<NavMeshAgent>();

        SetRecipeServerRpc();

        _leaveTimer = _leaveTime;

        _navMeshAgent.Warp(transform.position);
    }

    private void Update()
    {
        if (_leaveTimer <= 0 && _navMeshAgent.remainingDistance < 0.5)
        {
            DespawnCustomerServerRpc(NetworkObject);
        }

        if (_currentTable == null)
        {
            return;
        }

        if (_navMeshAgent.remainingDistance < 0.5)
        {
            _recipeImage.SetActive(true);
            _recipeText.text = _recipeSO.Name;
        }
        else
        {
            _recipeImage.SetActive(false);
        }

        _leaveTimer -= Time.deltaTime;
        if (_leaveTimer <= 0)
        {
            LeaveRestaurant();
        }
    }

    public void Serve(Plate plate)
    {
        float score = CheckPlate(plate);
        _currentTable.LeaveTable();
        DespawnCustomerServerRpc(NetworkObject);
        print(score);
    }

    private float CheckPlate(Plate plate)
    {
        return _recipeSO.CheckRecipe(plate);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetRecipeServerRpc()
    {
        int rand = _recipeListSO.GetRandomRecipeIndex();
        SetRecipeClientRpc(rand);
    }

    [ClientRpc]
    private void SetRecipeClientRpc(int randomRecipeIndex)
    {
        _recipeSO = _recipeListSO.RecipeList[randomRecipeIndex];
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

    private void LeaveRestaurant()
    {
        _navMeshAgent.SetDestination(new Vector3(-18, 1, -13));
        _currentTable.LeaveTable();
        _currentTable = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnCustomerServerRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject networkObject);
        networkObject.Despawn();
    }
}
