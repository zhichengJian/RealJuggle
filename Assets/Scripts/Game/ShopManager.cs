using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("商店界面")]
    [SerializeField] private GameObject _shopPanel;

    [Header("商店按钮")]
    [SerializeField] private GameObject _shopButton;

    [Header("商店物品")]
    [SerializeField] private GameObject[] _shoes;
    [SerializeField] private GameObject[] _balls;

    [Header("按钮")]
    [SerializeField] private Button _mainMenuButton;

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
    }

    private void Start()
    {
        if (_shopPanel != null)
        {
            _shopPanel.SetActive(false);
        }

        if (_shopButton != null)
        {
            _shopButton.SetActive(true);
        }

        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.RemoveAllListeners();
            _mainMenuButton.onClick.AddListener(() =>
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.MainMenu();
                }
                CloseShop();
            });
        }
    }

    public void OpenShop()
    {
        if (_shopPanel != null)
        {
            _shopPanel.SetActive(true);
        }

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.UpdateCoinText();
        }

        PlayButton playButton = FindObjectOfType<PlayButton>();
        if (playButton != null && playButton.gameObject != null)
        {
            playButton.gameObject.SetActive(false);
        }
    }

    public void CloseShop()
    {
        if (_shopPanel != null)
        {
            _shopPanel.SetActive(false);
        }

        PlayButton playButton = FindObjectOfType<PlayButton>();
        if (playButton != null && playButton.gameObject != null)
        {
            playButton.gameObject.SetActive(true);
        }
    }

    public void ShowShopButton()
    {
        if (_shopButton != null)
        {
            _shopButton.SetActive(true);
        }
    }

    public void HideShopButton()
    {
        if (_shopButton != null)
        {
            _shopButton.SetActive(false);
        }
    }

    public void SelectShoe(int index)
    {
        if (index >= 0 && index < _shoes.Length && _shoes[index] != null)
        {
            for (int i = 0; i < _shoes.Length; i++)
            {
                if (_shoes[i] != null)
                {
                    _shoes[i].SetActive(i == index);
                }
            }
        }
    }

    public void SelectBall(int index)
    {
        if (index >= 0 && index < _balls.Length && _balls[index] != null)
        {
            for (int i = 0; i < _balls.Length; i++)
            {
                if (_balls[i] != null)
                {
                    _balls[i].SetActive(i == index);
                }
            }
        }
    }
}