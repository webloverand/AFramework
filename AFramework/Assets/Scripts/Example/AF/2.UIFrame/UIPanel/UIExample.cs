/*******************************************************************
	* Copyright(c)
	* 文件名称: UIExample.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
namespace AFramework
{
	using UnityEngine;
	using UnityEngine.UI;

	public class UIExample : MonoBehaviour
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
					break;
				case ABResLoadFrom.PersistentDataPathAB:
					ABHotUpdate.Instance.ABMD5Request("ABMain", ABProcessevent, ABMD5Callback);
					break;
				case ABResLoadFrom.StreamingAssetAB:
					isABFinish = true;
					ABStatus.text = "AB包准备完成";
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
			}
		}
		private void Update()
		{
			if (isABFinish)
			{
				//A : 从AB包同步加载prefab
				if (Input.GetKeyDown(KeyCode.A))
				{
					//加载Resource路径下的Prefabs
					testPrefab = UIManager.Instance.OpenUI<UIABLoadTestPre>((int)testUIEnum.ABLoadTestPre, null, panelLayer: PanelLayer.Load, isClear: true);
				}
				//S : 释放A加载的prefab,无限缓存
				if (Input.GetKeyDown(KeyCode.S))
				{
					//加载Resource路径下的Prefabs
					UIManager.Instance.CloseUI<UIABLoadTestPre>();
				}
				//D : 释放所有资源,这里注意 : UI资源属于UIManager中的resLoader,如果要释放UI,请类似S键那样通过UIManager释放UI
				if (Input.GetKeyDown(KeyCode.D))
				{
					//加载Resource路径下的Prefabs
					resLoader.ReleaseAllRes();
				}
				//F : 释放UIChild所有子物体,回收到RecyclePoolTrs
				if (Input.GetKeyDown(KeyCode.F))
				{
					testPrefab.RecycleAllChild();
				}
				//G : 释放UIChild所有子物体,并会回收内存
				if (Input.GetKeyDown(KeyCode.G))
				{
					testPrefab.RecycleChild<UIChild>(0, true);
				}

				//Z : 释放父级物体,回收到RecyclePoolTrs
				if (Input.GetKeyDown(KeyCode.Z))
				{
					UIManager.Instance.CloseUI<UIABLoadTestPre>(1);
				}


				//X : 释放父级物体,连同子物体一起回收,请注意参数
				if (Input.GetKeyDown(KeyCode.X))
				{
					UIManager.Instance.CloseUI<UIABLoadTestPre>(0, true);
				}
			}
		}
		private void OnDestroy()
		{
			resLoader.Recycle2Cache();
		}
	}
}
