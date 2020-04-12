/*******************************************************************
* Copyright(c)
* 文件名称: MsgInfo.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections.Generic;

    /// <summary>
    /// 事件通用接口
    /// </summary>
    /// <param name="param"></param>
    public delegate void MsgEvent(int key,MsgInfo param);
    public class MsgInfo
    {
        public List<object> DataList;
        public MsgInfo(object data)
        {
            if(DataList == null)
            {
                DataList = new List<object>();
            }
            DataList.Add(data);
        }
        public MsgInfo(List<object> datalist)
        {
            if (datalist != null)
            {
                DataList = datalist;
            }
        }
    }

    public class MsgSpan
    {
        public const int Count = 3000;
	}
	/// <summary>
	/// 事件分类ID
	/// </summary>
	public class MgrID
    {
        public const int UIMsgID = 0;
        public const int AFMsgID = UIMsgID + MsgSpan.Count;
        public const int CFMsgID = AFMsgID + MsgSpan.Count;
    }
    public class EventInfo
	{
        public LinkedList<MsgEvent> mEventList;
        public bool Fire(int key, MsgInfo param)
        {
            if (mEventList == null)
            {
                return false;
            }

            var next = mEventList.First;
            MsgEvent call = null;
            LinkedListNode<MsgEvent> nextCache = null;

            while (next != null)
            {
                call = next.Value;
                nextCache = next.Next;
                call(key, param);

                next = next.Next ?? nextCache;
            }

            return true;
        }

        public bool Add(MsgEvent listener)
        {
            if (mEventList == null)
            {
                mEventList = new LinkedList<MsgEvent>();
            }

            if (mEventList.Contains(listener))
            {
                return false;
            }

            mEventList.AddLast(listener);
            return true;
        }

        public void Remove(MsgEvent listener)
        {
            if (mEventList == null)
            {
                return;
            }

            mEventList.Remove(listener);
        }

        public void RemoveAll()
        {
            if (mEventList == null)
            {
                return;
            }

            mEventList.Clear();
        }
    }
}
