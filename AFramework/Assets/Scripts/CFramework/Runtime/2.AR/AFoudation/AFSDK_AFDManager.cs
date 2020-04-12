/*******************************************************************
* Copyright(c)
* 文件名称: AFSDK_AFDManager.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/


namespace AFramework
{
#if AF_ARSDK_AFoudation
    using System;
    using UnityEngine;
    using System.Collections;
    using UnityEngine.XR.ARFoundation;

    public class AFSDK_AFDManager
    {
        
        public static void CheckSupport(MonoBehaviour RegisterObject,Action<bool> checkCall) 
        {
            IEnumeratorTool.StartCoroutine(CheckSupport(checkCall));
        }
        static IEnumerator CheckSupport(Action<bool> checkCall)
        {
            yield return ARSession.CheckAvailability();
            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                yield return ARSession.Install();
            }
            if (ARSession.state == ARSessionState.Ready)
            {
                checkCall(true);
            }
            else
            {
                switch (ARSession.state)
                {
                    case ARSessionState.Unsupported:
                        AFLogger.d("Your device does not support AR.");
                        checkCall(false);
                        break;
                    case ARSessionState.NeedsInstall:
                        AFLogger.d("软件更新失败，或者您拒绝了更新。变为本地放置模型");
                        checkCall(false);
                        break;
                }
            }
        }
    }
#endif
}
