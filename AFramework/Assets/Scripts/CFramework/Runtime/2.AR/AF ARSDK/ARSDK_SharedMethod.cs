#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AFramework
{
    public class ARSDK_SharedMethod 
    {
        /// <summary>
        /// 获取typeName类型是否存在
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetTypeUnknownAssembly(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }
#if !UNITY_WSA
            Assembly[] foundAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < foundAssemblies.Length; i++)
            {
                type = foundAssemblies[i].GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
#endif
            return null;
        }
#if UNITY_EDITOR
        public static BuildTargetGroup BuildTargetToBuildTargetGroup(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                case BuildTarget.WSAPlayer:
                    return BuildTargetGroup.WSA;
                default:
                    return BuildTargetGroup.Android;
            }
        }
        /// <summary>
        /// 获取givenType所在的Assembly的类型
        /// </summary>
        /// <param name="givenType"></param>
        /// <returns></returns>
        public static Type[] GetTypesOfType(Type givenType)
        {
#if UNITY_WSA && !UNITY_EDITOR
            return givenType.GetTypeInfo().Assembly.GetTypes();
#else
            return givenType.Assembly.GetTypes();
#endif
        }
#endif
    }
    public enum ARSDKType
    {
        VuforiaSDK,
        AFoudation,
        HuaWeiAR
    }
}
