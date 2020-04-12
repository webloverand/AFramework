/*******************************************************************
* Copyright(c)
* 文件名称: HttpNetTextureTool.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Networking;

    public class HttpNetTextureTool
    {
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="webRequestGetEvent"></param>
        /// <param name="SavePath">如果为""则不需要保存</param>
        /// <param name="isReturnPath">默认是返回内容</param>
        /// <returns></returns>
        public IEnumerator RealWebRequest(string url, WebRequestNetTextureEvent webRequestGetEvent, string savePath = "")
        {
            webRequest = UnityWebRequestTexture.GetTexture(url);
            httpRequestCount += 1;
            yield return webRequest.SendWebRequest();
            if (webRequest.error == null || webRequest.error == "")
            {
                Texture2D texture2D = DownloadHandlerTexture.GetContent(webRequest);
                if (savePath != null && savePath != "")
                {
                    FileHelper.CreatFile(savePath, texture2D.EncodeToPNG());
                }
                webRequestGetEvent(texture2D, savePath, DownStatus.Sucess, "");
                webRequest = null;
            }
            else
            {
                webRequest = null;
                DownManager.Instance.DebugDownError(httpRequestCount, url, webRequest.error);
                if (httpRequestCount <= 3)
                {
                    IEnumeratorTool.StartCoroutine(RealWebRequest(url, webRequestGetEvent));
                }
                else
                {
                    if (webRequestGetEvent != null)
                    {
                        webRequestGetEvent.Invoke(default(Texture2D), savePath, DownStatus.Fail, webRequest.error);
                    }
                }
            }
        }
        public void StartWebRequest(string url, WebRequestNetTextureEvent webRequestGetEvent,string savePath = "")
        {
            httpRequestCount = 0;
            IEnumeratorTool.StartCoroutine(RealWebRequest(url, webRequestGetEvent));
        }
    }
}
