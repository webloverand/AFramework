/*******************************************************************
* Copyright(c)
* 文件名称: BLEManager.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System;
    using UnityEngine;
    [MonoSingletonPath("[AFramework]/BLEReceiver")]
    public class BLEManager : MonoSingletonWithNewObject<BLEManager>
    {
        #region 蓝牙参数
        public bool isSupportBLE = false;
        public bool isHasBluePermission = false;
        /// <summary>
        /// 系统是否开启蓝牙,调用BlueInit函数此参数会被赋值
        /// </summary>
        public bool SysBluetoothState = false;
        /// <summary>
        /// 是否连接到蓝牙
        /// </summary>
        public bool isConnectedBlue = false;
        //连接的蓝牙信息
        public BLEDeviceInfo ConnectedBlueInfo;

        //是否连接到音频蓝牙
        public bool isConnectA2DPBlue;
        //连接到的音频蓝牙信息
        public BLEDeviceInfo ConnectedA2DPBlueInfo;

        #endregion
        BLEMsgHandler bLEMsgHandler;

#if UNITY_ANDROID
    AndroidJavaObject jo;
#endif
        /// <summary>
        /// 蓝牙初始化,请求开启蓝牙
        /// </summary>
        public void BlueInit(Action BlueInitFinishEvent, bool isListenA2DP = false,
            Action<bool, BLEDeviceInfo> A2DPListener = null, Action<bool> SupportBLECallBack = null,
            Action<bool> SystemBlueState = null, Action<bool> BluePermissionCallBack = null)
        {
            //挂载接收脚本
            bLEMsgHandler = gameObject.GetComponent<BLEMsgHandler>();
            if (bLEMsgHandler == null)
                bLEMsgHandler = gameObject.AddComponent<BLEMsgHandler>();
            this.A2DPListener = A2DPListener;
            this.SystemBlueState = SystemBlueState;
            bLEMsgHandler.SupportBLECallBack = SupportBLECallBack;
            bLEMsgHandler.SystemBlueState = SystemBlueStateCallBack;
            bLEMsgHandler.BluePermissionCallBack = BluePermissionCallBack;
#if UNITY_ANDROID
        if (jo == null)
        {
            AndroidJavaClass javaClass = new AndroidJavaClass("com.qianxi.afnative.BLEForUnity");
            jo = javaClass.CallStatic<AndroidJavaObject>("GetInstance");
        }
        bLEMsgHandler.RegisterBLEInitFinish(BlueInitFinishEvent);
        if (jo != null)
            jo.Call("BlueInit", isListenA2DP);
#elif UNITY_IPHONE
            iOSBluetoothLE.BlueInit(isListenA2DP);
#endif
        }
        /// <summary>
        /// 开启音频蓝牙的监听
        /// </summary>
        public void RegitsterA2DPListen()
        {
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("RegitsterA2DPListen");
#elif UNITY_IPHONE
                iOSBluetoothLE.RegitsterA2DPListen();
#endif
            }
        }
        /// <summary>
		/// 关闭音频蓝牙的监听
		/// </summary>
		public void UnRegitsterA2DPListen()
        {
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("UnRegitsterA2DPListen");
#elif UNITY_IPHONE
                iOSBluetoothLE.UnRegitsterA2DPListen();
#endif
            }
        }
        /// <summary>
        /// 如果不需要监听只需判断当前音频蓝牙的连接,可使用此函数
        /// </summary>
        public void JudgeConAudioBlueA2DP()
        {
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("JudgeConAudioBlueA2DP");
#elif UNITY_IPHONE
                iOSBluetoothLE.GetAudioName();
#endif
            }
        }
        //扫描全部的设备
        public void StartScan(Action<BLEDeviceInfo> ScanEvent = null, bool clearPeripheralList = true)
        {
            StartScan(null, ScanEvent, clearPeripheralList);
        }
        public void StartScan(string[] serviceUUIDs = null, Action<BLEDeviceInfo> ScanEvent = null,
            bool clearPeripheralList = true)
        {
            string serviceUUIDsString = "";
            if (serviceUUIDs != null)
            {
                serviceUUIDsString = serviceUUIDs.Length > 0 ? "" : null;
                foreach (string serviceUUID in serviceUUIDs)
                    serviceUUIDsString += serviceUUID + "|";
            }
            this.ScanEvent = ScanEvent;
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("StartScan", serviceUUIDsString,clearPeripheralList);
#elif UNITY_IPHONE
                iOSBluetoothLE.ScanDevice(serviceUUIDsString, clearPeripheralList,true);
#endif
            }
        }
        public void StopScan()
        {
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("StopScan");
#elif UNITY_IPHONE
                iOSBluetoothLE.stopScanDevice();
#endif
            }
        }
        public void ConnectBLE(BLEDeviceInfo bLEScanDeviceInfo, Action<bool> ConnectListener = null,
            Action<string> ServiceUUIDs = null, Action<CharacteristicInfo> CharacteristicUUIDs = null)
        {
            ConnectedBlueInfo = bLEScanDeviceInfo;
            this.ConnectListener = ConnectListener;
            if (bLEMsgHandler != null)
            {
                bLEMsgHandler.DiscoveyServiceUUID = ServiceUUIDs;
                bLEMsgHandler.CharacteristicUUID = CharacteristicUUIDs;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                Debug.Log("unity请求连接蓝牙:" + (jo != null));
            if (jo != null)
                jo.Call("ConnectBLEByName", bLEScanDeviceInfo.DeviceName);
#elif UNITY_IPHONE
                iOSBluetoothLE.connectToPeripheral(bLEScanDeviceInfo.MacAddress);
#endif
            }
        }
        public void ReadCharacteristic(CharacteristicInfo characteristicInfo, Action<bool, BLEDeviceInfo, string> ReadEvent)
        {
            ReadCharacteristic(characteristicInfo.bLEDeviceInfo.MacAddress, characteristicInfo.ServiceUUID, characteristicInfo.UUID, ReadEvent);
        }
        public void ReadCharacteristic(string MACAddress, string ServiceUUID, string CharacteristicUUID, Action<bool, BLEDeviceInfo, string> ReadEvent)
        {
            if (bLEMsgHandler != null)
            {
                bLEMsgHandler.ReadEvent = ReadEvent;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("ReadCharacteristic", MACAddress, ServiceUUID, CharacteristicUUID);
#elif UNITY_IPHONE
                iOSBluetoothLE.readCharacteristic( ServiceUUID, CharacteristicUUID);
#endif
            }
        }
        public void NotifyCharacteristic(CharacteristicInfo characteristicInfo, Action<bool, string> notifyEvent,
            Action<BLEDeviceInfo, string, string> notifyDataEvent)
        {
            NotifyCharacteristic(characteristicInfo.bLEDeviceInfo.MacAddress, characteristicInfo.ServiceUUID,
                characteristicInfo.UUID, notifyEvent, notifyDataEvent);
        }
        public void NotifyCharacteristic(string MACAddress, string ServiceUUID, string CharacteristicUUID,
            Action<bool, string> notifyEvent, Action<BLEDeviceInfo, string, string> notifyDataEvent)
        {
            if (bLEMsgHandler != null)
            {
                bLEMsgHandler.NotifyEvent = notifyEvent;
                bLEMsgHandler.NotifyDataEvent = notifyDataEvent;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("SubscribeCharacteristic", MACAddress, ServiceUUID, CharacteristicUUID);
#elif UNITY_IPHONE
                iOSBluetoothLE.subscribeCharacteristic( ServiceUUID, CharacteristicUUID);
#endif
            }
        }
        public void UnNotifyCharacteristic(CharacteristicInfo characteristicInfo, Action<bool, string> notifyEvent)
        {
            UnNotifyCharacteristic(characteristicInfo.bLEDeviceInfo.MacAddress, characteristicInfo.ServiceUUID,
                characteristicInfo.UUID, notifyEvent);
        }
        public void UnNotifyCharacteristic(string MACAddress, string ServiceUUID, string CharacteristicUUID,
           Action<bool, string> notifyEvent)
        {
            if (bLEMsgHandler != null)
            {
                bLEMsgHandler.UnNotifyEvent = notifyEvent;
                bLEMsgHandler.NotifyEvent = null;
                bLEMsgHandler.NotifyDataEvent = null;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("UnSubscribeCharacteristic", MACAddress, ServiceUUID, CharacteristicUUID);
#elif UNITY_IPHONE
                iOSBluetoothLE.unsubscribeCharacteristic( ServiceUUID, CharacteristicUUID);
#endif
            }
        }
        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="characteristicInfo"></param>
        /// <param name="writeEvent"></param>
        /// <param name="deltaTime">一次只能发送20字节,超20字节会分次发送,deltaTime为分次发送间的间隔,一般是20-40毫秒</param>
        public void WriteCharacteristic(CharacteristicInfo characteristicInfo, string SendData, Action<bool, string, string> writeEvent, int deltaTime = 30)
        {
            WriteCharacteristic(characteristicInfo.bLEDeviceInfo.MacAddress, characteristicInfo.ServiceUUID,
                characteristicInfo.UUID, SendData, writeEvent, deltaTime);
        }
        public void WriteCharacteristic(string MACAddress, string ServiceUUID, string CharacteristicUUID, string SendData,
         Action<bool, string, string> writeEvent, int deltaTime = 30)
        {
            if (bLEMsgHandler != null)
            {
                bLEMsgHandler.WriteEvent = writeEvent;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("WriteCharacteristic", MACAddress, ServiceUUID, CharacteristicUUID, SendData, deltaTime);
#elif UNITY_IPHONE
                iOSBluetoothLE.writeCharacteristic( ServiceUUID, CharacteristicUUID, SendData, deltaTime/1000f);
#endif
            }
        }
        public void DisConnect(Action disconnectCallBack = null)
        {
            if (bLEMsgHandler != null)
            {
                DisconnectCallBack = disconnectCallBack;
            }
            if (isConnectedBlue)
            {
                if (!Application.isEditor)
                {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("DisConnect");
#elif UNITY_IPHONE
                    iOSBluetoothLE.disconnetPeripheral();
#endif
                }
            }
        }
        public void DisConnectByName(string BLEName, Action disconnectCallBack = null)
        {
            if (bLEMsgHandler != null)
            {
                DisconnectCallBack = disconnectCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("DisConnectByName", BLEName);
#elif UNITY_IPHONE
                iOSBluetoothLE.disconnectPeripheralByName(BLEName);
#endif
            }
            isRecordInit = false;
        }
        public void BlueDeInit()
        {
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("BlueDeInit");
#elif UNITY_IPHONE
                iOSBluetoothLE.exitBLEForUnity();
#endif
            }
        }
        public void CheckRecordPermission(Action<bool> RecordPermissionCallBack)
        {
            if (bLEMsgHandler != null)
            {
                bLEMsgHandler.RecordPermissionCallBack = RecordPermissionCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                if (jo != null)
                    jo.Call("CheckRecordPermission");
#elif UNITY_IPHONE
                iOSBluetoothLE.CheckRecordPermission();
#endif
            }
        }
        bool isRecordInit = false;
        public void SetRecordParameter(String startRecordCommand, String stopRecordCommand,
            String serviceUUID, String writeCharacterUUID)
        {
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                if (jo != null)
                    jo.Call("SetRecordParameter", startRecordCommand, stopRecordCommand, serviceUUID, writeCharacterUUID);
#elif UNITY_IPHONE
                iOSBluetoothLE.SetRecordParameter(startRecordCommand, stopRecordCommand, serviceUUID, writeCharacterUUID);
#endif
            }
            isRecordInit = true;
        }
        public void StartRecord(Action<bool, string> RealRecordCallBack, int startCommandDelay = 500)
        {
            if (!isRecordInit)
            {
                RealRecordCallBack(false, "录音没有初始化");
            }
            if (bLEMsgHandler != null)
            {
                bLEMsgHandler.RealStartRecordCallBack = RealRecordCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                if (jo != null)
                    jo.Call("StartRecord", startCommandDelay);
#elif UNITY_IPHONE
                iOSBluetoothLE.StartRecord();
#endif
            }
        }
        public void StopRecord(Action<bool> RealFinishRecordCallBack, int stopCommandDelay = 2000)
        {
            if (bLEMsgHandler != null)
            {
                bLEMsgHandler.RealFinishRecordCallBack = RealFinishRecordCallBack;
            }
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                if (jo != null)
                    jo.Call("StopRecord", stopCommandDelay);
#elif UNITY_IPHONE
                iOSBluetoothLE.stopRecord();
#endif
            }
        }
        public void stopRecordForce()
        {
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                if (jo != null)
                    jo.Call("stopRecordForce");
#elif UNITY_IPHONE
                iOSBluetoothLE.stopRecordForce();
#endif
            }
        }

        Action<BLEDeviceInfo> ScanEvent = null;
        public void AddScanDevice(string DeviceName, string MacAddress, string Rssi)
        {
            ScanEvent?.Invoke(new BLEDeviceInfo(DeviceName, MacAddress, Rssi));
        }

        Action<bool, BLEDeviceInfo> A2DPListener = null;
        public void A2DPListen(bool isConnect, BLEDeviceInfo A2DPInfo)
        {
            isConnectA2DPBlue = isConnect;
            ConnectedA2DPBlueInfo = A2DPInfo;
            A2DPListener?.Invoke(isConnect, A2DPInfo);
        }

        Action<bool> ConnectListener = null;
        Action DisconnectCallBack = null;
        public void BlueConnectStateListener(bool state, BLEDeviceInfo bLEScanDeviceInfo)
        {
            if (state)
            {
                if (bLEScanDeviceInfo.DeviceName.Equals(ConnectedBlueInfo.DeviceName) &&
                    bLEScanDeviceInfo.MacAddress.Equals(ConnectedBlueInfo.MacAddress))
                {
                    isConnectedBlue = state;
                    ConnectListener?.Invoke(isConnectedBlue);
                    ConnectListener = null;
                }
            }
            else
            {

                isConnectedBlue = false;
                ConnectedBlueInfo = null;
                ConnectListener?.Invoke(isConnectedBlue);
                DisconnectCallBack?.Invoke();
                DisconnectCallBack = null;
            }
        }
        public void GetNewBlueState()
        {
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("GetNewBlueState");
#endif
            }
        }
        public void OnApplicationPause(bool focus)
        {
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
            if (jo != null)
                jo.Call("ApplicationPause", focus,false);
#elif UNITY_IPHONE
                iOSBluetoothLE.ApplicationPause(focus,false);
#endif
            }
        }
        //系统蓝牙回调
        Action<bool> SystemBlueState;
        void SystemBlueStateCallBack(bool isEnabled)
        {
            SysBluetoothState = isEnabled;
            SystemBlueState?.Invoke(isEnabled);
            if (!isEnabled)
            {
                isRecordInit = false;
            }
        }

        private void OnApplicationQuit()
        {
            BlueDeInit();
        }
    }
}
