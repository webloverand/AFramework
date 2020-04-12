#if UNITY_IOS //&& !UNITY_EDITOR

using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Native calls to iOS functions
/// </summary>
public static class iOSDownloaderNative
{
	[DllImport("__Internal")]
	public static extern void cancelDownload(long id);

	[DllImport("__Internal")]
	public static extern long startDownload(string url,string savePath);

	[DllImport("__Internal")]
	public static extern long getDownloadTask(string url);

	[DllImport("__Internal")]
	public static extern float getDownloadProgress(long id);

	[DllImport("__Internal")]
	public static extern int getDownloadStatus(long id);

	[DllImport("__Internal")]
	private static extern IntPtr getError(long id);

	public static string getErrorMessage(long id)
	{
		return Marshal.PtrToStringAuto (getError (id));
	}
}
	
public class iOSDownloader : IBackgroundDownloader
{
    // iOS download status codes.
	private enum iOSDownloadStatus
	{
			NSURLSessionTaskStateRunning = 0,                     /* The task is currently being serviced by the session */
			NSURLSessionTaskStateSuspended = 1,
			NSURLSessionTaskStateCanceling = 2,                   /* The task has been told to cancel.  The session will receive a URLSession:task:didCompleteWithError: message. */
			NSURLSessionTaskStateCompleted = 3,                   /* The task has completed and the session will receive no more delegate notifications */
	}

    public DownloadOperation StartDownload(string url)
	{
		return StartDownload (new BackgroundDownloadOptions (url));
	}
    
    public DownloadOperation StartDownload(BackgroundDownloadOptions options)
	{
        // Use the destination path stored in the 'options' instance.
        // On iOS, for init we only need the directory name, not the full path (including file name).
		//iOSDownloaderNative.init(Path.GetDirectoryName(options.DestinationPath));

		var id = iOSDownloaderNative.startDownload (options.URL,options.DestinationPath);
		var op = new iOSDownloadOperation (this, id, options.URL);

		return op;
	}

	public void CancelDownload(DownloadOperation operation)
	{
		var iOSOperation = operation as iOSDownloadOperation;

		if (iOSOperation == null)
		{
			Debug.LogError ("Unexpected error! an invalid DownloadOperation type for this platform!");
		}
		else
		{
			iOSDownloaderNative.cancelDownload(operation.ID);
		}
	}

	public float GetProgress(long id)
	{
		Profiler.BeginSample ("iOSDownloader.GetProgress");

		var progress = iOSDownloaderNative.getDownloadProgress (id);

		Profiler.EndSample();

		return progress;
	}

	public string GetError(long id)
	{
		Profiler.BeginSample ("iOSDownloader.GetError");

		var error = iOSDownloaderNative.getErrorMessage (id);

		Profiler.EndSample();

		return error;
	}

	public DownloadStatus GetStatus(long id)
	{
		Profiler.BeginSample ("iOSDownloader.GetStatus");

		DownloadStatus status = DownloadStatus.Failed;

		var iOSStatus = (iOSDownloadStatus)iOSDownloaderNative.getDownloadStatus (id);

		switch (iOSStatus)
		{
		case iOSDownloadStatus.NSURLSessionTaskStateCanceling:
			status = DownloadStatus.Failed;
			break;
		case iOSDownloadStatus.NSURLSessionTaskStateCompleted:

			// check if failed or success
			var error = GetError (id);

			status = string.IsNullOrEmpty (error) ? DownloadStatus.Successful : DownloadStatus.Failed;
			break;
		case iOSDownloadStatus.NSURLSessionTaskStateRunning:
			status = DownloadStatus.Running;
			break;
		case iOSDownloadStatus.NSURLSessionTaskStateSuspended:
			status = DownloadStatus.Paused;
			break;
		}

		Profiler.EndSample();

		return status;
	}

	public DownloadOperation GetDownloadOperation(string url)
	{
		var id = iOSDownloaderNative.getDownloadTask(url);
		
		return (id == -1) ? null : new iOSDownloadOperation (this, id, url);
	}
}

#endif