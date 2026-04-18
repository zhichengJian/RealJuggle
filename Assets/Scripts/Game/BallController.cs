using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("物理参数")]
    [Tooltip("重力缩放")]
    [SerializeField] private float _gravityScale = 1f;
    
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
            
            // 限制球不能低于地面（只在极端情况下限制）
            if (currentPosition.y < _groundHeight - 0.05f)
            {
                currentPosition.y = _groundHeight;
                _rigidbody.position = currentPosition;
            }
            
            // 限制X轴范围
            if (Mathf.Abs(currentPosition.x) > _xMoveRange)
            {
                currentPosition.x = Mathf.Clamp(currentPosition.x, -_xMoveRange, _xMoveRange);
                _rigidbody.position = currentPosition;
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