package com.qianxi.afnative;

import android.app.Activity;
import android.app.DownloadManager;
import android.content.Context;
import android.media.AudioManager;
import android.os.Build;
import android.os.Handler;

import com.qianxi.afnative.Function.AlbumOperate;
import com.qianxi.afnative.Function.AudioManagerOperate;
import com.qianxi.afnative.Function.DynamicPermission;
import com.qianxi.afnative.Function.PermissionCallBack;
import com.qianxi.afnative.Function.SystemInfo;
import com.unity3d.player.UnityPlayer;
/*
接口脚本
 */
public class AFNativeInterface {

    //全局使用的参数
    public Context mContext;
    public Activity mCurrentActivity;
    public Handler mainHandler;   //用来进入主程序流程

    public static AFNativeInterface Instance;

    public SystemInfo systemInfo;
    public AlbumOperate albumOperate;
    public DynamicPermission dynamicPermission;
    public AudioManagerOperate audioManagerOperate;

    private AFNativeInterface()
    {
        mCurrentActivity =  UnityPlayer.currentActivity;
        mContext  = mCurrentActivity.getApplicationContext();
        mainHandler = new Handler(mContext.getMainLooper());

        systemInfo = new SystemInfo();
        albumOperate = new AlbumOperate();
        dynamicPermission = new DynamicPermission();
        audioManagerOperate = new AudioManagerOperate(mContext);
    }
    public static AFNativeInterface GetInstance() {
        if (Instance == null) {
            Instance = new AFNativeInterface();
        }
        return Instance;
    }

    public static void SendToUnityMsg(String message)
    {
        UnityPlayer.UnitySendMessage("AFReceiver", "MsgDispose", message);
    }


    //获取产品/硬件的制造商
    public void GetManufacturer() {
        SendToUnityMsg("ManufacturerNative~"+Build.MANUFACTURER);
    }
    //获取手机设置里的语言和地区设置
    public void GetDeviceRegion ()
    {
        SendToUnityMsg("RegionNative~"+ systemInfo.getCountry());
    }

    //打开相册
    public void OpenAlbum()
    {
        albumOperate.OpenAlbum(mainHandler,mCurrentActivity);
    }
    //刷新相册
    public void ScanAlbum(String filePath){albumOperate.ScanAlbum(mCurrentActivity,filePath);}
    //获取唯一UUID
    public void GetUUID(){SendToUnityMsg("GetUUID~"+systemInfo.getUniversalID(mContext));}

    //获取截图所需权限
    public void GetCapturePermission()
    {
        dynamicPermission.GetCapturePermission( mCurrentActivity, new PermissionCallBack() {
            @Override
            public void GetPermissionFinish(String result) {
                SendToUnityMsg("CheckCapturePermission~" + result);
            }
        });
    }
    //录制视频保存到相册的权限
    public void GetRecordVedioPermission()
    {
        dynamicPermission.GetRecordVedioPermission( mCurrentActivity, new PermissionCallBack() {
            @Override
            public void GetPermissionFinish(String result) {
                SendToUnityMsg("CheckRecordVedioPermission~" + result);
            }
        });
    }
    //获取相册权限
    public void GetAlbumPermission()
    {
        dynamicPermission.GetAlbumPermission( mCurrentActivity, new PermissionCallBack() {
            @Override
            public void GetPermissionFinish(String result) {
                SendToUnityMsg("CheckPhotoPermission~" + result);
            }
        });
    }
    //获取存储权限
    public void GetStoragePermission()
    {
        dynamicPermission.GetStoragePermission( mCurrentActivity, new PermissionCallBack() {
            @Override
            public void GetPermissionFinish(String result) {
                SendToUnityMsg("CheckStoragePermission~" + result);
            }
        });
    }
    public void GetAudioPermission()
    {
        dynamicPermission.GetAudioPermission( mCurrentActivity, new PermissionCallBack() {
            @Override
            public void GetPermissionFinish(String result) {
                SendToUnityMsg("CheckAudioPermission~" + result);
            }
        });
    }
    public void StartMusic(){
        audioManagerOperate.StartMusic();
    }
    public void StopMusic(){
        audioManagerOperate.StopMusic();
    }
    //获取系统最大音量
    public void GetMaxVolumn()
    {
        audioManagerOperate.GetMaxVolumn();
    }
    //获取音量
    public void GetCurrentVolumn()
    {
        audioManagerOperate.GetCurrentVolumn();
    }
    //设置音量
    public void SetVolumn(String vol)
    {
        audioManagerOperate.SetVolumn(vol);
    }
    //注册音量更改监听
    public void RegisterVolumnChaged()
    {
        isRegisterVolumn = true;
        audioManagerOperate.RegisterVolumnChaged();
    }
    //注意注册与取消注册需要成对实现
    public void UnRegisterVolumnChaged()
    {
        isRegisterVolumn = false;
        audioManagerOperate.UnRegisterVolumnChaged();
    }
    //保证音量一致性,以免程序后台更改音量后Unity未接收到反馈
    boolean isRegisterVolumn = false;
    public void ApplicationPause(boolean pause)
    {
        if(pause)
        {
            //程序进入后台

        }
        else
        {
            if(isRegisterVolumn)
            {
                GetCurrentVolumn();
            }
        }
    }
    public void exitAFNativeInterface()
    {
        mCurrentActivity =  null;
        mContext  = null;
        mainHandler =  null;

        systemInfo =  null;
        albumOperate =  null;
        dynamicPermission =  null;
        audioManagerOperate =  null;
    }
    public void gotoPermissionSettings()
    {
        dynamicPermission.gotoPermissionSettings(mCurrentActivity);
    }
}
