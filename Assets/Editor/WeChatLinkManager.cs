using UnityEditor;
using UnityEngine;
using System.IO;

public class WeChatLinkManager : EditorWindow
{
    private const string linkSourcePath = "Assets/WX-WASM-SDK-V2/Runtime/Plugins/link_wechat.xml";
    private const string linkTargetPath = "Assets/WX-WASM-SDK-V2/Runtime/Plugins/link.xml";

    [MenuItem("Tools/WeChat/Enable WeChat Link")]
    public static void EnableWeChatLink()
    {
        if (File.Exists(linkSourcePath))
        {
            if (File.Exists(linkTargetPath))
            {
                File.Delete(linkTargetPath);
            }
            File.Copy(linkSourcePath, linkTargetPath);
            AssetDatabase.Refresh();
            Debug.Log("WeChat link.xml enabled!");
        }
        else
        {
            Debug.LogError("link_wechat.xml not found!");
        }
    }

    [MenuItem("Tools/WeChat/Disable WeChat Link")]
    public static void DisableWeChatLink()
    {
        if (File.Exists(linkTargetPath))
        {
            File.Delete(linkTargetPath);
            AssetDatabase.Refresh();
            Debug.Log("WeChat link.xml disabled!");
        }
        else
        {
            Debug.Log("link.xml not found, already disabled!");
        }
    }

    [MenuItem("Tools/WeChat/Toggle WeChat Link")]
    public static void ToggleWeChatLink()
    {
        if (File.Exists(linkTargetPath))
        {
            DisableWeChatLink();
        }
        else
        {
            EnableWeChatLink();
        }
    }
}