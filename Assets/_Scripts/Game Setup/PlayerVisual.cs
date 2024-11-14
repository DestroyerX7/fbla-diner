using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerVisual : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _image;

    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private GameObject _kickUi;
    private Button _button;

    private NetworkVariable<PlayerData> _playerData = new();

    private void Awake()
    {
        _image = GetComponent<Image>();
        _playerData.OnValueChanged += UpdateSprite;
    }

    public override void OnNetworkSpawn()
    {
        _image.sprite = PlayerCustomizationManager.Instance.GetSpriteByIndex(_playerData.Value.SpriteIndex);
        _button = GetComponent<Button>();
    }

    private void UpdateSprite(PlayerData previousVal, PlayerData newVal)
    {
        _image.sprite = PlayerCustomizationManager.Instance.GetSpriteByIndex(newVal.SpriteIndex);
    }

    public void Enable()
    {
        _image.enabled = true;
    }

    public void Disable()
    {
        _image.sprite = _defaultSprite;
    }

    public void SetPlayerData(PlayerData playerData)
    {
        SetPlayerDataServerRpc(playerData);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerDataServerRpc(PlayerData playerData)
    {
        _playerData.Value = playerData;
    }

    public void KickPlayer()
    {
        if (!IsHost || _playerData.Value.ClientId == NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        NetworkManager.Singleton.DisconnectClient(_playerData.Value.ClientId);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsHost)
        {
            return;
        }

        // _kickUi.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsHost)
        {
            return;
        }

        // _kickUi.SetActive(false);
    }
}
