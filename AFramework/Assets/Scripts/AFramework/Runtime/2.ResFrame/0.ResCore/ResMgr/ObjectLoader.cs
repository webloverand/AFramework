/*******************************************************************
* Copyright(c)
* 文件名称: ObjectLoader.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using UnityEngine;
    public class ObjectLoader 
    {
        public static GameObject InstantiateObjectSync(ObjLoadInfo objLoadInfo)
        {
            //从对象池拿取对象
            ResObject resObject = ResManager.Instance.GetObjectFromPool(objLoadInfo.mCRC);
            //对象池没有
            if(resObject == null)
            {
                //ResManager提供加载方法
                resObject = ResourceLoader.Instance.LoadResSyncForObj(objLoadInfo);

                if (resObject.mObj != null)
                {
                    if(objLoadInfo.ObjParentTrans == null)
                    {
                        resObject.mCloneObj = GameObject.Instantiate(resObject.mObj) as GameObject;
                    }
                    else
                    {
                        resObject.mCloneObj = GameObject.Instantiate(resObject.mObj, objLoadInfo.ObjParentTrans) as GameObject;
                    }
                    ResManager.Instance.CacheResObj(resObject);
                }
            }
            else
            {
                resObject.mCloneObj.transform.SetParent(objLoadInfo.ObjParentTrans);
                ResManager.Instance.CacheResObj(resObject);
            }
            return resObject.mCloneObj;
        }
        /// <summary>
        /// 异步加载Object
        /// </summary>
        /// <param name="objLoadInfo"></param>
        public static void InstantiateObjectASync(ObjLoadInfo objLoadInfo)
        {
            //从对象池拿取对象
            ResObject resObject = ResManager.Instance.GetObjectFromPool(objLoadInfo.mCRC);
            if(resObject == null)
            {
                //ResourceLoader加载资源
                ResourceLoader.Instance.LoadResAsyncForObj(objLoadInfo);
            }
            else
            {
                if(objLoadInfo.ObjParentTrans != null)
                {
                    resObject.mCloneObj.transform.SetParent(objLoadInfo.ObjParentTrans);
                }
                objLoadInfo.loadObjCall(true, resObject);
                objLoadInfo.Recycle2Cache();
            }
        }
    }
}
