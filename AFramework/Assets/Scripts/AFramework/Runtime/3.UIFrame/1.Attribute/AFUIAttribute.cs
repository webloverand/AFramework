/*******************************************************************
* Copyright(c)
* 文件名称: AFUIAttribute.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System;
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class AFUIAttribute : ManagerAtrribute
    {
        //UI路径
        string path;
        bool isNeedLanuageSuffix = false;
        string UIpath = "";

        public string UIPath
        {
            get
            {
                if(UIpath == "")
                {
                    UpdateAtrribute();
                }
                return UIpath;
            }
        }

        public AFUIAttribute(int Label, string Path, bool isNeedLanuageSuffix = false) : base(Label)
        {
            path = Path;
            this.isNeedLanuageSuffix = isNeedLanuageSuffix;
        }

        public override void UpdateAtrribute()
        {
            base.UpdateAtrribute();
            string suffix = "";
            if (isNeedLanuageSuffix)
            {
                suffix = AFStart.Instance.GetLanuageSuffix();
            }
            if(path.StartsWith("Assets/", StringComparison.Ordinal))
            {
                suffix += ".prefab";
            }
            UIpath = path + suffix;
        }
    }
}
