/*******************************************************************
	* Copyright(c)
	* 文件名称: ABDownExample.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
namespace AFramework
{
	using UnityEngine;
	using UnityEngine.UI;

	public class ABDownExample : MonoBehaviour
	{
		public Text MD5Text;
		public Text ABProcessText;
		public Text APPVersionText;

		public void Start()
		{
            ABDataHolder.Instance.RegisterABInit(ABStart);
            ABDataHolder.Instance.CheckAPPVersion();
        }
		private void ABStart(APPVersionStatus versionStatus)
		{
			switch (versionStatus)
			{
				case APPVersionStatus.Abandon:
					APPVersionText.text = "此APP版本版本已经废弃,请检查";
					break;
				case APPVersionStatus.Newest:
					APPVersionText.text = "此APP版本版本是最新的";
					ABHotUpdate.Instance.ABMD5Request("Main", ABProcessevent, ABMD5Callback);
					break;
				case APPVersionStatus.Update:
					APPVersionText.text = "此APP版本版本需要更新,但是也可以不更新使用此APP";
					ABHotUpdate.Instance.ABMD5Request("Main", ABProcessevent, ABMD5Callback);
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
					MD5Text.text = "下载MD5文件失败,Error:" + downError;
					break;
				case DownStatus.NoNetwork:
					MD5Text.text = "没有网络,请检查!";
					break;
				case DownStatus.Sucess:
					MD5Text.text = GetABStatusStr(ABDowninfo.aBState);
					break;
			}
		}
		void ABProcessevent(double process, bool isFinish, DownStatus downResult = DownStatus.Sucess, string downError = "")
		{
			ABProcessText.text = process + "";
		}
		public string GetABStatusStr(ABState aBState)
		{
			switch (aBState)
			{
				case ABState.Newest:
					return "本地AB包已经是最新";
				case ABState.UpdateAndLocalComplete:
					return "本地AB包完整但是需要更新";
				case ABState.UpdateAndLocalNotComplete:
					return "本地AB包不完整且需要更新";
			}
			return "";
		}
	}
}
