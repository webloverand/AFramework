/*******************************************************************
* Copyright(c)
* 文件名称: BLEMsgHandle.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System;
    using UnityEngine;

    public class BLEMsgHandler : MonoBehaviour
    {
        public Action<string> DiscoveyServiceUUID = null;
        public Action<CharacteristicInfo> CharacteristicUUID = null;
        public Action<bool, BLEDeviceInfo, string> ReadEvent = null;
        public Action<bool, string, string> WriteEvent = null;

        public Action<bool, string> NotifyEvent = null;
        public Action<bool, string> UnNotifyEvent = null;
        public Action<BLEDeviceInfo, string, string> NotifyDataEvent = null;
        public Action<bool> SystemBlueState = null;
        public Action<bool> RecordPermissionCallBack = null;
        public Action<bool, string> RealStartRecordCallBack = null;
        public Action<bool> RealFinishRecordCallBack = null;
        public Action<bool> SupportBLECallBack = null;
        public Action<bool> BluePermissionCallBack = null;


        public void BLEMsgDispose(string msg)
        {
            if (msg.Equals(""))
            {
                return;
            }
            string[] msgSplite = msg.Split('~');
            switch (msgSplite[0])
            {
                case "SystemBlueState":
                    SystemBlueState?.Invoke(msgSplite[1].Equals("1"));
                    break;
                case "BLEInitFinished":
                    BLEInitFinish?.Invoke();
                    break;
                case "A2DPBlueInfo":
                    bool isConnect = msgSplite[1].Equals("1");
                    if (isConnect)
                    {
                        BLEManager.Instance.A2DPListen(isConnect, new BLEDeviceInfo(msgSplite[2], msgSplite[3], ""));
                    }
                    else
                    {
                        BLEManager.Instance.A2DPListen(isConnect, null);
                    }
                    break;
                case "ScanBlueResult":
                    BLEManager.Instance.AddScanDevice(msgSplite[1], msgSplite[2], msgSplite[3]);
                    break;
                case "BlueConnectState":
                    BLEManager.Instance.BlueConnectStateListener(msgSplite[1].Equals("1"),
                        new BLEDeviceInfo(msgSplite[2], msgSplite[3]));
                    break;
                case "DiscoveryService":
                    DiscoveyServiceUUID?.Invoke(msgSplite[1]);
                    break;
                case "DiscoveryCharacteristic":
                    CharacteristicUUID?.Invoke(new CharacteristicInfo(new BLEDeviceInfo(msgSplite[1], msgSplite[2]),
                        msgSplite[3], msgSplite[4], msgSplite[5], msgSplite[6], msgSplite[7]));
                    break;
                case "ReadCharacter":
                    bool isRead = msgSplite[1].Equals("1");
                    if (isRead)
                    {
                        ReadEvent?.Invoke(isRead, new BLEDeviceInfo(msgSplite[2], msgSplite[3]), msgSplite[4]);
                    }
                    else
                    {
                        ReadEvent?.Invoke(isRead, null, msgSplite[2]);
                    }
                    break;
                case "NotifyCharacter":
                    NotifyEvent?.Invoke(msgSplite[1].Equals("1"), msgSplite[2]);
                    break;
                case "NotifyCharacterData":
                    NotifyDataEvent?.Invoke(new BLEDeviceInfo(msgSplite[1], msgSplite[2]), msgSplite[3], msgSplite[4]);
                    break;
                case "UnNotifyCharacter":
                    UnNotifyEvent?.Invoke(msgSplite[1].Equals("1"), msgSplite[2]);
                    break;
                case "WriteCharacter":
                    bool isWrite = msgSplite[1].Equals("1");
                    if (isWrite)
                    {
                        WriteEvent?.Invoke(isWrite, "", msgSplite[2]);
                    }
                    else
                    {
                        //写入失败
                        WriteEvent?.Invoke(isWrite, msgSplite[2], msgSplite[3]);
                    }
                    break;
                case "CheckRecordPermission":
                    RecordPermissionCallBack?.Invoke(msgSplite[1].Equals("1"));
                    break;
                case "RealStartRecord":
                    RealStartRecordCallBack?.Invoke(msgSplite[1].Equals("1"), msgSplite[2]);
                    break;
                case "RealFinishRecord":
                    RealFinishRecordCallBack?.Invoke(msgSplite[1].Equals("1"));
                    break;
                case "SupportBLE":
                    bool isSupport = msgSplite[1].Equals("1");
                    BLEManager.Instance.isSupportBLE = isSupport;
                    SupportBLECallBack?.Invoke(isSupport);
                    break;
                case "BluetoothLEPermission":
                    bool isHas = msgSplite[1].Equals("1");
                    BLEManager.Instance.isSupportBLE = isHas;
                    BluePermissionCallBack?.Invoke(isHas);
                    break;
            }

        }
        #region 事件
        Action BLEInitFinish;
        public void RegisterBLEInitFinish(Action BLEInitFinish)
        {
            this.BLEInitFinish += BLEInitFinish;
        }
        #endregion
    }

    public class BLEDeviceInfo
    {
        public string DeviceName;
        public string MacAddress;
        public string Rssi;

        public BLEDeviceInfo(string DeviceName, string MacAddress, string Rssi)
        {
            this.DeviceName = DeviceName;
            this.MacAddress = MacAddress;
            this.Rssi = Rssi;
        }
        public BLEDeviceInfo(string DeviceName, string MacAddress)
        {
            this.DeviceName = DeviceName;
            this.MacAddress = MacAddress;
        }
    }
    public class CharacteristicInfo
    {
        public BLEDeviceInfo bLEDeviceInfo;
        public bool CanRead;
        public bool CanWrite;
        public bool CanNotify;

        public string ServiceUUID;
        public string UUID;

        public CharacteristicInfo(BLEDeviceInfo bLEDeviceInfo, string ServiceUUID, string uuid, string CanRead, string CanWrite, string CanNotify)
        {
            this.bLEDeviceInfo = bLEDeviceInfo;
            this.ServiceUUID = ServiceUUID;
            this.UUID = uuid;
            this.CanRead = CanRead.Equals("1");
            this.CanWrite = CanWrite.Equals("1");
            this.CanNotify = CanNotify.Equals("1");
        }

    }
}
