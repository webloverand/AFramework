namespace AFramework
{
    using System;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    [Serializable]
    [AttributeUsage(AttributeTargets.Method,AllowMultiple = true,Inherited = false)]
    public class AFSDK_ScriptingDefineSymbolAttribute : Attribute
    {
        public const string SymbolPrefix = "AF_ARSDK_";
        public string SDKSymbol;

#if UNITY_EDITOR
        public BuildTargetGroup buildTargetGroup;
#endif
        [SerializeField]
        public string buildTargetGroupName;

        AFSDK_ScriptingDefineSymbolAttribute()
        {

        }
        public AFSDK_ScriptingDefineSymbolAttribute(string SDKSymbol, string buildTargetGroupName)
        {
            if(SDKSymbol == null)
            {
                AFLogger.e("SDKSymbol为空");
                return;
            }
            if(SDKSymbol == string.Empty)
            {
                AFLogger.e("SDKSymbol:" + SDKSymbol+ "An empty string isn't allowed.");
                return;
            }

            if (buildTargetGroupName == null)
            {
                AFLogger.e("buildTargetGroupName");
                return;
            }
            if (buildTargetGroupName == string.Empty)
            {
                AFLogger.e("buildTargetGroupName"+SDKSymbol+"An empty string isn't allowed.");
                return;
            }

            this.SDKSymbol = SDKSymbol;
            this.buildTargetGroupName = buildTargetGroupName;
            SetBuildTarget(buildTargetGroupName);
        }
        public AFSDK_ScriptingDefineSymbolAttribute(AFSDK_ScriptingDefineSymbolAttribute attributeToCopy)
        {
            SDKSymbol = attributeToCopy.SDKSymbol;
            SetBuildTarget(attributeToCopy.buildTargetGroupName);
        }

        private void SetBuildTarget(string groupName)
        {
            buildTargetGroupName = groupName;
#if UNITY_EDITOR
            Type buildTargetGroupType = typeof(BuildTargetGroup);
            try
            {
                buildTargetGroup = (BuildTargetGroup)Enum.Parse(buildTargetGroupType, groupName);
            }
            catch (Exception exception)
            {
                AFLogger.e(string.Format("'{0}' isn't a valid constant name of '{1}'.", groupName, buildTargetGroupType.Name));
                return;
            }

            if (buildTargetGroup == BuildTargetGroup.Unknown)
            {
                AFLogger.e(string.Format("'{0}' isn't allowed.", groupName));
                return;
            }
#endif
        }

    }
    public enum SDKLoadState
    {
        //这里的初始化是指playersetting中的一些设置
        LoadAndInit,//已经导入并已经初始化
        UnLoad,//未导入
        Loaded//导入且未初始化
    }
}
