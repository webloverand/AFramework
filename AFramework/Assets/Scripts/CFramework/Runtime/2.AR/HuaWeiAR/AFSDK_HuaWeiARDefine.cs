/*******************************************************************
* Copyright(c)
* 文件名称: AFSDK_HuaWeiARDefine.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
#if UNITY_EDITOR
    public class AFSDK_HuaWeiARDefine
    {
        public const string ScriptingDefineSymbol = AFSDK_ScriptingDefineSymbolAttribute.SymbolPrefix + "HuaWeiAR";
        static SDKLoadState Loadstate = SDKLoadState.UnLoad;

        [AFSDK_ScriptingDefineSymbolAttribute(ScriptingDefineSymbol, "Android")]
        private static bool IsAFoudationAvailable()
        {
            bool isLoad = ARSDK_SharedMethod.GetTypeUnknownAssembly("HuaweiARUnitySDK.ARSession") != null
                && ARSDK_SharedMethod.GetTypeUnknownAssembly("HuaweiARUnitySDK.ARAnchor") != null;
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
