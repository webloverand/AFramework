/* 
*   NatCorder
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Platforms {

    using AOT;
    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;
    using Dispatch;
    using Readback;

    public sealed class MediaRecorderiOS : IMediaRecorder {

        #region --Op vars--
        private readonly IntPtr recorder;
        private readonly int width, height;
        private readonly Action<string> callback;
        private readonly ReadbackContext readbackContext;
        #endregion

        
        #region --IMediaRecorder--

        public MediaRecorderiOS (IntPtr recorder, int width, int height, TextureFormat readbackFormat, string recordingPath, Action<string> callback) {
            this.recorder = recorder;
            this.width = width;
            this.height = height;
            this.callback = callback;
            this.readbackContext = new BlockingReadback(readbackFormat);
            // Start recording
            var self = GCHandle.Alloc(this, GCHandleType.Normal);
            recorder.StartRecording(recordingPath, OnRecording, (IntPtr)self);
        }

        public void Dispose () {
            readbackContext.Readback(null, ptr => recorder.StopRecording());
            readbackContext.Dispose();
        }
        
        public RenderTexture AcquireFrame () {
            return readbackContext.Allocate(width, height);
        }

        public void CommitFrame (RenderTexture frame, long timestamp) {
            readbackContext.Readback(frame, framePtr => recorder.EncodeFrame(framePtr, timestamp));
        }

        public void CommitSamples (float[] sampleBuffer, long timestamp) {
            recorder.EncodeSamples(sampleBuffer, sampleBuffer.Length, timestamp);
        }
        #endregion


        #region --Callbacks--

        [MonoPInvokeCallback(typeof(Action<IntPtr, IntPtr>))]
        private static void OnRecording (IntPtr context, IntPtr path) {
            var pathStr = Marshal.PtrToStringAuto(path);
            var instanceHandle = (GCHandle)context;
            var instance = instanceHandle.Target as MediaRecorderiOS;
            instanceHandle.Free();
            using (var dispatcher = new MainDispatcher())
                dispatcher.Dispatch(() => instance.callback(pathStr));
        }
        #endregion
    }
}