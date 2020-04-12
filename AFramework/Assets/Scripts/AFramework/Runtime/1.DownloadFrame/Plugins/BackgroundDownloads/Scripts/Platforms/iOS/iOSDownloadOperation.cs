#if UNITY_IOS //&& !UNITY_EDITOR

using BackgroundDownload.Utils;

public class iOSDownloadOperation : DownloadOperation
{
	
    private readonly Cached<DownloadStatus> cachedStatus;
    private readonly Cached<float> cachedProgress;

	public string URL
	{
		get { return options.URL; }
	}

	private iOSDownloader downloader;

	public iOSDownloadOperation(iOSDownloader downloader, long id, string url)
		: base(url)
	{
		this.downloader = downloader;
		this.id = id;

        cachedStatus = new Cached<DownloadStatus>(() => downloader.GetStatus(id));
        cachedProgress = new Cached<float>(() => downloader.GetProgress(id));
	}

	public override float Progress
	{
		get
		{
			return cachedProgress;
		}
	}

	public override DownloadStatus Status
	{
		get
		{
			return cachedStatus;
		}
	}

	public override string Error 
	{
		get
		{
			return downloader.GetError (this.id);
		}
	}
}

#endif