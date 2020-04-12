/* 
*   NatCorder
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder {

    using UnityEngine;
    using System;
    using System.IO;
    using Platforms;
    using Docs;

    /// <summary>
    /// Recorder for recording animated GIF images that loop forever
    /// </summary>
    [Doc(@"GIFRecorder")]
    public sealed class GIFRecorder : IMediaRecorder {

        #region --Client API--
        /// <summary>
        /// Create a GIF recorder
        /// </summary>
        /// <param name="imageWidth">Image width</param>
        /// <param name="imageHeight">Image height</param>
        /// <param name="frameDuration">Frame duration in seconds</param>
        /// <param name="recordingCallback">Recording callback</param>
        [Doc(@"GIFRecorderCtor")]
        public GIFRecorder (int imageWidth, int imageHeight, float frameDuration, Action<string> recordingCallback) {
            imageWidth = imageWidth >> 1 << 1;
            imageHeight = imageHeight >> 1 << 1;
            var readbackFormat = TextureFormat.RGBA32;
            var recordingDirectory = Application.persistentDataPath;
            var recordingFilename = string.Format("recording_{0}.gif", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"));
            switch (Application.platform) {
                case RuntimePlatform.OSXEditor:
                    recordingDirectory = Directory.GetCurrentDirectory();
                    goto case RuntimePlatform.OSXPlayer;
                case RuntimePlatform.OSXPlayer:
                    readbackFormat = TextureFormat.ARGB32;
                    goto case RuntimePlatform.IPhonePlayer;
                case RuntimePlatform.WindowsEditor:
                    recordingDirectory = Directory.GetCurrentDirectory();
                    goto case RuntimePlatform.WindowsPlayer;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.IPhonePlayer: {
                    var recordingPath = Path.Combine(recordingDirectory, recordingFilename);
                    var nativeRecorder = MediaRecorderBridge.CreateGIFRecorder(imageWidth, imageHeight, frameDuration);
                    this.internalRecorder = new MediaRecorderiOS(nativeRecorder, imageWidth, imageHeight, readbackFormat, recordingPath, recordingCallback);
                    break;
                }
                case RuntimePlatform.Android: {
                    var recordingPath = Path.Combine(recordingDirectory, recordingFilename);
                    var nativeRecorder = new AndroidJavaObject(@"com.yusufolokoba.natcorder.GIFRecorder", imageWidth, imageHeight, frameDuration);
                    this.internalRecorder = new MediaRecorderAndroid(nativeRecorder, imageWidth, imageHeight, recordingPath, recordingCallback);
                    break;
                }
                default:
                    Debug.LogError("NatCorder Error: GIFRecorder is not supported on this platform");
                    this.internalRecorder = null; // Self-destruct >:D
                    break;
            }
        }

        /// <summary>
        /// Stop recording and dispose the recorder.
        /// The recording callback is expected to be invoked soon after calling this method.
        /// </summary>
        [Doc(@"Dispose")]
        public void Dispose () {
            internalRecorder.Dispose();
        }

        /// <summary>
        /// Acquire a frame for encoding
        /// You will render or blit to this frame then commit it
        /// </summary>
        [Doc(@"AcquireFrame", @"AcquireFrameDiscussion")]
        public RenderTexture AcquireFrame () {
            return internalRecorder.AcquireFrame();
        }

        /// <summary>
        /// Commit a frame for encoding
        /// </summary>
        /// <param name="frame">Video frame to commit, must have been previously acquired from this recorder</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds</param>
        [Doc(@"CommitFrame", @"CommitFrameDiscussion"), Code(@"RecordCamera")]
        public void CommitFrame (RenderTexture frame, long timestamp) {
            internalRecorder.CommitFrame(frame, timestamp);
        }

        /// <summary>
        /// Not used on GIF recorders.
        /// </summary>
        [Doc(@"CommitSamplesGIF")]
        public void CommitSamples (float[] sampleBuffer, long timestamp) { }
        #endregion

        private readonly IMediaRecorder internalRecorder;
    }
}