
namespace AFramework
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;

    //[MonoSingletonPath("[AFramework]/AF_SDKManager")]
    public class AF_SDKManager  //: MonoSingletonWithNewObject<AF_SDKManager>
    {
        #region 判断SDK是否导入相关
#if UNITY_EDITOR
        public static ReadOnlyCollection<ScriptingDefineSymbolPredicateInfo> AvailableScriptingDefineSymbolPredicateInfos { get; private set; }
        public sealed class ScriptingDefineSymbolPredicateInfo
        {
            public readonly AFSDK_ScriptingDefineSymbolAttribute attribute;
            //用来判断SDK是否导入
            public readonly MethodInfo methodInfo;
            public ScriptingDefineSymbolPredicateInfo(AFSDK_ScriptingDefineSymbolAttribute attribute, MethodInfo methodInfo)
            {
                this.attribute = attribute;
                this.methodInfo = methodInfo;
            }
        }
        public static void PopulateAvailableScriptingDefineSymbolPredicateInfos()
        {
            List<ScriptingDefineSymbolPredicateInfo> predicateInfos = new List<ScriptingDefineSymbolPredicateInfo>();

            foreach (Type type in ARSDK_SharedMethod.GetTypesOfType(typeof(AF_SDKManager)))
            {
                MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                for (int index = 0; index < methodInfos.Length; index++)
                {
                    AFSDK_ScriptingDefineSymbolAttribute[] predicateAttributes = (AFSDK_ScriptingDefineSymbolAttribute[])methodInfos[index].GetCustomAttributes(typeof(AFSDK_ScriptingDefineSymbolAttribute), false);
                    if (predicateAttributes.Length == 0)
                    {
                        continue;
                    }
                    if (methodInfos[index].ReturnType != typeof(bool) || methodInfos[index].GetParameters().Length != 0)
                    {
                        AFLogger.d((methodInfos[index].ReturnType != typeof(bool)) + " " + (methodInfos[index].GetParameters().Length != 0));
                        AFLogger.e(string.Format("The method '{0}' on '{1}' has '{2}' specified but its signature is wrong. The method must take no arguments and return bool.",
                                                                                      methodInfos[index].Name,
                                                                                      type,
                                                                                      typeof(AFSDK_ScriptingDefineSymbolAttribute)));

                        return;
                    }
                    predicateInfos.AddRange(predicateAttributes.Select(predicateAttribute => new ScriptingDefineSymbolPredicateInfo(predicateAttribute, methodInfos[index])));
                }
            }

            predicateInfos.Sort((x, y) => string.Compare(x.attribute.SDKSymbol, y.attribute.SDKSymbol, StringComparison.Ordinal));
            AvailableScriptingDefineSymbolPredicateInfos = predicateInfos.AsReadOnly();
        }
        public static void ManageScriptingDefineSymbols()
        {
            BuildTargetGroup[] targetGroups = ApplicationTool.GetValidBuildTargetGroups();
            Dictionary<BuildTargetGroup, HashSet<string>> newSymbolsByTargetGroup = new Dictionary<BuildTargetGroup, HashSet<string>>(targetGroups.Length);
            //获取新的Symbols
            foreach (ScriptingDefineSymbolPredicateInfo predicateInfo in AvailableScriptingDefineSymbolPredicateInfos)
            {
                AFSDK_ScriptingDefineSymbolAttribute predicateAttribute = predicateInfo.attribute;
                string symbol = predicateAttribute.SDKSymbol;
                MethodInfo methodInfo = predicateInfo.methodInfo;
                if (!(bool)methodInfo.Invoke(null, null))
                {
                    continue;
                }
                AFSDK_ScriptingDefineSymbolAttribute[] allAttributes = (AFSDK_ScriptingDefineSymbolAttribute[])methodInfo.GetCustomAttributes(typeof(AFSDK_ScriptingDefineSymbolAttribute), false);
                foreach (AFSDK_ScriptingDefineSymbolAttribute attribute in allAttributes)
                {
                    BuildTargetGroup buildTargetGroup = attribute.buildTargetGroup;
                    HashSet<string> newSymbols;
                    if (!newSymbolsByTargetGroup.TryGetValue(buildTargetGroup, out newSymbols))
                    {
                        newSymbols = new HashSet<string>();
                        newSymbolsByTargetGroup[buildTargetGroup] = newSymbols;
                    }
                    newSymbols.Add(attribute.SDKSymbol);

                }
            }
            if(newSymbolsByTargetGroup.Count == 0)
            {
                foreach (BuildTargetGroup targetGroup in ApplicationTool.GetValidBuildTargetGroups())
                {
                    string[] currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup)
                                                       .Split(';')
                                                       .Distinct()
                                                       .OrderBy(symbol => symbol, StringComparer.Ordinal)
                                                       .Where(symbol => !symbol.StartsWith(AFSDK_ScriptingDefineSymbolAttribute.SymbolPrefix, StringComparison.Ordinal))
                                                       .ToArray();
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", currentSymbols));
                }
                return;
            }
            foreach (KeyValuePair<BuildTargetGroup, HashSet<string>> keyValuePair in newSymbolsByTargetGroup)
            {
                BuildTargetGroup targetGroup = keyValuePair.Key;
                if (targetGroup == BuildTargetGroup.Unknown)
                    continue;

                string[] newSymbols = keyValuePair.Value.OrderBy(symbol => symbol, StringComparer.Ordinal).ToArray();
                string[] currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup)
                                                      .Split(';')
                                                      .Distinct()
                                                      .OrderBy(symbol => symbol, StringComparer.Ordinal)
                                                      .ToArray();
                string[] ARSDKSystem = currentSymbols.Where(symbol => symbol.StartsWith(AFSDK_ScriptingDefineSymbolAttribute.SymbolPrefix, StringComparison.Ordinal)).ToArray();
                if (ARSDKSystem.SequenceEqual(newSymbols))
                {
                    continue;
                }
                AFLogger.d("ARSDK有变化");
               
                string[] finallySymbol = newSymbols.Concat(currentSymbols.Where(symbol => !symbol.StartsWith(AFSDK_ScriptingDefineSymbolAttribute.SymbolPrefix, StringComparison.Ordinal)).ToArray()).ToArray();
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", finallySymbol));

            }
        }
#endif
        #endregion

        static AF_SDKManager()
        {
#if UNITY_EDITOR
            PopulateAvailableScriptingDefineSymbolPredicateInfos();
#if UNITY_2018_1_OR_NEWER
            //EditorApplication.hierarchyChanged += AutoManageScriptingDefineSymbolsAndManageVRSettings;
#else
            //EditorApplication.hierarchyWindowChanged += AutoManageScriptingDefineSymbolsAndManageVRSettings;
#endif
#endif
        }

    }
}
