/*******************************************************************
	* Copyright(c)
	* 文件名称: ChangeLanuage.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
namespace AFramework
{
	using UnityEngine;
	using UnityEngine.UI;

	public class ChangeLanuage : MonoBehaviour
	{
		public bool isABFinish = false;
		public Text ABStatus;
		ResLoader resLoader;
		UIABLoadTestPre testPrefab;
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
			resLoader = ResLoader.Allocate();
			if (versionStatus == APPVersionStatus.Abandon)
			{
				AFLogger.e("APP版本已经废弃");
				return;
			}
			switch (ABDataHolder.Instance.GetABResLoadFrom())
			{
				case ABResLoadFrom.EditorRes:
					isABFinish = true;
					ABStatus.text = "AB包准备完成";
					LoadUI();
					break;
				case ABResLoadFrom.PersistentDataPathAB:
					ABHotUpdate.Instance.ABMD5Request("ABMain", ABProcessevent, ABMD5Callback);
					break;
				case ABResLoadFrom.StreamingAssetAB:
					isABFinish = true;
					ABStatus.text = "AB包准备完成";
					LoadUI();
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
				isABFinish = true;
				ABStatus.text = "AB包准备完成";
				LoadUI();
			}
		}
		private void OnDestroy()
		{
			resLoader.Recycle2Cache();
		}
		public Transform uiparent;
		public void LoadUI()
		{
			lanuageUIScript = UIManager.Instance.OpenUI<LanuageUIScript>(500);
		}
		LanuageUIScript lanuageUIScript;
		int SuffixIndex = 0;
		string[] SuffixStr = { "CHA", "ENG" };
		string[] BtnStr = { "切换成英文UI", "切换成中文UI" };
		public Text btnText;
		public void ChangeLanuageBtn()
		{
			if (SuffixIndex == 1)
			{
				SuffixIndex = 0;
			}
			else
			{
				SuffixIndex = 1;
			}
			btnText.text = BtnStr[SuffixIndex];
			AFStart.Instance.ChangeLanuageSuffix(SuffixStr[SuffixIndex]);
			lanuageUIScript.CloseSelf();
			lanuageUIScript = UIManager.Instance.OpenUI<LanuageUIScript>(500);
		}
	}
}
