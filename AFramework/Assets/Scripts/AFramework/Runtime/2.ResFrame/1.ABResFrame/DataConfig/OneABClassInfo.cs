using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AFramework
{
    //一个AB包对应的配置类
    [System.Serializable]
    public class OneABClassInfo
    {
        public bool isNeedPackageAB;
        public ARRecogType RecogType;
        public List<OneDataSetInfo> dataSetInfos = new List<OneDataSetInfo>();
        public List<OneResInfo> ResInfoForPlane = new List<OneResInfo>();
        //识别资源以及AB包的MD5
        public Dictionary<string, string> FileMD5 = new Dictionary<string, string>();
        //AB包的名称(唯一标识符) ---- 对应的大小
        public Dictionary<string, long> ABName = new Dictionary<string, long>();
    }
    [System.Serializable]
    public class OneDataSetInfo
    {
        //加载的dataset名称
        public string TargetDataSet;
        //识别名称及对应的信息
        public Dictionary<string, OneTargetInfo> TargetInfo = new Dictionary<string, OneTargetInfo>();
    }
    [System.Serializable]
    public struct OneTargetInfo
    {
        public string ImageTargetName;
        public OneResInfo oneResInfo;
    }
    [System.Serializable]
    public struct OneResInfo
    {
        [Tooltip("识别完成加载的资源名称:模型,视频等预制体,相对于Assets文件夹")]
        public string ResPath;
        [Tooltip("是否使用下面配置的位置/旋转/缩放,默认为false(使用默认值)")]
        public bool isUseConfig;
        public Vector3 ResPostion;
        public Vector3 ResRotation;
        public Vector3 ResScale;
    }
}

