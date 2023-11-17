using UnityEngine;
using Unity.Netcode;

public class PlayerVisual : NetworkBehaviour
{
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetColor(Color32 color)
    {
        SetColorServerRpc(color);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetColorServerRpc(Color32 color)
    {
        SetColorClientRpc(color);
    }

    [ClientRpc]
    private void SetColorClientRpc(Color32 color)
    {
        _meshRenderer.materials[0].color = color;
    }

    public void Enable()
    {
        _meshRenderer.enabled = true;
    }

    public void Disable()
    {
        _meshRenderer.enabled = false;
    }
}
