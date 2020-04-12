/* 
*   NatCorder
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Inputs {

    using UnityEngine;
    using System;
    using Clocks;
    using Docs;
    using Dispatch;

    /// <summary>
    /// Recorder input for recording a game camera
    /// </summary>
    [Doc(@"CameraInput")]
    public sealed class CameraInput : IDisposable {
        
        #region --Client API--
        /// <summary>
        /// Control number of successive camera frames to skip while recording.
        /// This is very useful for GIF recording, which typically has a lower framerate appearance
        /// </summary>
        [Doc(@"CameraInputNthFrame", @"CameraInputNthFrameDiscussion"), Code(@"RecordGIF")]
        public int recordEveryNthFrame = 1;

        /// <summary>
        /// Create a video recording input from a game camera
        /// </summary>
        /// <param name="mediaRecorder">Media recorder to receive committed frames</param>
        /// <param name="clock">Clock for generating timestamps</param>
        /// <param name="cameras">Game cameras to record</param>
        [Doc(@"CameraInputCtor")]
        public CameraInput (IMediaRecorder mediaRecorder, IClock clock, params Camera[] cameras) {
            this.mediaRecorder = mediaRecorder;
            this.cameras = cameras;
            this.clock = clock;
            DispatchUtility.onFrame += OnFrame;
        }

        /// <summary>
        /// Stop recorder input and teardown resources
        /// </summary>
        [Doc(@"CameraInputDispose")]
        public void Dispose () {
            DispatchUtility.onFrame -= OnFrame;
        }
        #endregion


        #region --Operations--

        private readonly IMediaRecorder mediaRecorder;
        private readonly Camera[] cameras;
        private readonly IClock clock;
        private int frameCount;

        private void OnFrame () {
            // Check frame index
            if (frameCount++ % recordEveryNthFrame != 0)
                return;
            // Acquire frame
            var encoderFrame = mediaRecorder.AcquireFrame();
            // Render every camera
            foreach (var camera in cameras) {
                var prevTarget = camera.targetTexture;
                camera.targetTexture = encoderFrame;
                camera.Render();
                camera.targetTexture = prevTarget;
            }
            // Commit frame                
            mediaRecorder.CommitFrame(encoderFrame, clock.Timestamp);
        }
        #endregion
    }
}