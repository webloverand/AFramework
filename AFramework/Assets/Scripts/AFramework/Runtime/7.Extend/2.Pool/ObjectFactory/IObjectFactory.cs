namespace AFramework
{
    /// <summary>
    /// 抽象工厂
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectFactory<T>
    {
        T Create();
    }
}
