
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: DeviceInfo.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
	
using AFramework;
using UnityEngine;
using UnityEngine.UI;
	
	public class DeviceInfo : MonoBehaviour
	{
	    public Text ConectNameText;
	    public Text ConnectStateText;
	    public Text MacAddressText;
	    public Text DeviceNameText;
	
	    public GameObject CharacterPrefab;
	    public Transform CharacterParent;
	    public BLEExample bLEExample;
	
	    public Text ConnectDeviceName;
	    CharacteristicInfo writeCharacterInfo = null;
	    public void Back()
	    {
	        for (int i = CharacterParent.childCount - 1; i >= 0; i--)
	        {
	            DestroyImmediate(CharacterParent.GetChild(i).gameObject);
	        }
	        writeCharacterInfo = null;
	        bLEExample.DeviceInfoBackScan();
	    }
	    public void Disconnect()
	    {
	        BLEManager.Instance.DisConnect();
	    }
	    public void OnEnable()
	    {
	        if(BLEManager.Instance.isConnectedBlue)
	        {
	            ConectNameText.text = BLEManager.Instance.ConnectedBlueInfo.DeviceName;
	            ConnectStateText.text = "Connected";
	            MacAddressText.text = BLEManager.Instance.ConnectedBlueInfo.MacAddress;
	            DeviceNameText.text = BLEManager.Instance.ConnectedBlueInfo.DeviceName;
	            ConnectDeviceName.text = BLEManager.Instance.ConnectedBlueInfo.DeviceName;
	        }
	        else
	        {
	            ConnectStateText.text = "Disconnect";
	            MacAddressText.text = "-";
	            DeviceNameText.text = "-";
	        }
	    }
	    public void OnDiscoveryService(string serviceUUID)
	    {
	        Debug.Log("搜索到service:" + serviceUUID);
	    }
	    public void OnDiscoveryCharacteristic(CharacteristicInfo characterInfo)
	    {
	        Debug.Log("搜索到Character:" + characterInfo.UUID + " 所属Service:" + characterInfo.ServiceUUID +
	           " 是否可读:"+ characterInfo .CanRead + " 是否可写:"+ characterInfo.CanWrite + " 是否可Notify:" + characterInfo.CanNotify);
	        Instantiate(CharacterPrefab, CharacterParent).AddComponent< DeviceInfoCharacter>().Init(characterInfo, bLEExample);
	        if(characterInfo.CanWrite)
	        {
	            writeCharacterInfo = characterInfo;
	        }
	    }
	    public void ToRecordPage()
	    {
	        if(writeCharacterInfo != null)
	        {
	            bLEExample.DeviceInfoToRecord(writeCharacterInfo);
	        }
	        else
	        {
	            bLEExample.TipPage.ShowTip("writeCharacterInfo为空", TipType.OneBnt);
	        }
	    }
	}
}
