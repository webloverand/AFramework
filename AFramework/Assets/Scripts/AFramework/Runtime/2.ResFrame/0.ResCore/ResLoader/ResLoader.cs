/*******************************************************************
* Copyright(c)
* 文件名称: ResLoader.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections.Generic;
    using UnityEngine;
    /// <summary>
    /// 单个物体的加载资源的记录
    /// </summary>
	public class ResLoader : DisposableObject, IResLoader, IPoolType
    {
        #region 提供给外界加载资源的接口
        /// <summary>
        /// 预加载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resType"></param>
        /// <param name="assetPath"></param>
        /// <param name="count"></param>
        /// <param name="clear"></param>
        public void PreLoadObj<T>(ResFromType resType, string assetPath, int count = 1, bool clear = false) where T : Object
        {
            if (CheckNoCanLoad())
            {
                AFLogger.e("此Loader已经被释放,请注意!");
                return;
            }
            for (int i = 0; i < count; i++)
            {
                ObjLoadInfo objLoadInfo = ObjLoadInfo.Allocate(resType, assetPath, null, false);
                ResManager.Instance.PreLoadObj(objLoadInfo);
                objLoadInfo.Recycle2Cache();
            }
        }
        /// <summary>
        /// 同步加载Prefab,返回其实例化的GameObject
        /// </summary>
        /// <param name="resType"></param>
        /// <param name="assetPath"></param>
        /// <param name="parTrans"></param>
        /// <param name="bClear"></param>
        /// <returns></returns>
        public GameObject InstantiateObjectSync(ResFromType resType, string assetPath, Transform parTrans = null, bool bClear = true)
        {
            if (CheckNoCanLoad())
            {
                AFLogger.e("此Loader已经被释放,请注意!");
                return null;
            }
            ObjLoadInfo objLoadInfo = ObjLoadInfo.Allocate(resType, assetPath, parTrans, bClear);
            GameObject objAsset = ResManager.Instance.InstantiateObjectSync(objLoadInfo);
            if (objAsset != null)
            {
                //加载完成后添加到加载资源列表中
                mObjList.AddValue(objAsset.GetInstanceID());
            }
            objLoadInfo.Recycle2Cache();
            return objAsset;
        }
        /// <summary>
        /// 异步加载Prefab
        /// </summary>
        /// <param name="resType"></param>
        /// <param name="assetPath"></param>
        /// <param name="parTrans"></param>
        /// <param name="loadObjFinish"></param>
        /// <param name="loadResPriority"></param>
        /// <param name="bClear"></param>
        public void InstantiateObjectASync(ResFromType resType, string assetPath,System.Action<bool, ResObject> loadObjFinish,
            Transform parTrans = null, LoadResPriority loadResPriority = LoadResPriority.RES_NUM,
            bool bClear = true)
        {
            if (CheckNoCanLoad())
            {
                AFLogger.e("此Loader已经被释放,请注意!");
                return;
            }
            ObjLoadInfo objLoadInfo = ObjLoadInfo.Allocate(resType, assetPath, parTrans, bClear, loadResPriority,
                AsyncObjCallBack, loadObjFinish);
            if (mAsyncLoadingObjDic.ContainsKey(assetPath))
            {
                mAsyncLoadingObjDic[assetPath].Add(objLoadInfo);
            }
            else
            {
                AsyncCount += 1;
                mAsyncLoadingObjDic.AddValue(assetPath, objLoadInfo);
                ResManager.Instance.InstantiateObjectASync(objLoadInfo);
            }
        }
        /// <summary>
        /// 取消异步加载资源
        /// </summary>
        /// <returns></returns>
        public bool CancleLoadRes(string resPath)
        {
            return ResManager.Instance.CancelResLoad(Crc32.GetCrc32(resPath));
        }
        public void PreLoadRes<T>(ResFromType resType, string assetPath, bool isSprite = false) where T : Object
        {
            LoadSync<T>(resType, assetPath, isSprite, false);
        }

        /// <summary>
        /// 根据类型加载sprite,不是异步,不提供回调函数参数
        /// </summary>
        /// <param name="resType">从哪里加载</param>
        /// <param name="assetPath"></param>
        /// isRecycleOnDestroy : 当ResLoader被销毁时是否从内存中释放
        /// <returns></returns>
        public Sprite LoadSpriteSync(string assetPath, ResFromType resType, bool DestroyCache = false)
        {
            if (CheckNoCanLoad())
            {
                AFLogger.e("此Loader已经被释放,请注意!");
                return null;
            }
            return LoadSync<Sprite>(resType, assetPath, true, DestroyCache);
        }
        /// <summary>
        /// 同步加载资源,这里不提供进度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resType">从哪里加载</param>
        /// <param name="assetPath"></param>
        /// <param name="isSprite">是否是加载Sprite</param>
        /// isRecycleOnDestroy : 当ResLoader被销毁时是否从内存中释放
        /// <returns></returns>
        public T LoadSync<T>(ResFromType resType, string assetPath, bool isSprite = false,
            bool DestroyCache = false)
            where T : Object
        {
            if (CheckNoCanLoad())
            {
                AFLogger.e("此Loader已经被释放,请注意!");
                return null;
            }
            ResLoadInfo resLoadInfo = ResLoadInfo.Allocate(resType, assetPath, isSprite, null, null, DestroyCache);
            Object resAsset = ResManager.Instance.LoadSync(resLoadInfo);
            if (resAsset != null)
            {
                //加载完成后添加到加载资源列表中
                mResList.AddValue(resLoadInfo.mResPath);
            }
            resLoadInfo.Recycle2Cache();
            return resAsset as T;
        }
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resType"></param>
        /// <param name="assetPath"></param>
        /// <param name="loadFinish"></param>
        /// <param name="isSprite"></param>
        /// <param name="DestroyCache">当ResLoader被销毁时是否从内存中释放</param>
        /// <param name="loadResPriority">加载的优先级</param>
        public void LoadAsync<T>(ResFromType resType, string assetPath, System.Action<bool, ResInfo> loadFinish,
            bool isSprite = false, bool DestroyCache = false, LoadResPriority loadResPriority = LoadResPriority.RES_NUM)
            where T : Object
        {
            if (CheckNoCanLoad())
            {
                AFLogger.e("此Loader已经被释放,请注意!");
                return;
            }
            ResLoadInfo resLoadInfo = ResLoadInfo.Allocate(resType, assetPath, isSprite, AsyncResCallBack, loadFinish, DestroyCache);
            //添加回调函数
            if (mAsyncLoadingResDic.ContainsKey(resLoadInfo.mResPath))
            {
                mAsyncLoadingResDic[resLoadInfo.mResPath].Add(resLoadInfo);
            }
            else
            {
                AsyncCount += 1;
                mAsyncLoadingResDic.AddValue(resLoadInfo.mResPath, resLoadInfo);
                ResManager.Instance.LoadAsync(resLoadInfo);
            }
        }
        public void CancelLoadRes(ResInfo resInfo)
        {
            if (mAsyncLoadingObjDic.ContainsKey(resInfo.ResPath))
            {
                mAsyncLoadingObjDic[resInfo.ResPath].ForEach((objLoadInfo) =>
                {
                    objLoadInfo.loadObjCall.InvokeGracefully(false, null);
                });
                mAsyncLoadingObjDic.Remove(resInfo.ResPath);
            }
        }

        //异步资源加载进度
        public float AsyncProgress
        {
            get
            {
                if (mAsyncLoadingResDic.Count == 0)
                {
                    return 1;
                }
                var unit = 1.0f / AsyncCount;
                var currentValue = unit * (AsyncCount - mAsyncLoadingResDic.Count);

                currentValue += ResManager.Instance.GetListProcess(new List<string>(mAsyncLoadingResDic.Keys), unit);
                return currentValue;
            }
        }
        #endregion

        #region 生成与回收
        public ResLoader()
        {
            IsRecycled = false;
        }

        public static ResLoader Allocate()
        {
            return SafeObjectPool<ResLoader>.Instance.Allocate();
        }

        public bool IsRecycled { get; set; }

        public void OnRecycled()
        {
            ReleaseAllRes();
            IsRecycled = true;
        }

        public void Recycle2Cache()
        {
            SafeObjectPool<ResLoader>.Instance.Recycle(this);
        }
        #endregion
        //已经加载的资源的唯一标识符
        List<string> mResList = new List<string>();
        //全部异步加载的数量
        int AsyncCount = 0;
        //异步加载资源列表,计算进度可以使用
        Dictionary<string, List<ResLoadInfo>> mAsyncLoadingResDic = new Dictionary<string, List<ResLoadInfo>>();
        //实例化资源
        List<int> mObjList = new List<int>();
        //异步加载GameObject
        Dictionary<string, List<ObjLoadInfo>> mAsyncLoadingObjDic = new Dictionary<string, List<ObjLoadInfo>>();
        /// <summary>
        /// 异步加载资源统一回调
        /// </summary>
        /// <param name="LoadResult"></param>
        /// <param name="resInfo"></param>
        public void AsyncResCallBack(bool LoadResult, ResInfo resInfo)
        {
            AsyncCount -= 1;
            if (!LoadResult)
            {
                ResManager.Instance.RemoveLoadFailRes(resInfo.mCRC);
                resInfo.Recycle2Cache();
                mAsyncLoadingResDic[resInfo.ResPath].ForEach((resLoadInfo) =>
                {
                    resLoadInfo.mRealListener.InvokeGracefully(LoadResult, null);
                    resLoadInfo.Recycle2Cache();
                });
                mAsyncLoadingResDic.Remove(resInfo.ResPath);
                return;
            }
            bool DestroyCache = true;
            //异步加载回调
            mAsyncLoadingResDic[resInfo.ResPath].ForEach((resLoadInfo) =>
            {
                //但凡有一个调用此资源的说不销毁,就不销毁
                if (!resLoadInfo.DestroyCache)
                {
                    DestroyCache = false;
                }
                resInfo.Retain();
                resLoadInfo.mRealListener.InvokeGracefully(LoadResult, resInfo);
                resLoadInfo.Recycle2Cache();
            });
            mAsyncLoadingResDic.Remove(resInfo.ResPath);
            resInfo.DestroyCache = DestroyCache;
            resInfo.mGUID = resInfo.ResObject.GetInstanceID();
            mResList.AddValue(resInfo.ResPath);
        }
        public void AsyncObjCallBack(bool LoadResult, ResInfo resInfo)
        {
            if (!LoadResult)
            {
                mAsyncLoadingObjDic.Remove(resInfo.ResPath);
                ResManager.Instance.RemoveLoadFailRes(resInfo.mCRC);
                resInfo.Recycle2Cache();
                mAsyncLoadingObjDic[resInfo.ResPath].ForEach((objLoadInfo) =>
                {
                    objLoadInfo.loadObjCall.InvokeGracefully(LoadResult, null);
                    objLoadInfo.Recycle2Cache();
                });
                mAsyncLoadingObjDic.Remove(resInfo.ResPath);
                return;
            }
            bool DestroyCache = false;
            mAsyncLoadingObjDic[resInfo.ResPath].ForEach((objLoadInfo) =>
            {
                ResObject resObject = ResObject.Allocate(objLoadInfo);
                resObject.mObj = resInfo.ResObject;
                resObject.ABName = resInfo.ABName;
                resObject.mCloneObj = GameObject.Instantiate(resObject.mObj, objLoadInfo.ObjParentTrans) as GameObject;
                mObjList.AddValue(resObject.mCloneObj.GetInstanceID());
                objLoadInfo.loadObjCall.InvokeGracefully(LoadResult, resObject);
                ResManager.Instance.CacheResObj(resObject);
                if (objLoadInfo.mClear)
                {
                    DestroyCache = true;
                }
                resObject.Retain();
                resInfo.Retain();
                objLoadInfo.Recycle2Cache();
            });
            resInfo.DestroyCache = DestroyCache;
            mAsyncLoadingObjDic.Remove(resInfo.ResPath);
        }
        /// <summary>
        /// 释放单个资源
        /// </summary>
        /// <param name="AssetPath"></param>
        /// <param name="destoryObj">是否销毁缓存</param>
        public void ReleaseRes(string AssetPath, bool destoryObj = false)
        {
            if (CheckNoCanLoad())
            {
                AFLogger.e("此Loader已经被释放,请注意!");
                return;
            }
            ResManager.Instance.ReleaseResouce(AssetPath, destoryObj);
            mResList.Remove(AssetPath);
        }
        /// <summary>
        /// 释放单个Obj资源,这里不提供根据路径释放Obj,因为一个预制体可能实例化了多个,无法确认
        /// 默认是回收到recycleParent,如果想要缓存也回收,请设置destoryCache为true,recycleParent为false或者设置maxCacheCount为0
        /// </summary>
        public void ReleaseObj(GameObject obj, int maxCacheCount = -1, bool destoryCache = false, bool recycleParent = true)
        {
            if (CheckNoCanLoad())
            {
                AFLogger.e("此Loader已经被释放,请注意!");
                return;
            }
            ResManager.Instance.ReleaseObj(obj, maxCacheCount, destoryCache, recycleParent);
            mObjList.Remove(obj.GetInstanceID());
        }
        /// <summary>
        /// 释放此loader持有的所有资源
        /// </summary>
        public void ReleaseAllRes()
        {
            if (mResList.Count <= 0 && mObjList.Count <= 0)
            {
                return;
            }
            if (CheckNoCanLoad())
            {
                AFLogger.e("此Loader已经被释放,请查看此方法是否调用了多次!");
                return;
            }
            ResManager.Instance.ReleaseResLoader(mResList);
            mResList.Clear();
            ResManager.Instance.ReleaseObjLoader(mObjList);
            mObjList.Clear();
        }
        public bool CheckNoCanLoad()
        {
            return IsRecycled;
        }
        //GC释放资源
        public override void Dispose()
        {
            ReleaseAllRes();
            base.Dispose();
        }
    }
}
