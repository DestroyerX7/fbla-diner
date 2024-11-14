using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    private PlayerInput _playerControls;
    private InputAction _moveAction;

    private Rigidbody _rb;
    [SerializeField] private float _moveSpeed = 5;
    [SerializeField] private float _rotateSpeed = 1000;

    [SerializeField] private MeshRenderer _meshRenderer;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        _playerControls = GetComponent<PlayerInput>();
        _moveAction = _playerControls.actions["Move"];

        _rb = GetComponent<Rigidbody>();

        transform.position = Vector3.up;

        UpdateTextureServerRpc(PlayerCustomization.PlayerData.SpriteIndex);
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
    private void UpdateTextureServerRpc(int textureIndex)
    {
        UpdateTextureClientRpc(textureIndex);
    }

    [ClientRpc]
    private void UpdateTextureClientRpc(int textureIndex)
    {
        _meshRenderer.materials[0].mainTexture = PlayerCustomizationManager.Instance.GetTextureByIndex(textureIndex);
    }
}
