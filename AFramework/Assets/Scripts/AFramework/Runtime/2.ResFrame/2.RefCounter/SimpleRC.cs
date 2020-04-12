namespace AFramework
{
    public interface IRefCounter
    {
        int RefCount { get; }  //引用计数

        void Retain(object refOwner = null);  //增加引用计数
        void Release(object refOwner = null); //减少引用计数
    }
    //手动释放
    public class SimpleRC : IRefCounter
    {
        //初始化引用计数为0
        public SimpleRC()
        {
            RefCount = 0;
        }

        public int RefCount { get; private set; }

        public void Retain(object refOwner = null)
        {
            ++RefCount;
        }

        public void Release(object refOwner = null)
        {
            --RefCount;
            if (RefCount == 0)
            {
                OnZeroRef();
            }
        }
        //如果引用计数为0时调用的函数
        protected virtual void OnZeroRef()
        {
        }
    }
}
