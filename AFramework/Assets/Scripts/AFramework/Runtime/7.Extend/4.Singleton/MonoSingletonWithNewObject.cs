using System.Reflection;
using UnityEngine;

namespace AFramework
{
    /// <summary>
    /// 无需挂载到gameobject上,继承此类的会根据MonoSingletonPath创建物体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingletonWithNewObject<T> : MonoBehaviour, ISingleton where T : MonoSingletonWithNewObject<T>, ISingleton
    {
        protected static T mInstance;

        public static T Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = MonoSingletonWithNewObjectCreator.CreateMonoSingleton<T>();
                }

                return mInstance;
            }
        }

        public virtual void Dispose(bool isDestroyObject = true)
        {
            if(isDestroyObject)
            {
                Destroy(gameObject);
            }
            else
            {
                mInstance = null;
            }
        }

        protected virtual void OnDestroy()
        {
            mInstance = null;
        }
      
        public virtual void OnSingletonInit()
        {

        }
    }
}
