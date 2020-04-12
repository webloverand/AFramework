namespace AFramework
{
    using System;
    /// <summary>
    /// 适合用于项目开发SimpleObjectPool : 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleObjectPool<T> : Pool<T>
    {
        readonly Action<T> mResetMethod;
        /// <summary>
		/// 使用外部传进来的创建方法
		/// </summary>
		/// <param name="factoryMethod"></param>
		/// <param name="resetMethod"></param>
		/// <param name="initCount"></param>
        public SimpleObjectPool(Func<T> factoryMethod, Action<T> resetMethod = null,int initCount = 0)
        {
            mFactory = new CustomObjectFactory<T>(factoryMethod);
            mResetMethod = resetMethod;

            for (int i = 0; i < initCount; i++)
            {
                mCacheStack.Push(mFactory.Create());
            }
        }

        public override bool Recycle(T obj)
        {
            mResetMethod.InvokeGracefully(obj);
            mCacheStack.Push(obj);
            return true;
        }
    }
}
