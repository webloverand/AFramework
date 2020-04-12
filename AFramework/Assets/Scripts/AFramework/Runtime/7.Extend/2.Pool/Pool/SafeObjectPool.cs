namespace AFramework
{
    using System;

    /// <summary>
    /// I pool able.
    /// </summary>
    public interface IPoolable
    {
        void OnRecycled();
        bool IsRecycled { get; set; }
    }

    /// <summary>
    /// 当前栈的数量,pool继承于它
    /// </summary>
    public interface ICountObserveAble
    {
        int CurCount { get; }
    }

	/// <summary>
	/// 适合用于库级开发,更多限制，要求开发者一开始就想好，更安全的SafeObjectPool : 事先就确定好分配多少个,可以无限取,但是缓存池中就存最大值数量的元素
	/// </summary>
	public class SafeObjectPool<T> : Pool<T>,ISingleton where T : class,IPoolable, new()
    {
        public static SafeObjectPool<T> Instance
        {
            get { return Singleton<SafeObjectPool<T>>.Instance; }
        }
        /// <summary>
        /// Init the specified maxCount and initCount.
        /// </summary>
        /// <param name="maxCount">Max Cache count.</param>
        /// <param name="initCount">Init Cache count.</param>
        public void Init(int maxCount, int initCount)
        {
            mFactory = new DefaultObjectFactory<T>();
            MaxCacheCount = maxCount;
            
            if (maxCount > 0)
            {
                initCount = Math.Min(maxCount, initCount);
            }

            if (CurCount < initCount)
            {
                for (var i = CurCount; i < initCount; ++i)
                {
                    Recycle(new T());
                }
            }
        }

        /// <summary>
        /// Gets or sets the max cache count.
        /// </summary>
        /// <value>The max cache count.</value>
        public int MaxCacheCount
        {
            get { return mMaxCount; }
            set
            {
                mMaxCount = value;

                if (mCacheStack != null)
                {
                    if (mMaxCount > 0)
                    {
                        if (mMaxCount < mCacheStack.Count)
                        {
                            int removeCount = mCacheStack.Count - mMaxCount;
                            while (removeCount > 0)
                            {
                                mCacheStack.Pop();
                                --removeCount;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allocate T instance.
        /// </summary>
        public override T Allocate()
        {
            var result = base.Allocate();
            result.IsRecycled = false;
            return result;
        }

        /// <summary>
        /// Recycle the T instance
        /// </summary>
        /// <param name="t">T.</param>
        public override bool Recycle(T t)
        {
            if (t == null || t.IsRecycled)
            {
                return false;
            }

            if (mMaxCount > 0)
            {
                if (mCacheStack.Count >= mMaxCount)
                {
                    t.OnRecycled();
                    return false;
                }
            }

            t.OnRecycled();
            t.IsRecycled = true;
            mCacheStack.Push(t);

            return true;
        }

        public void OnSingletonInit()
        {

        }
    }
}
