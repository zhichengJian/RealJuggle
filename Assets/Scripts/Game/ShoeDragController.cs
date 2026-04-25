using UnityEngine;

public class RealJuggleShoe : MonoBehaviour
{
    public Camera mainCamera;
    public float moveSpeed = 10f;
    public float returnSpeed = 5f;
    public float xMoveRange = 2f;
    public float yMoveRange = 2f;
    public float kickZDistance = 0.3f;
    public float kickForce = 8f;

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
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    isDrag = true;
                    kickTargetPosition = initialPosition;
                    kickTargetPosition.z += kickZDistance;
                    kickTargetPosition.y += yMoveRange;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrag = false;
        }

        if (isDrag)
        {
            Vector3 screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, fixedZ);
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
            }
        }
    }
}