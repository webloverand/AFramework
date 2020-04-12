
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: writeCharacter.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using AFramework;
using UnityEngine;
using UnityEngine.UI;
	
	public class writeCharacter : MonoBehaviour
	{
	    CharacteristicInfo characteristicInfo;
	    string sendMsg;
	    public void Init(CharacteristicInfo characteristicInfo,string sendMsg)
	    {
	        this.characteristicInfo = characteristicInfo;
	        this.sendMsg = sendMsg;
	        transform.GetComponent<Button>().onClick.AddListener(ReWrite);
	    }
	    public void ReWrite()
	    {
	
	    }
	}
}
