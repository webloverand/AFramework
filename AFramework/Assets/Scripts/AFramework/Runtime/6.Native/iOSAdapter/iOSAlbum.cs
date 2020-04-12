/*******************************************************************
* Copyright(c)
* 文件名称: iOSAlbum.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/

namespace AFramework
{
#if UNITY_IPHONE
    using System.Runtime.InteropServices;
    public class iOSAlbum
	{
          [DllImport("__Internal")]
          public static extern void iosOpenAlbum();

         [DllImport("__Internal")]
        public static extern void iosSaveImageToAlbum(string sourcePathP);
        [DllImport("__Internal")]
        public static extern void iosSaveVideoToAlbum(string sourcePathP);

        [DllImport("__Internal")]
        public static extern void iosGetPhotoPermission(string msgPrefix);
        [DllImport("__Internal")]
        public static extern void iosGetRecordPermission();
        [DllImport("__Internal")]
        public static extern void iosGetVideoRecordPermission();
    }
#endif
}
