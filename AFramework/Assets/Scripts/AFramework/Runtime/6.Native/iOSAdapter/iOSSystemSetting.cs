/*******************************************************************
* Copyright(c)
* 文件名称: iOSSystemSetting.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
#if UNITY_IPHONE
    using System.Runtime.InteropServices;
    public class iOSSystemSetting 
    {
        [DllImport("__Internal")]
        public static extern void setVolume(float v);
        [DllImport("__Internal")]
        public static extern void GetCurrentVolume();
        [DllImport("__Internal")]
        public static extern void RegisterVolumeChangeListener();
        [DllImport("__Internal")]
        public static extern void UnRegisterVolumeChangeListener();
        [DllImport("__Internal")]
        public static extern void stopOtherAudio();
        [DllImport("__Internal")]
        public static extern void resumeOtherAudio();

        [DllImport("__Internal")]
        public static extern void iOSToAPPSetting();
    }
#endif
}
