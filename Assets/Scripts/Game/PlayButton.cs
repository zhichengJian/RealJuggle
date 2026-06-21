using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayButton : MonoBehaviour
{
    public Button button;

    [Header("延迟设置")]
    [SerializeField] private float _gameStartDelay = 1f;

    private bool _hasStarted = false;
    private bool _isResetting = false;

    private RectTransform _rectTransform;
    private Image _buttonImage;
    private TextMeshProUGUI _buttonText;
    private GameObject _cachedBall; // 缓存球的引用

    private void Start()
    {
        // 缓存球的引用
        _cachedBall = GameObject.FindGameObjectWithTag("Ball");
        
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        _rectTransform = GetComponent<RectTransform>();
        _buttonImage = GetComponent<Image>();
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(StartGame);
        }
    }

    private void Update()
    {
        if (GameState.Instance == null || GameState.Instance.isGameStarted || _isResetting)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            if (button != null && button.gameObject.activeInHierarchy && button.enabled && _rectTransform != null)
            {
                Vector2 clickPosition = Input.mousePosition;
                    
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    clickPosition = Input.GetTouch(0).position;
                }
                    
                if (RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, clickPosition))
                {
                    StartGame();
                }
            }
        }
    }

    public void StartGame()
    {
        if (_hasStarted)
        {
            return;
        }

        if (GameState.Instance != null && GameState.Instance.isGameStarted)
        {
            return;
        }

        _hasStarted = true;

        StartCoroutine(DelayedStart());
    }

    private System.Collections.IEnumerator DelayedStart()
    {
        if (button != null)
        {
            button.interactable = false;
            button.enabled = false;

            if (_buttonImage != null)
            {
                _buttonImage.enabled = false;
            }

            if (_buttonText != null)
            {
                _buttonText.enabled = false;
            }
        }

        yield return new WaitForSeconds(_gameStartDelay);

        // 检查是否是挑战模式
        if (LevelManager.Instance != null && LevelManager.Instance.IsHeightChallengeMode())
        {
            // 设置游戏状态为已开始
            if (GameState.Instance != null)
            {
                GameState.Instance.isGameStarted = true;
            }
            
            // 隐藏 PlayButton
            GameObject playButtonObj = GameObject.Find("PlayButton");
            if (playButtonObj != null)
            {
                playButtonObj.SetActive(false);
            }
            
            // 挑战模式：启用物理，应用低重力，玩家用鞋子踢球
            GameObject ball = _cachedBall;
            if (ball != null)
            {
                Rigidbody rb = ball.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 先禁用 Kinematic，让球受重力影响下落
                    if (rb.isKinematic)
                    {
                        rb.isKinematic = false;
                    }
                    
                    // 设置速度
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    
                    // 挑战模式：使用默认重力，让鞋子踢力决定球的高度
                }
            }
        }
        else
        {
            // 普通游戏模式
            if (CameraController.Instance != null)
            {
                CameraController.Instance.StartMovement();
            }

            if (GameState.Instance == null)
            {
                yield break;
            }

            GameState.Instance.isGameStarted = true;

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ShowJuggleCount();
                
                // 隐藏挑战按钮
                GameObject challengeBtn = GameObject.Find("ChallengeBtn");
                if (challengeBtn == null)
                {
                    challengeBtn = GameObject.Find("挑战");
                }
                if (challengeBtn == null)
                {
                    challengeBtn = GameObject.Find("btn_challenge");
                }
                if (challengeBtn != null)
                {
                    challengeBtn.SetActive(false);
                }
                
                // 隐藏 PlayButton（包括图标）
                gameObject.SetActive(false);
            }

            if (ShopManager2.Instance != null)
            {
                ShopManager2.Instance.HideShopButton();
            }

            GameObject ball = _cachedBall;
            if (ball == null)
            {
                yield break;
            }

            BallController ballController = ball.GetComponent<BallController>();
            if (ballController == null)
            {
                yield break;
            }

            ballController.Kick();
        }
    }

    public void Reset()
    {
        _isResetting = true;
        _hasStarted = false;

        // 重新显示PlayButton（包括图标）
        gameObject.SetActive(true);

        if (button != null)
        {
            button.interactable = true;
            button.enabled = true;

            if (_buttonImage != null)
            {
                _buttonImage.enabled = true;
            }

            if (_buttonText != null)
            {
                _buttonText.enabled = true;
            }
        }

        StartCoroutine(ResetComplete());
    }

    private System.Collections.IEnumerator ResetComplete()
    {
        yield return new WaitForEndOfFrame();
        _isResetting = false;
    }
}
