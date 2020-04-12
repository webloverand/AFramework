using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AFramework
{
public class HttpTxtByPost
{
      int httpRequestCount = 0;
        public IEnumerator RealWebRequest(string url,WWWForm lstformData, WebRequestTextEvent webRequestGetEvent)
        {
            UnityWebRequest webRequest = UnityWebRequest.Post(url,lstformData);
            httpRequestCount += 1;
            yield return webRequest.SendWebRequest();
            if (webRequest.error == null || webRequest.error == "")
            {
                if (webRequestGetEvent != null) webRequestGetEvent.Invoke(webRequest.downloadHandler.text, DownStatus.Sucess, "");
            }
            else
            {
                DownManager.Instance.DebugDownError(httpRequestCount, url, webRequest.error);
                if (httpRequestCount <= 3)
                {
                   DownManager.Instance.RegisterRequest(RealWebRequest(url,lstformData, webRequestGetEvent));
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
        public void StartWebRequest(string url,  WWWForm lstformData,WebRequestTextEvent webRequestGetEvent)
        { 
            httpRequestCount = 0;
            DownManager.Instance.RegisterRequest(RealWebRequest(url,lstformData, webRequestGetEvent));
        }
}
}
