/* 
*   NatRender
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Readback {

    using UnityEngine;
    using System;

    public abstract class ReadbackContext : IDisposable {

        /// <summary>
        /// Allocate a RenderTexture for readback
        /// </summary>
        public abstract RenderTexture Allocate (int width, int height);

        /// <summary>
        /// Readback pixel data from an allocated RenderTexture into a pixel buffer.
        /// This function must be called on the Unity main thread.
        /// Note that the readback delegate may be invoked on any arbitrary thread.
        /// Also note that the RenderTexture MUST NOT be used once this function is called.
        /// </summary>
        /// <param name="frame">RenderTexture to readback from</param>
        /// <param name="handler">Handler invoked with opaque handle to texture readback data</param>
        public abstract void Readback (RenderTexture frame, Action<IntPtr> handler);

        /// <summary>
        /// Release resources and dispose instance
        /// </summary>
        public abstract void Dispose ();
    }
}