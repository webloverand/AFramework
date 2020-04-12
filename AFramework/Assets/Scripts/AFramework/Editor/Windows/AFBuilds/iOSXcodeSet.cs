/*******************************************************************
* Copyright(c)
* 文件名称: iOSXcodeSet.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor.iOS.Xcode;
    using UnityEngine;

    public class iOSXcodeSet
    {
        public static void SetTeamId(PBXProject pbx, string targetGUID, string teamID)
        {
            if (teamID != "")
            {
                pbx.SetTeamId(targetGUID, teamID);
            }
        }

        //设置frameworks
        public static void SetFrameworks(PBXProject pbx, string targetGUID, List<XcodeSettingList> frameworkToProjectList)
        {
            foreach (var framework in frameworkToProjectList)
            {
                if (framework.isAdd)
                {
                    pbx.AddFrameworkToProject(targetGUID, framework.Value, false);
                }
                else if (pbx.ContainsFramework(targetGUID, framework.Value))
                {
                    pbx.RemoveFrameworkFromProject(targetGUID, framework.Value);
                }
            }
        }
        //添加/移除lib
        public static void SetLibs(PBXProject pbx, string targetGUID, List<XcodeSettingDic> StaticlibraryDic)
        {
            foreach (var entry in StaticlibraryDic)
            {
                if (entry.isAdd)
                {
                    pbx.AddFileToBuild(targetGUID, pbx.AddFile(entry.Key, entry.Value, PBXSourceTree.Sdk));
                }
                else
                {
                    pbx.RemoveFile(pbx.AddFile(entry.Key, entry.Value, PBXSourceTree.Sdk));
                }
            }
        }
        public static void SetBuildProperty(PBXProject pbx, string targetGUID, List<XcodeSettingDic> buildPropertyDic)
        {
            foreach (var entry in buildPropertyDic)
            {
                List<string> list = new List<string>();
                if (entry.Value.Contains(","))
                {
                    string[] t = entry.Value.Split(',');
                    foreach (var flag in t)
                    {
                        list.Add(flag);
                    }
                }
                else
                {
                    list.Add(entry.Value);
                }
                if (entry.isAdd)
                {
                    //注意最后两个参数
                    pbx.UpdateBuildProperty(targetGUID, entry.Key, list, null);
                }
                else
                {
                    pbx.UpdateBuildProperty(targetGUID, entry.Key, null, list);
                }
            }
        }
        //拷贝文件
        public static void CopyFiles(PBXProject pbx, string targetGUID, List<XcodeSettingDic> CopyFileDic, string xcodePath)
        {
            foreach (var entry in CopyFileDic)
            {
                if (NeedCopy(entry.Key))
                {
                    string des = xcodePath + "/" + entry.Value;
                    FileHelper.CopyFile(entry.Key, des);
                    pbx.AddFileToBuild(targetGUID, pbx.AddFile(des, entry.Value, PBXSourceTree.Absolute));
                    AutoAddSearchPath(pbx, targetGUID, xcodePath, des);
                    AFLogger.d("copy file " + entry.Key + " -> " + des);
                }
            }
        }
        //复制文件夹
        public static void CopyFolders(PBXProject pbx, string targetGUID, List<XcodeSettingDic> CopyFolderDic, string xcodePath)
        {
            foreach (var Entry in CopyFolderDic)
            {
                string des = Path.Combine(xcodePath, Entry.Value);
                FileHelper.CopyFolder(Entry.Key, des, NeedCopy);
                AddFolderBuild(pbx, targetGUID, xcodePath, Entry.Value);
            }
        }
        //文件编译符号
        public static void SetFilesCompileFlag(PBXProject pbx, string targetGUID, List<XcodeSettingDic> FilesCompileFlagDIC)
        {
#if UNITY_2019_2_OR_NEWER
            string target = pbx.TargetGuidByName(pbx.GetUnityMainTargetGuid());
#else
            string target = pbx.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif
            foreach (var enery in FilesCompileFlagDIC)
            {
                string fileProjPath = enery.Key;
                List<string> list = new List<string>();
                string[] t = enery.Value.Split(',');
                foreach (var flag in t)
                {
                    list.Add(flag);
                }
                pbx.SetCompileFlagsForFile(target, targetGUID, list);
            }
        }


        private static void AddFolderBuild(PBXProject pbx, string targetGUID, string xcodePath, string root)
        {
            //获得源文件下所有目录文件
            string currDir = Path.Combine(xcodePath, root);
            if (root.EndsWith(".framework", System.StringComparison.Ordinal) || root.EndsWith(".bundle", System.StringComparison.Ordinal))
            {
#if UNITY_2019_2_OR_NEWER
            string target = pbx.TargetGuidByName(pbx.GetUnityMainTargetGuid());
#else
                string target = pbx.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif
                Debug.LogFormat("add framework or bundle to build:{0}->{1}", currDir, root);
                pbx.AddFileToBuild(target, pbx.AddFile(currDir, root, PBXSourceTree.Source));
                return;
            }
            List<string> folders = new List<string>(Directory.GetDirectories(currDir));
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string t_path = Path.Combine(currDir, name);
                string t_projPath = Path.Combine(root, name);
                if (folder.EndsWith(".framework", System.StringComparison.Ordinal) || folder.EndsWith(".bundle", System.StringComparison.Ordinal))
                {
#if UNITY_2019_2_OR_NEWER
            string target = pbx.TargetGuidByName(pbx.GetUnityMainTargetGuid());
#else
                    string target = pbx.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif
                    Debug.LogFormat("add framework or bundle to build:{0}->{1}", t_path, t_projPath);
                    pbx.AddFileToBuild(target, pbx.AddFile(t_path, t_projPath, PBXSourceTree.Source));
                    AutoAddSearchPath(pbx, targetGUID, xcodePath, t_path);
                }
                else
                {
                    AddFolderBuild(pbx, targetGUID, xcodePath, t_projPath);
                }
            }
            List<string> files = new List<string>(Directory.GetFiles(currDir));
            foreach (string file in files)
            {
                if (NeedCopy(file))
                {
                    string name = Path.GetFileName(file);
                    string t_path = Path.Combine(currDir, name);
                    string t_projPath = Path.Combine(root, name);
#if UNITY_2019_2_OR_NEWER
            string target = pbx.TargetGuidByName(pbx.GetUnityMainTargetGuid());
#else
                    string target = pbx.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif
                    pbx.AddFileToBuild(target, pbx.AddFile(t_path, t_projPath, PBXSourceTree.Source));
                    AutoAddSearchPath(pbx, targetGUID, xcodePath, t_path);
                    AFLogger.d("add file to build:" + Path.Combine(root, file));
                }
            }
        }
        //在复制文件加入工程时，当文件中有framework、h、a文件时，自动添加相应的搜索路径
        private static void AutoAddSearchPath(PBXProject proj, string targetGUID, string xcodePath, string filePath)
        {
            if (filePath.EndsWith(".framework", System.StringComparison.Ordinal))
            {
                //添加框架搜索路径
                string addStr = "$PROJECT_DIR" + Path.GetDirectoryName(filePath.Replace(xcodePath, ""));
                Hashtable arg = new Hashtable();
                Hashtable add = new Hashtable();
                arg.Add("+", add);
                arg.Add("=", new Hashtable());
                arg.Add("-", new Hashtable());
                var array = new ArrayList();
                array.Add(addStr);
                add.Add("FRAMEWORK_SEARCH_PATHS", array);
                SetBuildProperties(proj, targetGUID, arg);
            }
            else if (filePath.EndsWith(".h", System.StringComparison.Ordinal))
            {
                //添加头文件搜索路径
                string addStr = "$PROJECT_DIR" + Path.GetDirectoryName(filePath.Replace(xcodePath, ""));
                Hashtable arg = new Hashtable();
                Hashtable add = new Hashtable();
                arg.Add("+", add);
                arg.Add("=", new Hashtable());
                arg.Add("-", new Hashtable());
                var array = new ArrayList();
                array.Add(addStr);
                add.Add("HEADER_SEARCH_PATHS", array);
                SetBuildProperties(proj, targetGUID, arg);
            }
            else if (filePath.EndsWith(".a", System.StringComparison.Ordinal))
            {//添加静态库搜索路径
                string addStr = "$PROJECT_DIR" + Path.GetDirectoryName(filePath.Replace(xcodePath, ""));
                Hashtable arg = new Hashtable();
                Hashtable add = new Hashtable();
                arg.Add("+", add);
                arg.Add("=", new Hashtable());
                arg.Add("-", new Hashtable());
                var array = new ArrayList();
                array.Add(addStr);
                add.Add("LIBRARY_SEARCH_PATHS", array);
                SetBuildProperties(proj, targetGUID, arg);
            }
        }
        //设置编译属性
        private static void SetBuildProperties(PBXProject pbx, string targetGUID, Hashtable table)
        {
            if (table != null)
            {
                Hashtable setTable = table.SGet<Hashtable>("=");
                foreach (DictionaryEntry i in setTable)
                {
                    pbx.SetBuildProperty(targetGUID, i.Key.ToString(), i.Value.ToString());
                }
                Hashtable addTable = table.SGet<Hashtable>("+");
                foreach (DictionaryEntry i in addTable)
                {
                    ArrayList array = i.Value as ArrayList;
                    List<string> list = new List<string>();
                    foreach (var flag in array)
                    {
                        list.Add(flag.ToString());
                    }
                    pbx.UpdateBuildProperty(targetGUID, i.Key.ToString(), list, null);
                }
                Hashtable removeTable = table.SGet<Hashtable>("-");
                foreach (DictionaryEntry i in removeTable)
                {
                    ArrayList array = i.Value as ArrayList;
                    List<string> list = new List<string>();
                    foreach (var flag in array)
                    {
                        list.Add(flag.ToString());
                    }
                    pbx.UpdateBuildProperty(targetGUID, i.Key.ToString(), null, list);
                }
            }
        }
        private static bool NeedCopy(string file)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileEx = Path.GetExtension(file);
            if (fileName.StartsWith(".", System.StringComparison.Ordinal) || file.EndsWith(".gitkeep", System.StringComparison.Ordinal)
                || file.EndsWith(".DS_Store", System.StringComparison.Ordinal))
            {
                return false;
            }
            return true;
        }
    }
}
