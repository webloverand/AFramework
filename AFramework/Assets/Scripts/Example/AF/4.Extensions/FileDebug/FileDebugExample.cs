
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: FileDebugExample.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using System;
using System.Collections;
using AFramework;
using UnityEngine;
using UnityEngine.UI;
	
	public class FileDebugExample : MonoBehaviour
	{
	    public Text showDebugText; 
	    private void Start()
	    {
	        GetComponent<UnityFileDebug>().FileDebugInit("CSVTest", FileType.CSV);
	    }
	    float internalTime = 1;
	    float currentTime = 0;
	    private void Update()
	    {
	        currentTime += Time.deltaTime;
	        if(currentTime >= internalTime)
	        {
	            showDebugText.text += "\n输出时间:" + DateTime.Now.ToString(); 
	        }
	    }
	}
}
