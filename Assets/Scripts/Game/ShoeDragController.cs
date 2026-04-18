using UnityEngine;

public class RealJuggleShoe : MonoBehaviour
{
    public Camera mainCamera;
    public float moveSpeed = 10f;
    public float returnSpeed = 5f;
    public float xMoveRange = 2f;
    public float yMoveRange = 2f; // 控制Y轴移动范围和踢腿抬起高度
    public float kickZDistance = 0.3f; // 踢腿Z轴前移距离
    public float kickForce = 8f; // 踢腿力度

    private bool isDrag = false;
    private Vector3 initialPosition;
    private Vector3 kickTargetPosition;
    private float fixedZ;
    private Vector3 lastPosition; // 记录上一帧位置，用于计算速度

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // 保存初始位置
        initialPosition = transform.position;
        lastPosition = initialPosition;
        // 固定深度，永远不前后移动（用于屏幕坐标转换）
        fixedZ = mainCamera.WorldToScreenPoint(transform.position).z;
    }

    void Update()
    {
        // 按下
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    isDrag = true;
                    // 计算踢腿目标位置：Z轴前移，Y轴抬起
                    kickTargetPosition = initialPosition;
                    kickTargetPosition.z += kickZDistance;
                    kickTargetPosition.y += yMoveRange; // 使用yMoveRange作为踢腿抬起高度
                }
            }
        }

        // 抬起
        if (Input.GetMouseButtonUp(0))
        {
            isDrag = false;
        }

        // 拖动
        if (isDrag)
        {
            // 获取鼠标在世界空间中的位置（X/Y轴）
            Vector3 screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, fixedZ);
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);

            // 限制X/Y移动范围
            float targetX = Mathf.Clamp(worldPoint.x, initialPosition.x - xMoveRange, initialPosition.x + xMoveRange);
            float targetY = Mathf.Clamp(worldPoint.y, Mathf.Max(0f, initialPosition.y), initialPosition.y + yMoveRange * 2); // 增加Y轴移动范围

            // 计算当前目标位置：结合鼠标X/Y和踢腿Z/Y
            Vector3 currentTarget = new Vector3(targetX, targetY, kickTargetPosition.z);

            // 平滑移动到目标位置
            lastPosition = transform.position;
            transform.position = Vector3.Lerp(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        }
        // 平滑回到初始位置
        else
        {
            // 确保Y轴不小于0
            Vector3 targetPosition = initialPosition;
            targetPosition.y = Mathf.Max(0f, targetPosition.y);
            lastPosition = transform.position;
            transform.position = Vector3.Lerp(transform.position, targetPosition, returnSpeed * Time.deltaTime);
        }
    }

    // 碰撞检测，实现颠球
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Rigidbody ballRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if (ballRigidbody != null)
            {
                // 计算鞋子移动速度
                Vector3 shoeVelocity = (transform.position - lastPosition) / Time.deltaTime;
                
                // 计算踢腿方向：向上为主，带一点鞋子移动方向
                Vector3 kickDirection = Vector3.up;
                if (shoeVelocity.magnitude > 0.1f)
                {
                    // 调整方向计算，增加鞋子速度的影响
                    kickDirection = (Vector3.up * 0.6f + shoeVelocity.normalized * 0.4f).normalized;
                }
                
                // 计算实际踢力，根据鞋子速度动态调整
                float actualKickForce = kickForce;
                if (shoeVelocity.magnitude > 1.0f)
                {
                    // 根据鞋子速度增加踢力
                    actualKickForce = kickForce * (1.0f + Mathf.Min(shoeVelocity.magnitude * 0.5f, 2.0f));
                }
                
                // 施加踢力
                ballRigidbody.AddForce(kickDirection * actualKickForce, ForceMode.Impulse);
                
                // 添加随机旋转
                Vector3 randomTorque = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f)
                ) * actualKickForce * 0.3f;
                
                ballRigidbody.AddTorque(randomTorque, ForceMode.Impulse);
            }
        }
    }
    
    // 添加OnTriggerEnter作为碰撞检测的补充，防止快速移动时穿过
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            Rigidbody ballRigidbody = other.gameObject.GetComponent<Rigidbody>();
            if (ballRigidbody != null)
            {
                // 计算鞋子移动速度
                Vector3 shoeVelocity = (transform.position - lastPosition) / Time.deltaTime;
                
                // 计算踢腿方向
                Vector3 kickDirection = Vector3.up;
                if (shoeVelocity.magnitude > 0.1f)
                {
                    kickDirection = (Vector3.up * 0.6f + shoeVelocity.normalized * 0.4f).normalized;
                }
                
                // 计算实际踢力
                float actualKickForce = kickForce;
                if (shoeVelocity.magnitude > 1.0f)
                {
                    actualKickForce = kickForce * (1.0f + Mathf.Min(shoeVelocity.magnitude * 0.5f, 2.0f));
                }
                
                // 施加踢力
                ballRigidbody.AddForce(kickDirection * actualKickForce, ForceMode.Impulse);
            }
        }
    }
}