/*******************************************************************
* Copyright(c)
* 文件名称: ResManager.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections.Generic;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [MonoSingletonPath("[AFramework]/ResManager")]
    public class ResManager : MonoSingletonWithNewObject<ResManager>
    {
        static bool mResManagerInited = false;
        public static void Init()
        {
            if (mResManagerInited) return;
            mResManagerInited = true;

            SafeObjectPool<ResLoader>.Instance.Init(40, 20);
            SafeObjectPool<ResLoadInfo>.Instance.Init(40, 20);
            SafeObjectPool<ObjLoadInfo>.Instance.Init(40, 20);

            SafeObjectPool<ResourcesRes>.Instance.Init(10, 5);
            SafeObjectPool<NetImageRes>.Instance.Init(5, 1);
            SafeObjectPool<ResObject>.Instance.Init(200, 40);
            SafeObjectPool<AssetBundleItem>.Instance.Init(200, 100);
        }
        private void Start()
        {
            recycleTrs = GameObject.Find("RecyclePoolTrs").transform;
            recycleTrs.gameObject.SetActive(false);
        }
        public Transform recycleTrs;
        //缓存引用计数为零的资源列表，达到缓存最大的时候释放这个列表里面最早没用的资源，这样是为了提高效率，从磁盘加载容易出现卡顿
        protected CMapList<ResInfo> m_NoRefrenceAssetMapList = new CMapList<ResInfo>();
        //已经加载的资源以及等待加载的资源
        public Dictionary<uint, ResInfo> mResDictionary = new Dictionary<uint, ResInfo>();

        //资源关系依赖配表，可以根据crc来找到对应资源块 CRC对应的中间类
        protected Dictionary<uint, ResItem> mResConfigItem = new Dictionary<uint, ResItem>();
        //储存已加载的AB包，key为AB包名的crc 对象池
        protected Dictionary<uint, AssetBundleItem> mAssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();

        //InstanceID 实例化的Gameobject
        protected Dictionary<int, ResObject> mResouceObjDic = new Dictionary<int, ResObject>();
        //对象池,同一个物体可能创建多次,因此value是List列表 这里保存实例化的GameObject : 而Recycle2Cache会将GameObject以及加载的Prefab内存释放
        protected Dictionary<uint, List<ResObject>> mObjectPoolDic = new Dictionary<uint, List<ResObject>>();

        
        #region 加载资源
        public GameObject PreLoadObj(ObjLoadInfo objLoadInfo)
        {
            objLoadInfo.ObjParentTrans = recycleTrs;
            GameObject obj = ObjectLoader.InstantiateObjectSync(objLoadInfo);
            ReleaseObj(obj,-1, objLoadInfo.mClear, recycleTrs);
            return obj;
        }
        public Object LoadSync(ResLoadInfo resLoadInfo)
        {
            return ResourceLoader.Instance.LoadSync(resLoadInfo);
        }
        public void LoadAsync(ResLoadInfo resLoadInfo)
        {
            ResourceLoader.Instance.LoadAsync(resLoadInfo);
        }
        public GameObject InstantiateObjectSync(ObjLoadInfo objLoadInfo)
        {
            return ObjectLoader.InstantiateObjectSync(objLoadInfo);
        }
        public void InstantiateObjectASync(ObjLoadInfo objLoadInfo)
        {
            ObjectLoader.InstantiateObjectASync(objLoadInfo);
        }
        /// <summary>
        /// 取消异步加载资源
        /// </summary>
        /// <returns></returns>
        public bool CancelResLoad(uint crc)
        {
            ResInfo resInfo = GetRes(crc);
            if(resInfo != null)
            {
                if(resInfo.State != ResState.Ready)
                {
                    ResourceLoader.Instance.CancelLoad(resInfo);
                    resInfo.CancelLoad();
                }
            }
            return true;
        }
        /// <summary>
        /// 加载AB包
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        public ABResItem LoadAssetBundle(uint crc)
        {
            ABResItem ABResitem = new ABResItem();
            ResItem resItem = null;
            if (!mResConfigItem.TryGetValue(crc, out resItem) || resItem == null)
            {
                AFLogger.e(string.Format("LoadResourceAssetBundle error: can not find crc {0} in AssetBundleConfig", crc.ToString()));
                return null;
            }
            ABResitem.assetBundle = LoadAssetBundle(resItem.ABName);
            if (resItem.DependAssetBundle != null)
            {
                for (int i = 0; i < resItem.DependAssetBundle.Count; i++)
                {
                    LoadAssetBundle(resItem.DependAssetBundle[i]);
                }
            }
            ABResitem.mResItem = resItem;
            return ABResitem;
        }
        private AssetBundle LoadAssetBundle(string ABName)
        {
            AssetBundleItem item = null;
            uint crc = Crc32.GetCrc32(ABName);
            if (mAssetBundleItemDic.TryGetValue(crc, out item) && item != null)
            {
                item.Retain();
            }
            else
            {
                AssetBundle assetBundle = null;
                string fullPath = ABDataHolder.Instance.GetABPrefix() + ABName;
                if (ABDataHolder.Instance.JudgeCanLoadAB(fullPath) )
                {
                    assetBundle = AssetBundle.LoadFromFile(fullPath);
                }
                else
                {
                    AFLogger.e("不存在AB包路径 :" + ABName + " " + fullPath);
                }

                if (assetBundle == null)
                {
                    AFLogger.e(" Load AssetBundle Error:" + ABName + " " + fullPath);
                }

                item = SafeObjectPool<AssetBundleItem>.Instance.Allocate();
                item.assetBundle = assetBundle;
                item.Retain();
                mAssetBundleItemDic.Add(crc, item);
            }
            return item.assetBundle;
        }
        public byte[] LoadRes(string path)
        {
            return FileHelper.ReadByteArray(path);
        }
        public string LoadStr(string path)
        {
            return FileHelper.ReadTxtToStr(path);
        }
        public Texture2D LoadTexture(string path,int width,int height)
        {
            return FileHelper.ReadToTexture2D(path,width,height);
        }
        public Sprite LoadSprite(string path, int width, int height)
        {
            return FileHelper.ReadToSprite(path, width, height);
        }
        #endregion
        #region 资源DIC
        /// <summary>
        /// 获取已经加载的资源
        /// </summary>
        /// <param name="resLoadInfo"></param>
        /// <returns></returns>
        public ResInfo GetRes(ResLoadInfo resLoadInfo)
        {
            ResInfo resInfo = null;
            mResDictionary.TryGetValue(resLoadInfo.mCRC, out resInfo);
            return resInfo;
        }
        public ResInfo GetRes(uint crc)
        {
            ResInfo resInfo = null;
            mResDictionary.TryGetValue(crc, out resInfo);
            return resInfo;
        }
        /// <summary>
        /// 缓存加载好的资源
        /// </summary>
        /// <param name="ResPath"></param>
        /// <param name="resInfo"></param>
        public void CacheResource(string ResPath, ResInfo resInfo)
        {
            //缓存太多，清除最早没有使用的资源
            WashOut();
            mResDictionary.AddValue<uint, ResInfo>(Crc32.GetCrc32(ResPath), resInfo);
        }
        public void CacheResource(uint CRC, ResInfo resInfo)
        {
            //缓存太多，清除最早没有使用的资源
            WashOut();
            resInfo.mGUID = resInfo.ResObject.GetInstanceID();
            mResDictionary.AddValue<uint, ResInfo>(CRC, resInfo);
        }

        /// <summary>
        /// 移除加载失败的资源
        /// </summary>
        public void RemoveLoadFailRes(uint CRC)
        {
            if (mResDictionary.ContainsKey(CRC))
            {
                mResDictionary[CRC].Recycle2Cache();
                mResDictionary.Remove(CRC);
            }
        }
        /// <summary>
        /// 获取进度
        /// </summary>
        /// <param name="list"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public float GetListProcess(List<string> list, float unit)
        {
            float process = 0;
            list.ForEach((str) =>
            {
                process += mResDictionary[Crc32.GetCrc32(str)].Process;
            });
            return process * unit;
        }
        #endregion
        #region Object/AB Dic
        /// <summary>
        /// 根据crc查找ResItem
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        public ResItem FindResourceItme(uint crc)
        {
            ResItem item = null;
            mResConfigItem.TryGetValue(crc, out item);
            return item;
        }
        public void CacheResObj( ResObject resObject)
        {
            mResouceObjDic.AddValue<int, ResObject>(resObject.mCloneObj.GetInstanceID(),resObject);
        }
        public void CacheAssetbundle(uint crc,AssetBundleItem assetBundleItem)
        {
            if(mAssetBundleItemDic.ContainsKey(crc))
            {
                mAssetBundleItemDic[crc].Retain();
            }
            else
            {
                mAssetBundleItemDic.Add(crc, assetBundleItem);
            }
        }
        public void CacheABConfig(AssetBundleConfig config)
        {
            mResConfigItem.Clear();
            for (int i = 0; i < config.ABList.Count; i++)
            {
                ABBase abBase = config.ABList[i];
                //使用中间类进行保存
                ResItem item = new ResItem();
                item.mCrc = abBase.Crc;
                item.mResPath = abBase.Path;
                item.ABName = abBase.ABName;
                item.DependAssetBundle = abBase.ABDependce;
                if (mResConfigItem.ContainsKey(item.mCrc))
                {
                    AFLogger.e("重复的Crc:" + item.mCrc + " ab包名：" + item.ABName);
                }
                else
                {
                    mResConfigItem.Add(item.mCrc, item);
                }
            }
        }
        /// <summary>
        /// 清空对象池
        /// </summary>
        public void ClearCache()
        {
            List<uint> tempList = new List<uint>();
            foreach (uint key in mObjectPoolDic.Keys)
            {
                List<ResObject> st = mObjectPoolDic[key];
                for (int i = st.Count - 1; i >= 0; i--)
                {
                    ResObject resObj = st[i];
                    if (!System.Object.ReferenceEquals(resObj.mCloneObj, null) && resObj.mClear)
                    {
                        GameObject.Destroy(resObj.mCloneObj);
                        mResouceObjDic.Remove(resObj.mCloneObj.GetInstanceID());
                        resObj.Recycle2Cache();
                        st.Remove(resObj);
                    }
                }

                if (st.Count <= 0)
                {
                    tempList.Add(key);
                }
            }

            for (int i = 0; i < tempList.Count; i++)
            {
                uint temp = tempList[i];
                if (mObjectPoolDic.ContainsKey(temp))
                {
                    mObjectPoolDic.Remove(temp);
                }
            }
            tempList.Clear();
        }
        /// <summary>
        /// 从对象池取对象
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        public ResObject GetObjectFromPool(uint crc)
        {
            List<ResObject> st = null;
            if (mObjectPoolDic.TryGetValue(crc, out st) && st != null && st.Count > 0)
            {
                ResObject resObj = st[0];
                resObj.Retain();
                st.RemoveAt(0);
                GameObject obj = resObj.mCloneObj;
                if (!System.Object.ReferenceEquals(obj, null))
                {
                    resObj.IsRecyclePool = false;

#if UNITY_EDITOR
                    if (obj.name.EndsWith("(Recycle)"))
                    {
                        obj.name = obj.name.Replace("(Recycle)", "");
                    }
#endif
                }
                return resObj;
            }
            return null;
        }
        /// <summary>
        /// 清除某个资源在对象池中所有的对象
        /// </summary>
        /// <param name="crc"></param>
        public void ClearPoolObject(uint crc)
        {
            List<ResObject> st = null;
            if (!mObjectPoolDic.TryGetValue(crc, out st) || st == null)
                return;

            for (int i = st.Count - 1; i >= 0; i--)
            {
                ResObject resObj = st[i];
                if (resObj.mClear)
                {
                    st.Remove(resObj);
                    int tempID = resObj.mCloneObj.GetInstanceID();
                    GameObject.Destroy(resObj.mCloneObj);
                    resObj.Recycle2Cache();
                    mResouceObjDic.Remove(tempID);
                    SafeObjectPool<ResObject>.Instance.Recycle(resObj);
                }
            }

            if (st.Count <= 0)
            {
                mObjectPoolDic.Remove(crc);
            }
        }

        #endregion
        #region 资源清理
        public void ReleaseResLoader(List<string> resI)
        {
            AFLogger.d("ReleaseRes:" + resI.Count);
            for (int i = resI.Count - 1; i >= 0; i--)
            {
                //减少引用计数,当引用计数为0时才能释放ResInfo
                ReleaseResouce(resI[i]);
            }
        }
        public void ReleaseObjLoader(List<int> resI)
        {
            AFLogger.d("ReleaseRes:" + resI.Count);
            for (int i = resI.Count - 1; i >= 0; i--)
            {
                //确定是ObjectLoader创建的
                if (mResouceObjDic.ContainsKey(resI[i]))
                {
                    ReleaseObj(resI[i],0,destoryCache: mResouceObjDic[resI[i]].mClear);
                }
                else
                {
                    AFLogger.e("对象不是ObjectLoader创建的!");
                }
            }
        }
        public void ReleaseObj(GameObject obj, int maxCacheCount = -1, bool destoryCache = false, bool recycleParent = true)
        {
            ReleaseObj(obj.GetInstanceID(), maxCacheCount, destoryCache, recycleParent);
        }
        public void ReleaseObj(int InstanceID, int maxCacheCount = -1, bool destoryCache = false, bool recycleParent = true)
        {
            ResObject resObject = mResouceObjDic[InstanceID];
            if (mResouceObjDic[InstanceID].IsRecycled)
            {
                AFLogger.e(resObject.mCloneObj.name + "对象已经回收!");
                return;
            }
            if (mResouceObjDic[InstanceID].IsRecyclePool)
            {
                AFLogger.e(resObject.mCloneObj.name + "对象已经放回对象池!");
                return;
            }
#if UNITY_EDITOR
            resObject.mCloneObj.name += "(Recycle)";
#endif
            //最大缓存数量为0 : 则销毁Gameobject并回收resObject
            if (maxCacheCount == 0)
            {
                ReleaseResouce(resObject.mResPath, destoryCache);
                DestroyImmediate(resObject.mCloneObj);
                resObject.Recycle2Cache();
            }
            //回收到对象池
            else
            {
                List<ResObject> st = null;
                if (!mObjectPoolDic.TryGetValue(resObject.mCrc, out st) || st == null)
                {
                    st = new List<ResObject>();
                    mObjectPoolDic.Add(resObject.mCrc, st);
                }
                //回收到recycleTrs下
                if (resObject.mCloneObj)
                {
                    if (recycleParent)
                    {
                        resObject.mCloneObj.transform.SetParent(recycleTrs);
                    }
                    else
                    {
                        resObject.mCloneObj.SetActive(false);
                    }
                }
                //是否缓存到最大值,-1是无限制缓存
                if (maxCacheCount < 0 || st.Count < maxCacheCount)
                {
                    st.Add(resObject);
                    resObject.IsRecyclePool = true;
                    //ResourceManager做一个引用计数
                    resObject.Release();
                }
                else
                {
                    ReleaseResouce(resObject.mResPath, destoryCache);
                    DestroyImmediate(resObject.mCloneObj);
                    resObject.Recycle2Cache();
                }
            }
            mResouceObjDic.Remove(InstanceID);
        }
        /// <summary>
        /// 根据ResInfo卸载资源
        /// </summary>
        /// <param name="resInfo"></param>
        /// <param name="destoryObj"></param>
        /// <returns></returns>
        public bool ReleaseResouce(ResInfo resInfo, bool destoryObj = false)
        {
            if (resInfo == null)
                return false;

            ResInfo item = null;
            if (!mResDictionary.TryGetValue(resInfo.mCRC, out item) || null == item)
            {
                AFLogger.e("mResDictionary里不存在改资源：" + resInfo.ResObject.name + "  可能释放了多次");
                return false;
            }
            DestoryResouceItme(item, destoryObj);
            return true;
        }
        public bool ReleaseResouce(string ResPath)
        {
            ResInfo item = null;
            if (!mResDictionary.TryGetValue(Crc32.GetCrc32(ResPath), out item) || null == item)
            {
                AFLogger.e("mResDictionary里不存在改资源：" + ResPath + "  可能释放了多次");
            }
            DestoryResouceItme(item, item.DestroyCache);
            return true;
        }
        /// <summary>
        /// 不需要实例化的资源卸载，根据路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="destoryObj"></param>
        /// <returns></returns>
        public bool ReleaseResouce(string path, bool destoryObj)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            ResInfo item = null;
            if (!mResDictionary.TryGetValue(Crc32.GetCrc32(path), out item) || null == item)
            {
                AFLogger.e("mResDictionary里不存在改资源：" + path + "  可能释放了多次");
                return false;
            }
            DestoryResouceItme(item, destoryObj);
            return true;
        }
        /// <summary>
        /// 不需要实例化的资源的卸载，根据对象(比如Sprite,Texture)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="destoryObj"></param>
        /// <returns></returns>
        public bool ReleaseResouce(Object obj, bool destoryObj = false)
        {
            if (obj == null)
            {
                return false;
            }
            //根据GUID查找资源
            ResInfo item = null;
            foreach (ResInfo res in mResDictionary.Values)
            {
                if (res.mGUID == obj.GetInstanceID())
                {
                    item = res;
                }
            }

            if (item == null)
            {
                AFLogger.e("mResDictionary里不存在改资源：" + obj.name + "  可能释放了多次");
                return false;
            }
            //destoryObj为false : 不释放destoryObj,只是将item放在前面去
            DestoryResouceItme(item, destoryObj);
            return true;
        }
        /// <summary>
        ///  回收一个资源
        /// </summary>
        /// <param name="item"></param>
        /// <param name="destroyCache"></param>
        protected void DestoryResouceItme(ResInfo item, bool destroyCache = false)
        {
            if (item == null)
            {
                AFLogger.e("DestoryResouceItme:要释放的资源为空");
                return;
            }
            if(item.IsRecycled)
            {
                AFLogger.e("资源已经被回收,请检查代码是否回收了多次:" + item.ResPath);
                mResDictionary.Remove(item.mCRC);
                return;
            }
            if(item.RefCount <= 0)
            {
                AFLogger.e("资源引用计数<=0:" + item.ResPath);
                return;
            }
            //释放减少引用计数
            item.Release();
            //如果缓存下来,移到表头
            if (!destroyCache)
            {
                m_NoRefrenceAssetMapList.InsertToHead(item);
                return;
            }
            //不缓存,要根据引用计数判断是否释放内存
            if (item.ReleaseRes())
            {
                AFLogger.i("释放资源成功:" + item.ResPath);
                //资源缓存 
                if (!mResDictionary.Remove(item.mCRC))
                {
                    return;
                }

                m_NoRefrenceAssetMapList.Remove(item);
                //释放assetbundle引用
                ReleaseAssetBundle(item);
                item.Recycle2Cache();
                //在编辑器中加载，需要如此才能清除内存，因此在程序退出时要调用此方法清除内存
#if UNITY_EDITOR
                Resources.UnloadUnusedAssets();
#endif
            }
            //else
            //{
            //    AF_Logger.Info("释放资源失败(引用计数不为0或者并没有加载完成):" + item.ResPath);
            //}
        }
        //最大缓存个数
        private const int MAXCACHECOUNT = 500;
        /// <summary>
        /// 缓存太多，清除最早没有使用的资源
        /// </summary>
        protected void WashOut()
        {
            //当大于缓存个数时，进行一半释放
            while (m_NoRefrenceAssetMapList.Size() >= MAXCACHECOUNT)
            {
                for (int i = 0; i < MAXCACHECOUNT / 2; i++)
                {
                    ResInfo item = m_NoRefrenceAssetMapList.Back();
                    DestoryResouceItme(item, true);
                }
            }
        }
        /// <summary>
        /// 移除不用的资源
        /// </summary>
        public void RemoveUnusedRes()
        {
            List<uint> keys = new List<uint>(mResDictionary.Keys);
            for (var i = keys.Count - 1; i >= 0; --i)
            {
                var res = mResDictionary[keys[i]];
                if (res.RefCount <= 0 && res.State != ResState.Loading)
                {
                    if (res.ReleaseRes())
                    {
                        mResDictionary.Remove(keys[i]);
                        res.Recycle2Cache();
                    }
                }
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="item"></param>
        public void ReleaseAssetBundle(ResInfo item)
        {
            if (item == null)
            {
                return;
            }
            if(item.resItem == null)
            {
                return;
            }
            //先卸载依赖项再卸载自己
            if (item.resItem.DependAssetBundle != null && item.resItem.DependAssetBundle.Count > 0)
            {
                for (int i = 0; i < item.resItem.DependAssetBundle.Count; i++)
                {
                    UnLoadAssetBundle(item.resItem.DependAssetBundle[i]);
                }
            }
            UnLoadAssetBundle(item.ABName);
        }

        private void UnLoadAssetBundle(string ABName)
        {
            AssetBundleItem item = null;
            uint crc = Crc32.GetCrc32(ABName);
            if (mAssetBundleItemDic.TryGetValue(crc, out item) && item != null)
            {
                item.Release();
                if (item.RefCount <= 0 && item.assetBundle != null)
                {
                    item.assetBundle.Unload(true);
                    item.Recycle2Cache();
                    SafeObjectPool<AssetBundleItem>.Instance.Recycle(item);
                    mAssetBundleItemDic.Remove(crc);
                }
            }
        }
        #endregion

    }
}
