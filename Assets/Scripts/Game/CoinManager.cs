using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    [Header("金币设置")]
    [SerializeField] private Text _coinText;
    [SerializeField] private int _coinValue = 10;
    [SerializeField] private int _maxCoins = 99999;

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

    public void UpdateCoinText()
    {
        if (_coinText == null)
        {
            Debug.LogError("CoinManager: _coinText is null!");
            return;
        }
        
        if (SaveManager.Instance == null)
        {
            Debug.LogError("CoinManager: SaveManager.Instance is null!");
            return;
        }
        
        _coinText.text = SaveManager.Instance.Coins.ToString();
        Debug.Log("CoinManager: Updated coin text to " + _coinText.text);
    }
}