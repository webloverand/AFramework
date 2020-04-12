/* 
*   NatCorder
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Dispatch {

    using UnityEngine;
    using System;
    using System.Collections;
    
    [AddComponentMenu("")]
    public sealed class DispatchUtility : MonoBehaviour {

        #region --Op vars--
        public static event Action onFrame, onQuit;
        public static event Action<bool> onPause;
        private static readonly DispatchUtility instance;
        #endregion        


        #region --Operations--

        static DispatchUtility () {
            instance = new GameObject("NatCorder Dispatch Utility").AddComponent<DispatchUtility>();
            instance.StartCoroutine(instance.OnFrame());
        }

        void Awake () {
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(this);
        }
        
        void OnApplicationPause (bool paused) {
            if (onPause != null) onPause(paused);
        }
        
        void OnApplicationQuit () {
            if (onQuit != null) onQuit();
        }

        IEnumerator OnFrame () {
            var yielder = new WaitForEndOfFrame();
            for (;;) {
                yield return yielder;
                if (onFrame != null)
                    onFrame();
            }
        }
        #endregion
    }
}