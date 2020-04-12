/*******************************************************************
* Copyright(c)
* 文件名称: ABHelper.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/

namespace AFramework
{
    using System.Collections.Generic;

    public class ABClassDownInfo : HttpInfo, IPoolable
    {
        //MD5检测回调
        public ABMD5CallBack ABMD5Callback; //默认统一回调
        public ABMD5CallBack ABMD5CallbackLocal; //单独回调
        //新旧MD5文件转换
        public OneABClassInfo oldClassInfo;
        //AB包检测的状态
        public ABState aBState;
        //进度回调
        public ABProcessEvent ABprocessevent;

        public OneABClassInfo newClassInfo;
        //需要下载的总共大小
        public long totalSize = 0;
        //当前已经下载的大小
        public long CurSize = 0;
        //是否下载完成
        public bool IsDownFinish;
        //需要下载的AB包列表
        public List<string> NeedDownList;
        public double allprocess;

        public void CheckMD5Call(OneABClassInfo newClassInfo, ABState aBState, List<string> NeedDownList)
        {
            this.newClassInfo = newClassInfo;
            this.aBState = aBState;
            this.NeedDownList = NeedDownList;
            CalculateSize();
        }

        public static ABClassDownInfo Allocate(string ResClass, string m_srcUrl, string m_savePath, bool isMD5File,
            ABMD5CallBack aBMD5CallBack, ABMD5CallBack aBMD5CallBackLocal, ABProcessEvent ABprocessevent,
            OneABClassInfo OldABClassinfo = null, bool isHasRecog = false)
        {
            ABClassDownInfo aBDownInfo = SafeObjectPool<ABClassDownInfo>.Instance.Allocate();
            aBDownInfo.ResClass = ResClass;
            aBDownInfo.oldClassInfo = OldABClassinfo;
            aBDownInfo.isHasRecog = isHasRecog;
            aBDownInfo.m_srcUrl = m_srcUrl;
            aBDownInfo.m_savePath = m_savePath;
            aBDownInfo.isMD5File = isMD5File;
            aBDownInfo.ABMD5Callback = aBMD5CallBack;
            aBDownInfo.ABMD5CallbackLocal = aBMD5CallBackLocal;
            aBDownInfo.ABprocessevent = ABprocessevent;
            return aBDownInfo;
        }

        public bool IsRecycled { get; set; }
        public void OnRecycled()
        {
            IsRecycled = true;
            ResClass = "";
            ABMD5Callback = null;
            ABprocessevent = null;
            oldClassInfo = null;
            newClassInfo = null;
            NeedDownList = null;
            IsDownFinish = false;
            CurSize = 0;
            totalSize = 0;
            allprocess = 0;
        }
        public void Recycle2Cache()
        {
            SafeObjectPool<ABClassDownInfo>.Instance.Recycle(this);
        }
        /// <summary>
        /// 计算需要下载的AB包的总大小
        /// </summary>
        public void CalculateSize()
        {
            for (int i = 0; i < NeedDownList.Count; i++)
            {
                totalSize += newClassInfo.ABName[NeedDownList[i]];
            }
        }
        public void CalculateProcess(string ResClass, string ABKey,
            double process, bool isFinish)
        {
            allprocess = (CurSize + process * newClassInfo.ABName[ABKey]) / totalSize;
            if (isFinish)
            {
                CurSize += newClassInfo.ABName[ABKey];
                if (oldClassInfo != null)
                {
                    oldClassInfo.ABName[ABKey] = newClassInfo.ABName[ABKey];
                }
                Debug.Log("CurSize : " + CurSize + " totalSize:" + totalSize);
                if (CurSize.Equals(totalSize))
                {
                    //覆盖本地info.txt
                    FileHelper.CreatFile(ABHotUpdate.Instance.GetSavePathByClass(ResClass, false),
                        System.Text.Encoding.UTF8.GetBytes(SerializeHelper.ToJson(newClassInfo)));
                    ABprocessevent.InvokeGracefully(1, true, DownStatus.Sucess, "");
                }
                else
                {
                    ABprocessevent.InvokeGracefully(allprocess, false, DownStatus.Downloding, "");
                }
            }
            else
            {
                ABprocessevent.InvokeGracefully(allprocess, isFinish, DownStatus.Sucess, "");
            }
        }
    }
    public class OneDownload
    {
        public string URL;
        public bool isMD5OrAB;
        public OneDownload(string URL, bool isMD5OrAB)
        {
            this.URL = URL;
            this.isMD5OrAB = isMD5OrAB;
        }
    }
}
