/*******************************************************************
* Copyright(c)
* 文件名称: iOSSystemInfo.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
#if UNITY_IPHONE
    using System.Runtime.InteropServices;
    public class iOSSystemInfo
    {
        [DllImport("__Internal")]
        public static extern void GetUUIDInKeychain();
        [DllImport("__Internal")]
        public static extern void DeleteKeyChain();
        [DllImport("__Internal")]
        public static extern void GetRegion();
        [DllImport("__Internal")]
        public static extern void GetIphoneName();

        [DllImport("__Internal")]
        public static extern void exitUnityAdapter();
    }
#endif
}
