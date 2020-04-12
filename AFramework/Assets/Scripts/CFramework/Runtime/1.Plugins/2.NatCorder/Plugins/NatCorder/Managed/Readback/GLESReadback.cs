/* 
*   NatRender
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Readback {

    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Dispatch;

    public sealed class GLESReadback : ReadbackContext { // INCOMPLETE // REIMPLEMENT

        #region --Op vars--
        private readonly bool deferred;
        private readonly RenderDispatcher dispatcher;
        private static AndroidJavaObject GPUFence;
        private readonly Queue<PendingReadback> queue = new Queue<PendingReadback>();
        #endregion


        #region --Client API--

        public GLESReadback (bool deferred) {
            this.deferred = deferred;
            dispatcher = new RenderDispatcher();
            GPUFence = GPUFence ?? new AndroidJavaClass(@"com.yusufolokoba.natrender.GPUFence");
            DispatchUtility.onFrame += OnFrame;
        }

        public override RenderTexture Allocate (int width, int height) {
            var frameTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            frameTexture.antiAliasing = 1;
            var _ = frameTexture.colorBuffer;
            return frameTexture;
        }

        public override void Readback (RenderTexture frame, Action<IntPtr> handler) {
            var textureId = frame ? frame.GetNativeTexturePtr() : IntPtr.Zero;
            dispatcher.Dispatch(() => {
                var fence = (IntPtr)GPUFence.CallStatic<long>(@"create", deferred);
                if (GPUFence.CallStatic<bool>(@"complete", fence.ToInt64()))
                    handler(textureId);
                else queue.Enqueue(new PendingReadback {
                    texture = textureId,
                    fence = fence,
                    handler = handler
                });
            });
        }

        public override void Dispose () {
            Readback(null, ptr => {
                DispatchUtility.onFrame -= OnFrame;
                dispatcher.Dispose();
            });
        }
        #endregion


        #region --Operations--

        struct PendingReadback { public IntPtr texture, fence; public Action<IntPtr> handler; }

        void OnFrame () {
            dispatcher.Dispatch(() => {
                while (queue.Count > 0) {
                    var request = queue.Peek();
                    if (!GPUFence.CallStatic<bool>(@"complete", request.fence.ToInt64()))
                        break;
                    queue.Dequeue();
                    request.handler(request.texture);
                }
            });
        }
        #endregion
    }
}