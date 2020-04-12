
namespace AFramework
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public class ARSDK_Info : AssetPostprocessor
    {
        [UnityEditor.Callbacks.DidReloadScripts(1)]
        public static void CheckSDKState()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            if (AssetDatabasex.LoadAssetOfType<ARSDKSetting>("ARSDKSetting").IsCheckSdkStatus)
            {
                if (AF_SDKManager.AvailableScriptingDefineSymbolPredicateInfos != null)
                {
                    AF_SDKManager.ManageScriptingDefineSymbols();
                }
                else
                {
                    AFLogger.e("AF_SDKManager.AvailableScriptingDefineSymbolPredicateInfos为空");
                }
            }
        }
        /// <summary>
        /// 导入图片以及压缩图片将会调用
        /// </summary>
        void OnPreprocessTexture()
        {
            AFUnitySettingWindows aFUnitySettingWindows = AssetDatabasex.LoadAssetOfType<AFUnitySettingWindows>("AFUnitySetting");
            if(aFUnitySettingWindows.TextureSettingSwitch)
            {
                TextureImporter importer = (TextureImporter)assetImporter;
                importer.mipmapEnabled = false;
                importer.textureType = aFUnitySettingWindows.textureType;
                if(importer.textureType == TextureImporterType.Sprite)
                {
                    importer.spritePackingTag = aFUnitySettingWindows.spritePackingTag;
                }

                TextureImporterPlatformSettings AndroidTextureImport = new TextureImporterPlatformSettings();
                TextureImporterPlatformSettings iOSTextureImport = new TextureImporterPlatformSettings();
                TextureImporterPlatformSettings DefaultTextureImport = new TextureImporterPlatformSettings();

                string s = importer.assetPath.Substring(importer.assetPath.LastIndexOf('.') + 1);
                if (s == "png")
                {
                    importer.alphaIsTransparency = true;
                    AndroidTextureImport.format = aFUnitySettingWindows.AndroidImporterPlatformSettings.AlphaTextureImporterFormat;
                    iOSTextureImport.format = aFUnitySettingWindows.iOSImporterPlatformSettings.AlphaTextureImporterFormat;
                    DefaultTextureImport.format = aFUnitySettingWindows.defaultImporterPlatformSettings.AlphaTextureImporterFormat;
                }
                else
                {
                    importer.alphaIsTransparency = false;
                    AndroidTextureImport.format = aFUnitySettingWindows.AndroidImporterPlatformSettings.NoAlphaTextureImporterFormat;
                    iOSTextureImport.format = aFUnitySettingWindows.iOSImporterPlatformSettings.NoAlphaTextureImporterFormat;
                    DefaultTextureImport.format = aFUnitySettingWindows.defaultImporterPlatformSettings.NoAlphaTextureImporterFormat;
                }
                importer.isReadable = false;

                AndroidTextureImport.name = "Android";
                AndroidTextureImport.overridden = aFUnitySettingWindows.AndroidImporterPlatformSettings.overridden;
                AndroidTextureImport.maxTextureSize = aFUnitySettingWindows.AndroidImporterPlatformSettings.maxTextureSize;
                importer.SetPlatformTextureSettings(AndroidTextureImport);

                iOSTextureImport.name = "iPhone";
                iOSTextureImport.overridden = aFUnitySettingWindows.iOSImporterPlatformSettings.overridden;
                iOSTextureImport.maxTextureSize = aFUnitySettingWindows.iOSImporterPlatformSettings.maxTextureSize;
                importer.SetPlatformTextureSettings(iOSTextureImport);

                DefaultTextureImport.name = "Standalone";
                DefaultTextureImport.overridden = aFUnitySettingWindows.defaultImporterPlatformSettings.overridden;
                DefaultTextureImport.maxTextureSize = aFUnitySettingWindows.defaultImporterPlatformSettings.maxTextureSize;
                importer.SetPlatformTextureSettings(DefaultTextureImport);
            }
        }
        /// <summary>
        /// 图片已经被压缩、保存到指定目录下之后调用
        /// </summary>
        /// <param name="texture"></param>
        void OnPostprocessTexure(Texture2D texture)
        {
            Debug.Log("更改设置的图片名字:" + texture.name);
        }
    }
#endif
    public enum SDKLoadState
    {
        //这里的初始化是指playersetting中的一些设置
        LoadAndInit,//已经导入并已经初始化
        UnLoad,//未导入
        Loaded//导入且未初始化
    }
}
