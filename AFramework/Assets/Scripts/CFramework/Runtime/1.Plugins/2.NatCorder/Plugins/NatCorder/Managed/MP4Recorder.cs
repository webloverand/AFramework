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
    /// Recorder for recording MP4 videos encoded with the H.264 AVC/AAC codecs
    /// </summary>
    [Doc(@"MP4Recorder")]
    public sealed class MP4Recorder : IMediaRecorder {

        #region --Client API--
        /// <summary>
        /// Create an MP4 recorder
        /// </summary>
        /// <param name="videoWidth"></param>
        /// <param name="videoHeight"></param>
        /// <param name="videoFramerate"></param>
        /// <param name="audioSampleRate">Audio sample rate. Pass 0 for no audio.</param>
        /// <param name="audioChannelCount">Audio channel count. Pass 0 for no audio.</param>
        /// <param name="recordingCallback">Recording callback</param>
        /// <param name="videoBitrate">Video bitrate in bits per second</param>
        /// <param name="videoKeyframeInterval">Keyframe interval in seconds</param>
        [Doc(@"MP4RecorderCtor")]
        public MP4Recorder (int videoWidth, int videoHeight, int videoFramerate, int audioSampleRate, int audioChannelCount, Action<string> recordingCallback, int videoBitrate = (int)(960 * 540 * 11.4f), int videoKeyframeInterval = 3) {
            videoWidth = videoWidth >> 1 << 1;
            videoHeight = videoHeight >> 1 << 1;
            var readbackFormat = TextureFormat.RGBA32;
            var recordingDirectory = Application.persistentDataPath;
            var recordingFilename = string.Format("recording_{0}.mp4", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"));
            switch (Application.platform) {
                case RuntimePlatform.OSXEditor:
                    recordingDirectory = Directory.GetCurrentDirectory();
                    goto case RuntimePlatform.OSXPlayer;
                case RuntimePlatform.OSXPlayer:
                    readbackFormat = TextureFormat.ARGB32;
                    goto case RuntimePlatform.IPhonePlayer;
                case RuntimePlatform.WebGLPlayer:
                case RuntimePlatform.WindowsEditor:
                    recordingDirectory = Directory.GetCurrentDirectory();
                    goto case RuntimePlatform.WindowsPlayer;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.IPhonePlayer: {
                    var recordingPath = Path.Combine(recordingDirectory, recordingFilename);
                    var nativeRecorder = MediaRecorderBridge.CreateMP4Recorder(videoWidth, videoHeight, videoFramerate, videoBitrate, videoKeyframeInterval, audioSampleRate, audioChannelCount);
                    this.internalRecorder = new MediaRecorderiOS(nativeRecorder, videoWidth, videoHeight, readbackFormat, recordingPath, recordingCallback);
                    break;
                }
                case RuntimePlatform.Android: {
                    var recordingPath = Path.Combine(recordingDirectory, recordingFilename);
                    var nativeRecorder = new AndroidJavaObject(@"com.yusufolokoba.natcorder.MP4Recorder", videoWidth, videoHeight, videoFramerate, videoBitrate, videoKeyframeInterval, audioSampleRate, audioChannelCount);
                    this.internalRecorder = new MediaRecorderAndroid(nativeRecorder, videoWidth, videoHeight, recordingPath, recordingCallback);
                    break;
                }
                default:
                    Debug.LogError("NatCorder Error: MP4Recorder is not supported on this platform");
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
        /// Acquire a video frame for encoding.
        /// You will render or blit to this frame then commit it.
        /// </summary>
        [Doc(@"AcquireFrame", @"AcquireFrameDiscussion")]
        public RenderTexture AcquireFrame () {
            return internalRecorder.AcquireFrame();
        }

        /// <summary>
        /// Commit a video frame for encoding
        /// </summary>
        /// <param name="frame">Video frame to commit, must have been previously acquired from this recorder</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds</param>
        [Doc(@"CommitFrame", @"CommitFrameDiscussion"), Code(@"RecordCamera")]
        public void CommitFrame (RenderTexture frame, long timestamp) {
            internalRecorder.CommitFrame(frame, timestamp);
        }

        /// <summary>
        /// Commit an audio sample buffer for encoding
        /// </summary>
        /// <param name="sampleBuffer">Raw PCM audio sample buffer, interleaved by channel</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds</param>
        [Doc(@"CommitSamples", @"CommitSamplesDiscussion"), Code(@"RecordPCM")]
        public void CommitSamples (float[] sampleBuffer, long timestamp) {
            internalRecorder.CommitSamples(sampleBuffer, timestamp);
        }
        #endregion

        private readonly IMediaRecorder internalRecorder;
    }
}