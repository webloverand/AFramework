/*******************************************************************
* Copyright(c)
* 文件名称: iOSPostProcessBuild.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditor.iOS.Xcode;
    using UnityEngine;

    public class iOSPostProcessBuild : Editor
    {
        /// <summary>
        /// XCODE项目发布后的处理
        /// 键libs下是配置静态库；键frameworks下是配置框架；
        /// 键properties下是配置工程的编译属性；
        /// plist是编辑info.plist文件；f
        /// iles是待复制文件，folders是待复制文件夹；filesCompileFlg是文件编译符号的设置。
        /// 参考 : Unity成功导出Xcode工程后自动修改一些设置(https://blog.csdn.net/qq_33461689/article/details/78982696)
        /// 参考 : Unity导出xcode自动配置工具(http://www.voidcn.com/article/p-wrhdnoev-bnu.html)
        /// </summary>
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (BuildTarget.iOS != target)
            {
                return;
            }

            string[] s = AssetDatabasex.GetAssetPathStr("AppInfoConfig").Split('/');
            string XcodeSettingPath = PathTool.ProjectPath;
            for (int i = 0; i < s.Length - 1; i++)
            {
                XcodeSettingPath += s[i] + "/";
            }
            XcodeSettingPath += "Editor/XcodeSetting.txt";
            if (XcodeSettingPath == "" || !FileHelper.JudgeFilePathExit(XcodeSettingPath))
            {
                return;
            }

            XcodeSetting XcodeSetting = SerializeHelper.FromJson<XcodeSetting>(AssetDatabasex.LoadAssetOfType<TextAsset>("XcodeSetting").text);

            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject pbx = new PBXProject();
            pbx.ReadFromString(File.ReadAllText(projPath));
            string guid = pbx.TargetGuidByName("Unity-iPhone");

            iOSXcodeSet.SetTeamId(pbx, guid, XcodeSetting.TeamID);
            iOSXcodeSet.SetFrameworks(pbx, guid, XcodeSetting.frameworkToProjectList);
            iOSXcodeSet.SetLibs(pbx, guid, XcodeSetting.StaticlibraryDic);
            iOSXcodeSet.SetBuildProperty(pbx, guid, XcodeSetting.buildPropertyDic);
            //拷贝文件
            iOSXcodeSet.CopyFiles(pbx, guid, XcodeSetting.CopyFileDic, path);
            iOSXcodeSet.CopyFolders(pbx, guid, XcodeSetting.CopyFolderDic, path);
            iOSXcodeSet.SetFilesCompileFlag(pbx, guid, XcodeSetting.FilesCompileFlagDIC);

            File.WriteAllText(projPath, pbx.WriteToString());

            //修改Info.plist
            string plistPath = path + "/Info.plist";
            iOSInfoPlist pListEditor = new iOSInfoPlist(plistPath);

            if (XcodeSetting.Version != "")
            {
                pListEditor.Update("CFBundleShortVersionString", XcodeSetting.Version);
            }
            if (XcodeSetting.build != "")
            {
                pListEditor.Update("CFBundleVersion", XcodeSetting.build);
            }
            if (XcodeSetting.BundleIdentifier != "" && XcodeSetting.BundleIdentifier.Split('.').Length >= 3)
            {
                pListEditor.Update("CFBundleIdentifier", XcodeSetting.BundleIdentifier);
            }

            foreach (var entry in XcodeSetting.pListDataDic)
            {
                pListEditor.Update(entry.Key, entry.Value, entry.infoType, entry.isAdd);
            }

            foreach (var urlScheme in XcodeSetting.urlSchemeDic)
            {
                pListEditor.UpdateUrlScheme(urlScheme.Key, urlScheme.Value, urlScheme.isAdd);
            }

            foreach (var whiteUrlScheme in XcodeSetting.appQueriesSchemeList)
            {
                pListEditor.UpdateLSApplicationQueriesScheme(whiteUrlScheme.Value, whiteUrlScheme.isAdd);
            }
            pListEditor.Save();
        }

    }
}
