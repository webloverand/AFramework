/*******************************************************************
* Copyright(c)
* 文件名称: ResourceLoader.cs
* 简要描述: 
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections.Generic;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class ResourceLoader : Singleton<ResourceLoader>
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="resLoadInfo"></param>
        /// <returns></returns>
        public Object LoadSync(ResLoadInfo resLoadInfo)
        {
            if (resLoadInfo.mResPath == "")
            {
                AFLogger.e("资源路径为空,无法进行加载");
                return null;
            }
            ResInfo resInfo = ResManager.Instance.GetRes(resLoadInfo);
            if (resInfo != null && resInfo.State == ResState.Ready && (resInfo.ResObject.IsNotNull() || resInfo.ResStr.IsNotNullAndEmpty()))
            {
                resInfo.Retain();
                return resInfo.ResObject;
            }
            else if (resInfo == null)
            {
                resInfo = ResFactory.Create(resLoadInfo);
                if (resInfo.LoadSync())
                {
                    resInfo.Retain();
                    Debug.Log("加载成功:" + resInfo.RefCount);
                    ResManager.Instance.CacheResource(resLoadInfo.mResPath, resInfo);
                    return resInfo.ResObject;
                }
                else
                {
                    //加载失败,释放
                    resInfo.Recycle2Cache();
                }
            }
            else
            {
                AFLogger.e("同步遇到异步加载的资源 : " + resInfo.ResPath + ",请检查调用代码!");
            }
            return null;
        }
        public void LoadAsync(ResLoadInfo resLoadInfo)
        {
            if (resLoadInfo.mResPath == "")
            {
                AFLogger.e("资源路径为空,无法进行加载");
                return;
            }
            ResInfo resInfo = ResManager.Instance.GetRes(resLoadInfo);
            if (resInfo != null && resInfo.State == ResState.Ready && (resInfo.ResObject.IsNotNull() || resInfo.ResStr.IsNotNullAndEmpty()))
            {
                resLoadInfo.mListener.InvokeGracefully(true, resInfo);
            }
            else if (resInfo == null)
            {
                resInfo = ResFactory.Create(resLoadInfo);
                //加入队列
                if (resInfo.LoadAsync())
                {
                    ResManager.Instance.CacheResource(resLoadInfo.mResPath, resInfo);
                }
            }
            else
            {
                AFLogger.e("当前请求的资源正在加载，增加回调");
                //当前请求的资源正在加载，增加回调
                resInfo.RegisterListen(resLoadInfo.mListener);
            }
        }
        [SerializeField]
        private int mCurrentCoroutineCount;
        private int mMaxCoroutineCount = 6; //最快协成大概在6到8之间
        LinkedList<ResInfo> mResInfoTask = new LinkedList<ResInfo>();
        public void PushIEnumeratorTask(ResInfo task)
        {
            if (task == null)
            {
                return;
            }

            mResInfoTask.AddLast(task);
            TryStartNextIEnumeratorTask();
        }
        private void TryStartNextIEnumeratorTask()
        {
            if (mResInfoTask.Count == 0)
            {
                return;
            }

            if (mCurrentCoroutineCount >= mMaxCoroutineCount)
            {
                return;
            }

            var task = mResInfoTask.First.Value;
            mResInfoTask.RemoveFirst();

            ++mCurrentCoroutineCount;
            IEnumeratorTool.StartCoroutine(task.DoLoadAsync(OnIEnumeratorTaskFinish));
        }
        private void OnIEnumeratorTaskFinish(uint crc)
        {
            --mCurrentCoroutineCount;
            TryStartNextIEnumeratorTask();
        }
        public void CancelLoad(ResInfo task)
        {
            OnIEnumeratorTaskFinish(task.mCRC);
            ResManager.Instance.RemoveLoadFailRes(task.mCRC);
            ResManager.Instance.StopCoroutine(task.DoLoadAsync(OnIEnumeratorTaskFinish));
        }
        #region 为ObjectLoader提供的接口
        public ResObject LoadResSyncForObj(ObjLoadInfo objLoadInfo)
        {
            ResObject resObject = ResObject.Allocate(objLoadInfo);
            ResInfo resInfo = ResManager.Instance.GetRes(objLoadInfo.mCRC);
            //之前就未加载
            if (resInfo == null)
            {
                ResLoadInfo resLoadInfo = ResLoadInfo.Allocate(objLoadInfo);
                resObject.mObj = LoadSync(resLoadInfo);
                resLoadInfo.Recycle2Cache();
            }
            //之前就加载完成
            else if (resInfo.State == ResState.Ready)
            {
                resInfo.Retain();
                resObject.mObj = resInfo.ResObject;
            }
            else
            {
                AFLogger.e("同步遇到异步正在或者等待加载的资源 : " + resInfo.ResPath + ",请检查调用代码!");
            }
            return resObject;
        }
        public void LoadResAsyncForObj(ObjLoadInfo objLoadInfo)
        {
            ResInfo resInfo = ResManager.Instance.GetRes(objLoadInfo.mCRC);
            if (resInfo == null)
            {
                ResLoadInfo resLoadInfo = ResLoadInfo.Allocate(objLoadInfo);
                //资源没有加载
                LoadAsync(resLoadInfo);
                resLoadInfo.Recycle2Cache();
            }
            else if (resInfo.State != ResState.Ready)
            {
                //资源正在加载,是其他loader的异步加载
                resInfo.RegisterListen(objLoadInfo.loadResCall);
            }
            else
            {
                //资源已经加载完成,开始回调
                objLoadInfo.loadResCall(true, resInfo);
            }
        }
        #endregion
    }
}
