/*******************************************************************
* Copyright(c)
* 文件名称: ResHelper.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class ResLoadInfo : IPoolable,IPoolType
    {
        public string mResPath;  //资源路径
        public ResFromType mResFromType;  //资源加载来源
        public Action<bool,ResInfo> mListener;  //异步资源加载回调函数,是统一的回调函数
        public Action<bool, ResInfo> mRealListener; //异步资源加载回调函数
        public bool  mIsSprite;  
        public bool DestroyCache = false;  //物体被销毁后是否销毁内存
        public LoadResPriority loadResPriority; //加载优先级
        public uint mCRC;

        public bool IsRecycled { set; get; }
        /// <summary>
        /// 这里的mListen与调用loader传的参数并不相同
        /// </summary>
        /// <param name="resType"></param>
        /// <param name="assetPath"></param>
        /// <param name="isSprite"></param>
        /// <param name="mListen"></param>
        /// <returns></returns>
        public static ResLoadInfo Allocate(ResFromType resType,string assetPath,bool isSprite,
            Action<bool, ResInfo> mListen = null, Action<bool, ResInfo> mRealListener = null, bool DestroyCache = false,
            LoadResPriority loadResPriority = LoadResPriority.RES_NUM)
        {
            ResLoadInfo resLoadInfo = SafeObjectPool<ResLoadInfo>.Instance.Allocate();
            resLoadInfo.mResPath = assetPath;
            resLoadInfo.mResFromType = resType;
            resLoadInfo.mListener = mListen;
            resLoadInfo.mIsSprite = isSprite;
            resLoadInfo.DestroyCache = DestroyCache;
            resLoadInfo.loadResPriority = loadResPriority;
            resLoadInfo.mCRC = Crc32.GetCrc32(assetPath);
            resLoadInfo.mRealListener = mRealListener;
            return resLoadInfo;
        }
        public static ResLoadInfo Allocate(ObjLoadInfo objLoadInfo)
        {
            ResLoadInfo resLoadInfo = SafeObjectPool<ResLoadInfo>.Instance.Allocate();
            resLoadInfo.mResPath = objLoadInfo.mResPath;
            resLoadInfo.mResFromType = objLoadInfo.mResFromType;
            resLoadInfo.mIsSprite = false;
            resLoadInfo.mListener = objLoadInfo.loadResCall;
            resLoadInfo.mCRC = Crc32.GetCrc32(objLoadInfo.mResPath);
            resLoadInfo.DestroyCache = objLoadInfo.mClear;
            return resLoadInfo;
        }
        public void OnRecycled()
        {
            IsRecycled = true;
            mResPath = null;
            mResFromType = ResFromType. None;
        }

        public void Recycle2Cache()
        {
            SafeObjectPool<ResLoadInfo>.Instance.Recycle(this);
        }
    }
    public class ObjLoadInfo : IPoolable, IPoolType
    {
        public uint mCRC;
        public string mResPath;  //资源路径
        public ResFromType mResFromType;  //资源加载来源
        public LoadResPriority loadResPriority; //加载优先级
        //跳场景是否清除
        public bool mClear;
        //实例化的O父物体
        public Transform ObjParentTrans;
        public Action<bool, ResInfo> loadResCall;
        public Action<bool, ResObject> loadObjCall;

        public static ObjLoadInfo Allocate(ResFromType resType, string assetPath, Transform ObjParentTrans,
            bool mclear = false, LoadResPriority loadResPriority = LoadResPriority.RES_NUM,
            Action<bool, ResInfo> loadResCall = null, Action<bool, ResObject> loadObjCall = null)
        {
            ObjLoadInfo objLoadInfo = SafeObjectPool<ObjLoadInfo>.Instance.Allocate();
            objLoadInfo.mCRC = Crc32.GetCrc32(assetPath);
            objLoadInfo.mResFromType = resType;
            objLoadInfo.ObjParentTrans = ObjParentTrans;
            objLoadInfo.mClear = mclear;
            objLoadInfo.mResPath = assetPath;
            objLoadInfo.loadResPriority = loadResPriority;
            objLoadInfo.loadResCall = loadResCall;
            objLoadInfo.loadObjCall = loadObjCall;
            return objLoadInfo;
        }
        public bool IsRecycled { set; get; }
        public void OnRecycled()
        {
            IsRecycled = true;
        }
        public void Recycle2Cache()
        {
            SafeObjectPool<ObjLoadInfo>.Instance.Recycle(this);
        }
    }
    /// <summary>
    /// 加载优先级
    /// </summary>
    public enum LoadResPriority
    {
        RES_HIGHT = 0,//最高优先级
        RES_MIDDLE,//一般优先级
        RES_SLOW,//低优先级
        RES_NUM,
    }

    public enum ResState
    {
        Waiting = 0,  //等待加载
        Loading = 1, //加载中
        Ready = 2, //加载完成
    }

    public enum ResFromType
    {
        ABRes = 0,    //配合AB打包工具
        NetImageRes = 2,  //网络图片,用URL进行加载
        ResourcesRes = 4,  //Resources路径下的资源加载
        EditorRes, //编辑器中加载资源,主要用于测试,因此无需打AB包
        None
    }
    #region AB包相关类
    public class AssetBundleItem : IRefCounter, IPoolable
    {
        public AssetBundle assetBundle = null;

        public bool IsRecycled { get; set; }

        public int RefCount { get; set; }

        public void OnRecycled()
        {
            IsRecycled = true;
            RefCount = 0;
            assetBundle = null;
        }

        public void Retain(object refOwner = null)
        {
            RefCount++;
        }
        public void Release(object refOwner = null)
        {
            RefCount--;
        }
        public void Recycle2Cache()
        {
            SafeObjectPool<AssetBundleItem>.Instance.Recycle(this);
        }
    }
    //只是用来保存AssetBundleConfig里面的内容
    public class ResItem 
    {
        //资源路径的CRC
        public uint mCrc = 0;
        //资源路径
        public string mResPath = string.Empty;
        //该资源所在的AssetBundle
        public string ABName = string.Empty;
        //该资源所依赖的AssetBundle
        public List<string> DependAssetBundle = null;

        public void Copy(ABResItem ABResitem)
        {
            ABName = ABResitem.mResItem.ABName;
            DependAssetBundle = ABResitem.mResItem.DependAssetBundle;
        }
        public virtual void OnRecycled()
        {
            mCrc = 0;
            ABName = string.Empty;
            DependAssetBundle = null;
            mResPath = "";
        }
    }
    public class ABResItem
    {
        public ResItem mResItem;
        public AssetBundle assetBundle;
    }
    
    public class ResObject : ResItem,IPoolable, IRefCounter
    {
        public Object mObj;
        public GameObject mCloneObj;
        //跳场景是否清除
        public bool mClear;

        public bool IsRecycled { get; set; }
        public bool IsRecyclePool { get; set; }

        public int RefCount { get; set; }

        public override void OnRecycled()
        {
            base.OnRecycled();
            mCloneObj = null;
            mObj = null;
            mClear = false;
            IsRecycled = true;
            IsRecyclePool = false;
            RefCount = 0;
        }

        public void Release(object refOwner = null)
        {
            RefCount--;
        }

        public void Retain(object refOwner = null)
        {
            RefCount++;
        }
        public void Recycle2Cache()
        {
            SafeObjectPool<ResObject>.Instance.Recycle(this);
        }
        public static ResObject Allocate(ObjLoadInfo objLoadInfo)
        {
            ResObject resObject = SafeObjectPool<ResObject>.Instance.Allocate();
            resObject.mCrc = objLoadInfo.mCRC;
            resObject.mClear = objLoadInfo.mClear;
            resObject.mResPath = objLoadInfo.mResPath;
            resObject.IsRecyclePool = false;
            return resObject;
        }
        public static ResObject Allocate(ResInfo resInfo)
        {
            ResObject resObject = SafeObjectPool<ResObject>.Instance.Allocate();
            resObject.mClear = resInfo.DestroyCache;
            resObject.mResPath = resInfo.ResPath;
            resObject.IsRecyclePool = false;
            return resObject;
        }
        public static ResObject Copy(ResObject resObject)
        {
            ResObject newResObject = SafeObjectPool<ResObject>.Instance.Allocate();
            newResObject.mObj = resObject.mObj;
            newResObject.mClear = resObject.mClear;
            newResObject.IsRecyclePool = resObject.IsRecyclePool;
            newResObject.IsRecycled = resObject.IsRecycled;
            newResObject.RefCount = resObject.RefCount;
            newResObject.mResPath = resObject.mResPath;
            return newResObject;
        }
    }
    [Serializable]
    public class AssetBundleConfig
    {
        [XmlElement("ABList")]
        public List<ABBase> ABList { get; set; }
    }

    [Serializable]
    public class ABBase
    {
        //(全路径,方便后面加载资源)只是为了看对不对，如果为了优化流程，可以去掉
        [XmlAttribute("Path")]
        //根据路径找到AB包
        //文件的唯一标识
        public string Path { get; set; }
        [XmlAttribute("Crc")]
        public uint Crc { get; set; }
        [XmlAttribute("ABName")]
        public string ABName { get; set; }
        //资源名
        [XmlAttribute("AssetName")]
        public string AssetName { get; set; }
        //加载所依赖其他的AB包
        [XmlElement("ABDependce")]
        public List<string> ABDependce { get; set; }
    }

    //双向链表结构节点
    public class DoubleLinkedListNode<T> : IPoolable where T : class, new()
    {
        //前一个节点
        public DoubleLinkedListNode<T> prev = null;
        //后一个节点
        public DoubleLinkedListNode<T> next = null;
        //当前节点
        public T t = null;

        public bool IsRecycled { get; set; }

        public void OnRecycled()
        {
            IsRecycled = true;
            prev = null;
            next = null;
            t = null;
        }
    }

    //双向链表结构
    public class DoubleLinedList<T> where T : class, new()
    {
        //表头
        public DoubleLinkedListNode<T> Head = null;
        //表尾
        public DoubleLinkedListNode<T> Tail = null;
        public DoubleLinedList()
        {
            SafeObjectPool<DoubleLinkedListNode<T>>.Instance.Init(500, 500);
        }
        //个数
        protected int m_Count = 0;
        public int Count
        {
            get { return m_Count; }
        }

        /// <summary>
        /// 添加一个节点到头部
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public DoubleLinkedListNode<T> AddToHeader(T t)
        {
            DoubleLinkedListNode<T> pList = SafeObjectPool<DoubleLinkedListNode<T>>.Instance.Allocate();
            pList.next = null;
            pList.prev = null;
            pList.t = t;
            return AddToHeader(pList);
        }

        /// <summary>
        /// 添加一个节点到头部
        /// </summary>
        /// <param name="pNode"></param>
        /// <returns></returns>
        public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null)
                return null;

            pNode.prev = null;
            if (Head == null)
            {
                Head = Tail = pNode;
            }
            else
            {
                pNode.next = Head;
                Head.prev = pNode;
                Head = pNode;
            }
            m_Count++;
            return Head;
        }

        /// <summary>
        /// 添加节点到尾部
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public DoubleLinkedListNode<T> AddToTail(T t)
        {
            DoubleLinkedListNode<T> pList = SafeObjectPool<DoubleLinkedListNode<T>>.Instance.Allocate();
            pList.next = null;
            pList.prev = null;
            pList.t = t;
            return AddToTail(pList);
        }

        /// <summary>
        /// 添加节点到尾部
        /// </summary>
        /// <param name="pNode"></param>
        /// <returns></returns>
        public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null)
                return null;

            pNode.next = null;
            if (Tail == null)
            {
                Head = Tail = pNode;
            }
            else
            {
                pNode.prev = Tail;
                Tail.next = pNode;
                Tail = pNode;
            }
            m_Count++;
            return Tail;
        }

        /// <summary>
        /// 移除某个节点
        /// </summary>
        /// <param name="pNode"></param>
        public void RemoveNode(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null)
                return;

            if (pNode == Head)
                Head = pNode.next;

            if (pNode == Tail)
                Tail = pNode.prev;

            if (pNode.prev != null)
                pNode.prev.next = pNode.next;

            if (pNode.next != null)
                pNode.next.prev = pNode.prev;

            pNode.next = pNode.prev = null;
            pNode.t = null;
            SafeObjectPool<DoubleLinkedListNode<T>>.Instance.Recycle(pNode);
            m_Count--;
        }

        /// <summary>
        /// 把某个节点移动到头部
        /// </summary>
        /// <param name="pNode"></param>
        public void MoveToHead(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null || pNode == Head)
                return;

            if (pNode.prev == null && pNode.next == null)
                return;

            if (pNode == Tail)
                Tail = pNode.prev;

            if (pNode.prev != null)
                pNode.prev.next = pNode.next;

            if (pNode.next != null)
                pNode.next.prev = pNode.prev;

            pNode.prev = null;
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;
            if (Tail == null)
            {
                Tail = Head;
            }
        }
    }

    //封装双向列表
    public class CMapList<T> where T : class, new()
    {
        DoubleLinedList<T> m_DLink = new DoubleLinedList<T>();
        Dictionary<T, DoubleLinkedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkedListNode<T>>();

        ~CMapList()
        {
            Clear();
        }

        /// <summary>
        /// 清除列表
        /// </summary>
        public void Clear()
        {
            while (m_DLink.Tail != null)
            {
                Remove(m_DLink.Tail.t);
            }
        }

        /// <summary>
        /// 插入一个节点到表头
        /// </summary>
        /// <param name="t"></param>
        public void InsertToHead(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (m_FindMap.TryGetValue(t, out node) && node != null)
            {
                m_DLink.AddToHeader(node);
                return;
            }
            m_DLink.AddToHeader(t);
            m_FindMap.Add(t, m_DLink.Head);
        }

        /// <summary>
        /// 从表尾弹出一个结点
        /// </summary>
        public void Pop()
        {
            if (m_DLink.Tail != null)
            {
                Remove(m_DLink.Tail.t);
            }
        }

        /// <summary>
        /// 删除某个节点
        /// </summary>
        /// <param name="t"></param>
        public void Remove(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (!m_FindMap.TryGetValue(t, out node) || node == null)
            {
                return;
            }
            m_DLink.RemoveNode(node);
            m_FindMap.Remove(t);
        }

        /// <summary>
        /// 获取到尾部节点
        /// </summary>
        /// <returns></returns>
        public T Back()
        {
            return m_DLink.Tail == null ? null : m_DLink.Tail.t;
        }

        /// <summary>
        /// 返回节点个数
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return m_FindMap.Count;
        }

        /// <summary>
        /// 查找是否存在该节点
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Find(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (!m_FindMap.TryGetValue(t, out node) || node == null)
                return false;

            return true;
        }

        /// <summary>
        /// 刷新某个节点，把节点移动到头部
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Reflesh(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (!m_FindMap.TryGetValue(t, out node) || node == null)
                return false;

            m_DLink.MoveToHead(node);
            return true;
        }
    }
    [System.Serializable]
    public enum ConfigWritingMode
    {
        TXT,
        XML,
        Binary
    }
  
    #endregion
}
