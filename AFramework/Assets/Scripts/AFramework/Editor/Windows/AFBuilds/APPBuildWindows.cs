/*******************************************************************
* Copyright(c)
* 文件名称: BuildWindows.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections.Generic;
#if UNITY_EDITOR
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;
    [CreateAssetMenu(fileName = "APPBuildWindows", menuName = "AFramework/CreatAPPBuildWindows", order = 50)]
    public class APPBuildWindows : ScriptableObject
    {
        [OnValueChanged("OnPlatformChange")]
        [BoxGroup, HideLabel, EnumToggleButtons]
        public BuildPlatform buildPlatform;

        [InfoBox("XCode项目打包目标路径,请注意最后/后是APP名称,根据版本合成最终路径")]
        [ShowIf("@this.buildPlatform == BuildPlatform.IOS")]
        public string XCodeProjectPath = PathTool.ProjectPath + "BuildTarget/iOS/AFramework";
        [InfoBox("Android apk打包目标路径,请注意最后/后是APP名称,根据版本合成最终路径")]
        [ShowIf("@this.buildPlatform == BuildPlatform.Android")]
        public string AndroidAPKPath = PathTool.ProjectPath + "BuildTarget/Android/AFramework";
        [InfoBox("Platform公共属性,所有平台一致")]
        [InfoBox("APP名称")]
        public string DisplayName = "";
        [InfoBox("是否是Development打包")]
        public bool DevelopmentBuild;
        [InfoBox("设置打包场景,请注意这里的场景与BuildSetting保持一致,但是打包active可以不一致")]
        [ShowInInspector]
        List<SceneSelect> BuildScene = new List<SceneSelect>();

        [InfoBox("以下是iOS Platform自有属性")]
        [ShowIf("@this.buildPlatform == BuildPlatform.IOS")]
        [InlineEditor(Expanded = true)]
        [PropertySpace(30)]
        public AFXcodeSetting xcodeSetting;

        [InfoBox("以下是Android Platform自有属性")]
        [ShowIf("@this.buildPlatform == BuildPlatform.Android")]
        [InlineEditor(Expanded = true)]
        [PropertySpace(30)]
        public AndroidBuildSetting androidBuildSetting;

        [ButtonGroup("BuildAPP")]
        [Button("Android打包")]
        public void AndroidBuildApp()
        {
            if(CommonBuildSetting())
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, androidBuildSetting.ScriptingBackend);
                PlayerSettings.keystorePass = androidBuildSetting.keystorePass;
                PlayerSettings.keyaliasPass = androidBuildSetting.keyaliasPass;
                PlayerSettings.bundleVersion = androidBuildSetting.build;
                string savePath = AndroidAPKPath + androidBuildSetting.Version + "." + androidBuildSetting.build;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = androidBuildSetting.exportAsGoogleAndroidProject;
                if (!androidBuildSetting.exportAsGoogleAndroidProject)
                    savePath += ".apk";
                string[] t = AndroidAPKPath.Split('/');
                string filePath = t[0];
                if (t.Length > 1)
                {
                    for(int i = 1;i<t.Length-1;i++)
                    {
                        filePath += "/" + t[i];
                    }
                }
                FileHelper.CreateDirectory(filePath);
                BuildPipeline.BuildPlayer(FindEnableEditorrScenes(), savePath, BuildTarget.Android, BuildOptions.None);
                Debug.Log("apk路径:" + savePath);
                EditorUtility.RevealInFinder(filePath);
            }
        }
        [ButtonGroup("BuildAPP")]
        [Button("iOS打包XCode项目")]
        public void iOSBuildApp()
        {
            if(CommonBuildSetting())
            {
                string savePath = AndroidAPKPath + xcodeSetting.Version + "." + xcodeSetting.build;
                FileHelper.CreateDirectory(savePath);
                BuildPipeline.BuildPlayer(FindEnableEditorrScenes(), savePath, BuildTarget.iOS, BuildOptions.None);
                EditorUtility.RevealInFinder(savePath);
            }
        }

        private string[] FindEnableEditorrScenes()
        {
            List<string> editorScenes = new List<string>();
            foreach (SceneSelect scenePath in BuildScene)
            {
                if (!scenePath.isPackage) continue;
                editorScenes.Add(scenePath.ScenePath);
            }
            return editorScenes.ToArray();
        }

        bool CommonBuildSetting()
        {
            if((buildPlatform == BuildPlatform.Android && AndroidAPKPath == "")
                || (buildPlatform == BuildPlatform.IOS && XCodeProjectPath == ""))
            {
                AFLogger.EditorErrorLog("APP目标打包路径不能为空");
                return false;
            }
            EditorUserBuildSettings.development = DevelopmentBuild;
            PlayerSettings.productName = DisplayName;
            AF_ABConfig ABConfig = AssetDatabasex.LoadAssetOfType<AF_ABConfig>("AF_ABConfig");
            return true;
        }

        void OnPlatformChange()
        {
            if(buildPlatform == BuildPlatform.IOS && xcodeSetting == null)
            {
                AppInfoConfig appInfoConfig = AssetDatabasex.LoadAssetOfType<AppInfoConfig>("AppInfoConfig");
                xcodeSetting = new AFXcodeSetting(AssetDatabase.GetAssetPath(appInfoConfig));
                TextAsset textAsset = AssetDatabasex.LoadAssetOfType<TextAsset>("XcodeSetting");
                if (textAsset != null)
                {
                    xcodeSetting.InitWithExit(SerializeHelper.FromJson<XcodeSetting>(textAsset.text));
                }
            }
        }
        public void SyncSceneWithBuildSetting()
        {
            List<string> allScene = new List<string>();
            List<string> buildScene = new List<string>();
            foreach (SceneSelect sceneSelect in BuildScene)
            {
                buildScene.Add(sceneSelect.ScenePath);
            }
            //buildScene增加
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if(!buildScene.Contains(scene.path))
                {
                    SceneSelect sceneSelect = new SceneSelect();
                    sceneSelect.isPackage = scene.enabled;
                    sceneSelect.ScenePath = scene.path;
                    BuildScene.Add(sceneSelect);
                    buildScene.Add(scene.path);
                }
                allScene.Add(scene.path);
            }
            //buildScene同步EditorBuildSettings删除
            for(int i=BuildScene.Count - 1;i>=0;i--)
            {
                if(!allScene.Contains(BuildScene[i].ScenePath))
                {
                    BuildScene.RemoveAt(i);
                }
            }
        }
    }
    public enum BuildPlatform
    {
        Android,
        IOS
    }
    [SerializeField]
    public class SceneSelect
    {
        [HorizontalGroup("Scene", 50)]
        [HideLabel]
        public bool isPackage;
        [ReadOnly]
        [HideLabel]
        [HorizontalGroup("Scene", LabelWidth = 10)]
        public string ScenePath;

    }
#endif
}
