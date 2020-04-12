
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: FileExample.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using AFramework;
using UnityEngine;
	
	public class FileExample : MonoBehaviour
	{
	    public string filePath;
	    public string dicPath;
	    private void Start()
	    {
	        Debug.Log("文件后缀:"+FileHelper.GetFileEx(filePath));
	        Debug.Log("文件名称(带后缀):" + FileHelper.GetFileNameWithEx(filePath));
	        Debug.Log("文件名称(不带后缀):" + FileHelper.GetFileNameWithOutEx(filePath));
	        Debug.Log("文件大小:" + FileHelper.GetFileSize(filePath));
	    }
	}
}
