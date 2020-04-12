/*******************************************************************
* Copyright(c)
* 文件名称: iOSBluetoothLE.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
#if UNITY_IPHONE
    using System.Runtime.InteropServices;
    public class iOSBluetoothLE 
    {
        [DllImport("__Internal")]
        public static extern void BlueInit(bool islistenA2DP);
        [DllImport("__Internal")]
        public static extern void RegitsterA2DPListen();
        [DllImport("__Internal")]
        public static extern void UnRegitsterA2DPListen();
        [DllImport("__Internal")]
        public static extern void GetAudioName();
        [DllImport("__Internal")]
        public static extern void ScanDevice(string serviceUUIDsStringRaw, bool clearPeripheralList, bool allowDuplicates);
        [DllImport("__Internal")]
        public static extern void stopScanDevice();
        [DllImport("__Internal")]
        public static extern void connectToPeripheral(string macAddress);
        [DllImport("__Internal")]
        public static extern void readCharacteristic(string serviceString, string characteristicString);
        [DllImport("__Internal")]
        public static extern void writeCharacteristic(string serviceString, string characteristicString, string DataStr, float sendInternalTime);
        [DllImport("__Internal")]
        public static extern void subscribeCharacteristic( string serviceString, string characteristicString);

        [DllImport("__Internal")]
        public static extern void unsubscribeCharacteristic(string serviceString, string characteristicString);
        [DllImport("__Internal")]
        public static extern void disconnetPeripheral();
        [DllImport("__Internal")]
        public static extern void disconnectPeripheralByMacAddress(string macAddress);
        [DllImport("__Internal")]
        public static extern void disconnectPeripheralByName(string deviceName);
        [DllImport("__Internal")]
        public static extern void ApplicationPause(bool pause, bool isControllScan);
        [DllImport("__Internal")]
        public static extern void exitBLEForUnity();

        //录音相关
        [DllImport("__Internal")]
        public static extern void CheckRecordPermission();
        [DllImport("__Internal")]
        public static extern void SetRecordParameter(string startRecordCommand, string stopRecordCommand,
              string serviceUUID, string writeCharacterUUID);
        [DllImport("__Internal")]
        public static extern void StartRecord();
        [DllImport("__Internal")]
        public static extern void stopRecord();
        [DllImport("__Internal")]
        public static extern void stopRecordForce();
    }
#endif
}
