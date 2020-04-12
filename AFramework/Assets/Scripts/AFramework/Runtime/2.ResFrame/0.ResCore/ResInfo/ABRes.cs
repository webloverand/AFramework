/*******************************************************************
* Copyright(c)
* 文件名称: StreamAssetAB.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System;
    using System.Collections;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class ABRes : ResInfo
    {
        AssetBundleRequest ABRequest;
        public static ABRes Allocate(ResLoadInfo resLoadInfo)
        {
            ABRes res = SafeObjectPool<ABRes>.Instance.Allocate();
            if (res != null)
            {
                res.Init(resLoadInfo);
            }
            return res;
        }
        public override bool LoadSync()
        {
            if (mResPath.IsNullOrEmpty())
            {
                AFLogger.e("加载路径不能为空");
                return false;
            }
            mResState = ResState.Loading;
            uint crc = Crc32.GetCrc32(mResPath);
            ABResItem ABResitem = ResManager.Instance.LoadAssetBundle(crc);
            if(ABResitem.assetBundle == null)
            {
                AFLogger.e("加载的AssetBundle为空:"  + ResPath);
                return false;
            }
            resItem.Copy(ABResitem);
            if (isSprite)
                mAsset = ABResitem.assetBundle.LoadAsset<Sprite>(ResPath);
            else
                mAsset = ABResitem.assetBundle.LoadAsset<Object>(ResPath);
            if (mAsset.IsNull())
            {
                AFLogger.e("加载AB包资源为null,请检查路径:" + mResPath);
                mResState = ResState.Waiting;
            }
            else
            {
                mResState = ResState.Ready;
            }
            return mAsset != null;
        }
        public override IEnumerator DoLoadAsync(Action<uint> finishCallback)
        {
            if (mResPath.IsNullOrEmpty())
            {
                AFLogger.e("加载路径不能为空");
                yield break;
            }
            mResState = ResState.Loading;
            uint crc = Crc32.GetCrc32(mResPath);
            ABResItem ABResitem = ResManager.Instance.LoadAssetBundle(crc);
            if (ABResitem.assetBundle == null)
            {
                AFLogger.e("加载的AssetBundle为空:" + ResPath);
                yield break;
            }
            resItem.Copy(ABResitem);
            if (isSprite)
                ABRequest = ABResitem.assetBundle.LoadAssetAsync<Sprite>(ResPath);
            else
                ABRequest = ABResitem.assetBundle.LoadAssetAsync<Object>(ResPath);
            yield return ABRequest;
            if (!ABRequest.isDone)
            {
                AFLogger.e("Failed to Load Resources:" + mResPath);
                OnResLoadFaild();
                mListener.InvokeGracefully(false, this);
                finishCallback(mCRC);
                yield break;
            }

            mAsset = ABRequest.asset;
            mResState = ResState.Ready;
            mListener.InvokeGracefully(true, this);
            finishCallback(mCRC);
        }
        public override float CalculateProcess()
        {
            if(ABRequest != null)
            {
                return 0;
            }
            return ABRequest.progress;
        }
    }
}
