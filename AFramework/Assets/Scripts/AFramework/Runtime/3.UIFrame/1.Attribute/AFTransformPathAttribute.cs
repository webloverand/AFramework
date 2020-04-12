/*******************************************************************
* Copyright(c)
* 文件名称: AFTransformPathAttribute.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System;
    public class AFTransformPathAttribute : Attribute
    {
        public string TransPath;
        public AFTransformPathAttribute(string path)
        {
            TransPath = path;
        }
    }
}
