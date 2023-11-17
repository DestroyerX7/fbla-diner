using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    private PlayerInput _playerControls;
    private InputAction _moveAction;

    private Rigidbody _rb;
    [SerializeField] private float _moveSpeed = 5;
    [SerializeField] private float _rotateSpeed = 1000;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        base.OnNetworkSpawn();

        _playerControls = GetComponent<PlayerInput>();
        _moveAction = _playerControls.actions["Move"];

        _rb = GetComponent<Rigidbody>();

        transform.position = Vector3.up;

        UpdateColorServerRpc(PlayerCustomizationManager.Instance.GetColorByIndex(PlayerCustomization.ColorIndex));
        // GetComponent<MeshRenderer>().materials[0].color = PlayerCustomizationManager.Instance.GetColorByIndex(PlayerCustomization.ColorIndex);
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        Vector3 moveDir = new(moveInput.x, 0, moveInput.y);
        _rb.velocity = moveDir * _moveSpeed;

        if (moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg - 90;
            Quaternion to = Quaternion.Euler(0, -angle, 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, to, _rotateSpeed * Time.deltaTime);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateColorServerRpc(Color32 color)
    {
        UpdateColorClientRpc(color);
    }

    [ClientRpc]
    private void UpdateColorClientRpc(Color32 color)
    {
        GetComponent<MeshRenderer>().materials[0].color = color;
    }
}
