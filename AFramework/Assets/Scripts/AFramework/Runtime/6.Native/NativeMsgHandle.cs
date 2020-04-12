/*******************************************************************
* Copyright(c)
* 文件名称: NativeMsgHandle.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System;
    using UnityEngine;
	public class NativeMsgHandle : MonoBehaviour
	{
        public void MsgDispose(string msg)
        {
            string[] msgArray = msg.Split('~');
            switch(msgArray[0])
            {
                case "ManufacturerNative":
                    ManufacturerCallBack?.Invoke(msgArray[1]);
                    break;
                case "RegionNative":
                    RegionCallBack?.Invoke(msgArray[1]);
                    break;
                case "AlbumPicturePath":
                    //获取相册路径返回
                    AlbumPicturePathCallBack?.Invoke(msgArray[1]);
                    break;
                case "GetUUID":
                    UUIDCallBack?.Invoke(msgArray[1]);
                    break;
                case "CheckPhotoPermission":
                    AlbumPermissionCallBack?.Invoke(msgArray[1]);
                    break;
                case "CheckStoragePermission":
                    StoragePermissionCallBack?.Invoke(msgArray[1]);
                    break;
                case "GetSysMaxVolume": //获取系统最大音量回调,ios恒为1
                    SysMaxVolumeCallBack?.Invoke(msgArray[1]);
                    break;
                case "GetCurrentVolumn": //获取系统当前音量回调,当系统按键按下也会触发此回调,iOS回调是double还是???后续修改
                    CurrentVolumnCallBack?.Invoke(msgArray[1]);
                    break;
                case "CheckAudioPermission":
                    AudioPermissionCallBack?.Invoke(msgArray[1]);
                    break;
                case "SavePhotoOrVideo":
                    //如果保存成功则返回保存路径,否则是错误信息
                    SavePhotoOrVideoCallBack?.Invoke(msgArray[1].Equals("1"), msgArray[2]);
                    SavePhotoOrVideoCallBack = null;
                    break;
                case "CheckCapturePermission":
                    CapturePermissionCallBack?.Invoke(msgArray[1]);
                    break;
                case "CheckRecordVedioPermission":
                    VedioPermissionCallBack?.Invoke(msgArray[1]);
                    break;
            }
        }
        //请注意获取权限结果会返回0 1 2,0代表没有权限,1代表第一次获取权限,2代表之前就有权限
        //是因为第一次获取权限后执行的操作需要一点延时,比如获取相册权限后截图保存到相册就需要缓冲时间,否则截图无法保存到相册
        public Action<string> ManufacturerCallBack;
        public Action<string> RegionCallBack;
        public Action<string> AlbumPicturePathCallBack;
        public Action<string> AlbumPermissionCallBack;
        public Action<string> UUIDCallBack;
        public Action<string> StoragePermissionCallBack;
        public Action<string> SysMaxVolumeCallBack;
        public Action<string> CurrentVolumnCallBack;
        public Action<string> AudioPermissionCallBack;
        public Action<string> VedioPermissionCallBack;
        public Action<string> CapturePermissionCallBack;
        public Action<bool,string> SavePhotoOrVideoCallBack;
    }

}	
