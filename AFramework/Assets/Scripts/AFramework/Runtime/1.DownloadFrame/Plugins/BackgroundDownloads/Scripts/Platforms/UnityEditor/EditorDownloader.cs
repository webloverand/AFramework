#if UNITY_EDITOR

using System;

public class EditorDownloader : IBackgroundDownloader
{
	public void CancelDownload(DownloadOperation operation)
	{
        var editorOperation = operation as EditorDownloadOperation;

        if (editorOperation == null)
        {
            return;
        }

        editorOperation.Cancel();
	}

	public DownloadOperation StartDownload(BackgroundDownloadOptions options)
	{
		return new EditorDownloadOperation(options);
	}

	public DownloadOperation StartDownload(string url)
	{
		var options = new BackgroundDownloadOptions(url);

		return StartDownload(options);
	}

	public DownloadOperation GetDownloadOperation (string url)
	{
		return null;
	}
}

#endif