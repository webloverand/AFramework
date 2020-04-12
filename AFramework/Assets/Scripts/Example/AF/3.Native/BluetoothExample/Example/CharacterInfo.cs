
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: CharacterInfo.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using System;
using System.Collections.Generic;
using AFramework;
using UnityEngine;
using UnityEngine.UI;
	
	public class CharacterInfo : MonoBehaviour
	{
	    public GameObject ValuePrefab;
	    public Transform ValueParent;
	    public Text DeviceName;
	    public Text ServiceUUID;
	    public Text CharacterUUID;
	    public Text OperationText;
	
	    public GameObject NotifyBtn;
	    public Image NotifyImg;
	    public Text NotifyText;
	
	    public InputField writeInput;
	    public GameObject ReadBtn;
	
	    public RectTransform ReadTrans;
	    public GameObject WriteObj;
	
	    public GameObject readDefault;
	    public GameObject writeDefault;
	
	    CharacteristicInfo characteristicInfo;
	
	    List<GameObject> writeObj = new List<GameObject>();
	    List<GameObject> readObj = new List<GameObject>();
	    bool isNotify = true;
	    public BLEExample bLEExample;
	    public VerticalLayoutGroup verticalLayout;
	    public ContentSizeFitter contentSizeFitter;
	    public void Init(CharacteristicInfo characteristicInfo)
	    {
	        this.characteristicInfo = characteristicInfo;
	        ServiceUUID.text = characteristicInfo.ServiceUUID;
	        CharacterUUID.text = characteristicInfo.UUID;
	        string operationStr = "";
	        DeviceName.text = characteristicInfo.bLEDeviceInfo.DeviceName;
	
	        if (characteristicInfo.CanRead)
	        {
	            operationStr += "Read";
	            ReadBtn.SetActive(true);
	        }
	        else
	        {
	            ReadBtn.SetActive(false);
	        }
	        if (characteristicInfo.CanWrite)
	        {
	            if (operationStr != "")
	            {
	                operationStr += ",";
	            }
	            operationStr += "Write";
	            WriteObj.SetActive(true);
	            writeDefault.SetActive(true);
	            if (characteristicInfo.CanNotify || characteristicInfo.CanRead)
	            {
	                ReadTrans.gameObject.SetActive(true);
	                readDefault.SetActive(true);
	                //设置位置,有写位置则要下移
	                ReadTrans.anchoredPosition = new Vector2(8.5f, -620);
	            }
	            else
	            {
	                ReadTrans.gameObject.SetActive(false);
	                readDefault.SetActive(false);
	            }
	        }
	        else
	        {
	            WriteObj.SetActive(false);
	            writeDefault.SetActive(false);
	            if (characteristicInfo.CanNotify || characteristicInfo.CanRead)
	            {
	                ReadTrans.gameObject.SetActive(true);
	                readDefault.SetActive(true);
	                //设置位置,没有写位置则要上移
	                ReadTrans.anchoredPosition = new Vector2(8.5f, -370);
	            }
	            else
	            {
	                ReadTrans.gameObject.SetActive(false);
	                readDefault.SetActive(false);
	            }
	        }
	        if (characteristicInfo.CanNotify)
	        {
	            if (operationStr != "")
	            {
	                operationStr += ",";
	            }
	            operationStr += "Notify";
	            //默认订阅,底层会防止多次订阅
	            BLEManager.Instance.NotifyCharacteristic(characteristicInfo, NotifyInitResult, NotifyInfoCallback);
	        }
	        else
	        {
	            NotifyBtn.SetActive(false);
	        }
	        OperationText.text = operationStr;
	    }
	    bool isInit = false;
	    public void NotifyInitResult(bool notifyResult, string errorStr)
	    {
	        isInit = true;
	        NotifyBtn.GetComponent<Toggle>().isOn = notifyResult;
	        NotifyResult(notifyResult, errorStr);
	        isInit = false;
	    }
	    public void NotifyResult(bool notifyResult, string errorStr)
	    {
	        NotifyBtn.SetActive(true);
	        isNotify = notifyResult;
	        if (notifyResult)
	        {
	            NotifyImg.color = new Color32(0, 166, 255, 255);
	            NotifyText.text = "UnSubscribe";
	        }
	        else
	        {
	            NotifyImg.color = new Color32(147, 147, 147, 255);
	            NotifyText.text = "Subscribe";
	        }
	    }
	    public void UnNotifyResult(bool notifyResult, string errorStr)
	    {
	        NotifyBtn.SetActive(true);
	        isNotify = notifyResult;
	        if (notifyResult)
	        {
	            NotifyImg.color = new Color32(147, 147, 147, 255);
	            NotifyText.text = "Subscribe";
	        }
	        else
	        {
	            NotifyImg.color = new Color32(0, 166, 255, 255);
	            NotifyText.text = "UnSubscribe";
	        }
	    }
	    public void NotifyOnClick(bool isOn)
	    {
	        if (!isInit)
	        {
	            if (isOn)
	            {
	                BLEManager.Instance.NotifyCharacteristic(characteristicInfo, NotifyResult, NotifyInfoCallback);
	            }
	            else
	            {
	                BLEManager.Instance.UnNotifyCharacteristic(characteristicInfo, UnNotifyResult);
	            }
	        }
	    }
	    public void NotifyInfoCallback(BLEDeviceInfo bLEDeviceInfo, string updateCharacteristic, string info)
	    {
	        readDefault.SetActive(false);
	        GameObject obj = Instantiate(ValuePrefab, ValueParent) as GameObject;
	        obj.GetComponent<Text>().text = DateTime.UtcNow.ToString() + ":\n" + info;
	        readObj.Add(obj);
	        Invoke("LayoutUpdate", 0.1f);
	    }
	    public void writeBtnOnClick()
	    {
	        if (writeInput.text == null || writeInput.text == "")
	        {
	            bLEExample.TipPage.ShowTip("Input cannot be empty!", TipType.OneBnt);
	            return;
	        }
	        if (writeInput.text.Length % 2 == 1)
	        {
	            bLEExample.TipPage.ShowTip("The number of inputs cannot be odd!", TipType.OneBnt);
	            return;
	        }
	        String regex = "^[A-Fa-f0-9]+$";
	        if (!System.Text.RegularExpressions.Regex.IsMatch(writeInput.text, regex))
	        {
	            bLEExample.TipPage.ShowTip("The string must be hexadecimal!", TipType.OneBnt);
	            return;
	        }
	        //发送
	        BLEManager.Instance.WriteCharacteristic(characteristicInfo, writeInput.text, writeCallBack);
	    }
	    public void writeCallBack(bool isSuccess, string error, string sendMsg)
	    {
	        GameObject obj = Instantiate(ValuePrefab, ValueParent) as GameObject;
	        if (isSuccess)
	        {
	            writeDefault.SetActive(false);
	            obj.GetComponent<Text>().text = DateTime.UtcNow.ToString() + ":Success\n" + sendMsg;
	        }
	        else
	        {
	            obj.GetComponent<Text>().text = DateTime.UtcNow.ToString() + ":Failure\n" + sendMsg;
	        }
	        obj.AddComponent<writeCharacter>().Init(characteristicInfo, sendMsg);
	        obj.transform.SetSiblingIndex(WriteObj.transform.GetSiblingIndex() + 1);
	        writeObj.Add(obj);
	        Invoke("LayoutUpdate", 0.1f);
	    }
	    public void readBtnOnClick()
	    {
	        BLEManager.Instance.ReadCharacteristic(characteristicInfo, ReadDataCallBack);
	    }
	    public void ReadDataCallBack(bool isSuccess, BLEDeviceInfo bLEDeviceInfo, string data)
	    {
	        if (isSuccess)
	        {
	            readDefault.SetActive(false);
	            GameObject obj = Instantiate(ValuePrefab, ValueParent) as GameObject;
	            obj.GetComponent<Text>().text = DateTime.UtcNow.ToString() + ":\n" + data;
	            obj.transform.SetSiblingIndex(ReadTrans.GetSiblingIndex() + 1);
	            readObj.Add(obj);
	            Invoke("LayoutUpdate", 0.1f);
	        }
	        else
	        {
	            bLEExample.TipPage.ShowTip("读取Characteristic错误:" + data, TipType.OneBnt);
	        }
	    }
	    public void LayoutUpdate()
	    {
	        verticalLayout.padding.left = 45;
	        verticalLayout.SetLayoutVertical();
	    }
	    public void Back()
	    {
	        for (int i = 0; i < writeObj.Count; i++)
	        {
	            DestroyImmediate(writeObj[i]);
	        }
	        for (int i = 0; i < readObj.Count; i++)
	        {
	            DestroyImmediate(readObj[i]);
	        }
	        writeObj.Clear();
	        readObj.Clear();
	        bLEExample.CharacterInfoToDeviceInfo();
	    }
	}
}
