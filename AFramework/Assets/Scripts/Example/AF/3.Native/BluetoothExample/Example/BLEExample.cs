
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: BLEExample.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
	
using System.Collections.Generic;
using AFramework;
using UnityEngine;
using UnityEngine.UI;
	
	public class BLEExample : MonoBehaviour
	{
	    public GameObject DevicePre;
	    public GameObject ScanPage;
	    public GameObject DeviceInfoPage;
	    public GameObject CharacterInfoPage;
	    public GameObject RecordPage;
	    public Tip TipPage;
	    public Transform DeviceParent;
	    Dictionary<string, BLEScanDevice> scanDeviceDic = new Dictionary<string, BLEScanDevice>();
	
	    private void Awake()
	    {
	        BLEManager.Instance.BlueInit(BlueInitFinish, true, A2DPListen, SupportBLECallBack, SystemBlueStateCallBack, BluePermissionCallBack);
	        NativeInterface.Instance.GetMaxVolumn(MaxVolumnCallBack);
	        NativeInterface.Instance.GetCurrentVolumn(CurrentVolumnCallBack);
	        NativeInterface.Instance.RegisterVolumnChaged(CurrentVolumnCallBack);
	    }
	    public void SupportBLECallBack(bool isSupport)
	    {
	        if (!isSupport)
	        {
	            TipPage.ShowTip("设备不支持低功耗蓝牙", TipType.NoBtn);
	        }
	    }
	    public void BluePermissionCallBack(bool isHas)
	    {
	        if (!isHas)
	        {
	            TipPage.ShowTip("设备没有蓝牙权限,点击前往设置", TipType.OneBnt, ToAPPSetting);
	        }
	    }
	    public void ToAPPSetting()
	    {
	        NativeInterface.Instance.gotoPermissionSettings();
	    }
	    public int maxVolumn;
	    public int currentVolumn;
	    public void MaxVolumnCallBack(string result)
	    {
	        maxVolumn = int.Parse(result);
	    }
	    public void CurrentVolumnCallBack(string result)
	    {
	        currentVolumn = int.Parse(result);
	    }
	
	    public Text A2DPText;
	    public void A2DPListen(bool isConnected, BLEDeviceInfo bLEDeviceInfo)
	    {
	        if (!isConnected)
	        {
	            A2DPText.text = "  A2DP(连接的音频蓝牙) : 未连接";
	        }
	        else
	        {
	            A2DPText.text = "  A2DP(连接的音频蓝牙) : " + bLEDeviceInfo.DeviceName + " Address:" + bLEDeviceInfo.MacAddress;
	        }
	    }
	    public void BlueInitFinish()
	    {
	        Debug.Log("蓝牙模块初始化完成!");
	    }
	    public void SystemBlueStateCallBack(bool isEnabled)
	    {
	        Debug.Log("获取系统状态回调:" + isEnabled);
	        if (!isEnabled)
	        {
	            ScanPage.SetActive(true);
	            DeviceInfoPage.SetActive(false);
	            CharacterInfoPage.SetActive(false);
	            TipPage.gameObject.SetActive(false);
	            for (int i = DeviceParent.childCount - 1; i >= 0; i--)
	            {
	                DestroyImmediate(DeviceParent.GetChild(i).gameObject);
	            }
	            scanDeviceDic.Clear();
	        }
	    }
	    public void StartScan()
	    {
	        //BLEManager.Instance.StartScan(new string[1] { "6666" }, ScanDeviceEvent);
	        BLEManager.Instance.StartScan(null, ScanDeviceEvent);
	    }
	    public void ScanDeviceEvent(BLEDeviceInfo bLEScanDeviceInfo)
	    {
	        if (scanDeviceDic.ContainsKey(bLEScanDeviceInfo.MacAddress))
	        {
	            scanDeviceDic[bLEScanDeviceInfo.MacAddress].UpdateInfo(bLEScanDeviceInfo);
	        }
	        else
	        {
	            BLEScanDevice bLEScanDevice = Instantiate(DevicePre, DeviceParent).AddComponent<BLEScanDevice>();
	            bLEScanDevice.Init(bLEScanDeviceInfo, this);
	            scanDeviceDic.Add(bLEScanDeviceInfo.MacAddress, bLEScanDevice);
	        }
	    }
	    public void StopScan()
	    {
	        for (int i = DeviceParent.childCount - 1; i >= 0; i--)
	        {
	            DestroyImmediate(DeviceParent.GetChild(i).gameObject);
	        }
	        scanDeviceDic.Clear();
	        BLEManager.Instance.StopScan();
	    }
	    public void ShowConnectTip()
	    {
	        StopScan();
	        TipPage.ShowTip("正在连接,请稍候...", TipType.NoBtn);
	    }
	    public void ConnectListern(bool isConnect)
	    {
	        if (isConnect)
	        {
	            //连接成功,跳转页面
	            ScanPage.SetActive(false);
	            TipPage.gameObject.SetActive(false);
	            DeviceInfoPage.SetActive(true);
	            deviceInfo = DeviceInfoPage.GetComponent<DeviceInfo>();
	        }
	        else
	        {
	            //连接失败
	            TipPage.ShowTip("连接失败,请重试!", TipType.OneBnt);
	            StartScan();
	        }
	    }
	
	    //页面跳转
	    public void DeviceInfoBackScan()
	    {
	        DeviceInfoPage.SetActive(false);
	        BLEManager.Instance.DisConnect();
	        StartScan();
	        ScanPage.SetActive(true);
	    }
	    public void DeviceInfoToCharacterInfo(CharacteristicInfo characteristicInfo)
	    {
	        CharacterInfoPage.SetActive(true);
	        DeviceInfoPage.SetActive(false);
	        CharacterInfoPage.GetComponent<CharacterInfo>().Init(characteristicInfo);
	    }
	    public void CharacterInfoToDeviceInfo()
	    {
	        CharacterInfoPage.SetActive(false);
	        DeviceInfoPage.SetActive(true);
	    }
	    public void DeviceInfoToRecord(CharacteristicInfo characteristicInfo)
	    {
	        DeviceInfoPage.SetActive(false);
	        RecordPage.SetActive(true);
	        RecordPage.GetComponent<BlueRecord>().Init(characteristicInfo, this);
	    }
	    public void RecordToDeviceInfo()
	    {
	        DeviceInfoPage.SetActive(true);
	        RecordPage.SetActive(false);
	    }
	
	    DeviceInfo deviceInfo = null;
	    public void DiscoveryServices(string serviceUUID)
	    {
	        deviceInfo.OnDiscoveryService(serviceUUID);
	    }
	    public void DiscoveryCharacteristic(CharacteristicInfo characterInfo)
	    {
	        deviceInfo.OnDiscoveryCharacteristic(characterInfo);
	    }
	    private void OnApplicationPause(bool pause)
	    {
	        BLEManager.Instance.OnApplicationPause(pause);
	    }
	    private void OnApplicationQuit()
	    {
	        NativeInterface.Instance.UnRegisterVolumnChaged();
	    }
	}
}
