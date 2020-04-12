/*******************************************************************
* Copyright(c)
* 文件名称: NativeInterface.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/

namespace AFramework
{
    using System;
    using System.Collections;
    using System.IO;
    using NatCorder;
    using NatCorder.Clocks;
    using NatCorder.Inputs;
    using UnityEngine;
    /// <summary>
    /// 负责Android与iOS原生的交互
    /// </summary>
    [MonoSingletonPath("[AFramework]/AFReceiver")]
    public class NativeInterface : MonoSingletonWithNewObject<NativeInterface>
    {
        NativeMsgHandle nativeMsgHandle;
#if UNITY_ANDROID
        AndroidJavaObject jo;
#endif
        public override void OnSingletonInit()
        {
            base.OnSingletonInit();
            nativeMsgHandle = gameObject.AddComponent<NativeMsgHandle>();
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                AndroidJavaClass javaClass = new AndroidJavaClass("com.qianxi.afnative.AFNativeInterface");
                jo = javaClass.CallStatic<AndroidJavaObject>("GetInstance");
#endif
            }
        }
        #region Unity调用底层接口
        /// <summary>
        /// 判断设备机型(暂时只适用于Android)
        /// </summary>
        public void DeviceManufacturer(string Manufacturer = "", Action<bool> resultEvent = null, Action<string> getResultEvent = null)
        {
            ManufacturerStr = Manufacturer;
            GetManufacturerCallBack = getResultEvent;
            ManufacturerCallBack = resultEvent;
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.ManufacturerCallBack = JudgeManufacturer;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                jo.Call("GetManufacturer");
#elif UNITY_IPHONE
                iOSSystemInfo.GetIphoneName();
#endif
            }
        }
        /// <summary>
        /// 获取设备的语言和地区(是从手机设置里面读取的)
        /// </summary>
        public void GetDeviceRegion(Action<string> DeviceRegion)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.RegionCallBack = DeviceRegion;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                jo.Call("GetDeviceRegion");
#elif UNITY_IPHONE
                iOSSystemInfo.GetRegion();
#endif
            }
            else
            {
                DeviceRegion?.Invoke("CN");
            }
        }
        /// <summary>
        /// Android动态获取存储权限
        /// </summary>
        public void GetStoragePermission(Action<string> StoragePermissionCallBack)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.StoragePermissionCallBack = StoragePermissionCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                jo.Call("GetStoragePermission");
#elif UNITY_IPHONE
                StoragePermissionCallBack?.Invoke("2");
#endif
            }
        }
        public void GetUUID(Action<string> UUIDCallBack)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.UUIDCallBack = UUIDCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                jo.Call("GetUUID");
#elif UNITY_IPHONE
                iOSSystemInfo.GetUUIDInKeychain();
#endif
            }
        }
        /// <summary>
        /// 获取打开相册所需权限
        /// </summary>
        /// <param name="AlbumPermissionCallBack"></param>
        public void GetAlbumPermission(Action<string> AlbumPermissionCallBack)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.AlbumPermissionCallBack = AlbumPermissionCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                jo.Call("GetAlbumPermission");
#elif UNITY_IPHONE
                iOSAlbum.iosGetPhotoPermission("CheckPhotoPermission~");
#endif
            }
        }
        /// <summary>
        /// 打开相册
        /// </summary>
        /// <param name="AlbumPermissionCallBack"></param>
        public void OpenAlbum(Action<string> AlbumPermissionCallBack)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.AlbumPermissionCallBack = AlbumPermissionCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                jo.Call("OpenAlbum");
#elif UNITY_IPHONE
                iOSAlbum.iosOpenAlbum();
#endif
            }
        }
        public void StartMusic()
        {
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                jo.Call("StartMusic");
#elif UNITY_IPHONE
                iOSSystemSetting.resumeOtherAudio();
#endif
            }
        }
        public void StopMusic()
        {
#if UNITY_ANDROID
            jo.Call("StopMusic");
#elif UNITY_IPHONE
            iOSSystemSetting.stopOtherAudio();
#endif
        }
        //获取系统最大音量
        public void GetMaxVolumn(Action<string> MaxVolumnCallBack)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.SysMaxVolumeCallBack = MaxVolumnCallBack;
            }
#if UNITY_ANDROID
            jo.Call("GetMaxVolumn");
#elif UNITY_IPHONE
            MaxVolumnCallBack?.Invoke("1");
#endif
        }
        //获取音量
        public void GetCurrentVolumn(Action<string> CurrentVolumnCallBack)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.CurrentVolumnCallBack = CurrentVolumnCallBack;
            }
#if UNITY_ANDROID
            jo.Call("GetCurrentVolumn");
#elif UNITY_IPHONE
            iOSSystemSetting.GetCurrentVolume();
#endif
        }
        //设置音量
        public void SetVolumn(float vol)
        {
#if UNITY_ANDROID
			string v = vol.ToString();
            jo.Call("SetVolumn",v);
#elif UNITY_IPHONE
            iOSSystemSetting.setVolume(vol);
#endif
        }
        //
        public void RegisterVolumnChaged(Action<string> CurrentVolumnCallBack)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.CurrentVolumnCallBack = CurrentVolumnCallBack;
            }
#if UNITY_ANDROID
            jo.Call("RegisterVolumnChaged");
#elif UNITY_IPHONE
            iOSSystemSetting.RegisterVolumeChangeListener();
#endif
        }
        //取消注册与取消注册需要成对实现
        public void UnRegisterVolumnChaged()
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.CurrentVolumnCallBack = null;
            }
#if UNITY_ANDROID
            jo.Call("UnRegisterVolumnChaged");
#elif UNITY_IPHONE
            iOSSystemSetting.UnRegisterVolumeChangeListener();
#endif
        }
        /// <summary>
        /// Android动态获取录屏保存到相册权限(存储/相册/录音权限)
        /// </summary>
        public void GetVedioRecordPermission(Action<string> VedioRecordPermissionCallBack)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.VedioPermissionCallBack = VedioRecordPermissionCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                jo.Call("GetRecordVedioPermission");
#elif UNITY_IPHONE
                iOSAlbum.iosGetVideoRecordPermission();
#endif
            }
        }
        /// <summary>
        /// Android动态获取截图保存到相册权限(存储/相册权限)
        /// </summary>
        public void GetCapturePermission(Action<string> CapturePermissionCallBack)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.CapturePermissionCallBack = CapturePermissionCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                jo.Call("GetCapturePermission");
#elif UNITY_IPHONE
                iOSAlbum.iosGetPhotoPermission("CheckCapturePermission~");
#endif
            }
            else
            {
                CapturePermissionCallBack?.Invoke("1");
            }
        }
        /// <summary>
        /// Android动态获取录音权限
        /// </summary>
        public void GetAudioRecordPermission(Action<string> AudioRecordPermissionCallBack)
        {
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.AudioPermissionCallBack = AudioRecordPermissionCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                jo.Call("GetAudioPermission");
#elif UNITY_IPHONE
                iOSAlbum.iosGetRecordPermission();
#endif
            }
            else
            {
                AudioRecordPermissionCallBack?.Invoke("2");
            }
        }
        /// <summary>
        /// 指定相机截图
        /// </summary>
        public Texture2D CaptureScreen(Camera came, Rect r)
        {
            return CaptureScreen(new Camera[] { came }, r);
        }
        /// <summary>
        /// 多个相机截图
        /// </summary>
        public Texture2D CaptureScreen(Camera[] came, Rect r)
        {
            RenderTexture rt = new RenderTexture((int)r.width, (int)r.height, 10);
            foreach (Camera c in came)
            {
                c.targetTexture = rt;
                c.Render();
            }

            RenderTexture.active = rt;
            Texture2D screenShot = new Texture2D((int)r.width, (int)r.height, TextureFormat.RGB24, false);

            screenShot.ReadPixels(r, 0, 0);
            screenShot.Apply();
            foreach (Camera c in came)
            {
                c.targetTexture = null;
            }
            RenderTexture.active = null;
            Destroy(rt);

            return screenShot;
        }
        /// <summary>
        /// 默认保存到截图的目录
        /// </summary>
        public void SaveTextureToAlbum(string filename, Texture2D texture, Action<bool, string> SaveCallBack = null,
            TextureType textureType = TextureType.PNG)
        {
            byte[] data = null;
            switch (textureType)
            {
                case TextureType.EXR:
                    data = texture.EncodeToEXR();
                    break;
                case TextureType.JPG:
                    data = texture.EncodeToJPG();
                    break;
                case TextureType.PNG:
                    data = texture.EncodeToPNG();
                    break;
                case TextureType.TGA:
                    data = texture.EncodeToTGA();
                    break;
            }
            if (nativeMsgHandle != null)
            {
                nativeMsgHandle.SavePhotoOrVideoCallBack = SaveCallBack;
            }
            string destination = "";
            if (Application.isEditor)
            {
                destination = PathTool.PersistentDataPath + filename;
                File.WriteAllBytes(destination, data);
                SaveCallBack?.Invoke(true, destination);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                destination = "/mnt/sdcard/Pictures/Screenshots";
                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }
                destination = destination + "/" + filename;
                File.WriteAllBytes(destination, data);
#if UNITY_ANDROID
                // 安卓在这里需要去 调用原生的接口去 刷新一下，不然相册显示不出来
                jo.Call("ScanAlbum", destination);
#endif
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                destination = Application.persistentDataPath;
                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }
                destination = destination + "/" + filename;
                File.WriteAllBytes(destination, data);

#if UNITY_IPHONE
                iOSAlbum.iosSaveImageToAlbum(destination);
#endif
            }
            Destroy(texture);
        }
        public void SaveVedioToAlbum(string OldVedioPath, Action<bool, string> SaveCallBack = null)
        {
            nativeMsgHandle.SavePhotoOrVideoCallBack = SaveCallBack;
            if (!Application.isEditor)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    string destination = "/mnt/sdcard/Pictures/Screenshots";
                    if (!Directory.Exists(destination))
                    {
                        Directory.CreateDirectory(destination);
                    }
                    string filename = OldVedioPath.Split('/')[OldVedioPath.Split('/').Length - 1];
                    filename.Replace('_', 'a');
                    destination = destination + "/" + filename;
                    FileHelper.CopyFile(OldVedioPath, destination);
#if UNITY_ANDROID
                // 安卓在这里需要去 调用原生的接口去 刷新一下，不然相册显示不出来
                jo.Call("ScanAlbum", destination);
#endif
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
#if UNITY_IPHONE
                    iOSAlbum.iosSaveVideoToAlbum(OldVedioPath);
#endif
                }
            }
        }

        public void exitNative()
        {
            if (!Application.isEditor)
            {

#if UNITY_ANDROID
                jo.Call("exitAFNativeInterface");
#elif UNITY_IPHONE
                iOSSystemInfo.exitUnityAdapter();
#endif
            }
        }
        public void gotoPermissionSettings()
        {
            if (!Application.isEditor)
            {

#if UNITY_ANDROID
                jo.Call("gotoPermissionSettings");
#elif UNITY_IPHONE
                iOSSystemSetting.iOSToAPPSetting();
#endif
            }
        }
        #endregion

        //判断设备机型
        string ManufacturerStr;
        Action<String> GetManufacturerCallBack;
        Action<bool> ManufacturerCallBack;
        void JudgeManufacturer(string manufactureStr)
        {
            GetManufacturerCallBack?.Invoke(manufactureStr);
            ManufacturerCallBack?.Invoke(manufactureStr.Contains(ManufacturerStr));
            ManufacturerCallBack = null;
        }

        #region 录屏相关
        //录屏相关
        private bool recordMicrophone;
        private AudioSource microphoneSource;

        private IMediaRecorder videoRecorder;
        private IClock recordingClock;
        private CameraInput cameraInput;
        private AudioInput audioInput;
        bool isStartRecording = false;
        Action<string> RecordVedioCallBack = null;
        public void StartRecordVedio(Camera[] captureCamera, int videoWidth, int videoHeight, bool recordMicrophone = false, AudioSource microphoneSource = null,
            int recordTime = 10, Action<string> recordVedioCallBack = null)
        {
            this.recordMicrophone = recordMicrophone;
            this.microphoneSource = microphoneSource;
            RecordVedioCallBack = recordVedioCallBack;

            var sampleRate = recordMicrophone ? AudioSettings.outputSampleRate : 0;
            var channelCount = recordMicrophone ? (int)AudioSettings.speakerMode : 0;
            recordingClock = new RealtimeClock();
            videoRecorder = new MP4Recorder(
                videoWidth,
                videoHeight,
                30,
                sampleRate,
                channelCount,
                OnReplay
            );
            cameraInput = new CameraInput(videoRecorder, recordingClock, captureCamera);
            //录像
            StartMicrophone();
            audioInput = recordMicrophone ? new AudioInput(videoRecorder, recordingClock, microphoneSource, true) : null;
            if (audioInput != null)
                microphoneSource.mute = false;

            isStartRecording = true;
            Invoke("StopRecordVedio", recordTime);
        }
        public void StopRecordVedio()
        {
            CancelInvoke("StopRecordVedio");
            if (isStartRecording)
            {
                StopMicrophone();
                if (recordMicrophone)
                {
                    microphoneSource.mute = true;
                }
                audioInput?.Dispose();
                cameraInput.Dispose();
                videoRecorder.Dispose();
            }
        }
        private void StartMicrophone()
        {
            if (recordMicrophone)
            {
#if !UNITY_WEBGL || UNITY_EDITOR // No `Microphone` API on WebGL :(
                // Create a microphone clip
                microphoneSource.clip = Microphone.Start(null, true, 60, 48000);
                while (Microphone.GetPosition(null) <= 0) ;
                // Play through audio source
                microphoneSource.timeSamples = Microphone.GetPosition(null);
                microphoneSource.loop = true;
                microphoneSource.Play();
#endif
            }
        }
        private void StopMicrophone()
        {
            if (recordMicrophone)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                Microphone.End(null);
                microphoneSource.Stop();
#endif
            }
        }
        private void OnReplay(string path)
        {
            // 预览视频
            //#if UNITY_EDITOR
            //        EditorUtility.OpenWithDefaultApp(path);
            //#elif UNITY_IOS
            //            Handheld.PlayFullScreenMovie("file://" + path);
            //#elif UNITY_ANDROID
            //            Handheld.PlayFullScreenMovie(path);
            //#endif
            RecordVedioCallBack?.Invoke(path);
        }
        #endregion
    }
    public enum TextureType
    {
        JPG,
        PNG,
        TGA,
        EXR
    }
}
