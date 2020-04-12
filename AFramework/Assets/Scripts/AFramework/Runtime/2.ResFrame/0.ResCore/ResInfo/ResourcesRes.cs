/*******************************************************************
* Copyright(c)
* 文件名称: InternalRes.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections;
    using UnityEngine;
    public class ResourcesRes : ResInfo
    {
        //用来计算进度的
        private ResourceRequest mResourceRequest;
        public static ResourcesRes Allocate(ResLoadInfo resLoadInfo)
        {
            ResourcesRes res = SafeObjectPool<ResourcesRes>.Instance.Allocate();
            if (res != null)
            {
                res.Init(resLoadInfo);
            }
            return res;
        }
        //同步加载资源
        public override bool LoadSync()
        {
            if (mResPath.IsNullOrEmpty())
            {
                AFLogger.e("加载路径不能为空");
            }
            mResState = ResState.Loading;
            if(isSprite)
            {
                mAsset = Resources.Load(mResPath, typeof(Sprite));
            }
            else
            {
                mAsset = Resources.Load(mResPath);
            }
            if (mAsset.IsNull())
            {
                AFLogger.e("加载Resources资源为null,请检查路径:" + mResPath);
                mResState = ResState.Waiting;
            }
            else
            {
                mResState = ResState.Ready;
            }
            return mAsset != null;
        }
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        public override IEnumerator DoLoadAsync(System.Action<uint> finishCallback)
        {
            ResourceRequest resourcesRequest;
            if(isSprite)
            {
                resourcesRequest = Resources.LoadAsync<Sprite>(mResPath);
            }
            else
            {
                resourcesRequest = Resources.LoadAsync(mResPath);
            }
            mResourceRequest = resourcesRequest;
            yield return resourcesRequest;
            if (!resourcesRequest.isDone)
            {
                AFLogger.e("Failed to Load Resources:" + mResPath);
                OnResLoadFaild();
                mListener.InvokeGracefully(false, this);
                finishCallback(mCRC);
                yield break;
            }

            mAsset = resourcesRequest.asset;
            mResState = ResState.Ready;
            mListener.InvokeGracefully(true, this);
            finishCallback(mCRC);
        }

        public override void Recycle2Cache()
        {
            SafeObjectPool<ResourcesRes>.Instance.Recycle(this);
        }
        public override void OnRecycled()
        {
            IsRecycled = true;
        }
        public override float CalculateProcess()
        {
            if(mResourceRequest == null)
            {
                return 0;
            }
            return mResourceRequest.progress;
        }
    }
}
