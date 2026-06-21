using UnityEngine;

public class BallFallDetector : MonoBehaviour
{
    [Header("检测设置")]
    public float fallThreshold = -5f;
    public float minVelocity = 5f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (LevelManager.Instance == null) return;
        
        if (transform.position.y < fallThreshold)
        {
            if (rb != null && rb.velocity.y < -minVelocity)
            {
                LevelManager.Instance.OnHeightChallengeBallLanded();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (LevelManager.Instance == null) return;
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            LevelManager.Instance.OnHeightChallengeBallLanded();
        }
    }
}
