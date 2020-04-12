/* 
*   NatRender
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Readback {

    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;
    using Dispatch;

    public sealed class BlockingReadback : ReadbackContext {

        #region --Op vars--
        private readonly Texture2D framebuffer;
        private readonly IDispatcher readbackDispatcher, handlerDispatcher;
        #endregion
        

        #region --Client API--

        public BlockingReadback (TextureFormat format) {
            framebuffer = new Texture2D(16, 16, format, false, false);
            readbackDispatcher = new MainDispatcher();
            handlerDispatcher = new WorkDispatcher();
        }

        public override RenderTexture Allocate (int width, int height) {
            var frameTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            frameTexture.antiAliasing = 1;
            var _ = frameTexture.colorBuffer;
            return frameTexture;
        }

        public override void Readback (RenderTexture frame, Action<IntPtr> handler) {
            // Null checking
            if (!frame) {
                handlerDispatcher.Dispatch(() => handler(IntPtr.Zero));
                return;
            }
            // State checking
            if (framebuffer.width != frame.width || framebuffer.height != frame.height)
                framebuffer.Resize(frame.width, frame.height);
            // Readback
            var currentRT = RenderTexture.active;
            RenderTexture.active = frame;
            framebuffer.ReadPixels(new Rect(0, 0, framebuffer.width, framebuffer.height), 0, 0, false);
            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(frame);
            // Invoke handler
            var data = framebuffer.GetRawTextureData();
            handlerDispatcher.Dispatch(() => {
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                handler(dataHandle.AddrOfPinnedObject());
                dataHandle.Free();
                GC.Collect();
            });
        }

        public override void Dispose () {
            Readback(null, ptr => {
                using (var dispatcher = new MainDispatcher())
                    dispatcher.Dispatch(() => {
                        Texture2D.Destroy(framebuffer);
                        readbackDispatcher.Dispose();
                        handlerDispatcher.Dispose();
                    });
            });
        }
        #endregion
    }
}