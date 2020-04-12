# NatCorder API
NatCorder is a lightweight, easy-to-use, native video recording API for iOS and Android. NatCorder comes with a rich featureset including:
+ Record anything that can be rendered into a texture.
+ Record to MP4 videos and animated GIF images.
+ Control recording quality and file size with bitrate and keyframe interval.
+ Record at any resolution. You get to specify what resolution recording you want.
+ Get path to recorded video in device storage.
+ Record game audio with video.
+ Support for recording on macOS in the Editor or in Standalone builds.
+ Support for recording on Windows in the Editor or in Standalone builds.
+ Experimental support for recording on WebGL.

## Fundamentals of Recording
NatCorder provides a simple recording API with instances of the `IMediaRecorder` interface. **NatCorder works by encoding video and audio frames on demand**. To start recording, simply create a recorder corresponding to the media type you want to record:
```csharp
var gifRecorder = new GIFRecorder(...);
var videoRecorder = new MP4Recorder(...);
```

Once you create a recorder, you then commit frames to it. You can commit video and audio frames to these recorders. These committed frames are then encoded into a media file. When committing frames, you must provide the frame data with a corresponding timestamp. The spacing timestamps determine the final frame rate of the recording.

### Committing Video Frames
NatCorder records video using `RenderTexture`s. The general workflow is a three-step process:
1. Acquire a `RenderTexture` using `IMediaRecorder.AcquireFrame`
2. Blit or render to the `RenderTexture`
3. Commit the `RenderTexture` for encoding using `IMediaRecorder.CommitFrame`

When committing the `RenderTexture`s for encoding, you will need to provide a corresponding timestamp. For this purpose, you can use implementations of the `IClock` interface. Here is an example illustrating recording a `WebCamTexture`:
```csharp
WebCamTexture webcamPreview; // Start this somewhere
IMediaRecorder mediaRecorder;
IClock clock;

void StartRecording () {
    // Start recording
    clock = new RealtimeClock();
    mediaRecorder = new ... // Create a recorder here
}

void Update () {
    // Check that we are recording
    if (mediaRecorder != null) {
        var frame = mediaRecorder.AcquireFrame();           // Acquire a video frame
        Graphics.Blit(webcamPreview, frame);                // Blit the webcam preview to the frame
        mediaRecorder.CommitFrame(frame, clock.Timestamp);  // Commit the frame to the recorder
    }
}

void StopRecording () {
    // Stop recording
    mediaRecorder.Dispose();
    mediaRecorder = null;
}
```

### Committing Audio Frames
NatCorder records audio provided as interleaved PCM sample buffers (`float[]`). Similar to recording video frames, you will call the `IMediaRecorder.CommitSamples` method, passing in a sample buffer and a corresponding timestamp. It is important that the timestamps synchronize with those of video, so make sure to use the same `IClock` for generating video and audio timestamps. Below is an example illustrating recording game audio using Unity's `OnAudioFilterRead` callback:
```csharp
void OnAudioFilterRead (float[] data, int channels) {
    // Check that we are recording
    if (mediaRecorder != null)
        // Commit the audio frame
        mediaRecorder.CommitSamples(data, clock.Timestamp);
}
```

## Easier Recording with Recorder Inputs
In most cases, you will likely just want to record a game camera optionally with game audio. To do so, you don't need to manually acquire, blit, and commit frames. Instead, you can use NatCorder's recorder `Inputs`. A recorder `Input` is a lightweight utility class that eases out the process of recording some aspect of a Unity application. NatCorder comes with two recorder inputs: `CameraInput` and `AudioInput`. You can create your own recorder inputs to do more interesting things like add a watermark to the video, or retime the video. Here is a simple example showing recording a game camera:
```csharp
IMediaRecorder mediaRecorder;
CameraInput cameraInput;
AudioInput audioInput;

void StartRecording () {
    // Start recording
    mediaRecorder = new ...;
    // Create a camera input to record the main camera
    cameraInput = CameraInput.Create(mediaRecorder, Camera.main);
    // Create an audio input to record the scene's AudioListener
    audioInput = AudioInput.Create(mediaRecorder, sceneAudioListener);
}

void StopRecording () {
    // Destroy the recording inputs
    cameraInput.Dispose();
    audioInput.Dispose();
    // Stop recording
    mediaRecorder.Dispose();
    mediaRecorder = null;
}
```

___

## Limitations of the WebGL Backend
The WebGL backend is currently experimental. As a result, it has a few limitations in its operations. Firstly, it is an 'immediate-encode' backend. This means that video frames are encoded immediately they are committed to NatCorder. As a result, there is no support for custom frame timing (the `timestamp` provided to `CommitFrame` is always ignored).

Secondly, because Unity does not support the `OnAudioFilterRead` callback on WebGL, we cannot record game audio on WebGL (using an `AudioSource` or `AudioListener`). This is a limitation of Unity's WebGL implementation. However, you can still record raw audio data using the `IMediaRecorder.CommitSamples` API.

The `MP4Recorder` may record videos with the VP8/9 codec or H.264 codec, depending on the browser and device. These videos are always recorded in the `webm` container format. The `GIFRecorder` is not supported on WebGL.

## Using NatCorder with NatCam
If you use NatCorder with our NatCam camera API, then you will have to remove a duplicate copy of the `NatRender.aar` library **from NatCam**. The library can be found at `NatCam > Plugins > Android > NatRender.aar`.

## Tutorials
- [Unity Recording Made Easy](https://medium.com/@olokobayusuf/natcorder-unity-recording-made-easy-f0fdee0b5055)
- [Audio Workflows](https://medium.com/@olokobayusuf/natcorder-tutorial-audio-workflows-1cfce15fb86a)

## Requirements
- On Android, NatCorder requires API Level 23 and up
- On iOS, NatCorder requires iOS 8 and up
- On macOS, NatCorder requires macOS 10.13 and up
- On Windows, NatCorder requires Windows 8 and up
- On WebGL, NatCorder requires Chrome 47 or Safari 27 and up

## Notes
- NatCorder doesn't support recording UI canvases that are in Screen Space - Overlay mode. See [here](https://forum.unity3d.com/threads/render-a-canvas-to-rendertexture.272754/#post-1804847).
- On iOS, NatCorder requires the Metal graphics API. OpenGL ES is not supported on iOS.
- When building for WebGL, make sure that 'Use Prebuild Engine' is disabled in Build Settings.
- When recording audio, make sure that the 'Bypass Listener Effects' and 'Bypass Effects' flags on your `AudioSource`s are turned off.

## Quick Tips
- Please peruse the included scripting reference [here](https://olokobayusuf.github.io/NatCorder-Docs/)
- To discuss or report an issue, visit Unity forums [here](https://forum.unity.com/threads/natcorder-video-recording-api.505146/)
- Contact me at [olokobayusuf@gmail.com](mailto:olokobayusuf@gmail.com)

Thank you very much!