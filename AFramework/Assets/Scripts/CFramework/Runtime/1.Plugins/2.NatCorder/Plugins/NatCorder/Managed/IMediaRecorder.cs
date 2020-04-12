/* 
*   NatCorder
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder {

    using UnityEngine;
    using System;

    /// <summary>
    /// A recorder capable of recording video frames (and optionally audio frames) to a media output
    /// </summary>
    public interface IMediaRecorder : IDisposable {
        /// <summary>
        /// Acquire a frame for encoding
        /// You will render or blit to this frame then commit it
        /// </summary>
        RenderTexture AcquireFrame ();
        /// <summary>
        /// Commit a frame for encoding
        /// </summary>
        /// <param name="frame">Video frame to commit, must have been previously acquired from this recorder</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds</param>
        void CommitFrame (RenderTexture frame, long timestamp);
        /// <summary>
        /// Commit an audio sample buffer for encoding
        /// </summary>
        /// <param name="sampleBuffer">Raw PCM audio sample buffer, interleaved by channel</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds</param>
        void CommitSamples (float[] sampleBuffer, long timestamp);
    }
}