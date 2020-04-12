/*******************************************************************
* Copyright(c)
* 文件名称: AFUIConfig.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/

namespace AFramework
{
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    [CreateAssetMenu(fileName = "AFUIConfig", menuName = "AFramework/CreateAFUIConfig", order = 50)]
    public class AFUIConfig : ScriptableObject
    {
        public Vector2 Resolution = new Vector2(828,1472);
        public CanvasScaler.ScreenMatchMode MatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        [InfoBox("是否是竖屏应用")]
        public bool IsVertical = true;

        [Button("创建UIRoot",ButtonSizes.Medium)]
        public static void CreateUIManager()
        {
            GameObject uiRoot = GameObject.Instantiate(AssetDatabasex.LoadAssetOfType<GameObject>("UIRoot"));
            uiRoot.name = uiRoot.name.RemoveString("(Clone)");
            AFUIConfig AFUIconfig = AssetDatabasex.LoadAssetOfType<AFUIConfig>("AFUIConfig");
            CanvasScaler canvasScaler = uiRoot.transform.Find("MainCanvas").GetComponent<CanvasScaler>();
            canvasScaler.referenceResolution = AFUIconfig.Resolution;
            canvasScaler.screenMatchMode = AFUIconfig.MatchMode;
            canvasScaler.matchWidthOrHeight = AFUIconfig.IsVertical ? 1 : 0;
            Screen.SetResolution((int)AFUIconfig.Resolution.x, (int)AFUIconfig.Resolution.y, false);
        }

#if UNITY_EDITOR
        [MenuItem("Tools/AFramework/UI/CreateUIRoot")]
        [MenuItem("GameObject/UI/UIRoot")]
        public static void CreateUIRoot()
        {
            CreateUIManager();
        }
#endif
    }
}
