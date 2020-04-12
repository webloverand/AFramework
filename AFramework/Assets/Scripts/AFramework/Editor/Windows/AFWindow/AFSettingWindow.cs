/*******************************************************************
* Copyright(c)
* 文件名称: AFSettingWindow.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/

namespace AFramework
{
#if UNITY_EDITOR
    using UnityEditor;
    using Sirenix.OdinInspector.Editor;
    using UnityEngine;
    using System;
    using System.Collections.Generic;

#if AF_ARSDK_Vuforia
    using Vuforia;
#endif
    using Sirenix.OdinInspector;

    //1.%X : ctrl + X; 
    //2.#X : shift + X;
    //3.&X : alt + X; 
    //4._X ：X
    //5.%&X: ctrl  + alt + X

    public class AFSettingWindow : OdinMenuEditorWindow
    {
        public static int selectIndex = 1;
        [MenuItem("Tools/AFramework/Preferences %e",priority = 600)]
        [MenuItem("Assets/AFSettingWindow", priority = 0)]
        public static void OpenWindow()
        {
            GetWindow<AFSettingWindow>().Show(true);
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();
            this.titleContent = new GUIContent("AFramework设置");
            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle.Height = 30;
            tree.Selection.SupportsMultiSelect = false;

            AppInfoConfig appInfoConfig = AssetDatabasex.LoadAssetOfType<AppInfoConfig>("AppInfoConfig");
            ABAndPathConfig aBAndPathConfig = new ABAndPathConfig();
            AFXcodeSetting aFXcodeSetting = new AFXcodeSetting(AssetDatabase.GetAssetPath(appInfoConfig));
            TextAsset textAsset = AssetDatabasex.LoadAssetOfType<TextAsset>("XcodeSetting");
            if (textAsset != null)
            {
                aFXcodeSetting.InitWithExit(SerializeHelper.FromJson<XcodeSetting>(textAsset.text));
            }
            APPBuildWindows buildAppWindows = AssetDatabasex.LoadAssetOfType<APPBuildWindows>("APPBuildWindows");
            buildAppWindows.xcodeSetting = aFXcodeSetting;
            buildAppWindows.SyncSceneWithBuildSetting();

            tree.Add("App基本信息配置", appInfoConfig);
            tree.Add("AB打包配置", aBAndPathConfig);
            tree.Add("APP打包配置", buildAppWindows);
            tree.Add("AR SDK配置", new ARSDKConfig());
            tree.Add("Unity配置", AssetDatabasex.LoadAssetOfType<AFUnitySettingWindows>("AFUnitySetting"));
            tree.Add("UI配置", AssetDatabasex.LoadAssetOfType<AFUIConfig>("AFUIConfig"));
            tree.Add("多语言配置", AssetDatabasex.LoadAssetOfType<LanuageInfo>("LanuageInfo"));
            tree.Add("查找重复文件", new SearchDuplicateFiles());
            tree.Add("批量更改Raycast Target选项", new ChangeRaycastTarget());
            tree.Add("批量更换字体", new ChangeFont());

            tree.Selection.SupportsMultiSelect = false;
            List<OdinMenuItem> menuItems  = tree.MenuItems;
            OdinMenuTreeSelection odinMenuItems = tree.Selection;
            odinMenuItems.Add(menuItems[selectIndex]);
            return tree;
        }
        //window关闭时调用
        protected override void OnDestroy()
        {
            base.OnDestroy();
            //清空搜索到的物体,防止记录太多
            
        }
    }
    public class ABAndPathConfig
    {
        public ABAndPathConfig()
        {

            ABConfig = AssetDatabasex.LoadAssetOfType<AF_ABConfig>("AF_ABConfig");
            pathConfig = AssetDatabasex.LoadAssetOfType<AFPathConfig>("AFPathConfig");
            Debug.Log(ABConfig);
            EditorAssetPath.InitABBuildPath(ABConfig);

            pathConfig.ServerABPath = Environment.CurrentDirectory + "/AF-ABForServer";
            pathConfig.PhoneABPath = Environment.CurrentDirectory + "/AF-ABForPhone";
            pathConfig.StreamingABPath = Application.streamingAssetsPath + "/AF-ABForLocal";
            pathConfig.PersistentPath = Application.persistentDataPath;
            pathConfig.StreamingAssetPath = Application.streamingAssetsPath;
        }

        [BoxGroup, HideLabel, EnumToggleButtons]
        public ABAndPathTool aBAndPathTool;

        [ShowIf("@this.aBAndPathTool==ABAndPathTool.AB"), InlineEditor(Expanded = true)]
        public AF_ABConfig ABConfig;

        [ShowIf("@this.aBAndPathTool==ABAndPathTool.AB"), InlineEditor(Expanded = true)]
        [ButtonGroup("BuildAB")]
        public static void AndroidBuildAB()
        {
            ABBuildEditor.AndroidBuild();
        }

        [ShowIf("@this.aBAndPathTool==ABAndPathTool.AB"), InlineEditor(Expanded = true)]
        [ButtonGroup("BuildAB")]
        public static void IosBuildAB()
        {
            ABBuildEditor.iOSBuild();
        }

        [ShowIf("@this.aBAndPathTool==ABAndPathTool.Path"), InlineEditor(Expanded = true)]
        public AFPathConfig pathConfig;
    }
    public enum ABAndPathTool
    {
        AB,
        Path
    }

    public class ARSDKConfig
    {
        public ARSDKConfig()
        {
#if AF_ARSDK_Vuforia
            vuforiaConfiguration = AssetDatabasex.LoadAssetOfType<VuforiaConfiguration>("VuforiaConfiguration");
#endif
            SDKsetting = AssetDatabasex.LoadAssetOfType<ARSDKSetting>("ARSDKSetting");
        }
        [BoxGroup, HideLabel, EnumToggleButtons]
        public ARSDKTool aRSDKTool;

#if AF_ARSDK_Vuforia
        [ShowIf("@this.aRSDKTool==ARSDKTool.Vuforia"), InlineEditor(Expanded = true)]
        public VuforiaConfiguration vuforiaConfiguration;
#endif
        [ShowIf("@this.aRSDKTool==ARSDKTool.SdkSetting"), InlineEditor(Expanded = true)]
        public ARSDKSetting SDKsetting;
    }
    public enum ARSDKTool
    {
        Vuforia,
        ARFoudation,
        SdkSetting
    }
#endif
}
