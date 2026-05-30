using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinManager : MonoBehaviour
{
    [Header("金币设置")]
    [SerializeField] private TextMeshProUGUI _coinText;
    [SerializeField] private int _coinValue = 10;

    public static CoinManager Instance { get; private set; }

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
        UpdateCoinText();
    }

    public void AddCoins()
    {
        AddCoins(_coinValue);
    }

    public void AddCoins(int amount)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.AddCoins(amount);
        }
        UpdateCoinText();
    }

    public bool SpendCoins(int amount)
    {
        if (SaveManager.Instance != null)
        {
            bool success = SaveManager.Instance.SpendCoins(amount);
            if (success)
            {
                UpdateCoinText();
            }
            return success;
        }
        return false;
    }

    public int CurrentCoins
    {
        get
        {
            if (SaveManager.Instance != null)
            {
                return SaveManager.Instance.Coins;
            }
            return 0;
        }
    }

    public void UpdateCoinText()
    {
        if (_coinText == null || SaveManager.Instance == null) return;
        
        _coinText.text = SaveManager.Instance.Coins.ToString();
    }
}
