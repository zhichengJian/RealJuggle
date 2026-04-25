using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("UI设置")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;
    
    public static GameOverManager Instance { get; private set; }
    
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
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }
        
        if (_restartButton != null)
        {
            _restartButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(RestartGame);
        }
        
        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.RemoveAllListeners();
            _mainMenuButton.onClick.AddListener(MainMenu);
        }
    }
    
    public void GameOver()
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
        
        if (GameState.Instance != null)
        {
            GameState.Instance.isGameStarted = false;
        }
    }
    
    public void RestartGame()
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartGame();
        }
    }
    
    public void MainMenu()
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }
        
        if (GameState.Instance != null)
        {
            GameState.Instance.isGameStarted = false;
        }
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.MainMenu();
        }
    }
}