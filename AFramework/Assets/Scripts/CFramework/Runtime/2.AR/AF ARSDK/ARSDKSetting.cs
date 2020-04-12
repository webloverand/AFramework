/*******************************************************************
* Copyright(c)
* 文件名称: SDKSetting.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ARSDKSetting", menuName = "AFramework/CreatARSDKSetting", order = 50)]
    public class ARSDKSetting : ScriptableObject
    {

        [Title("AR SDK宏定义设置",titleAlignment:TitleAlignments.Left)]
        [InfoBox("是否自动判断SDK导入情况添加宏")]
        public bool IsCheckSdkStatus = true;

        [ButtonGroup("ARFoudation")]
        [Button("增加ARFoudation宏定义",ButtonSizes.Medium)]
        public void AddARFoudationDefine()
        {
            SetNewestSymbol(0,true);
        }
        [ButtonGroup("ARFoudation")]
        [Button("删除ARFoudation宏定义:", ButtonSizes.Medium)]
        public void DeleteARFoudationDefine()
        {
            SetNewestSymbol(0, false);
        }
        [ButtonGroup("Vuforia")]
        [Button("增加Vuforia宏定义", ButtonSizes.Medium)]
        public void AddVuforiaDefine()
        {
            SetNewestSymbol(1, true);
        }
        [ButtonGroup("Vuforia")]
        [Button("删除Vuforia宏定义", ButtonSizes.Medium)]
        public void DeleteVuforiaDefine()
        {
            SetNewestSymbol(1, false);
        }
        [ButtonGroup("HuaWeiAR")]
        [Button("增加华为AR宏定义", ButtonSizes.Medium)]
        public void AddHuaWeiARDefine()
        {
            SetNewestSymbol(2, true);
        }
        [ButtonGroup("HuaWeiAR")]
        [Button("删除华为AR宏定义", ButtonSizes.Medium)]
        public void DeleteHuaWeiARDefine()
        {
            SetNewestSymbol(2, false);
        }
        public string GetNewSymbol(int index)
        {
            string newSymbol = "AF_ARSDK_";
            switch (index)
            {
                case 0:
                    newSymbol += "AFoudation";
                    break;
                case 1:
                    newSymbol += "Vuforia";
                    break;
                case 2:
                    newSymbol += "HuaWeiAR";
                    break;
                default:
                    break;
            }
            return newSymbol;
        }
        public void SetNewestSymbol(int index,bool isAdd)
        {
            string newSymbol = GetNewSymbol(index);
            BuildTargetGroup[] targetGroups = ApplicationTool.GetValidBuildTargetGroups();
            Dictionary<BuildTargetGroup, HashSet<string>> newSymbolsByTargetGroup = new Dictionary<BuildTargetGroup, HashSet<string>>(targetGroups.Length);
            //获取新的Symbols
            foreach (BuildTargetGroup targetGroup in ApplicationTool.GetValidBuildTargetGroups())
            {
                string[] currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup)
                                                      .Split(';')
                                                      .Distinct()
                                                      .OrderBy(symbol => symbol, StringComparer.Ordinal)
                                                      .Where(symbol => !symbol.StartsWith(AFSDK_ScriptingDefineSymbolAttribute.SymbolPrefix, StringComparison.Ordinal))
                                                      .ToArray();
                string finallySymbol = string.Join(";", currentSymbols);
                if (isAdd && !currentSymbols.Contains(newSymbol))
                {
                    finallySymbol += ";" + newSymbol;
                }
                else if(!isAdd && currentSymbols.Contains(newSymbol))
                {
                    finallySymbol.RemoveString(newSymbol).Replace(";;",";");
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, finallySymbol);
            }
        }
    }
#endif
}
