using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("摄像机设置")]
    [SerializeField] private Vector3 _targetPosition = new Vector3(0, 1.2f, 3.3f);
    [SerializeField] private float _moveSpeed = 10f;
    
    private bool _isMoving = false;
    
    public static CameraController Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartMovement()
    {
        _isMoving = true;
    }
    
    private void Update()
    {
        if (_isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPosition,
                _moveSpeed * Time.deltaTime
            );
            
            if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
            {
                transform.position = _targetPosition;
                _isMoving = false;
            }
        }
    }
}
