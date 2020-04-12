using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AFramework
{

    public class HttpTxtTool
    {
        int httpRequestCount = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="webRequestGetEvent"></param>
        /// <param name="SavePath">如果为""则不需要保存</param>
        /// <param name="isReturnPath">默认是返回内容</param>
        /// <returns></returns>
        public IEnumerator RealWebRequest(string url, WebRequestTextEvent webRequestGetEvent, string SavePath = "",bool isReturnPath = false)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            httpRequestCount += 1;
            yield return webRequest.SendWebRequest();
            if (webRequest.error == null || webRequest.error == "")
            {
                if (SavePath != null && SavePath != "")
                {
                    FileHelper.CreatFile(SavePath, webRequest.downloadHandler.data, true);
                }
                if (isReturnPath)
                {
                    if (webRequestGetEvent != null) webRequestGetEvent.Invoke(SavePath, DownStatus.Sucess, "");
                }
                else
                {
                    if (webRequestGetEvent != null) webRequestGetEvent.Invoke(webRequest.downloadHandler.text, DownStatus.Sucess, "");
                }
            }
            else
            {
                DownManager.Instance.DebugDownError(httpRequestCount, url, webRequest.error);
                if (httpRequestCount <= 3)
                {
                    DownManager.Instance.RegisterRequest(RealWebRequest(url, webRequestGetEvent, SavePath, isReturnPath));
                }
                else
                {
                    if (webRequestGetEvent != null)
                    {
                        webRequestGetEvent.Invoke("", DownStatus.Fail, webRequest.error);
                    }
                }
            }
        }
        public void StartWebRequest(string url, WebRequestTextEvent webRequestGetEvent,string SavePath = "", bool isReturnPath = false)
        {
            httpRequestCount = 0;
            DownManager.Instance.RegisterRequest(RealWebRequest(url, webRequestGetEvent, SavePath, isReturnPath));
        }
    }
}
