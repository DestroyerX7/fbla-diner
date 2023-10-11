using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

// General rule for Manager Scripts :
// Have the server do everything and let the clients know afterward

// This is by far the most sketchy script imo.
public class RestaurantManager : NetworkBehaviour
{
    public static RestaurantManager Instance { get; private set; }

    private readonly List<DiningTable> _diningTables = new();
    private readonly Queue<Customer> _customers = new();

    [SerializeField] private Customer _customer;
    [SerializeField] private Vector3 _customerSpawnPos = new(-18, 1, -13);
    [SerializeField] private Vector3 _customerWaitPos = new(10, 1, -12);

    private const int _maxCustomersInLine = 5;

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

        InvokeRepeating(nameof(SpawnCustomerServerRpc), 5, 5);
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
        if (_customers.Count >= _maxCustomersInLine)
        {
            return;
        }

        Customer customer = Instantiate(_customer, _customerSpawnPos, Quaternion.identity);
        customer.GetComponent<NetworkObject>().Spawn(true);
        // _customers.Enqueue(customer);
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
}
