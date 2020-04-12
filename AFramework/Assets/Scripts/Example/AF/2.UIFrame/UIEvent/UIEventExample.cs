/*******************************************************************
	* Copyright(c)
	* 文件名称: UIEventExample.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
namespace AFramework
{
	using UnityEngine;
	using UnityEngine.UI;

	public class UIEventExample : MonoBehaviour
	{
		public Text ABStatus;
		private void Awake()
		{
			AFStart.RegisterStart(OnStart);
		}
		private void OnStart()
		{
			ABDataHolder.Instance.RegisterABInit(CheckVersionCall);
			ABDataHolder.Instance.CheckAPPVersion();
		}
		public void CheckVersionCall(APPVersionStatus versionStatus)
		{
			if (versionStatus == APPVersionStatus.Abandon)
			{
				AFLogger.e("APP版本已经废弃");
				return;
			}
			switch (ABDataHolder.Instance.GetABResLoadFrom())
			{
				case ABResLoadFrom.EditorRes:
					ABStatus.text = "AB包准备完成";
					LoadMainUI();
					break;
				case ABResLoadFrom.PersistentDataPathAB:
					ABHotUpdate.Instance.ABMD5Request("ABMain", ABProcessevent, ABMD5Callback);
					break;
				case ABResLoadFrom.StreamingAssetAB:
					ABStatus.text = "AB包准备完成";
					LoadMainUI();
					break;
			}
		}
		public void ABMD5Callback(ABClassDownInfo ABDowninfo, DownStatus downResult = DownStatus.Sucess, string downError = "")
		{
			switch (downResult)
			{
				case DownStatus.Downloding:

					break;
				case DownStatus.Fail:
					AFLogger.e("下载MD5文件失败,Error:" + downError);
					break;
				case DownStatus.NoNetwork:
					AFLogger.e("没有网络,请检查!");
					break;
				case DownStatus.Sucess:
					break;
			}
		}
		void ABProcessevent(double process, bool isFinish, DownStatus downResult = DownStatus.Sucess, string downError = "")
		{
			//AB包下载完成,解锁按键加载资源
			if (isFinish)
			{
				ABStatus.text = "AB包准备完成";
				LoadMainUI();
			}
		}
		public void LoadMainUI()
		{
			UIManager.Instance.OpenUI<UIMainEvent>((int)eventUITag.UIMainEvent, null, panelLayer: PanelLayer.Load, isClear: true);
		}
	}
}
