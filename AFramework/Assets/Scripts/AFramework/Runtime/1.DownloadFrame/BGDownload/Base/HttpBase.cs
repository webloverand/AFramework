using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AFramework
{
    /// <summary>
    /// 请求回调委托
    /// </summary>
    /// <param name="text">内容或者路径</param>
    /// <param name="downResult">下载结果</param>
    /// <param name="error">如果DownResult为Fail时,这里会返回其错误信息</param>
    public delegate void WebRequestTextEvent(string text, DownStatus downResult = DownStatus.Sucess, string error = "");
    public delegate void WebRequestByteEvent(byte[] data, DownStatus downResult = DownStatus.Sucess, string downError = ""); //内容回调委托,返回内容或者path
    public delegate void WebRequestNetTextureEvent(Texture2D texture2D, string savePath,
       DownStatus downResult = DownStatus.Sucess, string downError = ""); //内容回调委托,返回内容或者path
    //进度回调委托
    public delegate void WebRequestProcessEvent(HttpInfo httpInfo, double process, bool isFinish = false,
       DownStatus downResult = DownStatus.Sucess, string downError = "");

    public enum DownStatus
    {
        Downloding,//正在下载
        Sucess, //下载成功
        Fail, //下载失败
        NoNetwork //没有网络
    }

    public class HttpInfo
    {
        //类别名,对应AF_ABConfig配置的m_AllClass
        public string ResClass;
        //下载路径
        public string m_srcUrl;
        //保存路径
        public string m_savePath;
        //AB包名称
        public string ABName;
        //是否是MD5文件
        public bool isMD5File = false;
        public WebRequestProcessEvent processEvent;
        //是否是产品AB(需要识别的归为产品类型)
        public bool isHasRecog;

        public static HttpInfo AllocateInfo(string ResClass, string ABName, string m_srcUrl, string m_savePath,
            WebRequestProcessEvent processEvent, bool isMD5File = false, bool isHasRecog = false)
        {
            HttpInfo httpInfo = new HttpInfo();
            httpInfo.ResClass = ResClass;
            httpInfo.ABName = ABName;
            httpInfo.m_srcUrl = m_srcUrl;
            httpInfo.m_savePath = m_savePath;
            httpInfo.isMD5File = isMD5File;
            httpInfo.isHasRecog = isHasRecog;
            httpInfo.processEvent = processEvent;
            return httpInfo;
        }
    }
    public class HttpBase : MonoBehaviour
    {
        public HttpBase(HttpInfo httpInfo)
        {
            this.httpInfo = httpInfo;
        }
        /// 下载文件全路径，路径+文件名+后缀
        protected string m_saveFilePath;
        /// 原文件大小
        private long m_fileLength;
        /// 当前下载好了的大小
        private long m_currentLength;

        /// 是否开始下载
        public bool m_isStartDownload;
        private bool isStartDownload
        {
            get
            {
                return m_isStartDownload;
            }
        }
        //httpInfo对象,此处保存是为了传送进度
        public HttpInfo httpInfo;
        //上一帧的进度
        private float LastPro;
        //上一帧的进度
        private float CurPro;
        //请求次数
        private int WebRequestCount;
        //进度一致的次数,RequestLimit次认为没网
        int RequestCount = 0;
        int RequestLimit = 100;
        //产品AB包延迟没有网络的等待时间
        float NoNetworkTime = 0;

        protected DownStatus downResult;
        protected string downError = "";
        DownloadOperation downloadOperation;

        public void StartWebRequest()
        {
            WebRequestCount = 1;
            if (string.IsNullOrEmpty(httpInfo.m_srcUrl))
            {
                downResult = DownStatus.Fail;
                downError = "下载地址为空";
                DownloadFinish();
                return;
            }
            if (string.IsNullOrEmpty(httpInfo.m_savePath))
            {
                downResult = DownStatus.Fail;
                downError = "保存地址为空";
                DownloadFinish();
                return;
            }
            m_saveFilePath = httpInfo.m_savePath;
            if (WebRequestCount == 1)
            {
                LastPro = GetProcess();
            }

            FileHelper.DeleteFile(m_saveFilePath);
            downloadOperation = BackgroundDownloads.GetDownloadOperation(httpInfo.m_srcUrl);
            BackgroundDownloadOptions backgroundDownloadOptions = new BackgroundDownloadOptions(httpInfo.m_srcUrl, m_saveFilePath);
            if (downloadOperation == null ||
                 (downloadOperation != null && downloadOperation.Status != DownloadStatus.Paused))
            {
                downloadOperation = BackgroundDownloads.StartDownload(backgroundDownloadOptions);
            }
            else if (downloadOperation != null && downloadOperation.Status == DownloadStatus.Paused)
            {
                //断点续存
                downloadOperation = BackgroundDownloads.StartOrContinueDownload(backgroundDownloadOptions);
            }
            m_isStartDownload = true;
        }

        //Update更新进度
        public void OnUpdate()
        {
            if (isStartDownload && downloadOperation != null)
            {
                if (downloadOperation.IsDone)
                {
                    DownloadComplete();
                }
                else
                {
                    CalculateProgress();
                }
            }
        }
        /// <summary>
        /// 下载完成
        /// </summary>
        public void DownloadComplete()
        {
            switch (downloadOperation.Status)
            {
                case DownloadStatus.Failed:
                    //下载失败
                    LastPro = CurPro;
                    if (WebRequestCount <= 3)
                    {
                        DownManager.Instance.DebugDownError(WebRequestCount, httpInfo.m_srcUrl, downloadOperation.Error);
                        WebRequestCount += 1;
                        CancelDownload();
                        BackgroundDownloadOptions backgroundDownloadOptions = new BackgroundDownloadOptions(httpInfo.m_srcUrl, m_saveFilePath);
                        downloadOperation = BackgroundDownloads.StartDownload(backgroundDownloadOptions);
                    }
                    else
                    {
                        DownManager.Instance.RemoveAndFinish(httpInfo.m_srcUrl);
                        CancelDownload();
                        downResult = DownStatus.Fail;
                        downError = downloadOperation.Error;
                        httpInfo.processEvent.InvokeGracefully(httpInfo, CurPro, true, downResult, downError);
                        m_isStartDownload = false;
                        DownloadFinish();
                    }
                    break;
                case DownloadStatus.Successful:
                    //下载成功
                    //AFLogger.d("下载完成:" + downloadOperation.DestinationPath);
                    //AFLogger.d("下载完成后是否存在路径:" + m_saveFilePath + " " + FileHelper.JudgeFilePathExit(m_saveFilePath));
                    CurPro = 1;
                    LastPro = CurPro;
                    DownManager.Instance.RemoveAndFinish(httpInfo.m_srcUrl);
                    downResult = DownStatus.Sucess;
                    downError = "";
                    m_isStartDownload = false;
                    DownloadFinish();
                    httpInfo.processEvent.InvokeGracefully(httpInfo, CurPro, true, downResult, downError);
                    break;
            }
        }
        public void CalculateProgress()
        {
            CurPro = GetProcess();
            httpInfo.processEvent.InvokeGracefully(httpInfo, CurPro, false, DownStatus.Downloding, "");
            if (CurPro.Equals(LastPro))
            {
                RequestCount += 1;
            }
            else
            {
                RequestCount = 0;
            }
            //RequestLimit次进度没有动判断网络是否停止
            if (RequestCount > RequestLimit)
            {
                //判断是否有网络
                if (DownManager.Instance.JudgeNetworkState())
                {
                    RequestLimit = 0;
                }
                else
                {
                    //如果是AR入口下载的,需要等待180s再显示无网络
                    if (httpInfo.isHasRecog)
                    {
                        NoNetworkTime += Time.deltaTime;
                        if (NoNetworkTime >= 180)
                        {
                            NoNetworkTime = 0;
                            NoNetworkFunc();
                        }
                    }
                    else
                    {
                        NoNetworkFunc();
                    }
                }
            }
            LastPro = CurPro;
        }
        public void NoNetworkFunc()
        {
            DownManager.Instance.RemoveAndFinish(httpInfo.m_srcUrl);
            CancelDownload();
            downResult = DownStatus.NoNetwork;
            downError = "没有网络";
            DownloadFinish();
        }
        /// <summary>
        /// 获取下载进度
        /// </summary>
        /// <returns></returns>
        public float GetProcess()
        {
            if (downloadOperation != null)
            {
                return downloadOperation.Progress;
            }
            return 0;
        }
        /// <summary>
        /// 取消下载
        /// </summary>
        public void CancelDownload()
        {
            if (downloadOperation != null)
            {
                BackgroundDownloads.CancelDownload(downloadOperation);
            }
        }
        /// <summary>
        /// 下载完成后调用的函数
        /// </summary>
        public virtual void DownloadFinish()
        {
        }
    }
}
