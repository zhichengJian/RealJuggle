using UnityEngine;

public class Brick : MonoBehaviour
{
    private bool _isShrinking = false; // 标记是否正在缩小
    private float _shrinkTime = 0.1f; // 缩小动画时间（更快）
    private float _shrinkTimer = 0f; // 缩小计时器
    private Vector3 _initialScale; // 初始缩放

    private void Start()
    {
        // 记录初始缩放
        _initialScale = transform.localScale;
    }

    private void Update()
    {
        // 如果正在缩小
        if (_isShrinking)
        {
            // 更新计时器
            _shrinkTimer += Time.deltaTime;
            
            // 计算缩小进度（0到1之间）
            float shrinkProgress = Mathf.Clamp01(_shrinkTimer / _shrinkTime);
            
            // 计算当前缩放（从初始缩放到0）
            float currentScale = Mathf.Lerp(1f, 0f, shrinkProgress);
            transform.localScale = _initialScale * currentScale;
            
            // 缩小完成后销毁物体
            if (shrinkProgress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 检测是否被Ball碰撞
        if (collision.gameObject.CompareTag("Ball"))
        {
            // 开始缩小
            _isShrinking = true;
        }
    }
}
