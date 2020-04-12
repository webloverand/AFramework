using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AFramework
{
    public class HttpByteInfo : HttpInfo
    {
        public WebRequestByteEvent webRequestByteEvent;

        public static HttpByteInfo Allocate(string m_srcUrl, WebRequestByteEvent webRequestTextEvent, string m_savePath = "",
            WebRequestProcessEvent processEvent = null)
        {
            HttpByteInfo httpByteInfo = new HttpByteInfo();
            httpByteInfo.m_srcUrl = m_srcUrl;
            httpByteInfo.webRequestByteEvent = webRequestTextEvent;
            httpByteInfo.m_savePath = m_savePath;
            httpByteInfo.processEvent = processEvent;
            return httpByteInfo;
        }
    }
    public class HttpByte : HttpBase
    {
        public HttpByte(HttpByteInfo httpByteInfo) : base(httpByteInfo)
        {

        }
        public override void DownloadFinish()
        {
            base.DownloadFinish();
            HttpByteInfo httpByteInfo = (HttpByteInfo)httpInfo;
            if (httpByteInfo.webRequestByteEvent != null)
            {
                
                if(downResult == DownStatus.Fail || downResult == DownStatus.NoNetwork)
                    httpByteInfo.webRequestByteEvent.Invoke(default(byte[]),downResult,downError);
                else
                    httpByteInfo.webRequestByteEvent.Invoke(FileHelper.ReadByteArray(m_saveFilePath), downResult, downError);
            }
        }
    }
}
