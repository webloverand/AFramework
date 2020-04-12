package com.qianxi.afnative.Function;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.media.AudioManager;

import com.qianxi.afnative.AFNativeInterface;

public class AudioManagerOperate {
    AudioManager mAudioManager;
    Context mContext;

    //如果是APP修改的音量就不需要回调
    boolean isAppChangeVolumn = false;
    public AudioManagerOperate(Context mContext)
    {
        this.mContext = mContext;
    }
    AudioManager GetAudioManager()
    {
        if(mAudioManager == null)
            mAudioManager = (AudioManager) mContext.getSystemService(Context.AUDIO_SERVICE);
        return mAudioManager;
    }
    public void StartMusic(){
        GetAudioManager().abandonAudioFocus(null);
    }
    public void StopMusic(){
        if(GetAudioManager().isMusicActive()){
            GetAudioManager().requestAudioFocus(null, AudioManager.STREAM_MUSIC, AudioManager.AUDIOFOCUS_GAIN_TRANSIENT);
        }
    }
    //获取系统最大音量
    public void GetMaxVolumn()
    {
        AFNativeInterface.SendToUnityMsg("GetSysMaxVolume~" + GetAudioManager().getStreamMaxVolume(AudioManager.STREAM_MUSIC));
    }
    //获取音量
    public void GetCurrentVolumn()
    {
        AFNativeInterface.SendToUnityMsg("GetCurrentVolumn~" + GetAudioManager().getStreamVolume(AudioManager.STREAM_MUSIC));
    }
    //设置音量
    public void SetVolumn(String vol)
    {
        isAppChangeVolumn = true;
        GetAudioManager().setStreamVolume(AudioManager.STREAM_MUSIC,Integer.parseInt(vol),AudioManager.FLAG_SHOW_UI);
    }
    //注册音量更改监听
    public void RegisterVolumnChaged()
    {
        IntentFilter filter = new IntentFilter("android.media.VOLUME_CHANGED_ACTION");
        mContext.registerReceiver(mVolumeBroadcastReceiver, filter);
    }
    //注意注册与取消注册需要成对实现
    public void UnRegisterVolumnChaged()
    {
        mContext.unregisterReceiver(mVolumeBroadcastReceiver);
    }
    private BroadcastReceiver mVolumeBroadcastReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            switch (intent.getAction()) {
                case "android.media.VOLUME_CHANGED_ACTION":
                    if(!isAppChangeVolumn)
                    {
                        GetCurrentVolumn();
                    }
                    else
                    {
                        isAppChangeVolumn = false;
                    }
                    break;
            }
        }
    };


}
