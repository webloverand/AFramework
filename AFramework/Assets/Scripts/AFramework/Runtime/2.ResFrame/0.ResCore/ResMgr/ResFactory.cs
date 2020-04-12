/*******************************************************************
* Copyright(c)
* 文件名称: ResFactory.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/

namespace AFramework
{
    public class ResFactory
    {
        public static ResInfo Create(ResLoadInfo resLoadInfo)
        {
            switch(resLoadInfo.mResFromType)
            {
                case ResFromType.ResourcesRes:
                    return ResourcesRes.Allocate(resLoadInfo);
                case ResFromType.NetImageRes:
                    return NetImageRes.Allocate(resLoadInfo);
                case ResFromType.ABRes:
                    if(ABDataHolder.Instance.GetABResLoadFrom() == ABResLoadFrom.EditorRes)
#if UNITY_EDITOR
                        return EditorRes.Allocate(resLoadInfo);
#else
                    return null;
#endif
                    else
                        return ABRes.Allocate(resLoadInfo);
                case ResFromType.EditorRes:
#if UNITY_EDITOR
                    return EditorRes.Allocate(resLoadInfo);
#else
                    return null;
#endif
            }
            return null;
        }
    }
}
