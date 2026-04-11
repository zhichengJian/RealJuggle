using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("物理参数")]
    [Tooltip("下落速度（初始速度）")]
    [SerializeField] private float _fallSpeed = 0f;
    
    [Tooltip("重力缩放")]
    [SerializeField] private float _gravityScale = 1f;
    
    private Rigidbody _rigidbody;
    private Vector3 _originalGravity;
    
    private void Awake()
    {
        // 自动添加Rigidbody组件
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
            // 设置Rigidbody属性
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        
        // 自动添加SphereCollider组件
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
            // 适配足球模型大小
            collider.radius = 0.5f;
        }
        
        // 保存原始重力值
        _originalGravity = Physics.gravity;
        
        // 设置初始下落速度
        _rigidbody.velocity = new Vector3(0, -_fallSpeed, 0);
    }
    
    private void FixedUpdate()
    {
        // 应用自定义重力
        if (_rigidbody != null && _rigidbody.useGravity)
        {
            _rigidbody.AddForce(_originalGravity * _gravityScale, ForceMode.Acceleration);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // 检测是否碰撞到地面
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("游戏结束");
        }
    }
    
    // 更新重力缩放（当Inspector面板值变化时）
    private void OnValidate()
    {
        // 无需在OnValidate中更新，因为FixedUpdate会实时应用重力缩放
    }
}