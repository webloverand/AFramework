#if UNITY_EDITOR
using System;
namespace AFramework
{
    using System.IO;
    using UnityEngine;

    public class EditorAssetPath
    {
        //AB公共路径
        public static string ProjectPath = Environment.CurrentDirectory + "/";

        //打包目标路径
        public static string ABTargetPath;
        //Info文件目标路径
        public static string ABInfoPath;
        //用来判断APP version对应的AB包是否在维护,因此packageType为DevelopServer/Online才有用
        public static string AppInfoPath;
        /// <summary>
        /// 因为编辑器下更改static属性有时会变成"",因此使用前调用此函数进行初始化
        /// </summary>
        /// <param name="appInfoConfig"></param>
        public static void InitABBuildPath(AF_ABConfig aBConfig)
        {
            switch (aBConfig.packageABType)
            {
                case PackageABType.StreamingAssetAB:
                    ABTargetPath = GetStreamingAssetABPath(aBConfig);
                    ABInfoPath = GetStreamingAssetInfoPath(aBConfig, true);
                    AppInfoPath = ABInfoPath + "APPInfo.txt";
                    break;
                case PackageABType.PhoneAB:
                    AppInfoPath = PathTool.ProjectPath + "AF-ABForPhone/"  + "/APPInfo.txt";
                    ABTargetPath = PathTool.ProjectPath + "AF-ABForPhone/"  + "/";  //再根据类型拷贝AB包
                    ABInfoPath = PathTool.ProjectPath + "AF-ABForPhone/"  + "/";
                    break;
                case PackageABType.ServerAB:
                    if (aBConfig.isDevelopOrOnlineAB)
                    {
                        AppInfoPath = PathTool.ProjectPath + "AF-ABForServer/"+ aBConfig.CurrentAppType.ToString() + "/APPInfo.txt";
                        ABTargetPath = PathTool.ProjectPath + "AF-ABForServer/" + aBConfig.CurrentAppType.ToString() + "/AF-ABResource" + aBConfig.ABDevelopmentVersion + "/";
                        ABInfoPath = PathTool.ProjectPath + "AF-ABForServer/" + aBConfig.CurrentAppType.ToString() + "/AF-InfoFile" + aBConfig.ABDevelopmentVersion + "/";
                    }
                    else
                    {
                        AppInfoPath = PathTool.ProjectPath + "AF-ABForServer/" + aBConfig.CurrentAppType.ToString() +"/APPInfo.txt";
                        ABTargetPath = PathTool.ProjectPath + "AF-ABForServer/" + aBConfig.CurrentAppType.ToString() + "/AF-ABResource" + aBConfig.ABOnlineVersion + "/";
                        ABInfoPath = PathTool.ProjectPath + "AF-ABForServer/" + aBConfig.CurrentAppType.ToString() + "/AF-InfoFile" + aBConfig.ABOnlineVersion + "/";
                    }
                    break;
            }
        }
        /// <summary>
        /// 获取AB包保存路径
        /// </summary>
        /// <param name="aBConfig"></param>
        /// <returns></returns>
        public static string GetABPath(AF_ABConfig aBConfig, ABResLoadFrom ABResLoadfrom)
        {
            switch (ABResLoadfrom)
            {
                case ABResLoadFrom.EditorRes:
                    return "";
                case ABResLoadFrom.StreamingAssetAB:
                    return GetStreamingAssetABPath(aBConfig);
#if UNITY_EDITOR
                case ABResLoadFrom.PersistentDataPathAB:
                    return PathTool.ProjectPath + "AF-ABForServer/AF-ABResource" + aBConfig.ABDevelopmentVersion + "/";
#endif
            }
            return "";
        }
        /// <summary>
		/// 获取info保存路径
		/// </summary>
		/// <param name="aBConfig"></param>
		/// <returns></returns>
		public static string GetInfoPath(AF_ABConfig aBConfig, bool isCompletePath, ABResLoadFrom ABResLoadfrom)
        {
            switch (ABResLoadfrom)
            {
                case ABResLoadFrom.EditorRes:
                    return "";
                case ABResLoadFrom.StreamingAssetAB:
                    return GetStreamingAssetInfoPath(aBConfig, isCompletePath);
#if UNITY_EDITOR
                case ABResLoadFrom.PersistentDataPathAB:
                    return PathTool.ProjectPath + "AF-ABForServer/AF-InfoFile" + aBConfig.ABDevelopmentVersion + "/";
#endif
            }
            return "";
        }
        public static string GetStreamingAssetABPath(AF_ABConfig aBConfig)
        {
            if (aBConfig.isDevelopOrOnlineAB)
                return PathTool.StreamingAssetsPath + "AF-ABForLocal/AF-ABResource" + aBConfig.ABDevelopmentVersion + "/";
            else
                return PathTool.StreamingAssetsPath + "AF-ABForLocal/AF-ABResource" + aBConfig.ABOnlineVersion + "/";
        }
        public static string GetStreamingAssetInfoPath(AF_ABConfig aBConfig, bool isCompletePath)
        {
            string temp = "";
            if (isCompletePath)
#if UNITY_EDITOR
                temp = PathTool.ProjectPath + "Assets/Resources/AF-ABForLocal/AF-InfoFile";
#else
                temp = "";
#endif
            else
                temp = "AF-ABForLocal/AF-InfoFile" + aBConfig.ABDevelopmentVersion + "/"; //相对于Resources的目录
            if (aBConfig.isDevelopOrOnlineAB)
            {
                temp += aBConfig.ABDevelopmentVersion + "/";
            }
            else
            {
                temp += aBConfig.ABOnlineVersion + "/";
            }
            return temp;
        }
    }
}

#endif
