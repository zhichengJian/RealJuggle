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

        ApplySkin(ItemType.Jersey, SaveManager.Instance.CurrentJersey);
        ApplySkin(ItemType.Shoe, SaveManager.Instance.CurrentShoe);
        ApplySkin(ItemType.Ball, SaveManager.Instance.CurrentBall);
    }

    public void ApplySkin(ItemType type, string skinId)
    {
        Debug.Log($"SkinEquipManager.ApplySkin 被调用: type={type}, skinId={skinId}");
        switch (type)
        {
            case ItemType.Jersey:
                ApplyJerseySkin(skinId);
                break;
            case ItemType.Shoe:
                ApplyShoeSkin(skinId);
                break;
            case ItemType.Ball:
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
        Debug.Log($"查找材质: skinId={skinId}, materials长度={materials?.Length ?? 0}, names长度={names?.Length ?? 0}");
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == skinId && i < materials.Length)
            {
                Debug.Log($"找到材质: index={i}, material={materials[i]?.name}");
                return materials[i];
            }
        }
        Debug.LogError($"未找到材质: skinId={skinId}");
        return null;
    }
}
