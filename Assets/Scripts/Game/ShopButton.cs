using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    private Button _button;

    private void Start()
    {
        _button = GetComponent<Button>();

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OpenShop);
        }
    }

    private void OpenShop()
    {
        if (ShopManager2.Instance != null)
        {
            ShopManager2.Instance.OpenShop();
        }
    }
}
