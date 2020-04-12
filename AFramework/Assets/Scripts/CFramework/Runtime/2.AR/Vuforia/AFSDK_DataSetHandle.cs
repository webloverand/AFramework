#if AF_ARSDK_Vuforia
namespace AFramework
{
    using Vuforia;
    using UnityEngine;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using AFramework;
    using System.IO;
    using System.Xml.Serialization;

    public class AFSDK_DataSetHandle : MonoBehaviour
    {
        public static Dictionary<string, AssetBundle> NameToAB = new Dictionary<string, AssetBundle>();
        /// <summary>
        /// 从外部路径加载dataset
        /// </summary>
        /// <param name="absolutePath"></param>
        public static void LoadDataSetFromPath(string absolutePath)
        {
            ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
            if (!DataSet.Exists(absolutePath, VuforiaUnity.StorageType.STORAGE_ABSOLUTE))
            {
                AFLogger.d("load dataset Exists null:" + absolutePath);
                return;
            }
            else
            {
                objectTracker.Stop();
                DataSet dataSet = objectTracker.CreateDataSet();
                if (dataSet.Load(absolutePath, VuforiaUnity.StorageType.STORAGE_ABSOLUTE))
                {
                    //这里必须要停止跟踪才能激活DataSet
                    objectTracker.ActivateDataSet(dataSet);
                }
                objectTracker.Start();
            }
        }
        /// <summary>
        /// 默认的是从StreamingAssets/Vuforia加载程序集
        /// </summary>
        /// <param name="dataSetName"></param>
        public static void LoadDataSet(string dataSetName)
        {
            ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
            objectTracker.Stop();
            DataSet dataSet = objectTracker.CreateDataSet();
            if (dataSet.Load(dataSetName))
            {
                //这里必须要停止跟踪才能激活DataSet
                objectTracker.ActivateDataSet(dataSet);
            }
            objectTracker.Start();
        }
        /// <summary>
        /// 获取xml文件中的识别对象,比如全部的识别图名称,或者模型名称
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public static List<string> GetDataSetTarget(string xmlPath)
        {
            if (xmlPath == "")
            {
                AFLogger.d("未找到DataSet对应的xml文件,请检查设置!");
                return null;
            }
            if (!FileHelper.JudgeFilePathExit(xmlPath))
            {
                AFLogger.d("DataSet对应的xml路径不存在:" + xmlPath);
                return null;
            }
            string xmlContent = FileHelper.ReadTxtToStr(xmlPath);
            if (xmlContent.Contains("ImageTarget"))
            {
                FileInfo fileInfo = new FileInfo(xmlPath);
                using (FileStream fs = fileInfo.OpenRead())
                {
                    var knownTypes = new Type[] { typeof(VuforiaImage.ImageTargetList) };

                    XmlSerializer xmlserializer = new XmlSerializer(typeof(VuforiaImage.QCARConfig), knownTypes);
                    VuforiaImage.QCARConfig data = xmlserializer.Deserialize(fs) as VuforiaImage.QCARConfig;
                    List<string> imageTargets = new List<string>();
                    foreach (var OneImageTarget in data.Tracking.ImageTarget)
                    {
                        imageTargets.Add(OneImageTarget.name);
                    }
                    return imageTargets;
                }
            }
            else if (xmlContent.Contains("ModelTarget") && xmlContent.Contains("Assembly"))
            {
                FileInfo fileInfo = new FileInfo(xmlPath);
                using (FileStream fs = fileInfo.OpenRead())
                {
                    var knownTypes = new Type[] { typeof(VuforiaModel.ModelTargetInfo),
                        typeof(VuforiaModel.ModelTargetNameInfo),
                        typeof(VuforiaModel.Assembly) ,
                        typeof(VuforiaModel.Part) };

                    XmlSerializer xmlserializer = new XmlSerializer(typeof(VuforiaModel.QCARConfig), knownTypes);
                    VuforiaModel.QCARConfig data = xmlserializer.Deserialize(fs) as VuforiaModel.QCARConfig;
                    List<string> imageTargets = new List<string>();
                    imageTargets.Add(data.Assembly.Part.name);
                    return imageTargets;
                }
            }
            return null;
        }
        /// <summary>
        /// 配置单个识别图或者Model
        /// </summary>
        /// <param name="trackedName"></param>
        /// <param name="trackComName"></param>
        /// <param name="modelPre"></param>
        /// <returns></returns>
        public static Transform ConfigTrackable(string trackedName, string trackComName = "DefaultTrackableEventHandler", GameObject modelPre = null)
        {
            IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();
            foreach (TrackableBehaviour tb in tbs)
            {
                if (tb.TrackableName == trackedName)
                {
                    tb.gameObject.name = trackedName;
                    //动态添加识别组件
                    tb.gameObject.AddComponent(ARSDK_SharedMethod.GetTypeUnknownAssembly(trackComName));
                    tb.gameObject.AddComponent<TurnOffBehaviour>();

                    GameObject augmentation = Instantiate(modelPre, tb.transform);
                    return augmentation.transform;
                }
            }
            return null;
        }
        public static Transform ConfigTrackableByAB(string trackedName, string trackComName, string ABName, string modelName)
        {
            IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();
            foreach (TrackableBehaviour tb in tbs)
            {
                if (tb.TrackableName == trackedName)
                {
                    tb.gameObject.name = trackedName;
                    //动态添加识别组件
                    tb.gameObject.AddComponent(Type.GetType(trackComName));
                    tb.gameObject.AddComponent<TurnOffBehaviour>();

                    GameObject augmentation = Instantiate(NameToAB[ABName].LoadAsset<GameObject>(modelName), tb.transform);
                    if (tb.CurrentStatus == TrackableBehaviour.Status.TRACKED)
                        augmentation.gameObject.SetActive(true);
                    else
                        augmentation.gameObject.SetActive(false);
                    return augmentation.transform;
                }
            }
            return null;
        }
        /// <summary>  
        /// 关闭指定识别数据集  
        /// </summary>  
        /// <param name="datasetName">数据集名称或绝对路径</param>  
        public static void DeactivateDateset(string datasetName)
        {
            ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

            IEnumerable<DataSet> activeDataSets = objectTracker.GetActiveDataSets();

            List<DataSet> activeDataSetsToBeRemoved = activeDataSets.ToList();

            List<DataSet> dataSetsToBeActive = new List<DataSet>();

            foreach (DataSet ads in activeDataSetsToBeRemoved)
            {
                objectTracker.DeactivateDataSet(ads);
                if (!ads.Path.Contains(datasetName))
                {
                    dataSetsToBeActive.Add(ads);
                }
                else
                {
                    objectTracker.DestroyDataSet(ads, false);
                    var trackables = ads.GetTrackables();
                    foreach (var item in trackables)
                    {
                        TrackerManager.Instance.GetStateManager().DestroyTrackableBehavioursForTrackable(item, true);
                    }
                }
            }
            objectTracker.Stop();
            foreach (DataSet ds in dataSetsToBeActive)
            {
                objectTracker.ActivateDataSet(ds);
            }
            objectTracker.Start();
        }
    }
    namespace VuforiaImage
    {
        [Serializable]
        public class QCARConfig
        {
            [XmlElement("Tracking")]
            public ImageTargetList Tracking;
        }
        [Serializable]
        public class ImageTargetList
        {
            [XmlElement("ImageTarget")]
            public List<ImageTargetInfo> ImageTarget;
        }
        [Serializable]
        public class ImageTargetInfo
        {
            [XmlAttribute("name")]
            public string name;
            [XmlAttribute("size")]
            public string size;
        }
    }
    namespace VuforiaModel
    {
        [Serializable]
        public class QCARConfig
        {
            [XmlElement("Tracking")]
            public ModelTargetInfo Tracking;
            [XmlElement("Assembly")]
            public Assembly Assembly;
        }
        [Serializable]
        public class ModelTargetInfo
        {
            [XmlElement("ModelTarget")]
            public ModelTargetNameInfo ModelTarget;
        }
        [Serializable]
        public class ModelTargetNameInfo
        {
            [XmlAttribute("name")]
            public string name;
        }
        [Serializable]
        public class Assembly
        {
            [XmlAttribute("name")]
            public string name;
            [XmlAttribute("assemblyId")]
            public string assemblyId;
            [XmlElement("Part")]
            public Part Part;
            [XmlElement("EntryPoint")]
            public List<Part> EntryPoint;
        }
        [Serializable]
        public class EntryPoint
        {

        }
        [Serializable]
        public class Part
        {
            [XmlAttribute("name")]
            public string name;
            [XmlAttribute("translation")]
            public string translation;
            [XmlAttribute("rotation")]
            public string rotation;
        }
    }
}
#endif
