/*******************************************************************
* Copyright(c)
* 文件名称: EditorABRes.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections;
    using UnityEngine;
    /// <summary>
    /// 注意编辑器直接加载资源时并不提供进度
    /// </summary>
    public class EditorRes : ResInfo
    {
#if UNITY_EDITOR
        float ProcessEditor = 0;
        public static EditorRes Allocate(ResLoadInfo resLoadInfo)
        {
            EditorRes res = SafeObjectPool<EditorRes>.Instance.Allocate();
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
                return false;
            }

            mResState = ResState.Loading;
            if (isSprite)
            {
                mAsset = LoadAssetByEditor<Sprite>(mResPath);
            }
            else
            {
                mAsset = LoadAssetByEditor<Object>(mResPath);
            }
            if (mAsset.IsNull())
            {
                AFLogger.e("加载Editor Res资源为null,请检查路径:" + mResPath);
                mResState = ResState.Waiting;
            }
            else
            {
                mResState = ResState.Ready;
            }
            return mAsset != null;
        }
        public override IEnumerator DoLoadAsync(System.Action<uint> finishCallback)
        {
            if (mResPath.IsNullOrEmpty())
            {
                AFLogger.e("加载路径不能为空");
            }
            ProcessEditor = 0;
            if (isSprite)
            {
                mAsset = LoadAssetByEditor<Sprite>(mResPath);
            }
            else
            {
                mAsset = LoadAssetByEditor<Object>(mResPath);
            }
            if (mAsset.IsNull())
            {
                mResState = ResState.Waiting;
                AFLogger.e("加载Editor ABRes资源为null,请检查路径:" + mResPath);
                mListener.InvokeGracefully(false, this);
                finishCallback(mCRC);
                yield break;
            }
            else
            {
                mResState = ResState.Ready;
            }
            yield return new WaitForSeconds(0.5f); //只是用来模拟异步加载
            ProcessEditor = 1;
            mListener.InvokeGracefully(true, this);
            finishCallback(mCRC);
        }
        protected T LoadAssetByEditor<T>(string path) where T : Object
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        }
        public override float CalculateProcess()
        {
            return ProcessEditor;
        }
#endif
    }
}
