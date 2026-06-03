using UnityEngine;
using System.Collections;

public class RealJuggleShoe : MonoBehaviour
{
    public Camera mainCamera;
    public float moveSpeed = 10f;
    public float returnSpeed = 5f;
    public float xMoveRange = 2f;
    public float yMoveRange = 2f;
    public float kickZDistance = 0.3f;
    public float kickForce = 8f;
    
    [Header("触控范围")]
    [Tooltip("触控X轴范围（屏幕像素）")]
    [SerializeField] private float _touchRangeX = 100f;
    [Tooltip("触控Y轴范围（向下延伸，屏幕像素）")]
    [SerializeField] private float _touchRangeYDown = 150f;
    [Tooltip("触控Y轴范围（向上延伸，屏幕像素）")]
    [SerializeField] private float _touchRangeYUp = 100f;
    [Tooltip("触控范围偏移（屏幕像素，X=左右，Y=上下）")]
    [SerializeField] private Vector2 _touchOffset = Vector2.zero;

    private bool isDrag = false;
    private Vector3 initialPosition;
    private Vector3 kickTargetPosition;
    private float fixedZ;
    private Vector3 lastPosition;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        initialPosition = transform.position;
        lastPosition = initialPosition;
        fixedZ = mainCamera.WorldToScreenPoint(transform.position).z;
    }

    void Update()
    {
        if (!GameState.Instance.isGameStarted) return;

        HandleInput();
        UpdateMovement();
    }

    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (CheckTouchHit(touch.position))
                {
                    StartDrag(touch.position);
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                EndDrag();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (CheckMouseHit(Input.mousePosition))
                {
                    StartDrag(Input.mousePosition);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }
        }
    }

    private bool CheckTouchHit(Vector2 touchPosition)
    {
        return CheckScreenDistance(touchPosition);
    }

    private bool CheckMouseHit(Vector3 mousePosition)
    {
        return CheckScreenDistance(mousePosition);
    }

    private bool CheckScreenDistance(Vector3 screenPosition)
    {
        Vector3 shoeScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        shoeScreenPos.x += _touchOffset.x;
        shoeScreenPos.y += _touchOffset.y;
        
        float deltaX = Mathf.Abs(screenPosition.x - shoeScreenPos.x);
        float deltaY = screenPosition.y - shoeScreenPos.y;
        
        bool inXRange = deltaX < _touchRangeX;
        bool inYRange = deltaY > -_touchRangeYDown && deltaY < _touchRangeYUp;
        
        return inXRange && inYRange;
    }
    
    private void OnDrawGizmos()
    {
        DrawTouchArea();
    }
    
    private void OnDrawGizmosSelected()
    {
        DrawTouchArea();
        
        if (mainCamera == null) return;
        
        Vector3 shoeScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        Debug.Log("球鞋屏幕位置: " + shoeScreenPos + ", 参数范围: X=" + _touchRangeX + ", YDown=" + _touchRangeYDown + ", YUp=" + _touchRangeYUp + ", 偏移: " + _touchOffset);
    }
    
    private void DrawTouchArea()
    {
        if (mainCamera == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        
        Vector3 shoeScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        shoeScreenPos.x += _touchOffset.x;
        shoeScreenPos.y += _touchOffset.y;
        
        float minX = shoeScreenPos.x - _touchRangeX;
        float maxX = shoeScreenPos.x + _touchRangeX;
        float minY = shoeScreenPos.y - _touchRangeYDown;
        float maxY = shoeScreenPos.y + _touchRangeYUp;
        
        float z = shoeScreenPos.z;
        
        Vector3 bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(minX, minY, z));
        Vector3 bottomRight = mainCamera.ScreenToWorldPoint(new Vector3(maxX, minY, z));
        Vector3 topLeft = mainCamera.ScreenToWorldPoint(new Vector3(minX, maxY, z));
        Vector3 topRight = mainCamera.ScreenToWorldPoint(new Vector3(maxX, maxY, z));
        
        Gizmos.color = new Color(0f, 1f, 0f, 0.7f);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
        
        Vector3 offsetWorldPos = mainCamera.ScreenToWorldPoint(shoeScreenPos);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f);
        Gizmos.DrawWireSphere(offsetWorldPos, 0.1f);
    }

    private void StartDrag(Vector3 screenPosition)
    {
        isDrag = true;
        kickTargetPosition = initialPosition;
        kickTargetPosition.z += kickZDistance;
        kickTargetPosition.y += yMoveRange;
    }

    private void EndDrag()
    {
        isDrag = false;
    }
    
    public void ResetPosition()
    {
        isDrag = false;
        StartCoroutine(SmoothResetPosition());
    }
    
    private IEnumerator SmoothResetPosition()
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        float resetDuration = 0.5f;
        
        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / resetDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPosition, initialPosition, t);
            yield return null;
        }
        
        transform.position = initialPosition;
    }

    private void UpdateMovement()
    {
        Vector3 currentScreenPosition = Vector3.zero;

        if (isDrag)
        {
            if (Input.touchCount > 0)
            {
                currentScreenPosition = Input.GetTouch(0).position;
            }
            else
            {
                currentScreenPosition = Input.mousePosition;
            }

            Vector3 screenPoint = new Vector3(currentScreenPosition.x, currentScreenPosition.y, fixedZ);
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);

            float targetX = Mathf.Clamp(worldPoint.x, initialPosition.x - xMoveRange, initialPosition.x + xMoveRange);
            float targetY = Mathf.Clamp(worldPoint.y, Mathf.Max(0f, initialPosition.y), initialPosition.y + yMoveRange * 2);

            Vector3 currentTarget = new Vector3(targetX, targetY, kickTargetPosition.z);

            lastPosition = transform.position;
            transform.position = Vector3.Lerp(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 targetPosition = initialPosition;
            targetPosition.y = Mathf.Max(0f, targetPosition.y);
            lastPosition = transform.position;
            transform.position = Vector3.Lerp(transform.position, targetPosition, returnSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!GameState.Instance.isGameStarted) return;
        
        if (collision.gameObject.CompareTag("Ball"))
        {
            Rigidbody ballRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if (ballRigidbody != null)
            {
                Vector3 shoeVelocity = (transform.position - lastPosition) / Time.deltaTime;
                
                Vector3 kickDirection = Vector3.up;
                if (shoeVelocity.magnitude > 0.1f)
                {
                    kickDirection = (Vector3.up * 0.6f + shoeVelocity.normalized * 0.4f).normalized;
                }
                
                float actualKickForce = kickForce;
                if (shoeVelocity.magnitude > 1.0f)
                {
                    actualKickForce = kickForce * (1.0f + Mathf.Min(shoeVelocity.magnitude * 0.5f, 2.0f));
                }
                
                ballRigidbody.AddForce(kickDirection * actualKickForce, ForceMode.Impulse);
                
                Vector3 randomTorque = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f)
                ) * actualKickForce * 0.3f;
                
                ballRigidbody.AddTorque(randomTorque, ForceMode.Impulse);
                
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.OnBallJuggled();
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayKickSound();
                }
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!GameState.Instance.isGameStarted) return;
        
        if (other.gameObject.CompareTag("Ball"))
        {
            Rigidbody ballRigidbody = other.gameObject.GetComponent<Rigidbody>();
            if (ballRigidbody != null)
            {
                Vector3 shoeVelocity = (transform.position - lastPosition) / Time.deltaTime;
                
                Vector3 kickDirection = Vector3.up;
                if (shoeVelocity.magnitude > 0.1f)
                {
                    kickDirection = (Vector3.up * 0.6f + shoeVelocity.normalized * 0.4f).normalized;
                }
                
                float actualKickForce = kickForce;
                if (shoeVelocity.magnitude > 1.0f)
                {
                    actualKickForce = kickForce * (1.0f + Mathf.Min(shoeVelocity.magnitude * 0.5f, 2.0f));
                }
                
                ballRigidbody.AddForce(kickDirection * actualKickForce, ForceMode.Impulse);
                
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.OnBallJuggled();
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayKickSound();
                }
            }
        }
    }
}
