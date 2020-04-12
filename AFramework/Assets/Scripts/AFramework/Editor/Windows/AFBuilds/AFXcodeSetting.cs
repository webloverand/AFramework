/*******************************************************************
* Copyright(c)
* 文件名称: AFXcodeSetting.cs
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
    public class AFXcodeSetting : ScriptableObject
    {
        public AFXcodeSetting(string appinfoPath)
        {
            string[] t = appinfoPath.Split('/');

            XcodeSettingTxtPath = PathTool.ProjectPath;
            for (int i = 0; i < t.Length - 1; i++)
            {
                XcodeSettingTxtPath += t[i] + "/";
            }
            XcodeSettingTxtPath += "Editor/XcodeSetting.txt";
        }
        public void InitWithExit(XcodeSetting xcodeSetting)
        {
            TeamID = xcodeSetting.TeamID;
            Version = xcodeSetting.Version;
            build = xcodeSetting.build;

            BundleIdentifier = xcodeSetting.BundleIdentifier;
            StaticlibraryList = xcodeSetting.StaticlibraryDic;
            buildPropertyList = xcodeSetting.buildPropertyDic;
            pListDataList = xcodeSetting.pListDataDic;
            urlSchemeList = xcodeSetting.urlSchemeDic;
            frameworkToProjectList = xcodeSetting.frameworkToProjectList;
            appQueriesSchemeList = xcodeSetting.appQueriesSchemeList;

            CopyFileDic = xcodeSetting.CopyFileDic;
            CopyFolderDic = xcodeSetting.CopyFolderDic;
            FilesCompileFlagDIC = xcodeSetting.FilesCompileFlagDIC;
        }

        [InfoBox("Xcode配置保存路径")]
        [ReadOnly]
        public string XcodeSettingTxtPath;

        [InfoBox("此设置均高于PlayerSetting设置,为空或者格式不正确则不会设置,另外请注意保存,否则无法生效!", InfoMessageType.Warning)]
        [Button("保存XCode设置", ButtonSizes.Medium), PropertyOrder(-1)]
        public void SaveSetting()
        {
            XcodeSetting xcodeSetting = new XcodeSetting();
            xcodeSetting.TeamID = TeamID;
            xcodeSetting.Version = Version;
            xcodeSetting.build = build;
            xcodeSetting.BundleIdentifier = BundleIdentifier;

            xcodeSetting.StaticlibraryDic = StaticlibraryList;
            xcodeSetting.buildPropertyDic = buildPropertyList;
            xcodeSetting.pListDataDic = pListDataList;
            xcodeSetting.urlSchemeDic = urlSchemeList;
            xcodeSetting.frameworkToProjectList = frameworkToProjectList;
            xcodeSetting.appQueriesSchemeList = appQueriesSchemeList;
            xcodeSetting.CopyFileDic = CopyFileDic;
            xcodeSetting.CopyFolderDic = CopyFolderDic;
            xcodeSetting.FilesCompileFlagDIC = FilesCompileFlagDIC;

            FileHelper.CreatFile(XcodeSettingTxtPath, SerializeHelper.ToByteArray(xcodeSetting), true);
        }
        [InfoBox("TeamID")]
        public string TeamID = "";
        [InfoBox("包名")]
        [InfoBox("包名不符合规范,请检查!", InfoMessageType.Error, "CheckBundleIdentifier")]
        public string BundleIdentifier = "";
        [InfoBox("版本号")]
        public string Version = "";
        [InfoBox("build")]
        public string build = "";



        [InfoBox("framework库配置,例如:MediaPlayer.framework")]
        [ShowInInspector]
        public List<XcodeSettingList> frameworkToProjectList = new List<XcodeSettingList>();

        [InfoBox("lib库(静态库)配置")]
        [ShowInInspector]
        public List<XcodeSettingDic> StaticlibraryList = new List<XcodeSettingDic>();

        [InfoBox("build参数配置,如果含有多个值请用,连接即可,例如:ENABLE_BITCODE NO")]
        [ShowInInspector]
        public List<XcodeSettingDic> buildPropertyList = new List<XcodeSettingDic>();

        [InfoBox("info.plist参数配置")]
        [ShowInInspector]
        public List<XcodeInfoSettingDic> pListDataList = new List<XcodeInfoSettingDic>();

        [InfoBox("urlscheme配置")]
        [ShowInInspector]
        public List<XcodeSettingDic> urlSchemeList = new List<XcodeSettingDic>();

        [InfoBox("信任urlscheme配置")]
        [ShowInInspector]
        public List<XcodeSettingList> appQueriesSchemeList = new List<XcodeSettingList>();

        [InfoBox("拷贝文件,key值是文件完整路径,value值是目标路径文件的名称")]
        [ShowInInspector]
        public List<XcodeSettingDic> CopyFileDic = new List<XcodeSettingDic>();

        [InfoBox("拷贝文件夹,key值是文件夹完整路径,value值是目标路径文件夹的名称")]
        [ShowInInspector]
        public List<XcodeSettingDic> CopyFolderDic = new List<XcodeSettingDic>();

        [InfoBox("文件编译符号,如果含有多个值请用,连接即可")]
        [ShowInInspector]
        public List<XcodeSettingDic> FilesCompileFlagDIC = new List<XcodeSettingDic>();

        [Button("打开Xcode配置保存路径", ButtonSizes.Medium)]
        public void OpenXcodeSettingTxtPath()
        {
            if (FileHelper.JudgeFilePathExit(XcodeSettingTxtPath))
            {
                EditorUtility.RevealInFinder(XcodeSettingTxtPath);
            }
        }
        public bool CheckBundleIdentifier()
        {
            if (BundleIdentifier != null && BundleIdentifier != "" && BundleIdentifier.Split('.').Length >= 3)
            {
                return false;
            }
            return true;
        }
    }
    public class XcodeSetting
    {
        public string TeamID;
        public string Version;
        public string BundleIdentifier;
        public string build;
        public List<XcodeSettingDic> StaticlibraryDic;
        public List<XcodeSettingDic> buildPropertyDic;
        public List<XcodeInfoSettingDic> pListDataDic;
        public List<XcodeSettingList> frameworkToProjectList;
        public List<XcodeSettingDic> urlSchemeDic;
        public List<XcodeSettingList> appQueriesSchemeList;
        public List<XcodeSettingDic> CopyFileDic;
        public List<XcodeSettingDic> CopyFolderDic;
        public List<XcodeSettingDic> FilesCompileFlagDIC;
    }
    [SerializeField]
    public class XcodeSettingDic
    {
        [HorizontalGroup("Dic", 50)]
        [HideLabel]
        public bool isAdd = true;
        [HorizontalGroup("Dic", LabelWidth = 10)]
        public string Key;
        [HorizontalGroup("Dic", LabelWidth = 50)]
        public string Value;
        [LabelWidth(100)]
        public string Description;
    }
    [SerializeField]
    public class XcodeSettingList
    {
        [HorizontalGroup("List", 50)]
        [HideLabel]
        public bool isAdd = true;

        [HorizontalGroup("List", LabelWidth = 50)]
        public string Value;
        [LabelWidth(100)]
        public string Description;
    }
    public class XcodeInfoSettingDic : XcodeSettingDic
    {
        public XcodeInfoPListType infoType;
    }
    public enum XcodeInfoPListType
    {
        StringInfo,
        BoolInfo,
        IntInfo
    }
}
