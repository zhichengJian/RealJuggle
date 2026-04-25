using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    [Header("金币设置")]
    [SerializeField] private Text _coinText;
    [SerializeField] private int _coinValue = 10;
    [SerializeField] private int _maxCoins = 99999;
    
    private int _currentCoins = 0;
    
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
        _currentCoins += amount;
        _currentCoins = Mathf.Min(_currentCoins, _maxCoins);
        UpdateCoinText();
    }
    
    private void UpdateCoinText()
    {
        if (_coinText != null)
        {
            _coinText.text = _currentCoins.ToString();
        }
    }
}