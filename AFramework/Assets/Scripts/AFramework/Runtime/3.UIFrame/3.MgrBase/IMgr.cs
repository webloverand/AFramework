/*******************************************************************
* Copyright(c)
* 文件名称: IMgr.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System;
    public class ClassData
    {
        public ManagerAtrribute Attribute;
        public Type Type;
    }
    public interface IMgr
    {

        void Init();
	    void Start();

		void CheckType(Type type);
        T2 CreateInstance<T2>(string typeName, params object[] args) where T2 : class;
		ClassData GetClassData(int tag);

        void OnDestroy();
	}

}
