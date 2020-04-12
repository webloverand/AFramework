
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: LoadTextExample.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using AFramework;
using UnityEngine;
using UnityEngine.UI;
	
	public class LoadTextExample : MonoBehaviour
	{
	    public Text showPersistLoadContent;
	    private void Start()
	    {
	        AFStart.RegisterStart(OnStart);
	    }
	    public void OnStart()
	    {
	        FileHelper.CreatFile(PathTool.PersistentDataPath + "version.txt",
	            System.Text.Encoding.UTF8.GetBytes("这是测试persistent读取数据的文本"),true);
	        showPersistLoadContent.text = ResManager.Instance.LoadStr(PathTool.PersistentDataPath + "version.txt");
	    }
	}
}
