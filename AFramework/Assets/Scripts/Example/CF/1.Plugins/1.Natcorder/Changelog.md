## NatCorder 1.5.1
+ Add support for multiple game cameras in `CameraInput`.
+ Fixed memory leak when recording on Windows.
+ Refactored `CameraInput` to use instance constructor instead of static factory method.
+ Refactored `AudioInput` to use instance constructor instead of static factory method.

## NatCorder 1.5.0
+ Completely overhauled front-end API to provide a more recorder-oriented approach. Check out the README.md for more info.
+ Added support for running multiple recording sessions simultaneously on devices that support this.
+ Fixed crash when recording was stopped on some Android devices.
+ Refactored NatCorder namespace from `NatCorderU.Core` to `NatCorder`.
+ Refactored recorder input namespace from `NatCorderU.Core.Recorders` to `NatCorder.Inputs`.
+ Refactored recording clock namespace from `NatCorderU.Core.Clocks` to `NatCorder.Clocks`.
+ Refactored `CameraRecorder` to `CameraInput`.
+ Refactored `AudioRecorder` to `AudioInput`.
+ Refactored `IClock.CurrentTimestamp` to `IClock.Timestamp`.
+ Deprecated `NatCorder` class.
+ Deprecated `RecordingCallback` delegate type.
+ Deprecated `Container` enum.
+ Deprecated `VideoFormat` struct.
+ Deprecated `AudioFormat` struct.
+ Deprecated `IRecorder` struct.

## NatCorder 1.4.1
+ Greatly improved memory stability on iOS.
+ Fixed crash when GIF recording is stopped on iOS.
+ Fixed crash when recording MP4 with audio on Android.

## NatCorder 1.4.0
+ Greatly improved recording stability and performance on Android. As a result, NatCorder now requires a minimum of API level 23.
+ Greatly improved recording stability and performance on iOS Metal. As a result, NatCorder now requires a minimum of iOS 8.
+ Greatly improved GIF visual quality on Android.
+ Dropped support for OpenGL ES on iOS. NatCorder will only use Metal on iOS. 
+ Added support for the new Lightweight Render Pipeline (LWRP) and High Definition Render Pipeline (HDRP) with `CameraRecorder`.
+ Update iOS and macOS backend to generate `.mp4` instead of `.mov` file when recording MP4.
+ Added `FixedIntervalClock` for generating timestamps to maintain a constant framerate in recorded videos.
+ Added aspect fitting in `CameraRecorder`. This will ensure that videos will not appear stretched in the case of app autorotation or uneven recording sizes.
+ Added support for linear rendering on macOS and Windows.
+ Added a dedicated GIF recording example called Giffy.
+ Fixed `BufferOverflowException` when recording with audio on some Android devices.
+ Fixed `DllNotFoundException` when running on macOS.
+ Deprecated `VideoFormat.Screen` property. Manually create a video format with your intended resolution.
+ Deprecated `CameraRecorder.recordingMaterial` property. Use Image Effects instead.
+ Removed GIF recording from ReplayCam.

## NatCorder 1.3f2
+ Added `IClock` interface and `RealtimeClock` class. Clocks are lightweight objects that generate extremely accurate timestamps for recording. They allow for audio to be perfectly synchronized with video when recording with audio.
+ Added full support for pausing and resuming recording with `Recorder` classes with the new `Clock` infrastructure.
+ Changed `NatCorder.CommitFrame` to take in a `RenderTexture` with a corresponding timestamp.
+ Fixed duplicate key error when `NatCorder.CommitFrame` is called on iOS.
+ Fixed `CameraRecorder` destroying recording material once recording is finished.
+ Fixed unallocated buffer exception being raised when recording with audio on iOS 12.
+ Fixed rare `NullPointerException` crash when recording MP4 on Android.
+ Deprecated `AudioRecorder.Create` overload that took in both an `AudioSource` and `AudioListener`.
+ Deprecated `Frame` class.

## NatCorder 1.3f1
+ Added GIF recording on iOS, Android, macOS, and Windows!
+ Added `Recorders` namespace, `VideoRecorder` component, and `AudioRecorder` component for quickly recording different gameobjects like cameras and audio sources.
+ Added proper support for offline recording, where a set of pre-rendered frames are all committed to NatCorder in one loop.
+ Added `Container` enumeration for specifying container format for recording (MP4 or GIF).
+ Added `VideoFormat` and `AudioFormat` structs for configuring recording.
+ Fixed crash on macOS and iOS when very short video (less than 1 second) is recorded.
+ Ensure that `IsRecording` properly changes immediately after `StartRecording` and `StopRecording`.
+ Improved speed of `RecordingCallback` being invoked on WebGL.
+ Refactored `VideoCallback` to `RecordingCallback`.
+ Deprecated `Replay` API.
+ Deprecated `IAudioSource` interface.
+ Deprecated `Configuration` struct.

## NatCorder 1.2f2
+ Improved recording stability on iOS.
+ File paths on iOS and macOS are no more prepended with the `file://` protocol.
+ Fixed tearing in recorded video on iOS Metal.
+ Fixed audio being slightly behind of video on iOS and macOS.
+ Fixed crash on Android when very short video (less than 1 second) is recorded.
+ Fixed null reference exception when recording is stopped on OSX, Windows, and WebGL.

## NatCorder 1.2f1
+ We have significantly improved recording performance in iOS apps, especially in apps using the Metal API.
+ The Windows backend is no more experimental! It is now fully supported.
+ When recording with the `Replay` API, aspect fitting will be applied to prevent stretching in the video.
+ We have deprecated the Sharing API because we introduced a dedicated sharing API, [NatShare](https://github.com/olokobayusuf/NatShare-API).
+ Fixed audio stuttering in recorded videos on Windows.
+ Fixed tearing in recorded video when using OpenGL ES on iOS.
+ Fixed rare crash when recording in app that uses Metal API on iOS.
+ Fixed tearing and distortion in recorded video on macOS.
+ Fixed microphone audio recording from an older time in ReplayCam example.
+ Fixed crash when user tries to save video to camera roll from sharing dialog on iOS.
+ Deprecated `Microphone` API. Use `UnityEngine.Microphone` instead.
+ Deprecated `NatCorder.Verbose` flag.

## NatCorder 1.1f1
+ We have added a native macOS backend! The NatCorder recording API is now fully supported on macOS.
+ We have also added a native Windows backend! This backend is still experimental so it should not be used in production builds.
+ The Standalone backend (using FFmpeg) has been deprecated because we have added native Windows and macOS implementations.
+ We have significantly improved recording stability on Android especially for GPU-bound games.
+ Added support for different Unity audio DSP latency modes.
+ Added `sampleCount` property in `IAudioSource` interface.
+ Fixed crash when `StartRecording` is called on iOS running OpenGL ES2 or ES3.
+ Fixed rare crash on Android when recording with audio.
+ Fixed audio-video timing discrepancies on Android.
+ Fixed video tearing on Android when app does not use multithreaded rendering.
+ Fixed `FileUriExposedException` when `Sharing.Share` is called on Android 24 or newer.
+ Fixed `Sharing.GetThumbnail` not working on iOS.
+ Fixed `Sharing.SaveToCameraRoll` failing when permission is requested and approved on iOS.
+ Fixed `Sharing.SaveToCameraRoll` not working on Android.
+ Fixed rare crash on Android when a very short video (less than 1 second) is recorded.
+ Fixed build failing due to missing symbols for Sharing library on iOS.
+ Improved on microphone audio stuttering in ReplayCam example by adding minimal `Microphone` API in `NatCorderU.Extensions` namespace.
+ Refactored `Configuration.Default` to `Configuration.Screen`.
+ *Everything below*

## NatCorder 1.0f1
+ First release