using UnityEngine;

public class ShopItemRow : MonoBehaviour
{
    [Header("行配置")]
    [SerializeField] private float _itemSpacing = 20f;
    [SerializeField] private float _itemWidth = 100f;

    private void Awake()
    {
        ArrangeItems();
    }

    private void ArrangeItems()
    {
        float currentX = 0f;
        
        foreach (Transform child in transform)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect != null)
            {
                childRect.anchoredPosition = new Vector2(currentX, 0);
                currentX += _itemWidth + _itemSpacing;
            }
        }
    }
}