/*******************************************************************
* Copyright(c)
* 文件名称: AFUnitySettingWindows.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/

namespace AFramework
{
    using System.Collections.Generic;
#if UNITY_EDITOR
    using System.IO;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(fileName = "AFUnitySetting", menuName = "AFramework/CreateAFUnitySetting", order = 50)]
    public class AFUnitySettingWindows : ScriptableObject
    {
        public AFUnitySettingWindows()
        {

        }

        [BoxGroup, HideLabel, EnumToggleButtons]
        public UnitySettingTool unitySettingTool;
        #region HeaderFile
        [ShowIf("@this.unitySettingTool==UnitySettingTool.HeaderFile")]
        [ReadOnly]
        public readonly string NewBehaviourScriptPath = EditorApplication.applicationContentsPath + @"/Resources/ScriptTemplates/81-C# Script-NewBehaviourScript.cs.txt";

        [ShowIf("@this.unitySettingTool==UnitySettingTool.HeaderFile")]
        [TextArea(15, 30)]
        public string FileHeadStr = "" +
                    //自定义部分
                    "/*******************************************************************\n"
                    + "* Copyright(c)\n"
                    + "* 文件名称: #SCRIPTNAME#.cs\n"
                    + "* 简要描述:\n"
                    + "* 作者: 千喜\n"
                    + "* 邮箱: 2470460089@qq.com\n"
                    + "******************************************************************/\n"
                    //以下部分unity默认文
                    + "using UnityEngine;\n"
                    + "\n"
                    + "public class #SCRIPTNAME# : MonoBehaviour\n"
                    + "{\n"
                    + "}";


        [ShowIf("@this.unitySettingTool==UnitySettingTool.HeaderFile")]
        [Button(ButtonSizes.Medium)]
        public void UpdateFileHead()
        {
            byte[] curTexts = System.Text.Encoding.UTF8.GetBytes(FileHeadStr);
            using (FileStream fs = new FileStream(NewBehaviourScriptPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                if (fs != null)
                {
                    fs.SetLength(0);    //清空文件
                    fs.Write(curTexts, 0, curTexts.Length);
                    fs.Flush();
                    fs.Dispose();

                    AFLogger.d("Update File: 81-C# Script-NewBehaviourScript.cs.txt, Success");
                }
            }
        }
        #endregion
        #region Hierarchy
        [ShowIf("@this.unitySettingTool==UnitySettingTool.Hierarchy")]
        [OnValueChanged("HierarchySwitchFinc")]
        public bool HierarchySwitch = false;
        public static void HierarchySwitchFinc()
        {
            CustomHierarchy.OpenCustomHierarchy();
        }
        #endregion

        #region TextureSetting
        [ShowIf("@this.unitySettingTool==UnitySettingTool.TextureSetting")]
        public bool TextureSettingSwitch = false;

        [InfoBox(".png图片默认开启alpha通道,其他关闭")]
        [InfoBox("Read Enable自动为false")]
        [InfoBox("Generate Mip Maps自动为false")]
        //公共设置
        [ShowIf("@this.unitySettingTool==UnitySettingTool.TextureSetting")]
        public TextureImporterType textureType = TextureImporterType.Sprite;
        [ShowIf("@this.unitySettingTool==UnitySettingTool.TextureSetting&&this.textureType==TextureImporterType.Sprite")]
        public string spritePackingTag;

        [ShowIf("@this.unitySettingTool==UnitySettingTool.TextureSetting")]
        [InlineEditor(Expanded = true)]
        public ImageImport AndroidImporterPlatformSettings;

        [ShowIf("@this.unitySettingTool==UnitySettingTool.TextureSetting")]
        [InlineEditor(Expanded = true)]
        public ImageImport iOSImporterPlatformSettings;

        [ShowIf("@this.unitySettingTool==UnitySettingTool.TextureSetting")]
        [InlineEditor(Expanded = true)]
        public ImageImport defaultImporterPlatformSettings;
        #endregion

        #region ChangeNameSpace

        [ShowIf("@this.unitySettingTool==UnitySettingTool.ChangeNameSpace")]
        public string namespaceName = "AFramework";
        [ShowIf("@this.unitySettingTool==UnitySettingTool.ChangeNameSpace")]
        public string folder = "Assets/";

        [ShowIf("@this.unitySettingTool==UnitySettingTool.ChangeNameSpace")]
        [Button("修改namespace")]
        public void ChangeNamespace()
        {
            if (!string.IsNullOrEmpty(folder) && !string.IsNullOrEmpty(namespaceName))
            {

                List<string> filesPaths = new List<string>();
                filesPaths.AddRange(
                    Directory.GetFiles(Path.GetFullPath(".") + Path.DirectorySeparatorChar + folder, "*.cs", SearchOption.AllDirectories)
                );
                Dictionary<string, bool> scripts = new Dictionary<string, bool>();

                int counter = -1;
                foreach (string filePath in filesPaths)
                {

                    scripts[filePath] = true;

                    EditorUtility.DisplayProgressBar("Add Namespace", filePath, counter / (float)filesPaths.Count);
                    counter++;

                    string contents = File.ReadAllText(filePath);

                    string result = "";
                    bool havsNS = contents.Contains("namespace ");
                    string t = havsNS ? "" : "\t";

                    using (TextReader reader = new StringReader(contents))
                    {
                        int index = 0;
                        bool addedNS = false;
                        while (reader.Peek() != -1)
                        {
                            string line = reader.ReadLine();

                            if (line.IndexOf("using") > -1 || line.Contains("#"))
                            {
                                result += line + "\n";
                            }
                            else if (!addedNS && !havsNS)
                            {
                                result += "\nnamespace " + namespaceName + " {";
                                addedNS = true;
                                result += t + line + "\n";
                            }
                            else
                            {
                                if (havsNS && line.Contains("namespace "))
                                {
                                    if (line.Contains("{"))
                                    {
                                        result += "namespace " + namespaceName + " {\n";
                                    }
                                    else
                                    {
                                        result += "namespace " + namespaceName + "\n";
                                    }
                                }
                                else
                                {
                                    result += t + line + "\n";
                                }
                            }
                            ++index;
                        }
                        reader.Close();
                    }
                    if (!havsNS)
                    {
                        result += "}";
                    }
                    File.WriteAllText(filePath, result);
                }



                //处理加了命名空间后出现方法miss
                filesPaths.AddRange(
                    Directory.GetFiles(Path.GetFullPath(".") + Path.DirectorySeparatorChar + folder, "*.unnity", SearchOption.AllDirectories)
                );
                filesPaths.AddRange(
                    Directory.GetFiles(Path.GetFullPath(".") + Path.DirectorySeparatorChar + folder, "*.prefab", SearchOption.AllDirectories)
                );


                counter = -1;
                foreach (string filePath in filesPaths)
                {
                    EditorUtility.DisplayProgressBar("Modify Script Ref", filePath, counter / (float)filesPaths.Count);
                    counter++;

                    string contents = File.ReadAllText(filePath);

                    string result = "";
                    using (TextReader reader = new StringReader(contents))
                    {
                        int index = 0;
                        while (reader.Peek() != -1)
                        {
                            string line = reader.ReadLine();

                            if (line.IndexOf("m_ObjectArgumentAssemblyTypeName:") > -1 && !line.Contains(namespaceName))
                            {

                                string scriptName = line.Split(':')[1].Split(',')[0].Trim();
                                if (scripts.ContainsKey(scriptName))
                                {
                                    line = line.Replace(scriptName, "namespaceName." + scriptName);
                                }

                                result += line + "\n";
                            }
                            else
                            {
                                result += line + "\n";
                            }
                            ++index;
                        }
                        reader.Close();
                    }

                    File.WriteAllText(filePath, result);
                }


                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }
        #endregion
    }

    public enum UnitySettingTool
    {
        HeaderFile,
        Hierarchy,
        TextureSetting,
        ChangeNameSpace
    }

    //头文件解析
    public class ParseFileHead : AssetModificationProcessor
    {
        /// <summary>  
        /// 此函数在asset被创建完，文件已经生成到磁盘上，但是没有生成.meta文件和import之前被调用  
        /// </summary>  
        /// <param name="newFileMeta">newfilemeta 是由创建文件的path加上.meta组成的</param>  
        public static void OnWillCreateAsset(string newFileMeta)
        {
            string newFilePath = newFileMeta.Replace(".meta", "");
            string fileExt = Path.GetExtension(newFilePath);
            if (fileExt != ".cs")
            {
                return;
            }
            //注意，Application.datapath会根据使用平台不同而不同  
            string realPath = Application.dataPath.Replace("Assets", "") + newFilePath;
            string scriptContent = File.ReadAllText(realPath);

            //这里现自定义的一些规则  
            //scriptContent = scriptContent.Replace("#CreateTime#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

            File.WriteAllText(realPath, scriptContent);
        }
    }
#endif 
}
