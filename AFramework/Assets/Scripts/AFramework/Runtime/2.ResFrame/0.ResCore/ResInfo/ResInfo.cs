namespace AFramework
{
    using System;
    using System.Collections;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class ResInfo : SimpleRC, IPoolable
    {
        protected Object mAsset;
        protected string mAssetStr;
        protected string mResPath;
        //资源的AB包信息
        public ResItem resItem;
        protected ResState mResState = ResState.Waiting;

        //资源最后所使用的时间
        public float m_LastUseTime = 0.0f;

        protected LoadResPriority mLoadRePriority;
        //释放资源时是否销毁内存
        public bool DestroyCache;
        //是否加载Sprite
        public bool isSprite = false;
        //加载资源的CRC
        public uint mCRC
        {
            get
            {
                return resItem.mCrc;
            }
        }
        public string ResPath
        {
            get
            {
                return mResPath;
            }
        }
        public string ABName
        {
            get
            {
                return resItem.ABName;
            }
        }
        public float Process
        {
            get
            {
                switch (mResState)
                {
                    case ResState.Ready:
                        return 1;
                    case ResState.Loading:
                        return CalculateProcess();
                }
                return 0;
            }
        }
        public LoadResPriority ResPriority
        {
            get
            {
                return mLoadRePriority;
            }
        }
        //获取加载的资源
        public Object ResObject { get { return mAsset; } }
        //获取加载的资源
        public string ResStr { get { return mAssetStr; } }
        //资源唯一标识
        public long mGUID = 0;
        //资源加载的状态
        public ResState State
        {
            get { return mResState; }
        }
        //是否回收
        public bool IsRecycled { get; set; }
        //回收资源
        public bool ReleaseRes()
        {
            if (mResState != ResState.Ready)
            {
                AFLogger.i("Release Res失败:" + mResPath + " " + mResState);
                return false;
            }
            if (RefCount <= 0)
            {
                AFLogger.i("Release Res:" + mResPath);
                OnReleaseRes();
                return true;
            }
            else
            {
                AFLogger.i("Release Res:" + RefCount);
                return false;
            }
        }
        //引用计数为0时调用
        protected override void OnZeroRef()
        {

        }
        //获取依赖的资源
        public virtual string[] GetDependResList()
        {
            return null;
        }
        //依赖资源是否加载完成
        public bool IsDependResLoadFinish()
        {
            return true;
        }
        public ResInfo()
        {
            IsRecycled = false;
        }

        #region 子类实现
        public Action<bool, ResInfo> mListener;
        public void RegisterListen(Action<bool, ResInfo> action)
        {
            mListener += action;
        }
        public virtual bool LoadSync()
        {
            return false;
        }
        public virtual bool LoadAsync()
        {
            if (!CheckLoadAble())
            {
                AFLogger.e("资源不是等待下载状态:" + mResState);
                return false;
            }
            if (mResPath.IsNullOrEmpty())
            {
                AFLogger.e("加载路径不能为空");
                return false;
            }
            mResState = ResState.Loading;
            ResourceLoader.Instance.PushIEnumeratorTask(this);
            return true;
        }

        public virtual IEnumerator DoLoadAsync(Action<uint> finishCallback)
        {
            finishCallback(mCRC);
            yield break;
        }
        //释放资源调用
        protected virtual void OnReleaseRes()
        {
            //如果Image 直接释放了，这里会直接变成NULL
            if (mAsset != null)
            {
                if (mAsset is GameObject)
                {

                }
                else
                {
                    Resources.UnloadAsset(mAsset);
                }

                mAsset = null;
            }
        }
        //回收资源时调用
        public virtual void OnRecycled()
        {
            mResState = ResState.Waiting;
            IsRecycled = true;
            mListener = null;
        }
        public virtual void Recycle2Cache()
        {
            SafeObjectPool<ResInfo>.Instance.Recycle(this);
        }
        protected virtual bool CheckLoadAble()
        {
            return mResState == ResState.Waiting;
        }
        public virtual float CalculateProcess()
        {
            return 0;
        }
        public virtual void Init(ResLoadInfo resLoadInfo)
        {
            mResPath = resLoadInfo.mResPath;
            mLoadRePriority = resLoadInfo.loadResPriority;
            DestroyCache = resLoadInfo.DestroyCache;
            isSprite = resLoadInfo.mIsSprite;
            resItem = new ResItem();
            resItem.mCrc = Crc32.GetCrc32(mResPath);
            mListener = resLoadInfo.mListener;
            mResState = ResState.Waiting;
        }
        public virtual void OnResLoadFaild()
        {
            mListener.InvokeGracefully(false, this);
            mListener = null;
        }
        public virtual void CancelLoad()
        {
            mListener.InvokeGracefully(false, this);
            mResState = ResState.Waiting;
        }
        #endregion
    }
}
