using UnityEngine;
using UnityEngine.UI;

public class ShopScrollView : MonoBehaviour
{
    [Header("滑动设置")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _content;
    [SerializeField] private float _itemWidth = 200f;
    [SerializeField] private float _itemSpacing = 20f;

    private void Awake()
    {
        if (_scrollRect == null)
        {
            _scrollRect = GetComponent<ScrollRect>();
        }
        
        if (_content == null && _scrollRect != null)
        {
            _content = _scrollRect.content;
        }
    }

    private void Start()
    {
        ConfigureScrollRect();
        UpdateContentWidth();
    }

    private void ConfigureScrollRect()
    {
        if (_scrollRect == null) return;
        
        _scrollRect.horizontal = true;
        _scrollRect.vertical = false;
        _scrollRect.movementType = ScrollRect.MovementType.Clamped;
        _scrollRect.elasticity = 0.1f;
        _scrollRect.inertia = true;
        _scrollRect.decelerationRate = 0.135f;
        
        if (_scrollRect.verticalScrollbar != null)
        {
            _scrollRect.verticalScrollbar.gameObject.SetActive(false);
        }
    }

    private void UpdateContentWidth()
    {
        if (_content == null) return;
        
        HorizontalLayoutGroup layoutGroup = _content.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null) return;

        int itemCount = _content.childCount;
        if (itemCount == 0) return;

        RectTransform firstChild = _content.GetChild(0) as RectTransform;
        float itemWidth = firstChild != null ? firstChild.rect.width : _itemWidth;
        float paddingLeft = layoutGroup.padding.left;
        float paddingRight = layoutGroup.padding.right;
        float itemSpacing = layoutGroup.spacing;

        float totalWidth = paddingLeft + paddingRight + itemCount * (itemWidth + itemSpacing) - itemSpacing;
        _content.sizeDelta = new Vector2(totalWidth, _content.sizeDelta.y);
    }

    public void ScrollToItem(int index)
    {
        if (_content == null || index < 0 || index >= _content.childCount) return;
        
        float targetX = -index * (_itemWidth + _itemSpacing);
        _content.anchoredPosition = new Vector2(targetX, _content.anchoredPosition.y);
    }
}