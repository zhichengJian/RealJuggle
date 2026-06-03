using UnityEngine;

/// <summary>
/// 微信小游戏数据上报脚本
/// 解决 Console 报错：不遵循线上版本请使用自定义上报能力 WX.ReportGameStart
/// 仅在微信小游戏环境下生效
/// </summary>
public class WeChatReport : MonoBehaviour
{
    void Start()
    {
        // 暂时注释掉，等微信SDK正确导入后再恢复
        // #if UNITY_WEIXINMINIGAME
        // // 上报游戏启动数据（审核和数据分析需要）
        // WX.ReportGameStart(new ReportGameStartOption
        // {
        //     // 基础信息会自动填充，无需手动设置
        // });
        // 
        // UnityEngine.Debug.Log("[WeChatReport] 游戏启动数据已上报");
        // #endif
    }
}
