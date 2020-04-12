namespace AFramework
{
    using System;
    using System.Reflection;
    
    public class NonPublicObjectFactory<T> : IObjectFactory<T> where T : class
    {
        public T Create()
        {
            var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
            var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);
            return ctor.Invoke(null) as T;
        }
    }
}
