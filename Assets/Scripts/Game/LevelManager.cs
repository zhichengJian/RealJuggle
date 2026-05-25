using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class LevelConfig
{
    [Header("关卡配置")]
    public int levelNumber;
    public string levelName;
    public int targetJuggleCount = 0;
    public int coinReward = 100;
    
    [Header("挑战模式")]
    [Tooltip("启用挑战模式：无目标限制，只计数")]
    public bool isChallengeMode = false;

    [Header("金币设置")]
    [Tooltip("金币父物体，将包含所有jinbi开头的金币物体")]
    public GameObject coinsParent;

    [Header("砖块设置")]
    [Tooltip("砖块父物体，将包含所有brick开头的砖块物体")]
    public GameObject brickParent;
}

public class LevelManager : MonoBehaviour
{
    [Header("关卡配置")]
    public LevelConfig[] levels;
    private int _currentLevelIndex = 0;
    private int _currentJuggleCount = 0;
    private int _remainingBrickCount = 0;
    private bool _levelCompleted = false;

    [Header("UI引用")]
    [SerializeField] private TextMeshProUGUI _juggleCountText;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private GameObject _levelCompletePanel;
    [SerializeField] private Button _nextLevelButton;

    private LevelConfig _currentLevel;

    public static LevelManager Instance { get; private set; }

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
        LoadLevel(0);

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }

        if (_levelCompletePanel != null)
        {
            _levelCompletePanel.SetActive(false);
        }

        if (_juggleCountText != null)
        {
            _juggleCountText.enabled = false;
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

        if (_nextLevelButton != null)
        {
            _nextLevelButton.onClick.RemoveAllListeners();
            _nextLevelButton.onClick.AddListener(NextLevel);
        }
    }

    public void ShowJuggleCount()
    {
        if (_juggleCountText != null)
        {
            _juggleCountText.enabled = true;
        }
    }

    public void ResetLevel()
    {
        _currentJuggleCount = 0;
        _levelCompleted = false;
        UpdateUI();
    }

    public void OnBallJuggled()
    {
        if (_levelCompleted) return;
        if (!GameState.Instance.isGameStarted) return;

        if (_currentLevel.targetJuggleCount > 0)
        {
            _currentJuggleCount++;
            UpdateUI();

            if (_currentJuggleCount >= _currentLevel.targetJuggleCount)
            {
                LevelComplete();
            }
        }
        else if (_currentLevel.isChallengeMode)
        {
            _currentJuggleCount++;
            UpdateUI();
        }
    }

    public void OnCoinCollected()
    {
        if (_levelCompleted) return;
        if (!GameState.Instance.isGameStarted) return;

        StartCoroutine(CheckCoinCollection());
    }

    private System.Collections.IEnumerator CheckCoinCollection()
    {
        yield return new WaitForSeconds(0.2f);

        if (AreAllJinbiCollected())
        {
            LevelComplete();
        }
    }

    private bool AreAllJinbiCollected()
    {
        int activeCount = CountActiveJinbiObjects();
        return activeCount <= 0;
    }

    private int CountActiveJinbiObjects()
    {
        int count = 0;

        if (_currentLevel != null && _currentLevel.coinsParent != null)
        {
            count = CountJinbiRecursive(_currentLevel.coinsParent.transform);
        }

        return count;
    }

    private int CountJinbiRecursive(Transform parent)
    {
        int count = 0;

        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("jinbi"))
            {
                if (child.gameObject.activeInHierarchy)
                {
                    count++;
                }
            }

            count += CountJinbiRecursive(child);
        }

        return count;
    }

    public void OnBrickDestroyed()
    {
        if (_levelCompleted) return;
        if (!GameState.Instance.isGameStarted) return;

        if (_currentLevel.brickParent == null) return;

        _remainingBrickCount--;

        if (_remainingBrickCount <= 0)
        {
            LevelComplete();
        }
    }

    private void UpdateUI()
    {
        if (_juggleCountText != null)
        {
            if (_currentLevel.targetJuggleCount > 0)
            {
                _juggleCountText.text = _currentJuggleCount + "/" + _currentLevel.targetJuggleCount;
            }
            else if (_currentLevel.isChallengeMode)
            {
                _juggleCountText.text = _currentJuggleCount.ToString();
            }
        }
    }

    private void LevelComplete()
    {
        _levelCompleted = true;

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(_currentLevel.coinReward);
        }

        if (_juggleCountText != null)
        {
            _juggleCountText.enabled = false;
        }

        if (_levelCompletePanel != null)
        {
            _levelCompletePanel.SetActive(true);
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

        ResetBall();
        ResetLevel();

        if (_currentLevel.targetJuggleCount == 0 && !_currentLevel.isChallengeMode)
        {
            if (_currentLevel.coinsParent != null)
            {
                _currentLevel.coinsParent.SetActive(true);
                ActivateAllJinbiRecursive(_currentLevel.coinsParent.transform);
            }

            if (_currentLevel.brickParent != null)
            {
                _currentLevel.brickParent.SetActive(true);
                ActivateAllBricksRecursive(_currentLevel.brickParent.transform);
                _remainingBrickCount = CountBricksRecursive(_currentLevel.brickParent.transform);
            }
        }

        if (_juggleCountText != null)
        {
            _juggleCountText.enabled = (_currentLevel.targetJuggleCount > 0 || _currentLevel.isChallengeMode);
        }

        if (GameState.Instance != null)
        {
            GameState.Instance.isGameStarted = true;
        }

        if (CameraController.Instance != null)
        {
            CameraController.Instance.StartMovement();
        }

        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            BallController ballController = ball.GetComponent<BallController>();
            if (ballController != null)
            {
                ballController.Kick();
            }
        }
    }

    public void NextLevel()
    {
        if (_levelCompletePanel != null)
        {
            _levelCompletePanel.SetActive(false);
        }

        HideAllLevels();

        int nextLevelIndex = (_currentLevelIndex + 1) % levels.Length;
        LoadLevel(nextLevelIndex);

        ResetBall();

        if (_juggleCountText != null)
        {
            _juggleCountText.enabled = (_currentLevel.targetJuggleCount > 0 || _currentLevel.isChallengeMode);
        }

        if (GameState.Instance != null)
        {
            GameState.Instance.isGameStarted = true;
        }

        if (CameraController.Instance != null)
        {
            CameraController.Instance.StartMovement();
        }

        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            BallController ballController = ball.GetComponent<BallController>();
            if (ballController != null)
            {
                ballController.Kick();
            }
        }
    }

    public void MainMenu()
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }

        if (_levelCompletePanel != null)
        {
            _levelCompletePanel.SetActive(false);
        }

        if (GameState.Instance != null)
        {
            GameState.Instance.isGameStarted = false;
        }

        HideAllLevels();

        LoadLevel(0);

        if (_juggleCountText != null)
        {
            _juggleCountText.enabled = false;
        }

        ResetBall();
        ResetCamera();
        ResetPlayButton();
    }

    private void HideAllLevels()
    {
        foreach (LevelConfig level in levels)
        {
            if (level.coinsParent != null)
            {
                level.coinsParent.SetActive(false);
            }

            if (level.brickParent != null)
            {
                level.brickParent.SetActive(false);
            }
        }
    }

    private void LoadLevel(int levelIndex)
    {
        _currentLevelIndex = levelIndex;
        _currentLevel = levels[levelIndex];
        ResetLevel();

        if (_juggleCountText != null)
        {
            _juggleCountText.enabled = (_currentLevel.targetJuggleCount > 0 || _currentLevel.isChallengeMode);
        }

        if (_currentLevel.targetJuggleCount > 0)
        {
            if (_currentLevel.coinsParent != null)
            {
                _currentLevel.coinsParent.SetActive(false);
            }

            if (_currentLevel.brickParent != null)
            {
                _currentLevel.brickParent.SetActive(false);
            }
        }
        else if (_currentLevel.isChallengeMode)
        {
            if (_currentLevel.coinsParent != null)
            {
                _currentLevel.coinsParent.SetActive(false);
            }

            if (_currentLevel.brickParent != null)
            {
                _currentLevel.brickParent.SetActive(false);
            }
        }
        else if (_currentLevel.coinsParent != null)
        {
            _currentLevel.coinsParent.SetActive(true);
            ActivateAllJinbiRecursive(_currentLevel.coinsParent.transform);

            if (_currentLevel.brickParent != null)
            {
                _currentLevel.brickParent.SetActive(false);
            }
        }
        else if (_currentLevel.brickParent != null)
        {
            _currentLevel.brickParent.SetActive(true);
            ActivateAllBricksRecursive(_currentLevel.brickParent.transform);
            _remainingBrickCount = CountBricksRecursive(_currentLevel.brickParent.transform);

            if (_currentLevel.coinsParent != null)
            {
                _currentLevel.coinsParent.SetActive(false);
            }
        }
    }

    private void ActivateAllJinbiRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("jinbi"))
            {
                child.gameObject.SetActive(true);
            }

            ActivateAllJinbiRecursive(child);
        }
    }

    private void ActivateAllBricksRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("Brick"))
            {
                child.gameObject.SetActive(true);
            }

            ActivateAllBricksRecursive(child);
        }
    }

    private int CountBricksRecursive(Transform parent)
    {
        int count = 0;

        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("Brick"))
            {
                count++;
            }

            count += CountBricksRecursive(child);
        }

        return count;
    }

    private void ResetBall()
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            BallController ballController = ball.GetComponent<BallController>();
            if (ballController != null)
            {
                ball.transform.position = new Vector3(0, 1.8f, 0.5f);
                ballController.Reset();
            }
        }
    }

    private void ResetCamera()
    {
        if (CameraController.Instance != null)
        {
            Camera.main.transform.position = new Vector3(0, 1, 3);
        }
    }

    private void ResetPlayButton()
    {
        GameObject playObject = null;
        PlayButton playButton = null;
        
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            foreach (Transform child in canvas.transform)
            {
                Button buttonComponent = child.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    playObject = child.gameObject;
                    break;
                }
            }
        }
        
        if (playObject != null)
        {
            playObject.SetActive(true);
            
            playButton = playObject.GetComponent<PlayButton>();
            if (playButton == null)
            {
                playButton = playObject.AddComponent<PlayButton>();
                Button btnComp = playObject.GetComponent<Button>();
                if (btnComp != null)
                {
                    playButton.button = btnComp;
                }
            }
            
            if (playButton != null)
            {
                playButton.Reset();
            }
            
            Button buttonComp = playObject.GetComponent<Button>();
            if (buttonComp != null)
            {
                buttonComp.enabled = true;
                buttonComp.interactable = true;
            }
            
            Image imageComponent = playObject.GetComponent<Image>();
            if (imageComponent != null)
            {
                imageComponent.enabled = true;
            }
            
            Text textComponent = playObject.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.enabled = true;
            }
        }

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ShowShopButton();
        }
    }

    public int GetCurrentLevel()
    {
        return _currentLevelIndex + 1;
    }

    public LevelConfig GetCurrentLevelConfig()
    {
        return _currentLevel;
    }
    
    public int GetJuggleCount()
    {
        return _currentJuggleCount;
    }
    
    public bool IsChallengeMode()
    {
        return _currentLevel != null && _currentLevel.isChallengeMode;
    }
}
