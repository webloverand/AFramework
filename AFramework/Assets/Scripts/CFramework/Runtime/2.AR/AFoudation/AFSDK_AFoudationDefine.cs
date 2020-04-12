namespace AFramework
{
    using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;

    public class AFSDK_AFoudationDefine
    {
        public const string ScriptingDefineSymbol = AFSDK_ScriptingDefineSymbolAttribute.SymbolPrefix + "AFoudation";
        static SDKLoadState Loadstate = SDKLoadState.UnLoad;

        [AFSDK_ScriptingDefineSymbolAttribute(ScriptingDefineSymbol, "Android")]
        [AFSDK_ScriptingDefineSymbolAttribute(ScriptingDefineSymbol, "iOS")]
        private static bool IsAFoudationAvailable()
        {
            bool isLoad = FileHelper.ReadTxtToStr(System.Environment.CurrentDirectory + "/Packages/manifest.json")
                .Contains("com.unity.xr.arfoundation");

            if (isLoad && Loadstate == SDKLoadState.Loaded)
            {
                Loadstate = SDKLoadState.LoadAndInit;
            }
            else
            {
                if (isLoad)
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
            
        }
    }
#endif
}
