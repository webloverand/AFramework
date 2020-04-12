using UnityEngine;

/// <summary>
/// Responsible for initializing the iOS implementation.
/// This was placed inside a separate class due to this bug: https://issuetracker.unity3d.com/issues/conditional-compilation-problem-with-runtimeinitializeonloadmethod
/// </summary>
public class iOSRuntimeInit : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		#if UNITY_IOS && !UNITY_EDITOR
		//iOSDownloaderNative.init(BackgroundDownloadOptions.DEFAULT_DOWNLOAD_PATH);
		#endif
	}
}