using System.Collections.Generic;

namespace AFramework
{
    /// Automatic Reference Counting (ARC)
    /// is used internally to prevent pooling retained Objects.
    /// If you use retain manually you also have to
    /// release it manually at some point.
    /// SafeARC checks if the object has already been
    /// retained or released. It's slower, but you keep the information
    /// about the owners.
    public sealed class SafeARC : IRefCounter
    {
        public int RefCount
        {
            get { return mOwners.Count; }
        }

        public HashSet<object> Owners
        {
            get { return mOwners; }
        }
        //保存引用的对象
        readonly HashSet<object> mOwners = new HashSet<object>();

        public void Retain(object refOwner)
        {
            if (!Owners.Add(refOwner))
            {
                AFLogger.e("ObjectIsAlreadyRetainedByOwnerException");
                
            }
        }

        public void Release(object refOwner)
        {
            if (!Owners.Remove(refOwner))
            {
                AFLogger.e("ObjectIsNotRetainedByOwnerExceptionWithHint");
            }
        }
    }
}
