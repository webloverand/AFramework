using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AFramework
{
    public class DownManager : Singleton<DownManager>
    {
        public Action<string, string> NoNetwork;

        Dictionary<string, HttpBase> URLToHttpBase = new Dictionary<string, HttpBase>();

        //当前正在请求的HttpBase
        HttpBase CurRequesting;
        bool isDowning = false;
        int RequestIEnumID;

        public DownManager()
        {
            AFStart.RegisterUpdate(OnUpdate);
            RequestIEnumID = IEnumeratorTool.StartCoroutine(SendRequstFromList());
        }

        IEnumerator SendRequstFromList()
        {
            while (true)
            {
                if (!isDowning && URLToHttpBase.Count > 0)
                {
                    List<string> urls = new List<string>(URLToHttpBase.Keys);
                    AFLogger.d("请求URL为:" + urls[0]);
                    CurRequesting = URLToHttpBase[urls[0]];
                    CurRequesting.StartWebRequest();
                    isDowning = true;
                }
                else
                {
                    yield return null;
                }
            }
        }
        public void AddRequest(string url, HttpBase httpData)
        {
            if (URLToHttpBase.ContainsKey(url))
            {
                URLToHttpBase[url] = httpData;
            }
            else
            {
                URLToHttpBase.Add(url, httpData);
            }
        }
        public void RemoveAndFinish(string url)
        {
            RemoveRequest(url);
            CurRequesting = null;
            isDowning = false;
        }
        public void RemoveRequest(string url)
        {
            if (URLToHttpBase.ContainsKey(url))
            {
                URLToHttpBase.Remove(url);
            }
            else
            {
                //Debug.Log("需要移除的请求不存在:" + url);
            }
        }
        public void RemoveRequestByUser(string url)
        {
            if (CurRequesting.httpInfo.m_srcUrl.Equals(url))
            {
                CurRequesting.CancelDownload();
                CurRequesting = null;
                isDowning = false;
            }
            RemoveRequest(url);
        }
        //Update更新
        public void OnUpdate()
        {
            if (isDowning)
            {
                CurRequesting.OnUpdate();
            }
        }

        /// <summary>
        /// 判断是否有网络
        /// </summary>
        /// <returns></returns>
        public bool JudgeNetworkState()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 停止程序运行时停止下载
        /// </summary>
        public void OnApplicationQuit()
        {
            if (CurRequesting != null)
            {
                CurRequesting.CancelDownload();
            }
        }
        /// <summary>
        /// MD5文件下载没有进度,只有完成时回调
        /// </summary>
        /// <param name="aBDownInfo"></param>
        public void StartDownABMD5(ABClassDownInfo aBDownInfo)
        {
            if (JudgeNetworkState())
            {
                if (!URLToHttpBase.ContainsKey(aBDownInfo.m_srcUrl))
                {
                    FileHelper.DeleteFile(aBDownInfo.m_savePath);
                    HttpABMD5 httpAB = new HttpABMD5(aBDownInfo);
                    AddRequest(aBDownInfo.m_srcUrl, httpAB);
                }
            }
            else
            {
                //没有网络
                aBDownInfo.ABMD5Callback.InvokeGracefully(aBDownInfo, DownStatus.NoNetwork, "");
            }
        }
        public bool StartDownAB(HttpInfo httpInfo)
        {
            if (JudgeNetworkState())
            {
                if (!URLToHttpBase.ContainsKey(httpInfo.m_srcUrl))
                {
                    HttpAB httpAB = new HttpAB(httpInfo);
                    AddRequest(httpInfo.m_srcUrl, httpAB);
                }
                return true;
            }
            else
            {
                httpInfo.processEvent(httpInfo, 0, false, DownStatus.NoNetwork, "");
                return false;
            }
        }
        /// <summary>
        /// 默认返回获取到的内容
        /// </summary>
        /// <param name="m_srcUrl"></param>
        /// <param name="webRequestTextEvent"></param>
        /// <param name="returnPathOrCon"></param>
        /// <param name="m_savePath"></param>
        /// <param name="processEvent"></param>
        public void StartDownReturnPathOrContent(string m_srcUrl, WebRequestTextEvent webRequestTextEvent,
              bool returnPathOrCon = false, string m_savePath = "", WebRequestProcessEvent processEvent = null)
        {
            if (JudgeNetworkState())
            {
                if (!URLToHttpBase.ContainsKey(m_srcUrl))
                {

                    HttpTxt httpTxt = new HttpTxt(HttpTxtInfo.Allocate(m_srcUrl, webRequestTextEvent, returnPathOrCon, m_savePath, processEvent));
                    AddRequest(m_srcUrl, httpTxt);
                }
            }
            else
            {
                webRequestTextEvent("", DownStatus.NoNetwork, "");
            }
        }
        public void StartByteDown(string m_srcUrl, WebRequestByteEvent webRequestTextEvent, string m_savePath = "",
            WebRequestProcessEvent processEvent = null)
        {
            if (JudgeNetworkState())
            {
                if (!URLToHttpBase.ContainsKey(m_srcUrl))
                {
                    HttpByte httpTxt = new HttpByte(HttpByteInfo.Allocate(m_srcUrl, webRequestTextEvent, m_savePath, processEvent));
                    AddRequest(m_srcUrl, httpTxt);
                }
            }
            else
            {
                webRequestTextEvent(default(byte[]), DownStatus.NoNetwork, "");
            }
        }
        public void StartTextRequest(string url, WebRequestTextEvent webRequestGetEvent, string SavePath = "", bool isReturnPath = false)
        {
            if (JudgeNetworkState())
            {
                HttpTxtTool httpTool = new HttpTxtTool();
                httpTool.StartWebRequest(url, webRequestGetEvent, SavePath, isReturnPath);
            }
            else
            {
                webRequestGetEvent("", DownStatus.NoNetwork, "");
            }
        }
        public void StartByteRequest(string url, string SavePath, WebRequestByteEvent webRequestGetEvent)
        {
            if (JudgeNetworkState())
            {
                HttpByteTool httpTool = new HttpByteTool();
                httpTool.StartWebRequest(url, SavePath, webRequestGetEvent);
            }
            else
            {
                webRequestGetEvent(default(byte[]), DownStatus.NoNetwork, "");
            }
        }
        public void StartTextByPost(string url, WWWForm lstformData, WebRequestTextEvent webRequestGetEvent)
        {
            if (JudgeNetworkState())
            {
                HttpTxtByPost httpTool = new HttpTxtByPost();
                httpTool.StartWebRequest(url, lstformData, webRequestGetEvent);
            }
            else
            {
                webRequestGetEvent("", DownStatus.NoNetwork, "");
            }
        }
        public void StartNetTexture(string url, WebRequestNetTextureEvent webRequestGetEvent, string savePath = "")
        {
            if (JudgeNetworkState())
            {
                HttpNetTextureTool httpTool = new HttpNetTextureTool();
                httpTool.StartWebRequest(url, webRequestGetEvent, savePath);
            }
            else
            {
                webRequestGetEvent(default(Texture2D), "", DownStatus.NoNetwork, "");
            }
        }


        public void DebugDownError(int RequestCount, string url, string error)
        {
            AFLogger.e("第" + RequestCount + "次请求" + url + "错误:" + error);
        }
        public void RegisterRequest(IEnumerator enumerator)
        {
            IEnumeratorTool.StartCoroutine(enumerator);
        }
    }
}
