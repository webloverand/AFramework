namespace AFramework
{
    using System;

    /// <summary>
    ///没有公共构造函数的对象池
    /// </summary>
    public class NonPublicObjectPool<T> :Pool<T> where T : class,IPoolable
	{
        /// <summary>
        /// init指定的maxcount和initcount。
        /// </summary>
        /// <param name="maxCount"></param>
        /// <param name="initCount"></param>
        public NonPublicObjectPool(int maxCount, int initCount)
        {
            mFactory = new NonPublicObjectFactory<T>();
            if (maxCount > 0)
			{
				initCount = Math.Min(maxCount, initCount);
			}

			if (CurCount >= initCount) return;
			
			for (var i = CurCount; i < initCount; ++i)
			{
				Recycle(mFactory.Create());
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

				if (mCacheStack == null) return;
				if (mMaxCount <= 0) return;
				if (mMaxCount >= mCacheStack.Count) return;
				var removeCount = mMaxCount - mCacheStack.Count;
				while (removeCount > 0)
				{
					mCacheStack.Pop();
					--removeCount;
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
		/// 回收
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
                    //无法回收到回收池
					t.OnRecycled();
					return false;
				}
			}

			t.IsRecycled = true;
			t.OnRecycled();
			mCacheStack.Push(t);

			return true;
		}
	}
}
