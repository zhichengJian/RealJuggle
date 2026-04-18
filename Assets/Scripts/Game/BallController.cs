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
    
    [Header("移动范围")]
    [Tooltip("X轴移动范围")]
    [SerializeField] private float _xMoveRange = 3f;
    
    [Tooltip("Y轴移动范围（最大高度）")]
    [SerializeField] private float _yMoveRange = 5f;
    
    [Tooltip("地面高度（球的最小Y值）")]
    [SerializeField] private float _groundHeight = 0.1f;
    
    private Rigidbody _rigidbody;
    private float _fixedZPosition;
    
    private void Awake()
    {
        // 获取已存在的Rigidbody组件（需要手动添加）
        _rigidbody = GetComponent<Rigidbody>();
        
        // 固定Z轴位置
        _fixedZPosition = transform.position.z;
    }
    
    private void FixedUpdate()
    {
        if (_rigidbody != null)
        {
            // 应用重力缩放
            Physics.gravity = new Vector3(0, -9.81f * _gravityScale, 0);
            
            Vector3 currentPosition = _rigidbody.position;
            Vector3 velocity = _rigidbody.velocity;
            
            // 限制球不能低于地面（反弹效果）
            if (currentPosition.y < _groundHeight)
            {
                currentPosition.y = _groundHeight;
                _rigidbody.position = currentPosition;
                // 反弹：反转Y方向速度
                if (velocity.y < 0)
                {
                    velocity.y = -velocity.y * _groundBounceFactor;
                    _rigidbody.velocity = velocity;
                }
            }
            
            // 限制X轴范围（反弹效果）
            if (Mathf.Abs(currentPosition.x) > _xMoveRange)
            {
                float bounceX = currentPosition.x > 0 ? -1f : 1f;
                currentPosition.x = Mathf.Clamp(currentPosition.x, -_xMoveRange, _xMoveRange);
                _rigidbody.position = currentPosition;
                // 反弹：反转X方向速度
                velocity.x = bounceX * Mathf.Abs(velocity.x) * _wallBounceFactor;
                _rigidbody.velocity = velocity;
            }
            
            // 限制Y轴最大高度
            if (currentPosition.y > _yMoveRange)
            {
                currentPosition.y = _yMoveRange;
                _rigidbody.position = currentPosition;
            }
            
            // 固定Z轴位置（只在必要时限制）
            if (Mathf.Abs(currentPosition.z - _fixedZPosition) > 0.01f)
            {
                currentPosition.z = _fixedZPosition;
                _rigidbody.position = currentPosition;
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // 检测是否碰撞到地面
        if (collision.gameObject.CompareTag("Ground"))
        {
            // 临时注释游戏结束逻辑，方便测试弹跳
            // Debug.Log("游戏结束");
        }
    }
}