using UnityEngine;
using UnityEngine.UI;

public enum ShopItemType
{
    Jersey,
    Shoe,
    Ball
}

public class ShopItem : MonoBehaviour
{
    [Header("商品信息")]
    [SerializeField] private string _itemId;
    [SerializeField] private ShopItemType _itemType;
    [SerializeField] private int _price = 100;

    [Header("UI组件")]
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private Image _equippedDot;

    private bool _isOwned = false;
    private bool _isEquipped = false;

    private void Start()
    {
        CheckOwnership();
        SetupButtons();
        UpdateUI();
    }

    private void CheckOwnership()
    {
        if (SaveManager.Instance == null) return;

        switch (_itemType)
        {
            case ShopItemType.Jersey:
                _isOwned = SaveManager.Instance.OwnsJersey(_itemId);
                _isEquipped = SaveManager.Instance.CurrentJersey == _itemId;
                break;
            case ShopItemType.Shoe:
                _isOwned = SaveManager.Instance.OwnsShoe(_itemId);
                _isEquipped = SaveManager.Instance.CurrentShoe == _itemId;
                break;
            case ShopItemType.Ball:
                _isOwned = SaveManager.Instance.OwnsBall(_itemId);
                _isEquipped = SaveManager.Instance.CurrentBall == _itemId;
                break;
        }
    }

    private void SetupButtons()
    {
        if (_buyButton != null)
        {
            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(OnBuyClick);
        }

        if (_equipButton != null)
        {
            _equipButton.onClick.RemoveAllListeners();
            _equipButton.onClick.AddListener(OnEquipClick);
            
            if (_equippedDot == null)
            {
                _equippedDot = _equipButton.GetComponent<Image>();
            }
        }
    }

    public void OnBuyClick()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("ShopItem: SaveManager not found!");
            return;
        }

        if (_isOwned)
        {
            Debug.Log("ShopItem: Already owned!");
            return;
        }

        if (SaveManager.Instance.SpendCoins(_price))
        {
            switch (_itemType)
            {
                case ShopItemType.Jersey:
                    SaveManager.Instance.UnlockJersey(_itemId);
                    break;
                case ShopItemType.Shoe:
                    SaveManager.Instance.UnlockShoe(_itemId);
                    break;
                case ShopItemType.Ball:
                    SaveManager.Instance.UnlockBall(_itemId);
                    break;
            }

            _isOwned = true;
            _isEquipped = false;
            
            UpdateUI();
            
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.UpdateCoinText();
            }

            Debug.Log("ShopItem: Purchased " + _itemId);
        }
        else
        {
            Debug.Log("ShopItem: Not enough coins!");
        }
    }

    public void OnEquipClick()
    {
        if (!_isOwned)
        {
            Debug.Log("ShopItem: Must purchase first!");
            return;
        }

        if (SaveManager.Instance == null) return;

        switch (_itemType)
        {
            case ShopItemType.Jersey:
                SaveManager.Instance.CurrentJersey = _itemId;
                break;
            case ShopItemType.Shoe:
                SaveManager.Instance.CurrentShoe = _itemId;
                break;
            case ShopItemType.Ball:
                SaveManager.Instance.CurrentBall = _itemId;
                break;
        }

        _isEquipped = true;
        UpdateUI();
        NotifyEquipChange();
        Debug.Log("ShopItem: Equipped " + _itemId);
    }

    private void NotifyEquipChange()
    {
        ShopItem[] allItems = FindObjectsOfType<ShopItem>();
        foreach (ShopItem item in allItems)
        {
            if (item != this && item._itemType == _itemType)
            {
                item._isEquipped = false;
                item.UpdateUI();
            }
        }
    }

    private void UpdateUI()
    {
        if (_buyButton != null)
        {
            _buyButton.gameObject.SetActive(!_isOwned);
        }

        if (_equipButton != null)
        {
            _equipButton.gameObject.SetActive(_isOwned);
        }

        if (_equippedDot != null)
        {
            _equippedDot.gameObject.SetActive(_isOwned);
            
            if (_isEquipped)
            {
                _equippedDot.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                _equippedDot.color = new Color(0.392f, 0.392f, 0.392f, 1f);
            }
        }
    }

    public void RefreshStatus()
    {
        CheckOwnership();
        UpdateUI();
    }
}