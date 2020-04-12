using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using AFramework;
#if AF_ARSDK_Vuforia
using Vuforia;
#endif
#endif
namespace AFramework
{
    //云端AB开发过程中版本更改说明 :
    // 1. 开发过程中 : isDevelopOrOnline设置为true,打包会使用DevelopmentVersion,同时更改NewVersion增加DevelopmentVersion(这里上传路径为测试路径)
    // 2. 开发确认完成后,更改AppOnlineVersion为DevelopmentVersion,isDevelopOrOnline设置为false,重新打最终AB包,会使用AppOnlineVersion打最终包(这里上传为正式路径)
    // 3.因为个人的测试路径不同,因此可以更改DevelopPath来更改个人的测试路径,注意当isDevelopOrOnline为true的时候会使用OnlinePath合成下载路径

    [CreateAssetMenu(fileName = "AF_ABConfig", menuName = "AFramework/CreatABConfig", order = 0)]
    public class AF_ABConfig : ScriptableObject
    {
        [InfoBox("DataSet所在路径,会根据此路径寻找.dat与.xml文件")]
        public string defaultDataSetPath = "Assets/ResForAB/DataSetFile";

        [InfoBox("AB包所属类别")]
        [OnInspectorGUI("UpdateCategoryStr")]
        public List<AF_ABOneClass> m_AllClass = new List<AF_ABOneClass>();

        [InfoBox("需要整个打包成AB文件夹路径")]
        public List<AF_OneAB> m_AllFileAB = new List<AF_OneAB>();
        [InfoBox("需要打包该路径下的所有prefab的路径")]
        public List<AF_OneAB> m_AllPrefabAB = new List<AF_OneAB>();

        public PackageABType packageABType;

        [InfoBox("生成的配置表格式,如果资源项目与逻辑项目分开则必须选择TXT模式,如果在同一个项目中建议使用XML/Binary格式")]
        public ConfigWritingMode configWritingMode = ConfigWritingMode.XML;

        //获取所有Prefab的路径
        public string[] GetAllPrefabsPath()
        {
            string[] AllPrefabsPath = new string[m_AllPrefabAB.Count];
            for (int i = 0; i < m_AllPrefabAB.Count; i++)
            {
                AllPrefabsPath[i] = m_AllPrefabAB[i].Path;
            }
            return AllPrefabsPath;
        }
        //判断m_AllClass是否已经包含类别
        public bool JudgeClassExit(ARForProductType aBClassEnum)
        {
            foreach (AF_ABOneClass oneClass in m_AllClass)
            {
                if (aBClassEnum.ToString() == oneClass.ABClassType)
                    return true;
            }
            return false;
        }

        [PropertySpace(30)]
        [InfoBox("以下信息APP运行时也会使用,请注意测试与正式版本设置", infoMessageType: InfoMessageType.Warning)]
        [InfoBox("决定下载路径以及打包版本,上线项目需要设置为false")]
        public bool isDevelopOrOnlineAB = true;
        [InfoBox("所属App类型，用于区分框架相同具体功能不同的APP")]
        public AppType CurrentAppType;
        //当前开发的AB包版本
        [ShowIf("@isDevelopOrOnlineAB")]
        public double ABDevelopmentVersion;
        //上线时AB包的版本
        [ShowIf("@!isDevelopOrOnlineAB")]
        public double ABOnlineVersion;
        //本地AB包与云端AB的区别在于资源在本地,无需区分测试版路径
        public ABResLoadFrom ABResLoadfrom;
        [ShowIf("@this.ABResLoadfrom == ABResLoadFrom.PersistentDataPathAB")]
        public string ABDownURLPrefix = "https://qianxi.fahaxiki.cn/AB/";
        [ShowIf("@isDevelopOrOnlineAB && this.ABResLoadfrom == ABResLoadFrom.PersistentDataPathAB")]
        public string DevelopPrefix = "DevelopAB";
        [ShowIf("@!isDevelopOrOnlineAB && this.ABResLoadfrom == ABResLoadFrom.PersistentDataPathAB")]
        public string OnlinePrefix = "OnlineAB";
#if UNITY_EDITOR
        [PropertyTooltip("因为StreamingAsset下的资源会全部被打包到AB包中,因此在打Android或者iOS要删除其他平台的AB包")]
        [ShowIf("@this.ABResLoadfrom==ABResLoadFrom.PersistentDataPathAB")]
        [ButtonGroup("DeleteStreaming")]
        [Button(ButtonSizes.Small)]
        public void DeleteAndroidABOfStreamingAB()
        {
            FileHelper.DeleteDir(EditorAssetPath.GetStreamingAssetABPath(this) + "Android");
            FileHelper.DeleteDir(EditorAssetPath.GetStreamingAssetInfoPath(this, true) + "Android");
            AssetDatabase.Refresh();
        }
        [ShowIf("@this.ABResLoadfrom==ABResLoadFrom.PersistentDataPathAB")]
        [ButtonGroup("DeleteStreaming")]
        [Button(ButtonSizes.Small)]
        public void DeleteIosABOfStreamingAB()
        {
            FileHelper.DeleteDir(EditorAssetPath.GetStreamingAssetABPath(this) + "iOS");
            FileHelper.DeleteDir(EditorAssetPath.GetStreamingAssetInfoPath(this, true) + "iOS");
            AssetDatabase.Refresh();
        }
        [ShowIf("@this.ABResLoadfrom==ABResLoadFrom.PersistentDataPathAB")]
        [Button(ButtonSizes.Medium)]
        public void DeleteABOfStreamingAB()
        {
            FileHelper.DeleteDir(PathTool.StreamingAssetsPath + "AF-ABForLocal");
            FileHelper.DeleteDir(PathTool.ProjectPath + "Assets/Resources/AF-ABForLocal");
            AssetDatabase.Refresh();
        }
        public void UpdateCategoryStr()
        {
            AF_OneAB.CategoryStr.Clear();
            foreach (AF_ABOneClass oneClass in m_AllClass)
            {
                if (oneClass.ABClassType == "")
                {
                    AFLogger.EditorErrorLog("m_AllClass.ABClassType不能为空");
                    return;
                }
                AF_OneAB.CategoryStr.Add(oneClass.ABClassType);
            }
        }
#endif
    }
    [System.Serializable]
    public class AF_OneAB
    {
        [Tooltip("AB包唯一标识符")]
        public string mABIdentifier;
        [Tooltip("需要打包的文件夹路径,相对于Asset")]
        public string Path = "Assets/";
        [Tooltip("所属类别")]
        [ValueDropdown("CategoryStr", SortDropdownItems = true)]
        public List<string> CategoryOfOwnership;
#if UNITY_EDITOR
       public static List<string> CategoryStr = new List<string>();
#endif
    }
    [System.Serializable]
    public class AF_ABOneClass
    {
        public bool isNeedPackageAB = true;
        [Tooltip("AB包类别")]
        public string ABClassType;
        [PropertyTooltip("AR识别类型")]
        public ARRecogType ARrecogType;

        [ShowIf("@ARrecogType == ARRecogType.DataSet")]
        [Tooltip("DataSet列表配置")]
        public List<OneDataSet> dataSetList = new List<OneDataSet>();
        [ShowIf("@ARrecogType == ARRecogType.Plane")]
        [Tooltip("平面识别加载的Prefab")]
        public List<OneResInfo> ResInfoForPlane = new List<OneResInfo>();

        [Tooltip("此类型的AB包所属的APP类型")]
        public List<AppType> AppType = new List<AppType>();
#if UNITY_EDITOR
        public bool isSameAppType(AppType currentAppType)
        {
            return AppType.Contains(currentAppType);
        }
#endif
    }

    [System.Serializable]
    public class OneDataSet
    {
        public OneDataSet(string ABClassType)
        {
            this.ABClassType = ABClassType;
        }
        string ABClassType;
        [HorizontalGroup("DataSet")]
        [Tooltip("对应的Dataset名称,默认isHasDataset为false(mImagetTargetInfo配置均无效)")]
        public string TargetDataSetName;
#if UNITY_EDITOR && AF_ARSDK_Vuforia
        [HorizontalGroup("DataSet")]
        [Button("自动添加识别对象(未测试scan object导出的dataset)")]
        public void AutoAddTarget()
        {
            if (TargetDataSetName.IsNullOrEmpty())
            {
                AFLogger.EditorErrorLog("DataSet名称为空!");
                return;
            }
            AF_ABConfig ABConfig = AssetDatabasex.LoadAssetOfType<AF_ABConfig>("AF_ABConfig");
            char[] t = ABConfig.defaultDataSetPath.ToCharArray();
            string xmlPath = ABConfig.defaultDataSetPath;
            if (t[t.Length-1] == '/')
            {
                xmlPath += TargetDataSetName + ".xml";
            }
            else
            {

                xmlPath += "/"+TargetDataSetName + ".xml";
            }
            if (xmlPath == "")
            {
                AFLogger.d("未找到DataSet对应的xml文件,请检查m_AllFileAB中是否有配置CategoryOfOwnership为" +
                    ABClassType + "且isHasDataset为true");
                return;
            }
            if (mImagetTargetInfo == null)
            {
                mImagetTargetInfo = new List<OneTargetInfo>();
            }
            List<string> allRecogTarget = AFSDK_DataSetHandle.GetDataSetTarget(xmlPath);
            if (allRecogTarget == null)
            {
                AFLogger.EditorErrorLog("解析xml失败!");
                return;
            }
            for (int j = mImagetTargetInfo.Count - 1; j >= 0; j--)
            {
                bool isNeedRemove = true;
                for (int i = allRecogTarget.Count - 1; i >= 0; i--)
                {
                    if (mImagetTargetInfo[j].ImageTargetName.Equals(allRecogTarget[i]))
                    {
                        isNeedRemove = false;
                        allRecogTarget.RemoveAt(i);
                        break;
                    }
                }
                if (isNeedRemove)
                {
                    mImagetTargetInfo.RemoveAt(j);
                }
            }
            foreach (string recogName in allRecogTarget)
            {
                OneTargetInfo oneTargetInfo = new OneTargetInfo();
                oneTargetInfo.ImageTargetName = recogName;
                oneTargetInfo.oneResInfo.ResScale = new Vector3(1, 1, 1);
                mImagetTargetInfo.Add(oneTargetInfo);
            }
        }
#endif
        [Tooltip("识别配置列表")]
        public List<OneTargetInfo> mImagetTargetInfo = new List<OneTargetInfo>();
    }
    public enum PackageABType
    {
        //打包时为方便本地测试, 打的包直接放入Application.StreamingAsset文件夹下,注意这种打包模式不能上传到云端
        StreamingAssetAB,

        ServerAB, //生成上传到云端的AB包

        PhoneAB //生成 云端AB下载到手机中的格式(简单的说就是AB包以及info文件的位置与ServerAB不一致)
    }

    public enum ARRecogType
    {
        None,
        DataSet,
        Plane
    }

    public enum ABResLoadFrom
    {
        EditorRes,   //直接从编辑器加载, 以免一点点小改动就要重新打AB包, 毕竟拷贝AB包以及重写文件都需要等待时间
        StreamingAssetAB, //从StreamingAsset文件夹加载AB包,此模式本地自动绑定需要测试的AR入口(需要出现AR入口的选项), 避免所有联网功能, 可用于增加演示项目以及展会演示项目
        PersistentDataPathAB  //从PersistentDataPath文件夹下加载AB,此模式可以直接生成PhoneAB,然后拷贝到手机上测试
    }

    public enum AppType
    {
        Museum,
        SpaceG
    }
}
