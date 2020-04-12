using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;

namespace AFramework
{
    public class PathTool
    {
        private static string mPersistentDataPath = "";
        private static string mStreamingAssetsPath;
        private static string mDataPath;

        // 外部目录  
        public static string PersistentDataPath
        {
            get
            {
                if (null == mPersistentDataPath || "".Equals(mPersistentDataPath))
                {
                    mPersistentDataPath = Application.persistentDataPath + "/";
                }

                return mPersistentDataPath;
            }
        }

        // 内部目录
        public static string StreamingAssetsPath
        {
            get
            {
                if (null == mStreamingAssetsPath || "".Equals(mStreamingAssetsPath))
                {
                    mStreamingAssetsPath = Application.streamingAssetsPath + "/";
                }

                return mStreamingAssetsPath;
            }
        }
        public static string DataPath
        {
            get
            {
                if (null == mDataPath || "".Equals(mDataPath))
                {
                    mDataPath = Application.dataPath + "/";
                }

                return mDataPath;
            }
        }

        private static string mProjectPathPath;
        public static string ProjectPath
        {
            get
            {
                if (null == mProjectPathPath || "".Equals(mProjectPathPath))
                {
                    mProjectPathPath = Environment.CurrentDirectory + "/";
                }

                return mProjectPathPath;
            }
        }

        // 上一级目录
        public static string GetParentDir(string dir, int floor = 1)
        {
            string subDir = dir;

            for (int i = 0; i < floor; ++i)
            {
                int last = subDir.LastIndexOf('/');
                subDir = subDir.Substring(0, last);
            }

            return subDir;
        }
        /// <summary>
        /// 获取文件夹的所有文件信息
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="fileName"></param>
        /// <param name="outResult"></param>
        public static void GetFileInFolder(string dirName, string fileName, List<string> outResult)
        {
            if (outResult == null)
            {
                return;
            }

            var dir = new DirectoryInfo(dirName);

            if (null != dir.Parent && dir.Attributes.ToString().IndexOf("System") > -1)
            {
                return;
            }

            FileInfo[] fileInfos = dir.GetFiles(fileName);
            outResult.AddRange(fileInfos.Select(fileInfo => fileInfo.FullName));

            var dirInfos = dir.GetDirectories();
            foreach (var dinfo in dirInfos)
            {
                GetFileInFolder(dinfo.FullName, fileName, outResult);
            }
        }
    }
}
