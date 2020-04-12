namespace AFramework
{
    /// <summary>
    /// 定义了一个分配函数以及回收函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPool<T>
    {
        T Allocate();

        bool Recycle(T obj);
    }
    /// <summary>
    /// 定义了一个回收函数
    /// </summary>
    public interface IPoolType
    {
        void Recycle2Cache();
    }

}
