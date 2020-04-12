using System.Collections.Generic;

namespace AFramework {	
	
	public class ABHotUpdate : Singleton<ABHotUpdate>
	{
	    public ABHotUpdate()
	    {
	        SafeObjectPool<ABClassDownInfo>.Instance.Init(20, 5);
	    }
	    //保存正在下载的请求
	    Dictionary<string, List<OneDownload>> DowningInfo = new Dictionary<string, List<OneDownload>>();
	    //记录检测MD5后的信息
	    Dictionary<string, ABClassDownInfo> mABDownInfo = new Dictionary<string, ABClassDownInfo>();
	
	    /// <summary>
	    /// 下载云端MD5文件与本地MD5对比
	    /// </summary>
	    /// <param name="ResClass"></param>
	    /// <param name="processEvent">如果不是AR的AB,那么久需要直接下载,这里是AB包下载进度回调</param>
	    /// <param name="aBMD5CallBack">MD5下载完成后的回调,会将结果传回去</param>
	    /// <param name="isHasRecog"></param>
	    public void ABMD5Request(string ResClass, ABProcessEvent processEvent, ABMD5CallBack aBMD5CallBack = null, bool isHasRecog = false)
	    {
	        OneABClassInfo oldClassInfo = SerializeHelper.FromJson<OneABClassInfo>(FileHelper.ReadTxtToStr(GetSavePathByClass(ResClass, false)));
	        AddDownload(ResClass, true, ABDataHolder.Instance.GetMD5DownURL() + ResClass + "Info.txt");
	        DownManager.Instance.StartDownABMD5(ABClassDownInfo.Allocate(ResClass,
	            ABDataHolder.Instance.GetMD5DownURL() + ResClass + "Info.txt",
	            GetSavePathByClass(ResClass, true), true, aBMD5CallBack, ABMD5ResultCallback, processEvent, oldClassInfo, isHasRecog));
	    }
	    public void UpdateProductAB(string ResClass, ABProcessEvent processEvent)
	    {
	        //因为有语言的切换,因此这里要重新赋值
	        mABDownInfo[ResClass].ABprocessevent = processEvent;
	        DownClassAB(ResClass);
	    }
	    /// <summary>
	    /// 取消下载
	    /// </summary>
	    /// <param name="productName"></param>
	    public void RemoveDownLoadByProduct(string productName)
	    {
	        if (DowningInfo.ContainsKey(productName))
	        {
	            for (int i = 0; i < DowningInfo[productName].Count; i++)
	            {
	                DownManager.Instance.RemoveRequestByUser(DowningInfo[productName][i].URL);
	                RemoveDownload(productName, DowningInfo[productName][i].isMD5OrAB, DowningInfo[productName][i].URL);
	            }
	            DowningInfo.Remove(productName);
	        }
	    }
	
	    /// <summary>
	    /// MD5检测回调
	    /// </summary>
	    public void ABMD5ResultCallback(ABClassDownInfo ABDowninfo, DownStatus downResult, string downError)
	    {
	        //从保存下载的字典里移除
	        RemoveDownload(ABDowninfo.ResClass, true, ABDataHolder.Instance.GetMD5DownURL() + ABDowninfo.ResClass.ToString() + "Info.txt");
	        AddProductDownInfo(ABDowninfo);
	        //回调MD5的结果,可根据结果进行处理
	        ABDowninfo.ABMD5Callback.InvokeGracefully(ABDowninfo, downResult, downError);
	        if (downResult == DownStatus.Sucess)
	        {
	            //Debug.Log("MD5检测结果:" + ABDowninfo.aBState);
	            //这里分成了产品AB包和普通AB包,如果产品AB包需要直接下载,可在ABMD5callback回调中自己实现
	            if (ABDowninfo.isHasRecog)
	            {
	                if (ABDowninfo.aBState != ABState.Newest)
	                {
	                    //覆盖本地info.txt
	                    FileHelper.CreatFile(GetSavePathByClass(ABDowninfo.ResClass, false),
	                        System.Text.Encoding.UTF8.GetBytes(SerializeHelper.ToJson(ABDowninfo.newClassInfo)), true);
	                }
	            }
	            else
	            {
	                //Debug.Log("不是识别AB包的下载");
	                if (ABDowninfo.aBState == ABState.Newest)
	                {
	                    //覆盖本地info.txt
	                    FileHelper.CreatFile(GetSavePathByClass(ABDowninfo.ResClass, false),
	                        System.Text.Encoding.UTF8.GetBytes(SerializeHelper.ToJson(ABDowninfo.newClassInfo)), true);
	                    //没有需要热更的
	                    mABDownInfo[ABDowninfo.ResClass].IsDownFinish = true; //下载完成
	                    ABDowninfo.ABprocessevent.InvokeGracefully(1, true, DownStatus.Sucess, "");
	                }
	                else
	                {
	                    //计算大小并下载
	                    DownClassAB(ABDowninfo.ResClass);
	                }
	            }
	        }
	    }
	    /// <summary>
	    /// 下载AB包
	    /// </summary>
	    public void DownClassAB(string ResClass)
	    {
	        //首先进行
	        DownManager.Instance.StartByteDown(ABDataHolder.Instance.GetABConfigDownPath(), ABInit,
	            ABDataHolder.Instance.GetABConfigSavePath(), null);
	        for (int i = 0; i < mABDownInfo[ResClass].NeedDownList.Count; i++)
	        {
	            FileHelper.DeleteFile(PathTool.PersistentDataPath + mABDownInfo[ResClass].NeedDownList[i]);
	        }
	        for (int i = 0; i < mABDownInfo[ResClass].NeedDownList.Count; i++)
	        {
	            if (DownManager.Instance.StartDownAB(HttpInfo.AllocateInfo(ResClass, mABDownInfo[ResClass].NeedDownList[i],
	                ABDataHolder.Instance.GetABDownURL() + mABDownInfo[ResClass].NeedDownList[i],
	                PathTool.PersistentDataPath + mABDownInfo[ResClass].NeedDownList[i],
	                ABDownCallBack, false, true)))
	            {
	                AddDownload(ResClass, false, ABDataHolder.Instance.GetABDownURL() + mABDownInfo[ResClass].NeedDownList[i]);
	            }
	            else
	            {
	                break;
	            }
	        }
	    }
	    public void ABInit(byte[] data, DownStatus downResult, string downError)
	    {
	        //资源相关类进行初始化
	        ABDataHolder.Instance.LoadAssetManifest();
	    }
	    //计算进度
	    public void ABDownCallBack(HttpInfo httpInfo, double process, bool isFinish,
	        DownStatus downResult = DownStatus.Sucess, string downError = "")
	    {
	        if (downResult == DownStatus.Fail || downResult == DownStatus.NoNetwork)
	        {
	            mABDownInfo[httpInfo.ResClass].ABprocessevent.InvokeGracefully(mABDownInfo[httpInfo.ResClass].allprocess, false, downResult, downError);
	            return;
	        }
	        if (isFinish)
	        {
	            RemoveDownload(httpInfo.ResClass, false, httpInfo.m_srcUrl);
	        }
	        mABDownInfo[httpInfo.ResClass].CalculateProcess(httpInfo.ResClass, httpInfo.ABName, process, isFinish);
	    }
	
	    /// <summary>
	    /// 获取MD5文本的暂时保存路径/正式保存路径
	    /// </summary>
	    /// <param name="ResClass"></param>
	    /// <param name="isTemp"></param>
	    /// <returns></returns>
	    public string GetSavePathByClass(string ResClass, bool isTemp)
	    {
	        if (isTemp)
	            return PathTool.PersistentDataPath + ResClass + "TempInfo.txt";
	        else
	            return PathTool.PersistentDataPath + ResClass + "Info.txt";
	    }
	    /// <summary>
	    /// 保存AB包MD5检测完成后的下载信息
	    /// </summary>
	    public void AddProductDownInfo(ABClassDownInfo aBDownInfo)
	    {
	
	        if (mABDownInfo.ContainsKey(aBDownInfo.ResClass))
	        {
	            mABDownInfo[aBDownInfo.ResClass] = aBDownInfo;
	        }
	        else
	        {
	            mABDownInfo.Add(aBDownInfo.ResClass, aBDownInfo);
	        }
	    }
	    /// <summary>
	    /// 将正在下载的保存,方便后面取消下载
	    /// </summary>
	    /// <param name="ResClass"></param>
	    /// <param name="isMD5"></param>
	    /// <param name="URL"></param>
	    public void AddDownload(string ResClass, bool isMD5, string URL)
	    {
	        OneDownload one = new OneDownload(URL, isMD5);
	        if (DowningInfo.ContainsKey(ResClass))
	        {
	            if (!DowningInfo[ResClass].Contains(one))
	                DowningInfo[ResClass].Add(one);
	        }
	        else
	        {
	            List<OneDownload> oneList = new List<OneDownload>();
	            oneList.Add(one);
	            DowningInfo.Add(ResClass, oneList);
	        }
	    }
	    /// <summary>
	    /// 下载完成后移除下载
	    /// </summary>
	    /// <param name="ResClass"></param>
	    /// <param name="isMD5"></param>
	    /// <param name="URL"></param>
	    public void RemoveDownload(string ResClass, bool isMD5, string URL)
	    {
	        OneDownload one = new OneDownload(URL, isMD5);
	        if (DowningInfo.ContainsKey(ResClass))
	        {
	            if (!DowningInfo[ResClass].Contains(one))
	            {
	                DowningInfo[ResClass].Remove(one);
	            }
	        }
	    }
	
	}
}
