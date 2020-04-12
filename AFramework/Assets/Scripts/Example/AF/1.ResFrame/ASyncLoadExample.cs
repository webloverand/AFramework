/*******************************************************************
	* Copyright(c)
	* 文件名称: ASyncLoadExample.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
namespace AFramework
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class ASyncLoadExample : MonoBehaviour
	{
		ResLoader resLoader;
		SpriteRenderer spriteRender;
		public SpriteRenderer spriteRender1;
		//通过Internal从Resource路径加载Prefab,然后Instantiate,但是并不建议这种方式,建议使用resloader.InstantiateObject方式
		List<GameObject> Res = new List<GameObject>();
		//使用resloader.InstantiateObject方式加载,无需外面实例化资源与销毁物体
		List<GameObject> Obj = new List<GameObject>();

		public Transform ObjPar;
		public Transform UIPar;

		public bool isABFinish = false;
		public Text ABStatus;

		private void Awake()
		{
			AFStart.RegisterStart(OnStart);
			spriteRender = GetComponent<SpriteRenderer>();
		}
		private void OnStart()
		{
			ABDataHolder.Instance.RegisterABInit(ABStart);
			ABDataHolder.Instance.CheckAPPVersion();
		}
		private void ABStart(APPVersionStatus versionStatus)
		{
			resLoader = ResLoader.Allocate();
			if (versionStatus != APPVersionStatus.Abandon)
			{
				//这里为了保证PersistentDataPathAB时云端资源下载完成
				switch (ABDataHolder.Instance.GetABResLoadFrom())
				{
					case ABResLoadFrom.EditorRes:
						isABFinish = true;
						ABStatus.text = "AB包准备完成";
						break;
					case ABResLoadFrom.PersistentDataPathAB:
						ABHotUpdate.Instance.ABMD5Request("Main", ABProcessevent, ABMD5Callback);
						break;
					case ABResLoadFrom.StreamingAssetAB:
						isABFinish = true;
						ABStatus.text = "AB包准备完成";
						break;
				}
			}
			else
			{
				AFLogger.d("版本检测显示是废弃APP,请检查设置");
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
		public void loadSprite()
		{
			//请注意不要带后缀,加载Resource路径下的Sprite
			resLoader.LoadAsync<Sprite>(ResFromType.ResourcesRes, "LocalImageExample/ChangeSetGo", LoadSpriteCallBack, true);
		}
		public void LoadSpriteCallBack(bool result, ResInfo resInfo)
		{
			if (result)
			{
				spriteRender.sprite = resInfo.ResObject as Sprite;
			}
			else
			{
				AFLogger.e("加载" + resInfo.ResPath + "失败!");
			}
		}
		//释放resLoader加载的单个资源
		public void ReleaseSprite()
		{
			spriteRender.sprite = null;
			//如果传false内存不会释放
			resLoader.ReleaseRes("LocalImageExample/ChangeSetGo", true);
		}
		public void LoadPrefab()
		{
			//加载Resource路径下的Prefabs
			resLoader.LoadAsync<GameObject>(ResFromType.ResourcesRes, "ExampleRes/Models/OxygenTank", LoadResPrefab, DestroyCache: true);
		}
		public void LoadResPrefab(bool result, ResInfo resInfo)
		{
			if (result)
			{
				Res.Add(Instantiate(resInfo.ResObject as GameObject));
				AFLogger.d("从Resources加载并实例化了" + Res.Count + "个");
			}
			else
			{
				AFLogger.e("加载" + resInfo.ResPath + "失败!");
			}
		}
		public void ReleaseResPrefab()
		{
			if (Res.Count > 0)
			{
				AFLogger.d("释放从Resources加载的资源");
				DestroyImmediate(Res[Res.Count - 1]);
				Res.RemoveAt(Res.Count - 1);
				//满足释放的条件 : 1.释放资源时destoryObj为true,默认为false 2.此资源的引用计数为0
				//也就是说你加载此资源实例化了多少次,就需要释放多少次,并且最后一次释放调用destoryObj = true 
				resLoader.ReleaseRes("ExampleRes/Models/OxygenTank", true);
			}
			else
			{
				AFLogger.d("请先加载资源再释放");
			}
		}
		public void LoadSpriteFromLocalAB()
		{
			//从AB包加载Sprite,这里只测试了本地加载,加载来源配置要设置为streamasset
			resLoader.LoadAsync<Sprite>(ResFromType.ABRes, "Assets/ResForAB/ABMain/Img/Help.png", LoadABSpriteCallBack, true);
		}
		public void LoadABSpriteCallBack(bool result, ResInfo resInfo)
		{
			spriteRender1.sprite = resInfo.ResObject as Sprite;
		}
		//释放resLoader加载的单个资源
		public void ReleaseABSprite()
		{
			spriteRender1.sprite = null;
			//如果传false内存不会释放
			resLoader.ReleaseRes("Assets/ResForAB/ABMain/Img/Help.png", true);
		}
		public void LoadPrefabFromLocalAB()
		{
			resLoader.InstantiateObjectASync(ResFromType.ABRes, "Assets/ResForAB/ABMain/Prefabs/ABLoadTestPre.prefab",
				   LoadObjCallBack, UIPar, bClear: true);
		}
		public void LoadObjCallBack(bool result, ResObject resObject)
		{
			if (result)
			{
				Obj.Add(resObject.mCloneObj);
			}
		}
		public void ReleaseObjPrefab()
		{
			if (Obj.Count > 0)
			{
				AFLogger.d("释放从AB包加载的资源");
				//满足释放的条件 : 1.maxCacheCount为0 2.destoryObj为true 3.引用计数为0
				resLoader.ReleaseObj(Obj[Obj.Count - 1], 0, true);
				Obj.RemoveAt(Obj.Count - 1);
			}
			else
			{
				AFLogger.d("请先加载资源再释放");
			}
		}
		//专用于编辑器测试,路径为相对于Assets,加载Sprite
		public void LoadSpriteFromEditor()
		{
			resLoader.LoadAsync<Sprite>(ResFromType.EditorRes, "Assets/AFramewprk/Example/Resources/LocalImageExample/ChangeSetGo.png", LoadSprite, true, true);
		}
		//异步加载Sprite回调
		public void LoadSprite(bool result, ResInfo resInfo)
		{
			if (result)
			{
				spriteRender.sprite = resInfo.ResObject as Sprite;
			}
		}
		public void LoadNetImage()
		{
			resLoader.LoadAsync<Sprite>(ResFromType.NetImageRes, "https://upload-images.jianshu.io/upload_images/17787668-a507cc06cd0fc6fc.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240",
				   LoadSprite, DestroyCache: true);
		}
		//释放resLoader加载的所有资源,至于缓存会根据其加载时的属性
		public void ReleaseRes()
		{
			GetComponent<SpriteRenderer>().sprite = null;
			for (int i = Res.Count - 1; i >= 0; i--)
			{
				DestroyImmediate(Res[i]);
			}
			Res.Clear();
			resLoader.ReleaseAllRes();
			Obj.Clear();
		}
	}
}
