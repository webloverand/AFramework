using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AFramework
{
    [CreateAssetMenu(fileName = "AppInfoConfig", menuName = "AFramework/CreatAppInfoConfig", order = 0)]
    public class AppInfoConfig : ScriptableObject
    {
        //云端维护的APP(AB包)版本,启动时判断APP是否还有支持的AB包
        public List<double> AvailableABVersions = new List<double>();
        //包含已经上线的APP版本以及正在上线的APP版本,用来判断是否是最新APP版本
        public List<double> NewABVersion;

       
        [InfoBox("AFLogger输出模式,请注意上线版本设置为NoDebug")]
        public DebugMode debugMode = DebugMode.NormalDebug;
    }

    public enum DebugMode
    {
        NoDebug,
        NormalDebug,
        CSVFileDebug,
        JsonFileDebug,
        TXTFileDebug,
        EditorDebug
    }
}
