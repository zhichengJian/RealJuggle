using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShopManager2 : MonoBehaviour
{
    public static ShopManager2 Instance { get; private set; }

    [Header("商店界面")]
    [SerializeField] private GameObject _shopPanel;

    [Header("金币显示")]
    [SerializeField] private TextMeshProUGUI _shopCoinText;

    [Header("盲盒区域")]
    [SerializeField] private GameObject _blindBoxArea;
    [SerializeField] private Button _buyBtn;
    [SerializeField] private TextMeshProUGUI _priceText;
    
    [Header("抽奖价格配置")]
    [SerializeField] private int _firstPrice = 100;
    [SerializeField] private int _secondPrice = 300;
    [SerializeField] private int _normalPrice = 666;
    
    private int _drawCount = 0;
    private int CurrentPrice
    {
        get
        {
            if (_drawCount == 0) return _firstPrice;
            if (_drawCount == 1) return _secondPrice;
            return _normalPrice;
        }
    }

    [Header("获得物品展示")]
    [SerializeField] private GameObject _rewardArea;
    [SerializeField] private Image _rewardIcon;
    [SerializeField] private Button _confirmBtn;

    [Header("背包区域")]
    [SerializeField] private Transform _backpackContainer;
    [SerializeField] private GameObject _itemSlotPrefab;

    [Header("物品图标")]
    [SerializeField] private Sprite[] _shoeIcons;
    [SerializeField] private Sprite[] _ballIcons;
    [SerializeField] private Sprite[] _jerseyIcons;

    [Header("按钮")]
    [SerializeField] private Button _mainMenuButton;

    [Header("商店按钮")]
    [SerializeField] private GameObject _shopButton;

    // 缓存协程等待对象
    private WaitForSeconds _waitOneSecond = new WaitForSeconds(1f);

    // 缓存皮肤ID前缀
    private string[] _cachedShoeIds;
    private string[] _cachedBallIds;
    private string[] _cachedJerseyIds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // 缓存皮肤ID数组
        CacheSkinIds();
    }

    private void CacheSkinIds()
    {
        // 缓存鞋子ID
        _cachedShoeIds = new string[_shoeIcons.Length];
        for (int i = 0; i < _shoeIcons.Length; i++)
        {
            _cachedShoeIds[i] = "shoe_" + (i + 1);
        }
        
        // 缓存球ID
        _cachedBallIds = new string[_ballIcons.Length];
        for (int i = 0; i < _ballIcons.Length; i++)
        {
            _cachedBallIds[i] = "ball_" + (i + 1);
        }
        
        // 缓存球衣ID
        _cachedJerseyIds = new string[_jerseyIcons.Length];
        for (int i = 0; i < _jerseyIcons.Length; i++)
        {
            _cachedJerseyIds[i] = "jersey_" + (i + 1);
        }
    }

    private void Start()
    {
        if (_shopPanel != null) _shopPanel.SetActive(false);

        if (_buyBtn != null) _buyBtn.onClick.AddListener(BuyBlindBox);
        if (_confirmBtn != null) _confirmBtn.onClick.AddListener(ConfirmReward);
        if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(() =>
        {
            if (LevelManager.Instance != null) LevelManager.Instance.MainMenu();
            CloseShop();
        });

        ShowBlindBoxArea();
        RefreshBackpack();
    }

    public void OpenShop()
    {
        if (_shopPanel != null) _shopPanel.SetActive(true);
        if (_backpackContainer != null) _backpackContainer.gameObject.SetActive(true);
        ShowBlindBoxArea();
        RefreshBackpack();
        
        ResetBackpackPosition();
        UpdateBuyButtonState();
        
        // 刷新商店内的金币显示
        UpdateShopCoinText();
        
        // 刷新价格显示
        UpdatePriceText();
        
        // 也刷新全局金币显示
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.UpdateCoinText();
        }

        PlayButton playButton = FindObjectOfType<PlayButton>();
        if (playButton != null) playButton.gameObject.SetActive(false);
    }
    
    private void UpdateShopCoinText()
    {
        if (_shopCoinText != null && SaveManager.Instance != null)
        {
            _shopCoinText.text = SaveManager.Instance.Coins.ToString();
        }
    }

    public void CloseShop()
    {
        if (_shopPanel != null) _shopPanel.SetActive(false);

        PlayButton playButton = FindObjectOfType<PlayButton>();
        if (playButton != null) playButton.gameObject.SetActive(true);
    }

    public void ShowShopButton()
    {
        if (_shopButton != null) _shopButton.SetActive(true);
    }

    public void HideShopButton()
    {
        if (_shopButton != null) _shopButton.SetActive(false);
    }

    private void ShowBlindBoxArea()
    {
        if (_blindBoxArea != null) _blindBoxArea.SetActive(true);
        if (_rewardArea != null) _rewardArea.SetActive(false);
        if (_backpackContainer != null) _backpackContainer.gameObject.SetActive(true);
    }

    private void ShowRewardArea()
    {
        if (_blindBoxArea != null) _blindBoxArea.SetActive(false);
        if (_rewardArea != null) _rewardArea.SetActive(true);
        if (_backpackContainer != null) _backpackContainer.gameObject.SetActive(true);
    }

    private void BuyBlindBox()
    {
        if (CoinManager.Instance.CurrentCoins < CurrentPrice) return;
        
        bool spent = CoinManager.Instance.SpendCoins(CurrentPrice);
        if (!spent) return;
        
        _drawCount++;
        
        // 刷新商店内的金币显示
        UpdateShopCoinText();
        
        // 刷新价格显示
        UpdatePriceText();

        StartCoroutine(OpenBlindBox());
    }
    
    private void UpdatePriceText()
    {
        if (_priceText != null)
        {
            _priceText.text = CurrentPrice.ToString();
        }
    }

    private IEnumerator OpenBlindBox()
    {
        var availableItems = new System.Collections.Generic.List<(ItemType type, int index, Sprite icon)>();
        
        for (int i = 0; i < _cachedShoeIds.Length; i++)
        {
            if (!SaveManager.Instance.OwnsShoe(_cachedShoeIds[i]))
            {
                availableItems.Add((ItemType.Shoe, i, _shoeIcons[i]));
            }
        }
        
        for (int i = 0; i < _cachedBallIds.Length; i++)
        {
            if (!SaveManager.Instance.OwnsBall(_cachedBallIds[i]))
            {
                availableItems.Add((ItemType.Ball, i, _ballIcons[i]));
            }
        }
        
        for (int i = 0; i < _cachedJerseyIds.Length; i++)
        {
            if (!SaveManager.Instance.OwnsJersey(_cachedJerseyIds[i]))
            {
                availableItems.Add((ItemType.Jersey, i, _jerseyIcons[i]));
            }
        }
        
        if (availableItems.Count == 0)
        {
            if (_rewardIcon != null)
            {
                _rewardIcon.sprite = _shoeIcons[0];
            }
            ShowRewardArea();
            yield break;
        }
        
        int randomIndex = Random.Range(0, availableItems.Count);
        var selectedItem = availableItems[randomIndex];
        
        string selectedSkinId = GetSkinId(selectedItem.type, selectedItem.index);
        
        switch (selectedItem.type)
        {
            case ItemType.Shoe:
                SaveManager.Instance.UnlockShoe(selectedSkinId);
                break;
            case ItemType.Ball:
                SaveManager.Instance.UnlockBall(selectedSkinId);
                break;
            case ItemType.Jersey:
                SaveManager.Instance.UnlockJersey(selectedSkinId);
                break;
        }
        
        if (_rewardIcon != null)
        {
            _rewardIcon.sprite = selectedItem.icon;
        }
        
        // 停顿1秒后再显示奖励区域（使用缓存的等待对象）
        yield return _waitOneSecond;
        
        ShowRewardArea();
    }

    private void ConfirmReward()
    {
        ShowBlindBoxArea();
        RefreshBackpack();
        ResetBackpackPosition();
        UpdateBuyButtonState();
    }

    private void UpdateBuyButtonState()
    {
        if (_buyBtn == null) return;
        
        bool hasAvailable = false;
        for (int i = 0; i < _cachedShoeIds.Length; i++)
        {
            if (!SaveManager.Instance.OwnsShoe(_cachedShoeIds[i]))
            {
                hasAvailable = true;
                break;
            }
        }
        
        if (!hasAvailable)
        {
            for (int i = 0; i < _cachedBallIds.Length; i++)
            {
                if (!SaveManager.Instance.OwnsBall(_cachedBallIds[i]))
                {
                    hasAvailable = true;
                    break;
                }
            }
        }
        
        if (!hasAvailable)
        {
            for (int i = 0; i < _cachedJerseyIds.Length; i++)
            {
                if (!SaveManager.Instance.OwnsJersey(_cachedJerseyIds[i]))
                {
                    hasAvailable = true;
                    break;
                }
            }
        }
        
        _buyBtn.interactable = hasAvailable;
    }

    private void RefreshBackpack()
    {
        if (_backpackContainer == null || _itemSlotPrefab == null) return;

        for (int i = _backpackContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(_backpackContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < _cachedShoeIds.Length; i++)
        {
            if (SaveManager.Instance.OwnsShoe(_cachedShoeIds[i]))
            {
                CreateBackpackItem(_shoeIcons[i], ItemType.Shoe, i);
            }
        }

        for (int i = 0; i < _cachedBallIds.Length; i++)
        {
            if (SaveManager.Instance.OwnsBall(_cachedBallIds[i]))
            {
                CreateBackpackItem(_ballIcons[i], ItemType.Ball, i);
            }
        }

        for (int i = 0; i < _cachedJerseyIds.Length; i++)
        {
            if (SaveManager.Instance.OwnsJersey(_cachedJerseyIds[i]))
            {
                CreateBackpackItem(_jerseyIcons[i], ItemType.Jersey, i);
            }
        }

        Invoke("UpdateBackpackWidth", 0.1f);
    }

    private void ResetBackpackPosition()
    {
        if (_backpackContainer == null) return;
        
        RectTransform contentRT = _backpackContainer as RectTransform;
        if (contentRT != null)
        {
            float extraLeftSpace = 200f;
            contentRT.anchoredPosition = new Vector2(-extraLeftSpace, contentRT.anchoredPosition.y);
        }
    }

    private void UpdateBackpackWidth()
    {
        RectTransform contentRT = _backpackContainer as RectTransform;
        if (contentRT == null) return;

        int itemCount = contentRT.childCount;
        if (itemCount == 0) return;

        HorizontalLayoutGroup layoutGroup = contentRT.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null) return;

        RectTransform slotRT = _itemSlotPrefab.GetComponent<RectTransform>();
        float itemWidth = slotRT != null ? slotRT.rect.width : 80f;
        
        float paddingLeft = layoutGroup.padding.left;
        float paddingRight = layoutGroup.padding.right;
        float itemSpacing = layoutGroup.spacing;

        float extraLeftSpace = 200f;
        float totalWidth = paddingLeft + paddingRight + extraLeftSpace + itemCount * (itemWidth + itemSpacing) - itemSpacing;
        contentRT.sizeDelta = new Vector2(totalWidth, contentRT.sizeDelta.y);
    }

    private void CreateBackpackItem(Sprite icon, ItemType type, int index)
    {
        if (_backpackContainer == null || _itemSlotPrefab == null) return;

        GameObject slot = Instantiate(_itemSlotPrefab, _backpackContainer);
        
        // 获取物品图标和小绿点
        Image[] allImages = slot.GetComponentsInChildren<Image>();
        Image itemImage = null;
        Image dotImage = null;
        Button oldBtn = slot.GetComponentInChildren<Button>();
        
        foreach (Image img in allImages)
        {
            // 如果是原来按钮上的 Image，就是小绿点
            if (oldBtn != null && img.transform == oldBtn.transform)
            {
                dotImage = img;
            }
            // 否则是物品图标
            else
            {
                itemImage = img;
            }
        }

        // 设置物品图标
        if (itemImage != null && icon != null)
        {
            itemImage.sprite = icon;
            itemImage.enabled = true;
        }
        
        // 在 ItemSlot 根节点添加新的 Button 来接收点击
        Button newBtn = slot.GetComponent<Button>();
        if (newBtn == null)
        {
            newBtn = slot.AddComponent<Button>();
            newBtn.targetGraphic = itemImage;
            newBtn.transition = Selectable.Transition.None;
        }
        
        // 添加点击事件到新按钮
        if (newBtn != null)
        {
            newBtn.onClick.AddListener(() => {
                ToggleEquipItem(type, index);
            });
        }
        
        // 更新高亮状态
        UpdateItemSlotHighlight(slot, type, index, dotImage);
    }
    
    private void UpdateItemSlotHighlight(GameObject slot, ItemType type, int index, Image dotImage)
    {
        bool isEquipped = IsEquipped(type, index);
        
        // 设置高亮和小绿点
        if (isEquipped)
        {
            Color highlightColor = new Color(1f, 1f, 0.8f, 1f);
            Image bgImage = slot.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = highlightColor;
            }
            
            // 小绿点亮起来
            if (dotImage != null)
            {
                Color brightColor = dotImage.color;
                brightColor.a = 1f;
                dotImage.color = brightColor;
            }
        }
        else
        {
            Color normalColor = Color.white;
            Image bgImage = slot.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = normalColor;
            }
            
            // 小绿点暗下去
            if (dotImage != null)
            {
                Color darkColor = dotImage.color;
                darkColor.a = 0.3f;
                dotImage.color = darkColor;
            }
        }
    }
    
    private bool IsEquipped(ItemType type, int index)
    {
        string currentSkinId = "";
        string targetSkinId = GetSkinId(type, index);
        
        switch (type)
        {
            case ItemType.Shoe:
                currentSkinId = SaveManager.Instance.CurrentShoe;
                break;
            case ItemType.Ball:
                currentSkinId = SaveManager.Instance.CurrentBall;
                break;
            case ItemType.Jersey:
                currentSkinId = SaveManager.Instance.CurrentJersey;
                break;
        }
        
        return currentSkinId == targetSkinId;
    }
    
    private string GetSkinId(ItemType type, int index)
    {
        switch (type)
        {
            case ItemType.Shoe:
                return _cachedShoeIds[index];
            case ItemType.Ball:
                return _cachedBallIds[index];
            case ItemType.Jersey:
                return _cachedJerseyIds[index];
            default:
                return "";
        }
    }

    private void ToggleEquipItem(ItemType type, int index)
    {
        string skinId = GetSkinId(type, index);
        string defaultSkinId = "";
        
        switch (type)
        {
            case ItemType.Shoe:
                defaultSkinId = "shoe_0";
                if (SaveManager.Instance.CurrentShoe == skinId)
                {
                    SaveManager.Instance.CurrentShoe = defaultSkinId;
                }
                else
                {
                    SaveManager.Instance.CurrentShoe = skinId;
                }
                break;
                
            case ItemType.Ball:
                defaultSkinId = "ball_0";
                if (SaveManager.Instance.CurrentBall == skinId)
                {
                    SaveManager.Instance.CurrentBall = defaultSkinId;
                }
                else
                {
                    SaveManager.Instance.CurrentBall = skinId;
                }
                break;
                
            case ItemType.Jersey:
                defaultSkinId = "jersey_0";
                if (SaveManager.Instance.CurrentJersey == skinId)
                {
                    SaveManager.Instance.CurrentJersey = defaultSkinId;
                }
                else
                {
                    SaveManager.Instance.CurrentJersey = skinId;
                }
                break;
        }
        
        // 应用皮肤
        if (SkinEquipManager.Instance != null)
        {
            string skinToApply = (type == ItemType.Shoe) ? SaveManager.Instance.CurrentShoe :
                              (type == ItemType.Ball) ? SaveManager.Instance.CurrentBall :
                              SaveManager.Instance.CurrentJersey;
            SkinEquipManager.Instance.ApplySkin(type, skinToApply);
        }
        
        // 刷新背包
        RefreshBackpack();
    }
}

public enum ItemType
{
    Shoe,
    Ball,
    Jersey
}