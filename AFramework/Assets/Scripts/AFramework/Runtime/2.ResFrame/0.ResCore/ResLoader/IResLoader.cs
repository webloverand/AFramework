/*******************************************************************
* Copyright(c)
* 文件名称: IResLoader.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/


namespace AFramework
{
    using System;
    public interface IResLoader :  IPoolable,IPoolType
    {
        T LoadSync<T>(ResFromType resType, string assetPath, bool isSprite = false, bool DestroyCache = false) where T : UnityEngine.Object;
     
        void LoadAsync<T>(ResFromType resType, string assetPath, System.Action<bool, ResInfo> loadFinish,
        bool isSprite = false, bool DestroyCache = false, LoadResPriority loadResPriority = LoadResPriority.RES_NUM)
            where T : UnityEngine.Object;

        void ReleaseAllRes();
    }
}
