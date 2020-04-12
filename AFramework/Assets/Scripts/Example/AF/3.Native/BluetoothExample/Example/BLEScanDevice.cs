
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: BLEScanDevice.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using AFramework;
using UnityEngine;
using UnityEngine.UI;
	
	public class BLEScanDevice : MonoBehaviour
	{
	    BLEDeviceInfo BLEscaninfo;
	    Text DeviceNameText;
	    Text MACAddressText;
	    Text RssiText;
	    Image RssiFillImg;
	    GameObject rssiI;
	    BLEExample bLEExample;
	
	    public void Init(BLEDeviceInfo bLEScanDeviceInfo,BLEExample bLEExample)
	    {
	        DeviceNameText = transform.GetChild(0).GetComponent<Text>();
	        MACAddressText = transform.GetChild(1).GetComponent<Text>();
	        RssiText = transform.GetChild(5).GetComponent<Text>();
	        RssiFillImg = transform.GetChild(3).GetComponent<Image>();
	        transform.GetChild(6).GetComponent<Button>().onClick.AddListener(ConnectDevice);
	        rssiI = transform.GetChild(4).gameObject;
	        UpdateInfo(bLEScanDeviceInfo);
	        this.bLEExample = bLEExample;
	    }
	    public void UpdateInfo(BLEDeviceInfo bLEScanDeviceInfo)
	    {
	        DeviceNameText.text = bLEScanDeviceInfo.DeviceName;
	        MACAddressText.text = bLEScanDeviceInfo.MacAddress;
	        RssiText.text = bLEScanDeviceInfo.Rssi;
	        SetRssiFill(bLEScanDeviceInfo.Rssi);
	        BLEscaninfo = bLEScanDeviceInfo;
	    }
	    public void ConnectDevice()
	    {
	        bLEExample.ShowConnectTip();
	        BLEManager.Instance.ConnectBLE(BLEscaninfo,bLEExample.ConnectListern, bLEExample.DiscoveryServices, bLEExample.DiscoveryCharacteristic);
	    }
	
	    public void SetRssiFill(string rssi)
	    {
	        int rssiInt = int.Parse(rssi);
	        if (rssiInt >= -40)
	        {
	            rssiI.SetActive(false);
	            RssiFillImg.fillAmount = 1;
	        }
	        else if (rssiInt >= -50 && rssiInt < -40)
	        {
	            rssiI.SetActive(false);
	            RssiFillImg.fillAmount = 0.79f;
	        }
	        else if(rssiInt >= -60 && rssiInt <-50)
	        {
	            rssiI.SetActive(false);
	            RssiFillImg.fillAmount = 0.59f;
	        }
	        else if (rssiInt >= -70 && rssiInt < -60)
	        {
	            rssiI.SetActive(false);
	            RssiFillImg.fillAmount = 0.39f;
	        }
	        else if (rssiInt >= -80 && rssiInt < -70)
	        {
	            rssiI.SetActive(false);
	            RssiFillImg.fillAmount = 0.19f;
	        }
	        else if (rssiInt >= -90 && rssiInt < -80)
	        {
	            rssiI.SetActive(false);
	            RssiFillImg.fillAmount = 0f;
	        }
	        else
	        {
	            rssiI.SetActive(true);
	        }
	    }
	}
}
