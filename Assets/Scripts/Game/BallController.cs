using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("物理参数")]
    [Tooltip("重力缩放")]
    [SerializeField] private float _gravityScale = 1f;
    [Tooltip("地面反弹系数")]
    [SerializeField] private float _groundBounceFactor = 0.7f;
    [Tooltip("左右边界反弹系数")]
    [SerializeField] private float _wallBounceFactor = 0.8f;
    [Tooltip("砖块反弹系数")]
    [SerializeField] private float _brickBounceFactor = 0.5f;
    
    [Header("延时参数")]
    [Tooltip("球开始下落的延时时间（秒）")]
    [SerializeField] private float _startDelay = 1f;
    
    [Header("移动范围")]
    [Tooltip("X轴移动范围")]
    [SerializeField] private float _xMoveRange = 3f;
    
    [Tooltip("Y轴移动范围（最大高度）")]
    [SerializeField] private float _yMoveRange = 5f;
    
    [Tooltip("地面高度（球的最小Y值）")]
    [SerializeField] private float _groundHeight = 0.1f;
    
    private Rigidbody _rigidbody;
    private float _fixedZPosition;
    private bool _hasStarted = false;
    private bool _isGameOver = false;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }
        _fixedZPosition = transform.position.z;
    }
    
    public void Kick()
    {
        if (_hasStarted) return;
        _hasStarted = true;
        
        if (_rigidbody != null && _rigidbody.isKinematic)
        {
            StartCoroutine(DelayedStart());
        }
    }
    
    private System.Collections.IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(_startDelay);
        
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = false;
        }
    }
    
    private void FixedUpdate()
    {
        if (_isGameOver) return;
        
        if (_rigidbody != null && GameState.Instance.isGameStarted && !_rigidbody.isKinematic)
        {
            Physics.gravity = new Vector3(0, -9.81f * _gravityScale, 0);
            
            Vector3 currentPosition = _rigidbody.position;
            Vector3 velocity = _rigidbody.velocity;
            
            if (currentPosition.y < _groundHeight)
            {
                currentPosition.y = _groundHeight;
                _rigidbody.position = currentPosition;
                if (velocity.y < 0)
                {
                    velocity.y = -velocity.y * _groundBounceFactor;
                    _rigidbody.velocity = velocity;
                }
            }
            
            if (Mathf.Abs(currentPosition.x) > _xMoveRange)
            {
                float bounceX = currentPosition.x > 0 ? -1f : 1f;
                currentPosition.x = Mathf.Clamp(currentPosition.x, -_xMoveRange, _xMoveRange);
                _rigidbody.position = currentPosition;
                velocity.x = bounceX * Mathf.Abs(velocity.x) * _wallBounceFactor;
                _rigidbody.velocity = velocity;
            }
            
            if (currentPosition.y > _yMoveRange)
            {
                currentPosition.y = _yMoveRange;
                _rigidbody.position = currentPosition;
            }
            
            if (Mathf.Abs(currentPosition.z - _fixedZPosition) > 0.01f)
            {
                currentPosition.z = _fixedZPosition;
                _rigidbody.position = currentPosition;
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!GameState.Instance.isGameStarted || _isGameOver) return;
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGameOver = true;
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.GameOver();
            }
        }
        else if (collision.gameObject.CompareTag("Brick"))
        {
            Vector3 velocity = _rigidbody.velocity;
            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;
            velocity = Vector3.Reflect(velocity, normal) * _brickBounceFactor;
            _rigidbody.velocity = velocity;
        }
    }
    
    public void Reset()
    {
        _hasStarted = false;
        _isGameOver = false;
        
        if (_rigidbody != null)
        {
            if (!_rigidbody.isKinematic)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
            _rigidbody.isKinematic = true;
        }
    }
}