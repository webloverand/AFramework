/* 
*   NatCorder
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Dispatch {

    using AOT;
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public sealed class RenderDispatcher : IDispatcher { // Doesn't work in the Editor for some reason :/

        #region --Op vars--
        private readonly Queue<Action> queue = new Queue<Action>();
        private readonly object queueLock = new object();
        private readonly IntPtr weakSelf;
        private static readonly IntPtr delegateHandle;
        private static readonly UnityRenderingEvent renderingEvent;
        #endregion


        #region --Dispatcher--

        public RenderDispatcher () {
            weakSelf = (IntPtr)GCHandle.Alloc(this, GCHandleType.Weak);
            if (Application.platform == RuntimePlatform.Android)
                Dispatch(() => AndroidJNI.AttachCurrentThread());
        }

        public void Dispatch (Action action) {
            lock (queueLock)
				queue.Enqueue(action);
            GL.IssuePluginEvent(delegateHandle, weakSelf.ToInt32());
		}

		public void Dispose () {} // Nop
        #endregion


        #region --Operations--

        private void Update () {
            lock (queueLock)
                queue.Dequeue()();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UnityRenderingEvent (int eventID);

        [MonoPInvokeCallback(typeof(UnityRenderingEvent))]
        private static void OnRenderEvent (int managedInstance) {
            GCHandle handle = (GCHandle)(IntPtr)managedInstance;
            RenderDispatcher target = handle.Target as RenderDispatcher;
            if (target == null)
                return;
            target.Update();
        }

        static RenderDispatcher () {
            renderingEvent = OnRenderEvent;
            delegateHandle = Marshal.GetFunctionPointerForDelegate(renderingEvent);
        }
        #endregion
    }
}