namespace AFramework
{
#if AF_ARSDK_Vuforia
    using AFramework;
    using Vuforia;
    using UnityEngine;

    public class DynamicLoadVuforiaExample : MonoBehaviour
    {
        public GameObject ModelPre;
        private void Start()
        {
            VuforiaARController.Instance.RegisterVuforiaStartedCallback(LoadSet);
        }
        private void LoadSet()
        {
            if ( ModelPre != null && CameraDevice.Instance.IsActive() && TrackerManager.Instance != null)
            {
                //传绝对路径 : 只需传.xml所在的绝对路径即可
                //AFSDK_DataSetHandle.LoadDataSetFromPath(UnityPathTool.persistentDataPath + "/" + DatasetName + ".xml");
                //此示例采用相对路径 : 直接导入dataset,直接传dataset名称即可
                AFSDK_DataSetHandle.LoadDataSet("VuforiaMars_Images");
                //注意命名空间
                AFSDK_DataSetHandle.ConfigTrackable("Astronaut", "AFramework.ARSDK.AFSDK_DefaultTrackableHandle", ModelPre);
            }
        }
    }
#endif
}
