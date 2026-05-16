using UnityEngine;

public class SkinEquipManager : MonoBehaviour
{
    public static SkinEquipManager Instance { get; private set; }

    [Header("Renderer组件")]
    [SerializeField] private Renderer _bodyRenderer;
    [SerializeField] private Renderer[] _shoeRenderers;
    [SerializeField] private Renderer _ballRenderer;

    [Header("材质数组")]
    [SerializeField] private Material[] _jerseyMaterials;
    [SerializeField] private Material[] _shoeMaterials;
    [SerializeField] private Material[] _ballMaterials;

    [Header("材质名称映射")]
    [SerializeField] private string[] _jerseyNames = { "jersey_0", "jersey_1", "jersey_2", "jersey_3", "jersey_4" };
    [SerializeField] private string[] _shoeNames = { "shoe_0", "shoe_1", "shoe_2", "shoe_3", "shoe_4" };
    [SerializeField] private string[] _ballNames = { "ball_0", "ball_1", "ball_2", "ball_3", "ball_4" };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyInitialSkins();
    }

    private void ApplyInitialSkins()
    {
        if (SaveManager.Instance == null) return;

        ApplySkin(ShopItemType.Jersey, SaveManager.Instance.CurrentJersey);
        ApplySkin(ShopItemType.Shoe, SaveManager.Instance.CurrentShoe);
        ApplySkin(ShopItemType.Ball, SaveManager.Instance.CurrentBall);
    }

    public void ApplySkin(ShopItemType type, string skinId)
    {
        switch (type)
        {
            case ShopItemType.Jersey:
                ApplyJerseySkin(skinId);
                break;
            case ShopItemType.Shoe:
                ApplyShoeSkin(skinId);
                break;
            case ShopItemType.Ball:
                ApplyBallSkin(skinId);
                break;
        }
    }

    private void ApplyJerseySkin(string skinId)
    {
        if (_bodyRenderer == null) return;
        
        Material material = GetMaterial(_jerseyMaterials, _jerseyNames, skinId);
        if (material != null)
        {
            _bodyRenderer.material = material;
        }
    }

    private void ApplyShoeSkin(string skinId)
    {
        if (_shoeRenderers == null || _shoeRenderers.Length == 0) return;
        
        Material material = GetMaterial(_shoeMaterials, _shoeNames, skinId);
        if (material != null)
        {
            foreach (Renderer renderer in _shoeRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = material;
                }
            }
        }
    }

    private void ApplyBallSkin(string skinId)
    {
        if (_ballRenderer == null) return;
        
        Material material = GetMaterial(_ballMaterials, _ballNames, skinId);
        if (material != null)
        {
            _ballRenderer.material = material;
        }
    }

    private Material GetMaterial(Material[] materials, string[] names, string skinId)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == skinId && i < materials.Length)
            {
                return materials[i];
            }
        }
        return null;
    }
}
