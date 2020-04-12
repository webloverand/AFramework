
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: CharacterInfo.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
	
using AFramework;
using UnityEngine;
using UnityEngine.UI;
	
	public class DeviceInfoCharacter : MonoBehaviour
	{
	    CharacteristicInfo characteristicInfo;
	    BLEExample bLEExample;
	    
	    public void Init(CharacteristicInfo characteristicInfo,BLEExample bLEExample)
	    {
	        this.characteristicInfo = characteristicInfo;
	        this.bLEExample = bLEExample;
	        transform.GetChild(0).GetComponent<Text>().text = characteristicInfo.ServiceUUID;
	        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = characteristicInfo.UUID;
	        transform.GetChild(3).GetComponent<Button>().onClick.AddListener(()=>
	        {
	            this.bLEExample.DeviceInfoToCharacterInfo(characteristicInfo);
	        });
	    }
	}
}
