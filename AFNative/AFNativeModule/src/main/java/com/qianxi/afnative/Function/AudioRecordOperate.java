package com.qianxi.afnative.Function;

import android.content.Context;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioRecord;
import android.media.AudioTrack;
import android.media.MediaRecorder;
import android.util.Log;

public class AudioRecordOperate {
    AudioRecord audioRecord;
    AudioTrack audioTrack;

    int frequency = 44100;
    int channelConfiguration = AudioFormat.CHANNEL_CONFIGURATION_MONO;
    int audioEncoding = AudioFormat.ENCODING_PCM_16BIT;
    int recBufSize,playBufSize;
    public  boolean isRecording = false;

    public AudioRecordOperate() {
        //录音初始化
        //这两个值设置小了会导致声音断断续续播放,有杂音的可能性是发声处离录音太近(硬件那边说的),因为我的需求是将录制的声音传送给硬件播放,所以不存在这个问题
        recBufSize = AudioRecord.getMinBufferSize(frequency,
                channelConfiguration, audioEncoding) * 4;
        playBufSize = AudioTrack.getMinBufferSize(frequency,
                channelConfiguration, audioEncoding) * 4;
        audioRecord = new AudioRecord(MediaRecorder.AudioSource.MIC, frequency,
                channelConfiguration, audioEncoding, recBufSize);
        audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, frequency,
                channelConfiguration, audioEncoding,
                playBufSize, AudioTrack.MODE_STREAM);
    }

    //检查是否有录音权限
    public boolean isHasRecordPermission(final Context context) {
        //开始录制音频
        try {
            // 防止某些手机崩溃，例如联想
            audioRecord.startRecording();
        } catch (IllegalStateException e) {
            e.printStackTrace();
        }
        /**
         * 根据开始录音判断是否有录音权限
         */
        if (audioRecord.getRecordingState() != AudioRecord.RECORDSTATE_RECORDING) {
            return false;
        }
        audioRecord.stop();
        return true;
    }
    public void StartRecord()
    {
        try {
            isRecording = true;
            Log.d("AudioRecordOperate", "AudioRecordOperate:开始录制" + isRecording);
            final byte[] buffer = new byte[recBufSize];
            audioRecord.startRecording();//开始录制
            audioTrack.play();//开始播放
            while (isRecording) {
                Log.d("AudioRecordOperate", "写入声音");
                //从MIC保存数据到缓冲区
                int bufferReadResult = audioRecord.read(buffer, 0,recBufSize);

                byte[] tmpBuf = new byte[bufferReadResult];
                System.arraycopy(buffer, 0, tmpBuf, 0, bufferReadResult);
                //写入数据即播放
                audioTrack.write(tmpBuf, 0, tmpBuf.length);
            }
        } catch (Throwable t) {
            Log.d("AudioRecordOperate", "错误信息:" + t.getMessage());
        }
    }
    public void StopRecord()
    {
        audioTrack.stop();
        audioRecord.stop();
        isRecording = false;
    }
}
