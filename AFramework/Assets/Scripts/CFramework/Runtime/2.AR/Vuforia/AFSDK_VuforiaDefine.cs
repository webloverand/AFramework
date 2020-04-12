namespace AFramework
{
#if UNITY_EDITOR
    using UnityEditor;
    public class AFSDK_VuforiaDefine
    {
        public const string ScriptingDefineSymbol = AFSDK_ScriptingDefineSymbolAttribute.SymbolPrefix + "Vuforia";
        static SDKLoadState Loadstate = SDKLoadState.UnLoad;

        [AFSDK_ScriptingDefineSymbolAttribute(ScriptingDefineSymbol,"Android")]
        [AFSDK_ScriptingDefineSymbolAttribute(ScriptingDefineSymbol, "iOS")]
        [AFSDK_ScriptingDefineSymbolAttribute(ScriptingDefineSymbol, "WSA")]
        private static bool IsVuforiaAvailable()
        {
            bool isLoad = ARSDK_SharedMethod.GetTypeUnknownAssembly("DefaultTrackableEventHandler") != null &&
                         ARSDK_SharedMethod.GetTypeUnknownAssembly("DefaultModelRecoEventHandler") != null &&
                         ARSDK_SharedMethod.GetTypeUnknownAssembly("DefaultInitializationErrorHandler") != null;
            if (isLoad && Loadstate == SDKLoadState.Loaded)
            {
                Loadstate = SDKLoadState.LoadAndInit;
            }
            else
            {
                if(isLoad)
                {
                    Loadstate = SDKLoadState.Loaded;
                    SDKLoadInit();
                }
                else
                {
                    Loadstate = SDKLoadState.UnLoad;
                }
            }
            return isLoad;
        }
        static void SDKLoadInit()
        {
            BuildTargetGroup activeBuildTarget = ARSDK_SharedMethod.BuildTargetToBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
#if UNITY_2019_2_OR_NEWER

#elif UNITY_2018_1_OR_NEWER
            if (!PlayerSettings.GetPlatformVuforiaEnabled(activeBuildTarget))
            {
                PlayerSettings.SetPlatformVuforiaEnabled(activeBuildTarget, true);
            }
#endif
            if (PlayerSettings.Android.androidTVCompatibility)
            {
                PlayerSettings.Android.androidTVCompatibility = false;
            }
            PlayerSettings.iOS.targetOSVersionString = "11.0";
            PlayerSettings.iOS.cameraUsageDescription = "Required for augmented reality support.";
        }
    }
#endif
}
