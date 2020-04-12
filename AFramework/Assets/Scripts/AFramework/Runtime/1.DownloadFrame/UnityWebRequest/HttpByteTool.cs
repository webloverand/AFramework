using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
namespace AFramework {
    /// <summary>
    /// 用于从网络获取数据
    /// </summary>
    public class HttpByteTool  {
        int httpRequestCount = 0;

        UnityWebRequest webRequest;
        public float Process
        {
            get
            {
                if (webRequest != null)
                {
                    return webRequest.downloadProgress;
                }
                return 1;
            }
        }

        public IEnumerator RealWebRequest (string url, string SavePath,
            WebRequestByteEvent webRequestGetEvent) {
            webRequest = UnityWebRequest.Get (url);
            httpRequestCount += 1;
            yield return webRequest.SendWebRequest ();
            if (webRequest.error == null || webRequest.error == "") {
                if (SavePath != null && SavePath != "") {
					FileHelper.CreatFile (SavePath, webRequest.downloadHandler.data, true);
                }
                if (webRequestGetEvent != null) {
                        webRequestGetEvent.Invoke (webRequest.downloadHandler.data,DownStatus.Sucess,"");
                }
                webRequest = null;
            } else
            {
                webRequest = null;
                DownManager.Instance.DebugDownError(httpRequestCount, url, webRequest.error);
                if (httpRequestCount <= 3) {
                    DownManager.Instance.RegisterRequest (RealWebRequest (url, SavePath, webRequestGetEvent));
                }
                else
                {
                    if (webRequestGetEvent != null)
                    {
                        webRequestGetEvent.Invoke(default(byte[]), DownStatus.Fail, webRequest.error);
                    }
                }
            }
        }
        public void StartWebRequest (string url, string SavePath, WebRequestByteEvent webRequestGetEvent) {
            httpRequestCount = 0;
            DownManager.Instance.RegisterRequest (RealWebRequest (url, SavePath, webRequestGetEvent));
        }
    }
}
