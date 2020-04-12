/*******************************************************************
* Copyright(c)
* 文件名称: MsgManager.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/

namespace AFramework
{
    using System;
    using System.Collections.Generic;
    public class MsgManager : Singleton<MsgManager>
    {
        Dictionary<int, EventInfo> EventIDToAction = new Dictionary<int, EventInfo>();

        #region 功能函数
        public bool Register(int key, MsgEvent fun)
        {
            var kv = key;
            EventInfo wrap;
            if (!EventIDToAction.TryGetValue(kv, out wrap))
            {
                wrap = new EventInfo();
                EventIDToAction.Add(kv, wrap);
            }

            if (wrap.Add(fun))
            {
                return true;
            }

            AFLogger.d("Already Register Same Event:" + key);
            return false;
        }

        public void UnRegister(int key, MsgEvent fun) 
        {
            EventInfo wrap;
            if (EventIDToAction.TryGetValue(key, out wrap))
            {
                wrap.Remove(fun);
            }
        }

        public void UnRegister(int key) 
        {
            EventInfo wrap;
            if (EventIDToAction.TryGetValue(key, out wrap))
            {
                wrap.RemoveAll();
                wrap = null;

                EventIDToAction.Remove(key);
            }
        }

        public bool Send(int key, MsgInfo param) 
        {
            EventInfo wrap;
            if (EventIDToAction.TryGetValue(key, out wrap))
            {
                return wrap.Fire(key, param);
            }
            return false;
        }
        #endregion

        #region 高频率使用的API
        public static bool SendEvent(int key, MsgInfo param) 
        {
            return Instance.Send(key, param);
        }

        public static bool RegisterEvent(int key, MsgEvent fun) 
        {
            return Instance.Register(key, fun);
        }

        public static void UnRegisterEvent(int key, MsgEvent fun) 
        {
            Instance.UnRegister(key, fun);
        }
        #endregion
    }


}
