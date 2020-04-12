using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
#if UNITY_EDITOR
using UnityEditor;
namespace AFramework
{
    public class ABBuildEditor
    {
        //初步打包的路径
        private static string m_BundleTargetPath = "";
        //打包平台
        static BuildTarget buildTarget;

        //key:ab包名,value : 路径 所有文件夹ab包dir
        private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
        //过滤的list
        private static List<string> m_AllFileAB = new List<string>();
        //单个prefab的ab包
        private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
        //储存所有有效路径
        private static List<string> m_ConfigFile = new List<string>();

        [MenuItem("Tools/AFramework/AB/Build AB(ios)")]
        public static void iOSBuild()
        {
            buildTarget = BuildTarget.iOS;
            Build();
        }
        [MenuItem("Tools/AFramework/AB/Build AB(Android)")]
        public static void AndroidBuild()
        {
            buildTarget = BuildTarget.Android;
            Build();
        }
        public static void Build()
        {
            m_BundleTargetPath = EditorAssetPath.ProjectPath + "AssetBundle/" + buildTarget.ToString() + "/";
            AppInfoConfig appInfoConfig = AssetDatabasex.LoadAssetOfType<AppInfoConfig>("AppInfoConfig");
            AF_ABConfig abConfig = AssetDatabasex.LoadAssetOfType<AF_ABConfig>("AF_ABConfig");
            EditorAssetPath.InitABBuildPath(abConfig);
            //清空
            m_ConfigFile.Clear();
            m_AllFileAB.Clear();
            m_AllFileDir.Clear();
            m_AllPrefabDir.Clear();
            string mTargetPath = EditorAssetPath.ABTargetPath ;
            string mInfoPath = EditorAssetPath.ABInfoPath;
            if (abConfig.packageABType != PackageABType.PhoneAB)
            {
                 mTargetPath = EditorAssetPath.ABTargetPath + buildTarget.ToString() + "/";
                 mInfoPath = EditorAssetPath.ABInfoPath + buildTarget.ToString() + "/";
            }

            if (Directory.Exists(mTargetPath))
            {
                FileUtil.DeleteFileOrDirectory(mTargetPath);
            }
            if (Directory.Exists(mInfoPath))
            {
                FileUtil.DeleteFileOrDirectory(mInfoPath);
            }
            Directory.CreateDirectory(mInfoPath);
            Directory.CreateDirectory(mTargetPath);
            Directory.CreateDirectory(m_BundleTargetPath);
            //创建APP版本文件
            FileHelper.CreatFile(EditorAssetPath.AppInfoPath, SerializeHelper.ToByteArray(appInfoConfig), true);
            Dictionary<string, bool> ClassToNeedPackageAB = new Dictionary<string, bool>();
            for (int i = 0; i < abConfig.m_AllClass.Count; i++)
            {
                if (abConfig.m_AllClass[i].isSameAppType(abConfig.CurrentAppType))
                {
                    ClassToNeedPackageAB.Add(abConfig.m_AllClass[i].ABClassType, abConfig.m_AllClass[i].isNeedPackageAB);
                }
            }
            //剔除重复路径,将单个文件及文件夹的路径保存下来
            //先处理文件夹再处理单个文件,因为有可能prefab依赖某个文件中的东西,所以要进行过滤
            foreach (AF_OneAB oneAB in abConfig.m_AllFileAB)
            {
                bool isNeedPackageAB = false;
                for (int k = 0; k < oneAB.CategoryOfOwnership.Count; k++)
                {
                    if (ClassToNeedPackageAB.ContainsKey(oneAB.CategoryOfOwnership[k]) &&
                        ClassToNeedPackageAB[oneAB.CategoryOfOwnership[k]])
                    {
                        isNeedPackageAB = true;
                        break;
                    }
                }
                if(!isNeedPackageAB)
                {
                    continue;
                }
                if (m_AllFileDir.ContainsKey(oneAB.mABIdentifier))
                {
                    AFLogger.EditorErrorLog("AB包配置名字重复，请检查！");
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    m_AllFileDir.Add(oneAB.mABIdentifier, oneAB.Path);
                    //保存已打包的AB包路径
                    m_AllFileAB.Add(oneAB.Path);
                    m_ConfigFile.Add(oneAB.Path);
                }
            }
            string[] AllPrefabsPath = abConfig.GetAllPrefabsPath();
            if (AllPrefabsPath.Length > 0)
            {
                //获取路径下的所有prefab
                string[] allStr = AssetDatabase.FindAssets("t:Prefab", AllPrefabsPath);
                AFLogger.EditorInfoLog("获取的路径长度:" + allStr.Length);
                for (int i = 0; i < allStr.Length; i++)
                {
                    bool isNeedPackageAB = false;
                    for (int k = 0; k < abConfig.m_AllPrefabAB[i].CategoryOfOwnership.Count; k++)
                    {
                        if (ClassToNeedPackageAB.ContainsKey(abConfig.m_AllPrefabAB[i].CategoryOfOwnership[k]) &&
                            ClassToNeedPackageAB[abConfig.m_AllPrefabAB[i].CategoryOfOwnership[k]])
                        {
                            isNeedPackageAB = true;
                            break;
                        }
                    }
                    if (!isNeedPackageAB)
                    {
                        continue;
                    }
                    //GUID转asset路径
                    string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
                    EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, i * 1.0f / allStr.Length);
                    m_ConfigFile.Add(path);
                    if (!ContainAllFileAB(path))
                    {
                        //获取资源的所有依赖
                        string[] allDepend = AssetDatabase.GetDependencies(path);
                        List<string> allDependPath = new List<string>();
                        //遍历依赖文件
                        for (int j = 0; j < allDepend.Length; j++)
                        {
                            if (!ContainAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs", System.StringComparison.Ordinal))
                            {
                                m_AllFileAB.Add(allDepend[j]);
                                allDependPath.Add(allDepend[j]);
                            }
                        }
                        if (m_AllPrefabDir.ContainsKey(abConfig.m_AllPrefabAB[i].mABIdentifier))
                        {
                            AFLogger.EditorErrorLog("存在相同名字的Prefab！名字：" + abConfig.m_AllPrefabAB[i].mABIdentifier);
                            EditorUtility.ClearProgressBar();
                        }
                        else
                        {
                            m_AllPrefabDir.Add(abConfig.m_AllPrefabAB[i].mABIdentifier, allDependPath);
                        }
                    }
                }
            }
            //点击打包之后可以看见更改的文件的.meta文件改变
            //文件夹设置ABName
            foreach (string name in m_AllFileDir.Keys)
            {
                SetABName(name, m_AllFileDir[name]);
            }
            //单个文件设置包名
            foreach (string name in m_AllPrefabDir.Keys)
            {
                SetABName(name, m_AllPrefabDir[name]);
            }

            //打包
            BunildAssetBundle(abConfig);

            //清除AB包名,因为上面更改了.meta文件,我们很多时候会使用svn或者git,有可能会导致一不小心上传特别乱
            string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0; i < oldABNames.Length; i++)
            {
                AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
                EditorUtility.DisplayProgressBar("清除AB包名", "名字：" + oldABNames[i], i * 1.0f / oldABNames.Length);
            }

            EditorUtility.ClearProgressBar();
            //根据设置生成文件
            ABBuildFinishEvent.BundleFinish(mTargetPath, m_BundleTargetPath, abConfig, mInfoPath, appInfoConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        static void SetABName(string name, string path)
        {
            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
            if (assetImporter == null)
            {
               AFLogger.EditorErrorLog("不存在此路径文件：" + path);
            }
            else
            {
                assetImporter.assetBundleName = name.ToLower();
            }
        }

        static void SetABName(string name, List<string> paths)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                SetABName(name.ToLower(), paths[i]);
            }
        }

        static void BunildAssetBundle(AF_ABConfig abConfig)
        {
            string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
            //key : 为全路径 value :为名字
            Dictionary<string, string> resPathDic = new Dictionary<string, string>();
            for (int i = 0; i < allBundles.Length; i++)
            {
                string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
                for (int j = 0; j < allBundlePath.Length; j++)
                {
                    //判断有没有包含脚本文件
                    if (allBundlePath[j].EndsWith(".cs"))
                        continue;

                    //Debug.Log("此AB包：" + allBundles[i] + "下面包含的资源文件路径：" + allBundlePath[j]);
                    if (ValidPath(allBundlePath[j]))
                    {
                        resPathDic.Add(allBundlePath[j], allBundles[i]);
                    }
                }
            }

            //现在已经设置好了在这一次要打包的AB包，因此要清除无用的AB包,比如之前存在的AB包
            DeleteAB();
            //生成自己的配置表
            WriteData(resPathDic, abConfig.configWritingMode,abConfig.packageABType);

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(m_BundleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);
            if (manifest == null)
            {
                AFLogger.EditorErrorLog("AssetBundle" + buildTarget.ToString() + "打包失败,请检查设置!");
            }
            else
            {
                AFLogger.d("AssetBundle" + buildTarget.ToString() + "打包完毕");
            }
        }

        //写入数据
        //resPathDic : 路径对应的AB包
        static void WriteData(Dictionary<string, string> resPathDic, ConfigWritingMode configWritingMode, PackageABType packageABType)
        {
            AssetBundleConfig config = new AssetBundleConfig();
            config.ABList = new List<ABBase>();
            foreach (string path in resPathDic.Keys)
            {
                //进行赋值
                ABBase abBase = new ABBase();
                abBase.Path = path;
                abBase.Crc = Crc32.GetCrc32(path);
                abBase.ABName = resPathDic[path];
                abBase.AssetName = path.Remove(0, path.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
                abBase.ABDependce = new List<string>();
                string[] resDependce = AssetDatabase.GetDependencies(path);
                for (int i = 0; i < resDependce.Length; i++)
                {
                    string tempPath = resDependce[i];
                    //排除自身与脚本文件
                    if (tempPath == path || path.EndsWith(".cs", System.StringComparison.Ordinal))
                        continue;

                    string abName = "";
                    //所依赖的资源是不是在其他AB包里面
                    if (resPathDic.TryGetValue(tempPath, out abName))
                    {
                        if (abName == resPathDic[path])
                            continue;
                        //判断是否已经添加到aBBase包里面
                        if (!abBase.ABDependce.Contains(abName))
                        {
                            abBase.ABDependce.Add(abName);
                        }
                    }
                }
                config.ABList.Add(abBase);
            }
            switch (configWritingMode)
            {
                case ConfigWritingMode.TXT:
                    string txtPath = EditorAssetPath.ABInfoPath + buildTarget.ToString() + "/AssetbundleConfig.txt";
                    if (File.Exists(txtPath)) File.Delete(txtPath);
                    FileStream fileStream1 = new FileStream(txtPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    StreamWriter sw1 = new StreamWriter(fileStream1, System.Text.Encoding.UTF8);
                    string configJson = SerializeHelper.ToJson(config);
                    sw1.Write(configJson);
                    sw1.Close();
                    fileStream1.Close();
                    break;
                case ConfigWritingMode.XML:
                    CreateXML(config, packageABType);
                    break;
                case ConfigWritingMode.Binary:
                    foreach (ABBase abBase in config.ABList)
                    {
                        abBase.Path = "";
                    }
                    string BinaryPath ="Assets/AssetbundleConfig.bytes";
                    FileStream fs = new FileStream(PathTool.ProjectPath + BinaryPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, config);
                    fs.Close();
                    CreateXML(config,packageABType);
                    AssetDatabase.Refresh();
                    SetABName("assetbundleconfig", BinaryPath);
                    break;
            }
        }
        static void CreateXML(AssetBundleConfig config, PackageABType packageABType)
        {
            //写入xml
            string xmlPath = "";
            if(packageABType == PackageABType.PhoneAB)
            {
                xmlPath = EditorAssetPath.ABInfoPath + "/AssetbundleConfig.xml";
            }
            else
            {
                xmlPath = EditorAssetPath.ABInfoPath + buildTarget.ToString() + "/AssetbundleConfig.xml";
            }
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            
            FileStream fileStream = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
            XmlSerializer xs = new XmlSerializer(config.GetType());
            xs.Serialize(sw, config);
            sw.Close();
            fileStream.Close();
        }
        /// <summary>
        /// 删除无用/冗余AB包
        /// </summary>
        static void DeleteAB()
        {
            //现在要打包的所有AB包名
            string[] allBundlesName = AssetDatabase.GetAllAssetBundleNames();
            DirectoryInfo direction = new DirectoryInfo(m_BundleTargetPath);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                //判断当前文件名是否是要打包的AB包名
                if (ConatinABName(files[i].Name, allBundlesName) || files[i].Name.EndsWith(".meta") || files[i].Name.EndsWith(".manifest") || files[i].Name.EndsWith("assetbundleconfig"))
                {
                    continue;
                }
                else
                {
                    //删除多余的AB包
                    if (File.Exists(files[i].FullName))
                    {
                        File.Delete(files[i].FullName);
                    }
                    if (File.Exists(files[i].FullName + ".manifest"))
                    {
                        File.Delete(files[i].FullName + ".manifest");
                    }
                }
            }
        }

        /// <summary>
        /// 遍历文件夹里的文件名与设置的所有AB包进行检查判断，判断当前文件名是否是要打包的AB包名,有就代表不需删除,没有代表要删除
        /// </summary>
        /// <param name="name"></param>
        /// <param name="strs"></param>
        /// <returns></returns>
        static bool ConatinABName(string name, string[] strs)
        {
            for (int i = 0; i < strs.Length; i++)
            {
                if (name == strs[i])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否包含在已经有的AB包里，做来做AB包冗余剔除
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static bool ContainAllFileAB(string path)
        {
            for (int i = 0; i < m_AllFileAB.Count; i++)
            {
                //为了排除文件夹相似，因此要判断一下包含之后还要判断第一个字符为/，保证是该文件夹下的子文件
                if (path == m_AllFileAB[i] || (path.Contains(m_AllFileAB[i]) && (path.Replace(m_AllFileAB[i], "")[0] == '/')))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断是否是有效路径,包含prefabs路径或者文件夹路径就认为是有效路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static bool ValidPath(string path)
        {
            for (int i = 0; i < m_ConfigFile.Count; i++)
            {
                if (path.Contains(m_ConfigFile[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
#endif
