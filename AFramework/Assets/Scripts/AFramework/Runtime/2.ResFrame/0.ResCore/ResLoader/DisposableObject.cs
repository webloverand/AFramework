/*******************************************************************
* Copyright(c)
* 文件名称: DisposableObject.cs
* 简要描述: 
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System;

    public class DisposableObject : IDisposable
    {
        private Boolean mDisposed = false;

        ~DisposableObject()
        {
            Dispose(false);
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Overrides it, to dispose managed resources.
        protected virtual void DisposeGC()
        {
        }

        // Overrides it, to dispose unmanaged resources
        protected virtual void DisposeNGC()
        {
        }

        private void Dispose(Boolean disposing)
        {
            if (mDisposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeGC();
            }

            DisposeNGC();

            mDisposed = true;
        }
    }
}
