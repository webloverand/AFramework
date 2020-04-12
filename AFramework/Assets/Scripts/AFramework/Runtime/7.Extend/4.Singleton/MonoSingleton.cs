/*******************************************************************
* Copyright(c)
* 文件名称: MonoSingleton.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/

namespace AFramework
{
    using UnityEngine;
    /// <summary>
    /// 不会自动创建gameobject,因此需要主动挂在到场景中的物体上
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour, ISingleton where T : MonoSingleton<T>, ISingleton
    {
        protected static T mInstance;

        public static T Instance
        {
            get
            {
                if(mInstance == null)
                {
                    AFLogger.e(typeof(T) + "未添加到GameObject上,请检查");
                }
                return mInstance;
            }
        }

        protected virtual void Awake()
        {
            if (mInstance == null)
            {
                mInstance = (T)this;
                mInstance.OnSingletonInit();
            }
            else
            {
                AFLogger.e("Get a second instance of this class" + this.GetType());
            }
        }

        public virtual void Dispose(bool isDestroyObject = true)
        {
            if (isDestroyObject)
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
