/*******************************************************************
* Copyright(c)
* 文件名称: MsgBehaviour.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/


using System.Collections.Generic;

namespace AFramework
{
    public class MsgBehaviour
    {
        MsgManager mMsgManager = MsgManager.Instance;
        /// <summary>
        /// 类别的起始ID,可看MgrID
        /// </summary>
        protected virtual int ManagerId { set; get; }
        protected List<int> AllRegisterID = new List<int>();


        public void RegisterEvent(int msgID, MsgEvent msgEvent) 
		{
            AllRegisterID.Add(msgID);
            mMsgManager.Register(ManagerId+msgID, msgEvent);
        }
        public void UnRegisterEvent(int msgID, MsgEvent msgEvent)
        {
            AllRegisterID.Remove(msgID);
            mMsgManager.UnRegister(ManagerId+msgID, msgEvent);
        }
        public void UnAllIDRegisterEvent(int msgID)
        {
            AllRegisterID.Remove(msgID);
            mMsgManager.UnRegister(ManagerId + msgID);
        }
        public void UnAllRegisterEvent()
        {
            AllRegisterID.ForEach((i) =>
            {
                UnAllIDRegisterEvent(i);
            });
            AllRegisterID.Clear();
        }
        public void SendMsg(int msgID,MsgInfo msgInfo)
        {
            mMsgManager.Send(ManagerId + msgID, msgInfo);
        }
    }
}
