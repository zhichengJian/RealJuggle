using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    
    [Tooltip("启用高度挑战模式")]
    public bool isHeightChallengeMode = false;

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
    
    [Header("高度挑战模式")]
    [SerializeField] private TextMeshProUGUI _heightText;
    [SerializeField] private TextMeshProUGUI _finalHeightText;
    [SerializeField] private GameObject _heightChallengePanel;
    [SerializeField] private Button _exitChallengeButton;
    [SerializeField] private Button _challengeButton; // 挑战按钮引用
    private LevelConfig _currentLevel;
    private float _currentMaxHeight = 0f;
    private bool _isHeightChallengeActive = false;
    private bool _originalBallIsKinematic = false; // 保存足球原始状态
    private GameObject _cachedBall; // 缓存球的引用，避免重复查找

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
        // 缓存球的引用
        _cachedBall = GameObject.FindGameObjectWithTag("Ball");
        
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

        // 初始化高度挑战UI
        if (_heightChallengePanel != null)
        {
            _heightChallengePanel.SetActive(false);
        }

        if (_heightText != null)
        {
            _heightText.gameObject.SetActive(false);
        }

        if (_finalHeightText != null)
        {
            _finalHeightText.gameObject.SetActive(false);
        }

        // 自动查找挑战按钮
        if (_challengeButton == null)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button btn in allButtons)
            {
                if (btn.gameObject.name.Contains("挑战") || 
                    btn.gameObject.name.Contains("Challenge"))
                {
                    _challengeButton = btn;
                    break;
                }
            }
        }
        
        // 确保挑战按钮显示
        if (_challengeButton != null)
        {
            _challengeButton.gameObject.SetActive(true);
        }

        if (_exitChallengeButton != null)
        {
            _exitChallengeButton.gameObject.SetActive(false);
        }

        // 绑定按钮事件
        if (_nextLevelButton != null)
        {
            _nextLevelButton.onClick.RemoveAllListeners();
            _nextLevelButton.onClick.AddListener(NextLevel);
        }
        
        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.RemoveAllListeners();
            _mainMenuButton.onClick.AddListener(MainMenu);
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
        // 高度挑战模式下不触发关卡逻辑
        if (_isHeightChallengeActive) return;
        
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
        // 挑战模式不触发关卡完成
        if (_isHeightChallengeActive) return;
        
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
            
            // 在面板显示时重新绑定按钮事件
            if (_nextLevelButton != null)
            {
                _nextLevelButton.onClick.RemoveAllListeners();
                _nextLevelButton.onClick.AddListener(NextLevel);
            }
            
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveAllListeners();
                _mainMenuButton.onClick.AddListener(MainMenu);
            }
        }

        if (GameState.Instance != null)
        {
            GameState.Instance.isGameStarted = false;
        }
        
        // 复位鞋子位置
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

        if (_cachedBall != null)
        {
            BallController ballController = _cachedBall.GetComponent<BallController>();
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

        if (_cachedBall != null)
        {
            BallController ballController = _cachedBall.GetComponent<BallController>();
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
        
        // 显示商店按钮
        if (ShopManager2.Instance != null)
        {
            ShopManager2.Instance.ShowShopButton();
        }
        
        // 激活挑战按钮
        if (_challengeButton != null)
        {
            _challengeButton.gameObject.SetActive(true);
        }
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
        if (_cachedBall != null)
        {
            BallController ballController = _cachedBall.GetComponent<BallController>();
            if (ballController != null)
            {
                _cachedBall.transform.position = new Vector3(0, 1.8f, 0.5f);
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

        // 先尝试用名称查找 PlayButton
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            // 先尝试直接查找名为 "PlayButton" 的对象
            playObject = FindChildByName(canvas.transform, "PlayButton");
            
            // 如果找不到，就查找有 PlayButton 组件的对象
            if (playObject == null)
            {
                foreach (Transform child in canvas.transform)
                {
                    PlayButton pb = child.GetComponent<PlayButton>();
                    if (pb != null)
                    {
                        playObject = child.gameObject;
                        break;
                    }
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

        if (ShopManager2.Instance != null)
        {
            ShopManager2.Instance.ShowShopButton();
        }
    }

    private GameObject FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
            
            GameObject found = FindChildByName(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    public int GetCurrentLevel()
    {
        return _currentLevelIndex + 1;
    }

    public void StartHeightChallenge()
    {
        _isHeightChallengeActive = true;
        _currentMaxHeight = 0f;

        // 重置游戏状态，防止球提前运动
        if (GameState.Instance != null)
        {
            GameState.Instance.isGameStarted = false;
        }

        // 重置并静止球的状态
        if (_cachedBall != null)
        {
            // 重置球位置
            _cachedBall.transform.position = new Vector3(0, 1.8f, 0.5f);
            
            Rigidbody rb = _cachedBall.GetComponent<Rigidbody>();
            if (rb != null)
            {
                _originalBallIsKinematic = rb.isKinematic;
                // 确保球静止不动
                rb.isKinematic = true;
            }
        }

        // 隐藏所有UI
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

        // 隐藏挑战按钮
        if (_challengeButton != null)
        {
            _challengeButton.gameObject.SetActive(false);
        }

        // 显示挑战面板和实时高度
        if (_heightChallengePanel != null)
        {
            _heightChallengePanel.SetActive(true);
        }

        if (_heightText != null)
        {
            _heightText.gameObject.SetActive(true);
            _heightText.text = "0.0 M";
        }

        // 隐藏最终高度显示
        if (_finalHeightText != null)
        {
            _finalHeightText.gameObject.SetActive(false);
        }

        // 隐藏返回按钮
        if (_exitChallengeButton != null)
        {
            _exitChallengeButton.gameObject.SetActive(false);
        }

        // 隐藏主界面的按钮（除了PlayButton）
        HideMainUIButtons();

        // 显示PlayButton用于开始挑战
        GameObject playButton = GameObject.Find("PlayButton");
        if (playButton != null)
        {
            playButton.SetActive(true);
            
            // 重置PlayButton状态
            PlayButton pb = playButton.GetComponent<PlayButton>();
            if (pb != null)
            {
                pb.Reset();
            }
        }
    }

    private void HideMainUIButtons()
    {
        // 隐藏ChallengeBtn
        GameObject challengeBtn = GameObject.Find("ChallengeBtn");
        if (challengeBtn != null)
        {
            challengeBtn.SetActive(false);
        }

        // 隐藏商店按钮
        if (ShopManager2.Instance != null)
        {
            ShopManager2.Instance.HideShopButton();
        }
    }

    private void Update()
    {
        if (!_isHeightChallengeActive) return;

        if (_cachedBall != null)
        {
            float currentHeight = _cachedBall.transform.position.y;

            if (_heightText != null)
            {
                _heightText.text = $"{currentHeight:F1} M";
            }

            // 持续记录高度，直到球落地
            if (currentHeight > _currentMaxHeight)
            {
                _currentMaxHeight = currentHeight;
            }
        }
    }

    public void OnHeightChallengeBallLanded()
    {
        if (!_isHeightChallengeActive) return;

        _isHeightChallengeActive = false;
        
        // 恢复球的重力设置
        if (_cachedBall != null)
        {
            BallController ballController = _cachedBall.GetComponent<BallController>();
            if (ballController != null)
            {
                ballController.ResetGravityScale();
            }
        }

        // 确保挑战面板显示
        if (_heightChallengePanel != null)
        {
            _heightChallengePanel.SetActive(true);
        }

        // 隐藏实时高度显示
        if (_heightText != null)
        {
            _heightText.gameObject.SetActive(false);
        }

        // 显示最终高度
        if (_finalHeightText != null)
        {
            _finalHeightText.gameObject.SetActive(true);
            _finalHeightText.text = $"{_currentMaxHeight:F1} M";
        }

        // 显示返回按钮
        if (_exitChallengeButton != null)
        {
            _exitChallengeButton.gameObject.SetActive(true);
        }
    }
    
    public void UpdateMaxHeight(float height)
    {
        if (height > _currentMaxHeight)
        {
            _currentMaxHeight = height;
        }
    }

    private void RestoreBallState()
    {
        if (_cachedBall != null)
        {
            Rigidbody rb = _cachedBall.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = _originalBallIsKinematic;
            }
        }
    }

    public void ExitHeightChallenge()
    {
        _isHeightChallengeActive = false;

        // 恢复足球原始状态
        RestoreBallState();
        
        // 恢复球的重力设置
        if (_cachedBall != null)
        {
            BallController ballController = _cachedBall.GetComponent<BallController>();
            if (ballController != null)
            {
                ballController.ResetGravityScale();
            }
        }

        // 隐藏挑战面板
        if (_heightChallengePanel != null)
        {
            _heightChallengePanel.SetActive(false);
        }

        if (_heightText != null)
        {
            _heightText.gameObject.SetActive(false);
        }

        if (_finalHeightText != null)
        {
            _finalHeightText.gameObject.SetActive(false);
        }

        if (_exitChallengeButton != null)
        {
            _exitChallengeButton.gameObject.SetActive(false);
        }

        // 返回主界面
        MainMenu();

        // 在 MainMenu() 之后恢复挑战按钮显示
        if (_challengeButton != null)
        {
            _challengeButton.gameObject.SetActive(true);
        }
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

    public bool IsHeightChallengeMode()
    {
        return _isHeightChallengeActive;
    }
}
