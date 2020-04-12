
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: DynamicLoadNetExample.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
#if AF_ARSDK_Vuforia
using UnityEngine;
using AFramework;
using UnityEngine.UI;
using Vuforia;
using System.Collections.Generic;
	
	public class DynamicLoadNetExample : MonoBehaviour
	{
	    ResLoader resLoader;
	    private void Awake()
	    {
	        AFStart.RegisterStart(OnStart);
	    }
	    private void OnStart()
	    {
	        ABDataHolder.Instance.RegisterABInit(ABStart);
	        ABDataHolder.Instance.CheckAPPVersion();
	    }
	    private void ABStart(APPVersionStatus versionStatus)
	    {
	        resLoader = new ResLoader();
	        if (versionStatus != APPVersionStatus.Abandon)
	        {
	            //这里为了保证PersistentDataPathAB时云端资源下载完成
	            switch (ABDataHolder.Instance.GetABResLoadFrom())
	            {
	                case ABResLoadFrom.EditorRes:
	                    ABStatus.text = "AB包准备完成";
	                    LoadDataSet();
	                    break;
	                case ABResLoadFrom.PersistentDataPathAB:
	                    ABHotUpdate.Instance.ABMD5Request(Key, ABProcessevent, ABMD5Callback);
	                    break;
	                case ABResLoadFrom.StreamingAssetAB:
	                    ABStatus.text = "AB包准备完成";
	                    LoadDataSet();
	                    break;
	            }
	        }
	        else
	        {
	            AFLogger.e("版本检测显示是废弃APP,请检查设置");
	        }
	    }
	
	    public void ABMD5Callback(ABClassDownInfo ABDowninfo, DownStatus downResult = DownStatus.Sucess, string downError = "")
	    {
	        switch (downResult)
	        {
	            case DownStatus.Downloding:
	
	                break;
	            case DownStatus.Fail:
	                AFLogger.e("下载MD5文件失败,Error:" + downError);
	                break;
	            case DownStatus.NoNetwork:
	                AFLogger.e("没有网络,请检查!");
	                break;
	            case DownStatus.Sucess:
	                break;
	                
	        }
	    }
	    void ABProcessevent(double process, bool isFinish, DownStatus downResult = DownStatus.Sucess, string downError = "")
	    {
	        //AB包下载完成,解锁按键加载资源
	        if (isFinish)
	        {
	            ABStatus.text = "AB包准备完成";
	            LoadDataSet();
	        }
	    }
	    bool isARReady = false; //是否已经加载dataset
	    bool isDataSetReady = false;
	    public Text ABStatus;
	    OneABClassInfo oneABClassInfo;
	    string Key = "RecogTest";
	    public void LoadDataSet()
	    {
	        Debug.Log("OneABClassInfo路径:" + ABHotUpdate.Instance.GetSavePathByClass(Key, false));
	        oneABClassInfo = FileHelper.ReadJsonTxtToObject<OneABClassInfo>(ABHotUpdate.Instance.GetSavePathByClass(Key, false));
	        isDataSetReady = true;
	        Debug.Log(oneABClassInfo.dataSetInfos);
	    }
	    private void Update()
	    {
	        if (!isARReady && isDataSetReady && CameraDevice.Instance.IsActive() && TrackerManager.Instance != null)
	        {
	            isARReady = true;
	            foreach(OneDataSetInfo oneDataSetInfo in oneABClassInfo.dataSetInfos)
	            {
	                AFSDK_DataSetHandle.LoadDataSetFromPath(PathTool.PersistentDataPath + "/" + oneDataSetInfo.TargetDataSet + ".xml");
	                List<string> targetInfo = new List<string>(oneDataSetInfo.TargetInfo.Keys);
	                for (int i = 0; i < targetInfo.Count; i++)
	                {
	                    if(oneDataSetInfo.TargetInfo[targetInfo[i]].oneResInfo.ResPath != "")
	                    {
	                        GameObject objPrefabs = resLoader.LoadSync<GameObject>(ResFromType.ABRes, oneDataSetInfo.TargetInfo[targetInfo[i]].oneResInfo.ResPath);
	                        Transform trans = AFSDK_DataSetHandle.ConfigTrackable(targetInfo[i], modelPre: objPrefabs); //生成模型
	                        if (oneDataSetInfo.TargetInfo[targetInfo[i]].oneResInfo.isUseConfig)
	                        {
	                            trans.localEulerAngles = oneDataSetInfo.TargetInfo[targetInfo[i]].oneResInfo.ResRotation;
	                            trans.localScale = oneDataSetInfo.TargetInfo[targetInfo[i]].oneResInfo.ResScale;
	                            trans.localPosition = oneDataSetInfo.TargetInfo[targetInfo[i]].oneResInfo.ResPostion;
	                        }
	                    }
	                }
	            }
	        }
	    }
	}
#endif
}
