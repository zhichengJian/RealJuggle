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
        
        // 绑定按钮事件
        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(RestartGame);
        }
        
        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.AddListener(MainMenu);
        }
    }
    
    public void GameOver()
    {
        // 挑战模式下不显示游戏结束面板
        if (LevelManager.Instance != null && LevelManager.Instance.IsHeightChallengeMode())
        {
            return;
        }
        
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
        
        if (GameState.Instance != null)
        {
            GameState.Instance.isGameStarted = false;
        }
        
        RealJuggleShoe[] shoes = FindObjectsOfType<RealJuggleShoe>();
        foreach (RealJuggleShoe shoe in shoes)
        {
            shoe.ResetPosition();
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
        
        // 恢复挑战按钮显示
        GameObject challengeBtn = GameObject.Find("ChallengeBtn");
        if (challengeBtn == null)
        {
            challengeBtn = GameObject.Find("挑战");
        }
        if (challengeBtn == null)
        {
            challengeBtn = GameObject.Find("btn_challenge");
        }
        if (challengeBtn == null)
        {
            // 遍历所有根对象（包括禁用的）
            foreach (GameObject rootObj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                challengeBtn = FindChallengeButton(rootObj.transform);
                if (challengeBtn != null)
                {
                    break;
                }
            }
        }
        if (challengeBtn != null)
        {
            challengeBtn.SetActive(true);
        }
    }
    
    private GameObject FindChallengeButton(Transform parent)
    {
        if (parent.gameObject.name.Contains("挑战") || parent.gameObject.name.Contains("Challenge"))
        {
            return parent.gameObject;
        }
        
        foreach (Transform child in parent)
        {
            GameObject found = FindChallengeButton(child);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
}
