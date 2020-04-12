/*******************************************************************
* Copyright(c)
* 文件名称: AFSDK_VuforiaHelper.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
#if AF_ARSDK_Vuforia
    using UnityEngine;
    using Vuforia;
    public class AFSDK_VuforiaHelper
    {
        // 关闭相机
        public void CloseCameraDevice()
        {
            CameraDevice.Instance.Stop();
            CameraDevice.Instance.Deinit();
        }

        // 打开相机
        public void OpenCameraDevice()
        {
            CameraDevice.Instance.Init();
            CameraDevice.Instance.Start();
        }

        // 关闭识别
        public void CloseCloudReco()
        {
            CloudRecoBehaviour cloudRecoBehaviour = GameObject.FindObjectOfType(typeof(CloudRecoBehaviour)) as CloudRecoBehaviour;
            cloudRecoBehaviour.CloudRecoEnabled = false;
        }

        // 打开识别
        public void OpenCloudReco()
        {
            CloudRecoBehaviour cloudRecoBehaviour = GameObject.FindObjectOfType(typeof(CloudRecoBehaviour)) as CloudRecoBehaviour;
            cloudRecoBehaviour.CloudRecoEnabled = true;
        }
        public void OpenFlash()
        {
            CameraDevice.Instance.SetFlashTorchMode(true);
        }

        public void CloseFlash()
        {
            CameraDevice.Instance.SetFlashTorchMode(false);
        }
    }
#endif
}
