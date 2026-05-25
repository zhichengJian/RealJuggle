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

    private void Start()
    {
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
        }

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.HideShopButton();
        }

        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
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

    public void Reset()
    {
        _isResetting = true;
        _hasStarted = false;

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
