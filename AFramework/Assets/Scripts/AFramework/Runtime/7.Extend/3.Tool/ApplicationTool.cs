namespace AFramework
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    public class ApplicationTool
    {
        /// <summary>
        /// 退出程序函数
        /// </summary>
        public static void ApplicationQuit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
                Application.Quit();      
#endif
            
        }

#if UNITY_EDITOR
        /// <summary>
        /// 返回支持的BuildTargetGroup
        /// </summary>
        /// <returns></returns>
        public static BuildTargetGroup[] GetValidBuildTargetGroups()
        {
            return new BuildTargetGroup[] { BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.WSA };
        }
#endif
    }
}
