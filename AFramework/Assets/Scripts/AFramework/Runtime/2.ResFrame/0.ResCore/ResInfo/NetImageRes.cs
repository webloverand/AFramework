/*******************************************************************
* Copyright(c)
* 文件名称: NetImageRes.cs
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

    public class NetImageRes : ResInfo
    {
        HttpNetTextureTool httpNetTextureTool;

        public static NetImageRes Allocate(ResLoadInfo resLoadInfo)
        {
            NetImageRes res = SafeObjectPool<NetImageRes>.Instance.Allocate();
            if (res != null)
            {
                res.Init(resLoadInfo);
            }
            return res;
        }
        //加载网络图片是异步的,不提供同步方法
        public override bool LoadSync()
        {
            AFLogger.e("加载网络图片不提供同步加载方法,因为网络请求本身就是异步的");
            return false;
        }
        Action<uint> FinishEvent;
        public override IEnumerator DoLoadAsync(Action<uint> finishCallback)
        {
            FinishEvent = finishCallback;
            httpNetTextureTool = new HttpNetTextureTool();
            yield return httpNetTextureTool.RealWebRequest(mResPath, NetTextureEvent);
        }
        public void NetTextureEvent(Texture2D texture2D, string savePath, DownStatus downResult, string downError)
        {
            if(downResult == DownStatus.Sucess)
            {
                mResState = ResState.Ready;
                mAsset = CommonTool.TextureToSprite(texture2D) as Object;
                mListener.InvokeGracefully(true, this);
            }
            else
            {
                mResState = ResState.Waiting;
                mListener.InvokeGracefully(false,this);
            }
            FinishEvent.InvokeGracefully(mCRC);
        }
        public override float CalculateProcess()
        {
            if (httpNetTextureTool == null)
                return 0;
            return httpNetTextureTool.Process;
        }
        protected override void OnReleaseRes()
        {
            if(mAsset != null)
            {
                GameObject.Destroy(mAsset);
                mAsset = null;
            }
        }
        public override void OnRecycled()
        {
            IsRecycled = true;
            httpNetTextureTool = null;
        }
    }
}
