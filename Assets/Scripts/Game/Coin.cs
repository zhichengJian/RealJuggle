using UnityEngine;

public class Coin : MonoBehaviour
{
    private bool _isShrinking = false;
    private float _shrinkTime = 0.15f;
    private float _shrinkTimer = 0f;
    private Vector3 _initialScale;
    private Vector3 _savedScale;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _savedScale = transform.localScale;
    }

    private void Start()
    {
        _initialScale = _savedScale;
    }

    private void Update()
    {
        if (_isShrinking)
        {
            _shrinkTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_shrinkTimer / _shrinkTime);
            transform.localScale = _initialScale * Mathf.Lerp(1f, 0f, progress);

            if (progress >= 1f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        _isShrinking = false;
        _shrinkTimer = 0f;
        transform.localScale = _savedScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!GameState.Instance.isGameStarted) return;

        if (other.CompareTag("Ball"))
        {
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddCoins();
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnCoinCollected();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoinSound();
            }

            _isShrinking = true;
        }
    }
}