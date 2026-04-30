using UnityEngine;

public class Brick : MonoBehaviour
{
    private bool _isShrinking = false;
    private float _shrinkTime = 0.1f;
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
            float shrinkProgress = Mathf.Clamp01(_shrinkTimer / _shrinkTime);
            float currentScale = Mathf.Lerp(1f, 0f, shrinkProgress);
            transform.localScale = _initialScale * currentScale;

            if (shrinkProgress >= 1f)
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.OnBrickDestroyed();
                }
                gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        _isShrinking = false;
        _shrinkTimer = 0f;
        transform.localScale = _savedScale;
        if (_collider != null)
        {
            _collider.enabled = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!GameState.Instance.isGameStarted) return;

        if (collision.gameObject.CompareTag("Ball"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBrickSound();
            }

            _isShrinking = true;
        }
    }
}