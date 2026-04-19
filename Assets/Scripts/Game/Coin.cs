using UnityEngine;

public class Coin : MonoBehaviour
{
    private bool _isShrinking = false;
    private float _shrinkTime = 0.15f;
    private float _shrinkTimer = 0f;
    private Vector3 _initialScale;

    private void Start()
    {
        _initialScale = transform.localScale;
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
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            _isShrinking = true;
        }
    }
}
