/*******************************************************************
* Copyright(c)
* 文件名称: v.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/


namespace AFramework
{
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;
    [CreateAssetMenu(fileName = "APPAndroidBuildSetting", menuName = "AFramework/CreatAPPAndroidBuildSetting", order = 100)]
    public class AndroidBuildSetting : ScriptableObject
    {
        [InfoBox("版本号")]
        public string Version = "";
        [InfoBox("build")]
        public string build = "";

        [InfoBox("KeyStore密码")]
        public string keystorePass;
        [InfoBox("KeyStore alias密码")]
        public string keyaliasPass;
        [InfoBox("Mono还是IL2CPP")]
        public ScriptingImplementation ScriptingBackend;
        [InfoBox("是否打包成AndroidStudio项目")]
        public bool exportAsGoogleAndroidProject = false;
    }
}
