/*******************************************************************
* Copyright(c)
* 文件名称: test.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
	using UnityEngine;

	public class AndroidStoragePermission : MonoBehaviour
	{
        DownloadOperation downloadOperation;
        private void Start()
        {
            NativeInterface.Instance.GetStoragePermission(StoragePermission);
        }
        public void StoragePermission(string s)
        {
            downloadOperation = BackgroundDownloads.GetDownloadOperation("https://qianxi.fahaxiki.cn/AB/OnlineAB/AF-ABForServer/APPInfo.txt");
            BackgroundDownloadOptions backgroundDownloadOptions = new BackgroundDownloadOptions("https://qianxi.fahaxiki.cn/AB/OnlineAB/AF-ABForServer/APPInfo.txt", PathTool.PersistentDataPath + "APPInfo.txt");
            if (downloadOperation == null ||
                (downloadOperation != null && downloadOperation.Status == DownloadStatus.Successful))
            {
                downloadOperation = BackgroundDownloads.StartDownload(backgroundDownloadOptions);
            }
            else if (downloadOperation != null && downloadOperation.Status != DownloadStatus.Successful)
            {
                //断点续存
                downloadOperation = BackgroundDownloads.StartOrContinueDownload(backgroundDownloadOptions);
            }
            Debug.Log("downloadOperation是否为空:" + (downloadOperation == null));
        }

        private void Update()
        {
            if(downloadOperation != null)
            {
                if(downloadOperation.Status == DownloadStatus.Failed)
                {
                    Debug.Log("下载失败:" + downloadOperation.Error);
                }
                else if (downloadOperation.Status == DownloadStatus.Successful)
                {
                    Debug.Log("下载完成:" + downloadOperation.DestinationPath);

                    Debug.Log("下载完成文件是否存在:" + FileHelper.JudgeFilePathExit(PathTool.PersistentDataPath + "APPInfo.txt"));
                    downloadOperation = null;
                }
                else
                {
                    Debug.Log("下载失败:???");
                }
            }
        }
    }
}	
