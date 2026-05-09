using UnityEngine;

public class ShopSwipeController : MonoBehaviour
{
    [Header("滑动设置")]
    [SerializeField] private float _moveSpeed = 0.15f;
    [SerializeField] private float _inertiaFactor = 0.92f;
    [SerializeField] private float _minInertiaSpeed = 1f;

    private Vector2 _startTouchPos;
    private Vector2 _lastTouchPos;
    private float _velocity = 0f;
    private bool _isDragging = false;

    private RectTransform _rectTransform;
    private float _maxScrollX;
    private bool _hasCalculated = false;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!_hasCalculated && gameObject.activeInHierarchy)
        {
            CalculateMaxScroll();
            _hasCalculated = true;
        }

        HandleInput();
        
        if (!_isDragging)
        {
            ApplyInertia();
        }
    }

    private void CalculateMaxScroll()
    {
        _maxScrollX = 0f;
        
        foreach (Transform row in transform)
        {
            float rowMaxX = 0f;
            
            foreach (Transform child in row)
            {
                RectTransform childRect = child.GetComponent<RectTransform>();
                if (childRect != null)
                {
                    float rightEdge = childRect.anchoredPosition.x + childRect.sizeDelta.x;
                    if (rightEdge > rowMaxX)
                    {
                        rowMaxX = rightEdge;
                    }
                }
            }
            
            if (rowMaxX > _maxScrollX)
            {
                _maxScrollX = rowMaxX;
            }
        }
        
        if (_rectTransform != null)
        {
            _maxScrollX -= _rectTransform.sizeDelta.x;
            _maxScrollX = Mathf.Max(_maxScrollX, 0f);
        }
    }

    private void HandleInput()
    {
        if (!_hasCalculated) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _startTouchPos = touch.position;
                    _lastTouchPos = touch.position;
                    _isDragging = true;
                    _velocity = 0f;
                    break;

                case TouchPhase.Moved:
                    if (_isDragging)
                    {
                        Vector2 currentPos = touch.position;
                        float deltaX = currentPos.x - _lastTouchPos.x;
                        _velocity = deltaX * 2f;
                        MoveContent(deltaX);
                        _lastTouchPos = currentPos;
                    }
                    break;

                case TouchPhase.Ended:
                    if (_isDragging)
                    {
                        _isDragging = false;
                    }
                    break;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            
            if (!_isDragging)
            {
                _startTouchPos = mousePos;
                _lastTouchPos = mousePos;
                _isDragging = true;
                _velocity = 0f;
            }
            else
            {
                float deltaX = mousePos.x - _lastTouchPos.x;
                _velocity = deltaX * 2f;
                MoveContent(deltaX);
                _lastTouchPos = mousePos;
            }
        }
        else if (_isDragging && Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }
    }

    private void MoveContent(float delta)
    {
        foreach (Transform row in transform)
        {
            RectTransform rowRect = row.GetComponent<RectTransform>();
            if (rowRect != null)
            {
                Vector2 currentPos = rowRect.anchoredPosition;
                currentPos.x += delta * _moveSpeed;
                currentPos.x = Mathf.Clamp(currentPos.x, -_maxScrollX, 0);
                rowRect.anchoredPosition = currentPos;
            }
        }
    }

    private void ApplyInertia()
    {
        if (Mathf.Abs(_velocity) > _minInertiaSpeed)
        {
            foreach (Transform row in transform)
            {
                RectTransform rowRect = row.GetComponent<RectTransform>();
                if (rowRect != null)
                {
                    Vector2 currentPos = rowRect.anchoredPosition;
                    currentPos.x += _velocity * _moveSpeed;
                    currentPos.x = Mathf.Clamp(currentPos.x, -_maxScrollX, 0);
                    rowRect.anchoredPosition = currentPos;
                }
            }
            _velocity *= _inertiaFactor;
        }
        else if (Mathf.Abs(_velocity) > 0)
        {
            _velocity = 0;
        }
    }

    public void ResetCalculation()
    {
        _hasCalculated = false;
    }
}