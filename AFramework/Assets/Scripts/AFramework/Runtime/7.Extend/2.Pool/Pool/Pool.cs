namespace AFramework
{
    using System.Collections.Generic;
    /// <summary>
    /// SimpleObjectPool的二级父物体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Pool<T> : IPool<T>,ICountObserveAble
    {
        #region ICountObserverable
        /// <summary>
        /// 获取当前对象池中对象数量
        /// </summary>
        public int CurCount
        {
            get { return mCacheStack.Count; }
        }
        #endregion
        //抽象工厂
        protected IObjectFactory<T> mFactory;

        protected Stack<T> mCacheStack = new Stack<T>();

        /// <summary>
        /// 默认为12
        /// </summary>
        protected int mMaxCount = 12;

        public virtual T Allocate()
        {
            return mCacheStack.Count == 0
                ? mFactory.Create()
                : mCacheStack.Pop();
        }
        /// <summary>
        /// 回收
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract bool Recycle(T obj);
    }
}
