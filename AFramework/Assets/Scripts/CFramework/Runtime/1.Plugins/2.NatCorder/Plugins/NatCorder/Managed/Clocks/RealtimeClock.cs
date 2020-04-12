/* 
*   NatCorder
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Clocks {

    using UnityEngine;
    using System.Runtime.InteropServices;
    using Docs;

    /// <summary>
    /// Realtime clock for generating timestamps
    /// </summary>
    [Doc(@"RealtimeClock")]
    public sealed class RealtimeClock : IClock {
        
        #region --Op vars--
        private long? initialTimestamp; // absolute
        private long lastEventTime;     // absolute
        private long pausedTime;        // delta
        private readonly object timeFence = new object();
        #endregion
        

        #region --Client API--
        /// <summary>
        /// Create a realtime clock
        /// </summary>
        [Doc(@"RealtimeClockCtor")]
        public RealtimeClock () {
            #if !UNITY_EDITOR && UNITY_ANDROID
            system = system ?? new AndroidJavaClass(@"java.lang.System");
            #endif
        }

        /// <summary>
        /// Current timestamp in nanoseconds.
        /// The very first value reported by this property will always be zero.
        /// Do not use this property if the clock is paused as the reported value will be incorrect until the clock is resumed.
        /// </summary>
        [Doc(@"Timestamp", @"RealtimeClockTimestampDiscussion")]
        public long Timestamp {
            get {
                lock (timeFence) {
                    // Sycnhronize the native call to protect `initialTimestamp` offset
                    var now = CurrentTimestamp();
                    initialTimestamp = initialTimestamp ?? now;
                    if (IsPaused)
                        Debug.LogWarning("NatCorder Warning: Realtime clock will report wrong time when clock is paused!");
                    return now - (long)initialTimestamp - pausedTime;
                }
            }
        }

        /// <summary>
        /// Is the clock paused?
        /// </summary>
        [Doc(@"RealtimeClockIsPaused")]
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Pause the clock.
        /// This is useful for pausing and resuming recordings
        /// </summary>
        [Doc(@"RealtimeClockPause")]
        public void Pause () {
            lock (timeFence) {
                if (IsPaused)
                    return; // Can't trust anyone :(
                lastEventTime = CurrentTimestamp();
                IsPaused = true;
            }
        }

        /// <summary>
        /// Resume the clock
        /// </summary>
        [Doc(@"RealtimeClockResume")]
        public void Resume () {
            lock (timeFence) {
                if (!IsPaused)
                    return;
                pausedTime += CurrentTimestamp() - lastEventTime;
                IsPaused = false;
            }
        }
        #endregion


        #region --Operations--

        private const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        @"__Internal";
        #else
        @"NatCorder";
        #endif

        #if UNITY_IOS || UNITY_WEBGL || UNITY_STANDALONE || UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = @"NCCurrentTimestamp")]
        public static extern long CurrentTimestamp ();
        #elif UNITY_ANDROID
        private static AndroidJavaClass system;

        public static long CurrentTimestamp () {
            AndroidJNI.AttachCurrentThread();
            return system.CallStatic<long>(@"nanoTime");
        }
        #else
        public static long CurrentTimestamp () { return 0L; }
        #endif
        #endregion
    }
}