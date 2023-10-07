using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RestaurantManager : NetworkBehaviour
{
    public static RestaurantManager Instance { get; private set; }

    private readonly List<DiningTable> _diningTables = new();
    private readonly Queue<Customer> _customers = new();

    [SerializeField] private Customer _customer;
    [SerializeField] private Vector3 _customerSpawnPos = new(-18, 1, -13);

    // Clients may get a null reference exeption since Instance = null idk
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        while (_customers.Count > 0 && GetOpenDiningTable() != null)
        {
            _customers.Dequeue().SetDiningTable(GetOpenDiningTable());
        }

        // Temp
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SpawnCustomerServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCustomerServerRpc()
    {
        Customer customer = Instantiate(_customer, _customerSpawnPos, Quaternion.identity);
        customer.GetComponent<NetworkObject>().Spawn(true);
        _customers.Enqueue(customer);
    }

    public void AddDiningTable(DiningTable diningTable)
    {
        _diningTables.Add(diningTable);
    }

    public DiningTable GetOpenDiningTable()
    {
        return _diningTables.FirstOrDefault(d => d.IsOpen());
    }
}
