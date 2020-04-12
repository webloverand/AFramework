/*******************************************************************
* Copyright(c)
* 文件名称: ManagerBase.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System;
    using System.Collections.Generic;
    public class ManagerAtrribute : Attribute
    {
        public int IntTag { get; set; } = -1;
        public string StrTag { get; private set; }
        public ManagerAtrribute(string Id)
        {
            this.StrTag = Id;
        }
        public ManagerAtrribute(int Id)
        {
            this.IntTag = Id;
        }
        public virtual void UpdateAtrribute()
        {
        }
    }


    /// <summary>
    /// 虽有管理器的基类
    /// </summary>
    /// <typeparam name="T">是管理器实例</typeparam>
    /// <typeparam name="V">标签属性</typeparam>
    public class ManagerBase<T, V> : MsgBehaviour, IMgr where T : IMgr, new()
        where V : ManagerAtrribute
    {
        static protected T Inst;

        static public T Instance
        {
            get
            {
                if (Inst == null)
                {
                    Inst = new T();
                }

                return Inst;
            }
        }
        private Dictionary<int, ClassData> ClassDataMap_IntKey { get; set; }
        private Dictionary<string, ClassData> ClassDataMap_StringKey { get; set; }
        protected ManagerBase()
        {
            this.ClassDataMap_IntKey = new Dictionary<int, ClassData>();
            this.ClassDataMap_StringKey = new Dictionary<string, ClassData>();
        }

        private Type vtype = null;
        virtual public void CheckType(Type type)
        {
            if (vtype == null)
            {
                vtype = typeof(V); //attribute类型
            }

            var attrs = type.GetCustomAttributes(vtype, false);
            if (attrs.Length > 0)
            {
                var attr = attrs[0];
                if (attr is V)
                {
                    var _attr = (V)attr;
                    if (_attr.IntTag != -1)
                    {
                        SaveAttribute(_attr.IntTag, new ClassData() { Attribute = _attr, Type = type });
                    }
                    else if (_attr.StrTag != null)
                    {
                        SaveAttribute(_attr.StrTag, new ClassData() { Attribute = _attr, Type = type });
                    }
                }
            }
        }


        virtual public void Init()
        {
        }

        virtual public void Start()
        {
        }


        /// <summary>
        /// 通过tag 获取class信息
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public ClassData GetClassData(int tag)
        {
            ClassData classData = null;
            this.ClassDataMap_IntKey.TryGetValue(tag, out classData);
            return classData;
        }

        /// <summary>
        /// 通过tag 获取class信息
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public ClassData GetClassData(string tag)
        {
            ClassData classData = null;
            this.ClassDataMap_StringKey.TryGetValue(tag, out classData);
            return classData;
        }


        /// <summary>
        /// 通过类型获取class信息
        /// </summary>
        /// <returns></returns>
        public ClassData GetClassData<TN>()
        {
            return GetClassData(typeof(TN));
        }

        /// <summary>
        /// 通过类型获取class信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ClassData GetClassData(Type type)
        {
            var classDatas = GetAllClassDatas();
            foreach (var value in classDatas)
            {
                if (value.Type == type)
                {
                    return value;
                }
            }
            return null;
        }
        /// <summary>
        /// 更新Atrribute
        /// </summary>
        public void UpdateStrClassData()
        {
            List<string> keys = new List<string>(ClassDataMap_StringKey.Keys);
            foreach (var key in keys)
            {
                ClassDataMap_StringKey[key].Attribute.UpdateAtrribute();
            }
        }
        /// <summary>
        /// 更新Atrribute
        /// </summary>
        public void UpdateIntClassData()
        {
            List<int> keys = new List<int>(ClassDataMap_IntKey.Keys);
            foreach (var key in keys)
            {
                ClassDataMap_IntKey[key].Attribute.UpdateAtrribute();
            }
        }
        /// <summary>
        /// 获取所有的ClassData
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<ClassData> GetAllClassDatas()
        {
            IEnumerable<ClassData> classDatas = new List<ClassData>();
            if (this.ClassDataMap_IntKey.Count > 0)
            {
                classDatas = this.ClassDataMap_IntKey.Values;
            }
            else if (this.ClassDataMap_StringKey.Count > 0)
            {
                classDatas = this.ClassDataMap_StringKey.Values;
            }

            return classDatas;
        }

        /// <summary>
        /// 保存属性
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        public void SaveAttribute(int tag, ClassData data)
        {
            this.ClassDataMap_IntKey[tag] = data;
        }
        /// <summary>
        /// 保存属性
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        public void SaveAttribute(string tag, ClassData data)
        {
            this.ClassDataMap_StringKey[tag] = data;
        }

        //下面三种创建实例的方法是为了适配构造函数
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public T2 CreateInstance<T2>(ClassData cd, params object[] args) where T2 : class
        {
            if (cd.Type != null)
            {
                if (args.Length == 0)
                {
                    return Activator.CreateInstance(cd.Type) as T2;
                }
                else
                {
                    return Activator.CreateInstance(cd.Type, args) as T2;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="args"></param>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public T2 CreateInstance<T2>(int tag, params object[] args) where T2 : class
        {
            var cd = GetClassData(tag);
            if (cd == null)
            {
                AFLogger.d("没有找到:" + tag + " -" + typeof(T2).Name);
                return null;
            }

            return CreateInstance<T2>(cd, args);
        }
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="args"></param>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public T2 CreateInstance<T2>(string tag, params object[] args) where T2 : class
        {
            var cd = GetClassData(tag);
            if (cd == null)
            {
                AFLogger.d("没有找到:" + tag + " -" + typeof(T2).Name);
                return null;
            }

            return CreateInstance<T2>(cd, args);
        }

        public virtual void OnDestroy()
        {

        }
    }
}

