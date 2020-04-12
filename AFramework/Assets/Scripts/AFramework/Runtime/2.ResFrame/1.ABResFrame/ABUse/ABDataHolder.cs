using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AFramework {	
	public class ABDataHolder : Singleton<ABDataHolder>
	{
	    AppInfoConfig appInfo;
	    AF_ABConfig ABConfig;
	    //下载路径的前缀
	    string URLPrefix = "";
	    //是否是国内服务器
	    public bool RegionIsChina = true;
	    bool IsABCheckFinish = false;
	    //配置文件的AB包
	    AssetBundle configAB;
	    public void ABSysInit(AppInfoConfig appInfo, AF_ABConfig ABConfig)
	    {
	        this.appInfo = appInfo;
	        this.ABConfig = ABConfig;
	        AFLogger.d("ABSysInit");
	    }
	    APPVersionStatus versionStatus;
	    System.Action<APPVersionStatus> ABCheckInitFinish;
	    /// <summary>
	    /// 注册AB包初始化完成的回调,此初始化包括检测版本号以及下载AssetbundleConfig文件以及加载的一系列初始化,返回的bool值是指APP版本号状态
	    /// </summary>
	    public void RegisterABInit(System.Action<APPVersionStatus> ABInitFinish)
	    {
	        if (IsABCheckFinish)
	        {
	            ABInitFinish.Invoke(versionStatus);
	        }
	        else
	        {
	            this.ABCheckInitFinish = ABInitFinish;
	        }
	    }

	    public void CheckAPPVersion()
	    {
	        switch (ABConfig.ABResLoadfrom)
	        {
	            case ABResLoadFrom.PersistentDataPathAB:
                    DownManager.Instance.StartByteDown(GetURLPrefix() + "APPInfo.txt", AppVersionCall, PathTool.PersistentDataPath + "APPInfo.txt", null);
                    break;
	            case ABResLoadFrom.StreamingAssetAB:
	                string infoPath = "";
	                if (ABConfig.isDevelopOrOnlineAB)
	                    infoPath = "AF-ABForLocal/AF-InfoFile" + ABConfig.ABDevelopmentVersion + "/APPInfo";
	                else
	                    infoPath = "AF-ABForLocal/AF-InfoFile" + ABConfig.ABOnlineVersion + "/APPInfo";
	                TextAsset textAsset = Resources.Load<TextAsset>(infoPath);
	                if (textAsset != null)
	                {
	                    AppVersionCall(textAsset.bytes);
	                }
	                else
	                {
	                    AFLogger.e("Resources没有appInfo.txt,请检查");
	                }
	                break;
	            case ABResLoadFrom.EditorRes:
	                ABInit(default(byte[]));
	                break;
	        }
	    }
        public AppType GetAPPType()
        {
			return ABConfig.CurrentAppType;
        }
        public List<AF_ABOneClass> GetAllClass()
        {
			return ABConfig.m_AllClass;

		}
		public bool JudgeCanLoadAB(string abPath)
	    {
	        if ((ABConfig.ABResLoadfrom == ABResLoadFrom.StreamingAssetAB) ||
	            (ABConfig.ABResLoadfrom == ABResLoadFrom.PersistentDataPathAB && FileHelper.JudgeFilePathExit(abPath)))
	        {
	            return true;
	        }
	        return false;
	    }
	    public ABResLoadFrom GetABResLoadFrom()
	    {
	        return ABConfig.ABResLoadfrom;
	    }
	
	
	    /// <summary>
	    /// 版本文件下载回调
	    /// </summary>
	    /// <param name="data"></param>
	    /// <param name="downResult"></param>
	    /// <param name="downError"></param>
	    public void AppVersionCall(byte[] data, DownStatus downResult = DownStatus.Sucess, string downError = "")
	    {
	        if (downResult != DownStatus.Sucess)
	        {
	            AFLogger.e("下载Version文件未成功,请检查!");
	            return;
	        }
	        AppInfoConfig appInfoConfig = SerializeHelper.FromJson<AppInfoConfig>(System.Text.Encoding.UTF8.GetString(data));
	        double appV = 0;
	        if (ABConfig.isDevelopOrOnlineAB)
	        {
	            appV = ABConfig.ABDevelopmentVersion;
	        }
	        else
	        {
	            appV = ABConfig.ABOnlineVersion;
	        }
	        if (appInfoConfig.AvailableABVersions.Contains(appV))
	        {
	            if (appInfoConfig.NewABVersion.Contains(appV))
	            {
	                versionStatus = APPVersionStatus.Newest;
	            }
	            else
	            {
	                versionStatus = APPVersionStatus.Update;
	            }
	            if (CheckABConfig())
	            {
	                ABInit(default(byte[]));
	            }
	        }
	        else
	        {
	            versionStatus = APPVersionStatus.Abandon;
	            ABCheckInitFinish.InvokeGracefully(versionStatus);
	            IsABCheckFinish = true;
	        }
	    }
	    /// <summary>
	    /// 检测AssetbundleConfig文件是否存在
	    /// </summary>
	    public bool CheckABConfig()
	    {
	        //本地没有需要下载
	        switch (ABConfig.ABResLoadfrom)
	        {
	            case ABResLoadFrom.PersistentDataPathAB:
	                //检查本地是否已经有AssetbundleConig文件
	                if (CheckABConfigExit(GetABPrefix()))
	                {
	                    return true;
	                }
	                else
	                {
	                    DownManager.Instance.StartByteDown(GetABConfigDownPath(), ABInit, GetABConfigLocalPath(), null);
	                }
	                break;
	            case ABResLoadFrom.StreamingAssetAB:
#if UNITY_EDITOR
	                if (CheckABConfigExit(PathTool.ProjectPath + "Assets/Resources/AF-ABForLocal/AF-InfoFile" + GetVersionStr() + "/" + GetStrByPlatform()))
	                {
	                    return true;
	                }
#else
	                if (!Resources.Load<TextAsset>("AF-ABForLocal/AF-InfoFile" + GetVersionStr() + "/" + GetStrByPlatform()))
	                {
	                    return true;
	                }
#endif
	                AFLogger.e("StreamingAssetAB模式下没有AssetbundleConfig文件,请检查是否未打对应平台的AB或者选错AB包加载来源");
	                break;
	            case ABResLoadFrom.EditorRes:
	                return true;
	        }
	        return false;
	    }
	    /// <summary>
	    /// 下载Assetbundleconfig文件回调
	    /// </summary>
	    /// <param name="data"></param>
	    /// <param name="downResult"></param>
	    /// <param name="downError"></param>
	    public void ABInit(byte[] data, DownStatus downResult = DownStatus.Sucess, string downError = "")
	    {
	        if (downResult != DownStatus.Sucess)
	        {
	            AFLogger.d("下载ABConfig文件失败:" + downError);
	            return;
	        }
	        ABConfigInit();
	        IsABCheckFinish = true;
	        //开始注册事件
	        ABCheckInitFinish.InvokeGracefully(versionStatus);
	    }
	    public bool CheckABConfigExit(string ConfigPathPre)
	    {
	        switch (ABConfig.configWritingMode)
	        {
	            case ConfigWritingMode.Binary:
	                Debug.Log("ABConfig路径:" + ConfigPathPre + ABConfigName);
	                return FileHelper.JudgeFilePathExit(ConfigPathPre + ABConfigName);
	            case ConfigWritingMode.TXT:
	                Debug.Log("ABConfig路径:" + (ConfigPathPre + ABConfigName + ".txt"));
	                return FileHelper.JudgeFilePathExit(ConfigPathPre + ABConfigName + ".txt");
	            case ConfigWritingMode.XML:
	                Debug.Log("ABConfig路径:" + (ConfigPathPre + ABConfigName + ".xml"));
	                return FileHelper.JudgeFilePathExit(ConfigPathPre + ABConfigName + ".xml");
	        }
	        return false;
	    }
	    /// <summary>
	    /// AB包加载初始化
	    /// </summary>
	    public void ABConfigInit()
	    {
	        switch (ABConfig.ABResLoadfrom)
	        {
	            case ABResLoadFrom.PersistentDataPathAB:
	                SafeObjectPool<ABRes>.Instance.Init(40, 10);
	                LoadAssetManifest();
	                break;
	            case ABResLoadFrom.StreamingAssetAB:
	                SafeObjectPool<ABRes>.Instance.Init(40, 10);
	                LoadAssetManifest();
	                break;
	            case ABResLoadFrom.EditorRes:
	                SafeObjectPool<EditorRes>.Instance.Init(40, 20);
	                break;
	        }
	    }
	    /// <summary>
	    /// 加载Androidmanifest
	    /// </summary>
	    /// <returns></returns>
	    public bool LoadAssetManifest()
	    {
	        AssetBundleConfig config = null;
	        switch (ABConfig.configWritingMode)
	        {
	            case ConfigWritingMode.Binary:
	                if (FileHelper.JudgeFilePathExit(GetABConfigLocalPath()))
	                {
	                    if (configAB != null)
	                    {
	                        configAB.Unload(false);
	                    }
	                    configAB = AssetBundle.LoadFromFile(GetABConfigLocalPath());
	                    TextAsset textAsset = configAB.LoadAsset<TextAsset>(ABConfigName);
	
	                    if (textAsset == null)
	                    {
	                        Debug.LogError("AssetBundleConfig is no exist!");
	                        return false;
	                    }
	                    //反序列化，得到打包的信息
	                    MemoryStream stream = new MemoryStream(textAsset.bytes);
	                    BinaryFormatter bf = new BinaryFormatter();
	                    config = (AssetBundleConfig)bf.Deserialize(stream);
	                    stream.Close();
	                }
	                else
	                {
	                    AFLogger.e("AssetbundleConfig文件不存在");
	                    return false;
	                }
	                break;
	            case ConfigWritingMode.TXT:
	                string abJson = "";
	                if (ABConfig.ABResLoadfrom == ABResLoadFrom.PersistentDataPathAB)
	                {
	                    abJson = FileHelper.ReadTxtToStr(GetABConfigLocalPath());
	                    if (abJson.Equals(""))
	                    {
	                        AFLogger.e("AssetbundleConfig文件不存在或者内容为空");
	                        return false;
	                    }
	                }
	                else if (ABConfig.ABResLoadfrom == ABResLoadFrom.StreamingAssetAB)
	                {
	                    string abConfigPath = "AF-ABForLocal/AF-InfoFile" + GetVersionStr() + "/" + GetStrByPlatform() + ABConfigName;
	                    TextAsset textAsset = ResManager.Instance.LoadSync(ResLoadInfo.Allocate(ResFromType.ResourcesRes, abConfigPath, false)) as TextAsset;
	                    abJson = textAsset.text;
	                }
	                config = SerializeHelper.FromJson<AssetBundleConfig>(abJson);
	                break;
	            case ConfigWritingMode.XML:
	                XmlDocument xml = new XmlDocument();
	                MemoryStream ms = null;
	                if (ABConfig.ABResLoadfrom == ABResLoadFrom.PersistentDataPathAB)
	                {
	                    xml.Load(GetABConfigLocalPath());
	                }
	                else
	                {
	                    string abConfigPath = "AF-ABForLocal/AF-InfoFile" + GetVersionStr() + "/" + GetStrByPlatform() + ABConfigName;
	                    TextAsset textAsset = ResManager.Instance.LoadSync(ResLoadInfo.Allocate(ResFromType.ResourcesRes, abConfigPath, false)) as TextAsset;
	
	                    //由于编码问题，要设置编码方式
	                    byte[] encodeString = System.Text.Encoding.UTF8.GetBytes(textAsset.text);
	                    //以流的形式来读取
	                    ms = new MemoryStream(encodeString);
	                    xml.Load(ms);
	                }
	                if (xml == null)
	                {
	                    AFLogger.e("AssetbundleConfig文件不存在或者内容为空");
	                    return false;
	                }
	                XmlSerializer xmldes = new XmlSerializer(typeof(AssetBundleConfig));
	                StringReader sr = new StringReader(xml.InnerXml);
	                config = (AssetBundleConfig)xmldes.Deserialize(sr);
	                if (ms != null)
	                {
	                    ms.Close();
	                    ms.Dispose();
	                }
	                if (sr != null)
	                {
	                    sr.Close();
	                    sr.Dispose();
	                }
	                break;
	        }
	        ResManager.Instance.CacheABConfig(config);
	        return true;
	    }
	    string ABConfigName = "AssetbundleConfig";
	    public string GetABConfigDownPath()
	    {
	        if (ABConfig.configWritingMode == ConfigWritingMode.Binary)
	        {
	            return GetABDownURL() + ABConfigName;
	        }
	        else
	        {
	            return GetMD5DownURL() + GetABConfigPath();
	        }
	    }
	    public string GetABConfigSavePath()
	    {
	        if (ABConfig.configWritingMode == ConfigWritingMode.Binary)
	        {
	            return PathTool.PersistentDataPath + ABConfigName;
	        }
	        else
	        {
	            return PathTool.PersistentDataPath + GetABConfigPath();
	        }
	    }
	    public string GetABConfigLocalPath()
	    {
	        if (ABConfig.configWritingMode == ConfigWritingMode.Binary)
	        {
	            return GetABPrefix() + ABConfigName;
	        }
	        else
	        {
	            return PathTool.PersistentDataPath + GetABConfigPath();
	        }
	    }
	    public string GetABConfigPath()
	    {
	        switch (ABConfig.configWritingMode)
	        {
	            case ConfigWritingMode.Binary:
	                return ABConfigName;
	            case ConfigWritingMode.TXT:
	                return ABConfigName + ".txt";
	            case ConfigWritingMode.XML:
	                return ABConfigName + ".xml";
	        }
	        return "";
	    }
	    public string GetURLPrefix()
	    {
	        if (URLPrefix == "")
	        {
	            if (ABConfig.isDevelopOrOnlineAB)
	            {
	                URLPrefix = ABConfig.ABDownURLPrefix + "/" + ABConfig.DevelopPrefix + "/AF-ABForServer/" + ABConfig.CurrentAppType.ToString() +
	                    "/";
	            }
	            else
	            {
	                URLPrefix = ABConfig.ABDownURLPrefix + "/" + ABConfig.OnlinePrefix + "/AF-ABForServer/" + ABConfig.CurrentAppType.ToString() + "/";
	            }
	        }
	        return URLPrefix;
	    }
	    string ABDownPrefix = "";
	    /// <summary>
	    /// 获取公共路径,无需中英文路径
	    /// </summary>
	    /// <returns>The down URLB y name.</returns>
	    public string GetABDownURL()
	    {
	        if (ABDownPrefix == "")
	            ABDownPrefix = GetURLPrefix() + "AF-ABResource" + GetVersionStr() + "/" + GetStrByPlatform();
	        return ABDownPrefix;
	    }
	    string MD5DownPrefix = "";
	    public string GetMD5DownURL()
	    {
	        if (MD5DownPrefix == "")
	            MD5DownPrefix = GetURLPrefix() + "AF-InfoFile" + GetVersionStr() + "/" + GetStrByPlatform();
	        return MD5DownPrefix;
	    }
	    string ABPrefix = "";
	    public string GetABPrefix()
	    {
	        if (ABPrefix == "")
	        {
	            switch (ABConfig.ABResLoadfrom)
	            {
	                case ABResLoadFrom.PersistentDataPathAB:
	                    ABPrefix = PathTool.PersistentDataPath;
	                    break;
	                case ABResLoadFrom.StreamingAssetAB:
	                    ABPrefix = PathTool.StreamingAssetsPath + "AF-ABForLocal/AF-ABResource" + GetVersionStr() + "/" + GetStrByPlatform();
	                    break;
	            }
	        }
	        return ABPrefix;
	    }
	    string VersionStr = "";
	    public string GetVersionStr()
	    {
	        if (VersionStr == "")
	        {
	            if (ABConfig.isDevelopOrOnlineAB)
	                VersionStr = ABConfig.ABDevelopmentVersion.ToString();
	            else
	                VersionStr = ABConfig.ABOnlineVersion.ToString();
	        }
	        return VersionStr;
	    }
	
	    string platformStr = "";
	    public string GetStrByPlatform()
	    {
	        if (platformStr == "")
	        {
#if UNITY_ANDROID
	            platformStr = "Android/";
#elif UNITY_IOS
	            platformStr = "iOS/";
#endif
	        }
	        return platformStr;
	    }
	
	
#if UNITY_EDITOR
	    [UnityEditor.Callbacks.DidReloadScripts(1)]
	    public static void UpdateABClass()
	    {
	        string ABConfigPath = AssetDatabasex.GetAssetPathStr("AF_ABConfig");
	        AF_ABConfig abConfig = null;
	        if (!ABConfigPath.IsNotNullAndEmpty())
	        {
	            abConfig = new AF_ABConfig();
	            ABConfigPath = "Assets/Scripts/AFData/AF-ConfigPath/AF_ABConfig.asset";
	        }
	        else
	        {
	            abConfig = AssetDatabase.LoadAssetAtPath<AF_ABConfig>(ABConfigPath);
	        }
	        foreach (ARForProductType aBClass in System.Enum.GetValues(typeof(ARForProductType)))
	        {
	            if (abConfig != null && abConfig.JudgeClassExit(aBClass))
	            {
	                continue;
	            }
	            AF_ABOneClass aF_ABOneClass = new AF_ABOneClass();
	            aF_ABOneClass.ABClassType = aBClass.ToString();
	            abConfig.m_AllClass.Add(aF_ABOneClass);
	        }
	        if (!FileHelper.JudgeFilePathExit(System.Environment.CurrentDirectory + "/" + ABConfigPath))
	        {
	            AssetDatabase.CreateAsset(abConfig, ABConfigPath);
	            AssetDatabase.SaveAssets();
	            AssetDatabase.Refresh();
	        }
	    }
#endif
	}
	public enum APPVersionStatus
	{
	    Newest, //APP是最新的
	    Abandon, //APP资源不再维护
	    Update  //APP需要更新,但是资源还是在维护的,因此可以用
	}
	
	[System.Serializable]
	//与后台保持一致/注意顺序,更改的话要查看AF_ABConfig类型是否正确
	public enum ARForProductType
	{
	}
}
