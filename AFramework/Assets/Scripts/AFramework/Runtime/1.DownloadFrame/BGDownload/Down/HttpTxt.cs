using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AFramework
{
    /// <summary>
    /// 下载txt文档
    /// </summary>
    public class HttpTxtInfo : HttpInfo
    {
        public  WebRequestTextEvent webRequestTextEvent;
        public bool returnPathOrCon;

        public static HttpTxtInfo Allocate(string m_srcUrl, WebRequestTextEvent webRequestTextEvent,
              bool returnPathOrCon = false, string m_savePath = "", WebRequestProcessEvent processEvent = null)
        {
            HttpTxtInfo httpTxtInfo = new HttpTxtInfo();
            httpTxtInfo.m_srcUrl = m_srcUrl;
            httpTxtInfo.m_savePath = m_savePath;
            httpTxtInfo.returnPathOrCon = returnPathOrCon;
            httpTxtInfo.webRequestTextEvent = webRequestTextEvent;
            httpTxtInfo.processEvent = processEvent;
            return httpTxtInfo;
        }
    }
    public class HttpTxt : HttpBase
    {
        public HttpTxt(HttpTxtInfo httpTxtInfo) : base(httpTxtInfo)
        {

        }
        public override void DownloadFinish()
        {
            base.DownloadFinish();
            HttpTxtInfo httpTxtInfo = (HttpTxtInfo)httpInfo;
            if (httpTxtInfo.webRequestTextEvent != null)
            {
                if (httpTxtInfo.returnPathOrCon)
                {
                    httpTxtInfo.webRequestTextEvent.Invoke(m_saveFilePath, downResult, downError);
                }
                else
                {
                   if(downResult == DownStatus.Fail)
                        httpTxtInfo.webRequestTextEvent.Invoke("", downResult, downError);
                   else
                        httpTxtInfo.webRequestTextEvent.Invoke(FileHelper.ReadTxtToStr(m_saveFilePath), downResult, downError);
                }
            
            }
        }
    }
}
