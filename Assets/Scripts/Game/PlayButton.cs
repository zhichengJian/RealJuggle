using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    public Button button;

    [Header("延迟设置")]
    [SerializeField] private float _gameStartDelay = 1f;

    private bool _hasStarted = false;
    private bool _isResetting = false;

    private void Start()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

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

        if (Input.GetMouseButtonDown(0))
        {
            if (button != null && button.gameObject.activeInHierarchy && button.enabled)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
                    {
                        StartGame();
                    }
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

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.enabled = false;
            }

            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.enabled = false;
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

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.enabled = true;
            }

            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.enabled = true;
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