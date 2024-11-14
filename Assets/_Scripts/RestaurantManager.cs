using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// General rule for Manager Scripts :
// Have the server do everything and let the clients know afterward

// This is by far the most sketchy script imo.
public class RestaurantManager : NetworkBehaviour
{
    public static RestaurantManager Instance { get; private set; }

    private readonly List<DiningTable> _diningTables = new();

    // customers waiting in line are still in the queue and it causes a bug
    private readonly Queue<Customer> _customers = new();

    [SerializeField] private Customer _customer;
    [SerializeField] private Vector3[] _customerSpawnPositions;

    [SerializeField] private Vector3 _customerWaitPos = new(10, 1, -12);

    private const int _maxCustomersInLine = 5;

    [SerializeField] private GameObject _loseScreeen;
    [SerializeField] private GameObject _winScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Clients may get a null reference exeption since Instance = null idk
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
        {
            return;
        }

        InvokeRepeating(nameof(SpawnCustomerServerRpc), 5, 10);
        DayCycleManager.Instance.StartDay();
        FoodSelectionManager.Instance.SpawnMachinesServerRpc();

        // _customer.GetComponent<NetworkObject>().SpawnAsPlayerObject()

        // NetworkManager.Singleton.
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        while (_customers.Count > 0 && GetOpenDiningTable() != null)
        {
            Customer customer = _customers.Dequeue();
            DequeueCustomerClientRpc();
            DiningTable diningTable = GetOpenDiningTable();
            customer.SetDiningTable(diningTable);
            diningTable.SetCustomer(customer);
        }

        foreach (Customer customer in _customers)
        {
            customer.WaitInLine();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCustomerServerRpc()
    {
        if (_customers.Count >= _maxCustomersInLine || !DayCycleManager.Instance.DayActive.Value)
        {
            return;
        }

        Vector3 spawnPos = GetCustomerSpawnPos();
        Customer customer = Instantiate(_customer, spawnPos, Quaternion.identity);
        customer.GetComponent<NetworkObject>().Spawn(true);
        EnqueueCustomerClientRpc(customer.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    private void EnqueueCustomerClientRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject customerNetworkObject);
        _customers.Enqueue(customerNetworkObject.GetComponent<Customer>());
    }

    [ClientRpc]
    private void DequeueCustomerClientRpc()
    {
        if (IsServer)
        {
            return;
        }

        _customers.Dequeue();
    }

    public void AddDiningTable(DiningTable diningTable)
    {
        _diningTables.Add(diningTable);
    }

    // Why is this public.
    public DiningTable GetOpenDiningTable()
    {
        return _diningTables.FirstOrDefault(d => d.IsOpen());
    }

    // Why is this public.
    public Vector3 GetWaitPos(Customer customer)
    {
        int customerIndex = System.Array.IndexOf(_customers.ToArray(), customer);
        return GetWaitPos(customerIndex);
    }

    // Why is this public.
    public Vector3 GetWaitPos(int customerLineIndex)
    {
        return _customerWaitPos + 2 * customerLineIndex * Vector3.left;
    }

    public Vector3 GetCustomerSpawnPos()
    {
        int randIndex = Random.Range(0, _customerSpawnPositions.Length);
        return _customerSpawnPositions[randIndex];
    }

    public void Lose()
    {
        CancelInvoke("SpawnCustomerServerRpc");
        DayCycleManager.Instance.Stop();
        LoseServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoseServerRpc()
    {
        LoseClientRpc();
    }

    [ClientRpc]
    private void LoseClientRpc()
    {
        _loseScreeen.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
    }

    public void Win()
    {
        CancelInvoke("SpawnCustomerServerRpc");
        DayCycleManager.Instance.Stop();
        WinServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void WinServerRpc()
    {
        WinClientRpc();
    }

    [ClientRpc]
    private void WinClientRpc()
    {
        _winScreen.SetActive(true);
    }

    public void ClearCustomersLine()
    {
        _customers.Clear();
    }
}
