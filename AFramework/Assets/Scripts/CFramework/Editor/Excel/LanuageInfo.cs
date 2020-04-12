/*******************************************************************
* Copyright(c)
* 文件名称: LanuageTXTPath.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(fileName = "LanuageInfo", menuName = "AFramework/CreateLanuageInfo", order = 50)]
    public class LanuageInfo : SerializedScriptableObject
    {
        [ListDrawerSettings(ShowItemCount = true)]
        public Dictionary<string, string> LanuageNameToPath = new Dictionary<string, string>();

        public string excelPath = PathTool.ProjectPath;

        [Button("TXT文件转Excel表格")]
        public void TxtToExcel()
        {
            List<string> LanuageDes = new List<string>();
            List<string> LanuageName = new List<string>(LanuageNameToPath.Keys);
            Dictionary<string, Dictionary<string, string>> LanuageNameToCon = new Dictionary<string, Dictionary<string, string>>();

            Dictionary<Vector2, string> excelCon = new Dictionary<Vector2, string>();
            excelCon.Add(new Vector2(1, 1), "Description");
            for (int i = 0; i < LanuageName.Count; i++)
            {
                Dictionary<string, string> txtCon = new Dictionary<string, string>();
                TextAsset ta = AssetDatabase.LoadAssetAtPath<TextAsset>(LanuageNameToPath[LanuageName[i]]);
                string text = ta.text;
                string[] lines = text.Split('\n');
                foreach (string line in lines)
                {
                    if (line == null)
                    {
                        continue;
                    }
                    string[] keyAndValue = line.Split('=');
                    if (keyAndValue.Length < 2)
                    {
                        AFLogger.EditorErrorLog(LanuageName + "语言文件路径为:" + LanuageNameToPath[LanuageName[i]] +
                            "不符合格式要求,请检查:" + line);
                        continue;
                    }
                    if (i == 0)
                    {
                        LanuageDes.Add(keyAndValue[0]);
                    }
                    if (txtCon.ContainsKey(keyAndValue[0]))
                    {
                        AFLogger.EditorErrorLog("相同的描述:" + keyAndValue[0]);
                        return;
                    }
                    txtCon.Add(keyAndValue[0], keyAndValue[1]);
                }
                LanuageNameToCon.Add(LanuageName[i], txtCon);
                excelCon.Add(new Vector2(1, i + 2), LanuageName[i]);
            }
            for (int j = 0; j < LanuageDes.Count; j++)
            {
                excelCon.Add(new Vector2(j + 2, 1), LanuageDes[j]);
                for (int k = 0; k < LanuageName.Count; k++)
                {
                    excelCon.Add(new Vector2(j + 2, k + 2), LanuageNameToCon[LanuageName[k]][LanuageDes[j]]);
                }
            }
            ExcelHelper.WriteExcel(excelPath, excelCon);
            EditorUtility.RevealInFinder(excelPath);
            AFLogger.d("TXT文件转Excel表格转化完成");
        }

        [Button("Excel表格转TXT文件")]
        public void ExcelToTxt()
        {
            string[,] ExcelInfo = ExcelHelper.GetFirstSheetInfo(excelPath);
            for (int j = 1; j < ExcelInfo.GetLength(1); j++)
            {
                string oneLanuage = "";
                for (int i = 1; i < ExcelInfo.GetLength(0); i++)
                {
                    if (i != 1)
                    {
                        oneLanuage += "\n";
                    }
                    oneLanuage += ExcelInfo[i, 0] + "=" + ExcelInfo[i, j].Replace("=", "#");
                }
                oneLanuage = oneLanuage.Replace("\n\n", "\n");
                if (!LanuageNameToPath.ContainsKey(ExcelInfo[0, j]))
                {
                    AFLogger.EditorErrorLog("请注意Excel表格与LanuageNameToPath的对应!");
                    return;
                }
                FileHelper.CreatFile(LanuageNameToPath[ExcelInfo[0, j]],
                    System.Text.Encoding.UTF8.GetBytes(oneLanuage), true);
            }
            AFLogger.d("Excel表格转TXT文件转换完成!");
        }
    }
}
