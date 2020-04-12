//
//  AudioRecord.m
//  Unity-iPhone
//
//  Created by 邓顺好 on 2020/1/2.
//

#import <Foundation/Foundation.h>
#import "AudioRecord.h"
#import "SendToUnity.h"

#define kInputBus 1
#define kOutputBus 0

@implementation AudioRecord
- (void)CheckRecordPermission {
    AVAuthorizationStatus videoAuthStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio];
    if (videoAuthStatus == AVAuthorizationStatusNotDetermined) {
        // 未询问用户是否授权
        AVAudioSession *audioSession = [AVAudioSession sharedInstance];
        if ([audioSession respondsToSelector:@selector(requestRecordPermission:)]) {
            [audioSession performSelector:@selector(requestRecordPermission:) withObject:^(BOOL granted) {
                if (granted) {
                    //用户选择允许
                    [SendToUnity SendBlueMsg:@"CheckRecordVedioPermission~1"];
                } else {
                    //用户选择不允许
                    [SendToUnity SendBlueMsg:@"CheckRecordVedioPermission~0"];
                }
            }];
        }
    } else if(videoAuthStatus == AVAuthorizationStatusRestricted || videoAuthStatus == AVAuthorizationStatusDenied) {
        //用户在第一次系统弹窗后选择不允许之后，再次录音的时候会走这里“麦克风权限未授权”
        [SendToUnity SendBlueMsg:@"CheckRecordVedioPermission~0"];
    } else{
        // 已授权
        [SendToUnity SendBlueMsg:@"CheckRecordVedioPermission~2"];
    }
}



-(bool)isReadyToRecord
{
    if (isAudioReady == NO) {
        [self readyForRecord];
        if (isAudioReady) {
            return true;
        } else {
            return false;
        }
    }
    else
    {
        return true;
    }
}
//录音辅助函数
- (void)StartRecord {
    [[AVAudioSession sharedInstance] setActive:YES error:nil];
    AudioOutputUnitStart(audioUnit); //输出音频
}
- (void)stopRecordAudioUnit {
    AudioOutputUnitStop(audioUnit);
    [[AVAudioSession sharedInstance] setActive:AVAudioSessionCategoryPlayAndRecord withOptions:AVAudioSessionSetActiveOptionNotifyOthersOnDeactivation
                                         error:nil];
}
//录音准备工作
-(void)readyForRecord {
    if ([self hasMicAuthorization]) {
        //重定向音频
        [self startAudioSession];
        [self prepareAudioUnit];
        [self switchBluetooth];
        [self setupAudioUnit_recordAndSpeak];
        [self setupRecordCallback];
        [self setupPlayCallback];
        isAudioReady = YES;
    } else {
        isAudioReady = NO;
    }
}
- (BOOL)hasMicAuthorization {
    AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio];
    if (authStatus == AVAuthorizationStatusNotDetermined || authStatus == AVAuthorizationStatusRestricted || authStatus ==AVAuthorizationStatusDenied) {
        
        return NO;
    }
    return YES;
}
- (void)startAudioSession {
    AVAudioSession *audioSession = [AVAudioSession sharedInstance];
    //通过重写audio route属性来重定向音频。使输出音频到音频蓝牙中
    [audioSession setCategory:AVAudioSessionCategoryPlayAndRecord withOptions:AVAudioSessionCategoryOptionAllowBluetoothA2DP
                        error:nil];
}
- (void)prepareAudioUnit {
    AudioComponentDescription description;
    description.componentType = kAudioUnitType_Output;
    description.componentSubType = kAudioUnitSubType_RemoteIO;//type对应的子类型
    description.componentFlags = 0;
    description.componentFlagsMask = 0;
    description.componentManufacturer = kAudioUnitManufacturer_Apple;//厂商
    
    ioUnitRef = AudioComponentFindNext(NULL, &description);
    CheckStatus(AudioComponentInstanceNew(ioUnitRef, &audioUnit), @"初始化音频单元失败", NO);
}
- (void)switchBluetooth {
    AVAudioSessionPortDescription *_bluetoothPort = [self bluetoothAudioDevice];
    BOOL changeResult = [[AVAudioSession sharedInstance] setPreferredInput:_bluetoothPort
                                                                     error:nil];
    
    NSLog(@"changeResult:%d", changeResult);
}
- (void)setupAudioUnit_recordAndSpeak {
    UInt32 flag = 1;
    CheckStatus(AudioUnitSetProperty(audioUnit, kAudioOutputUnitProperty_EnableIO, kAudioUnitScope_Input, kInputBus, &flag, sizeof(flag)), @"打开麦克风成功", NO);
    CheckStatus(AudioUnitSetProperty(audioUnit, kAudioOutputUnitProperty_EnableIO, kAudioUnitScope_Output, kOutputBus, &flag, sizeof(flag)), @"扬声器配置失败", NO);
    
    streamDescription.mFormatID = kAudioFormatLinearPCM;
    streamDescription.mSampleRate = 44100.00;
    streamDescription.mFormatFlags = kAudioFormatFlagIsSignedInteger | kAudioFormatFlagIsPacked;
    streamDescription.mChannelsPerFrame = 1;
    streamDescription.mFramesPerPacket = 1;
    streamDescription.mBitsPerChannel = 16;
    streamDescription.mBytesPerFrame = 2;
    streamDescription.mBytesPerPacket = 2;
    
    CheckStatus(AudioUnitSetProperty(audioUnit, kAudioUnitProperty_StreamFormat, kAudioUnitScope_Output, kInputBus, &streamDescription, sizeof(streamDescription)), @"设置麦克风音频参数", NO);
    CheckStatus(AudioUnitSetProperty(audioUnit, kAudioUnitProperty_StreamFormat, kAudioUnitScope_Input, kOutputBus, &streamDescription, sizeof(streamDescription)), @"设置扬声器音频参数", NO);
    
}
- (void)setupRecordCallback {
    AURenderCallbackStruct renderCallbackStruct;
    renderCallbackStruct.inputProc = recordingCallback;
    renderCallbackStruct.inputProcRefCon = (__bridge void *)self;
    CheckStatus(AudioUnitSetProperty(audioUnit, kAudioOutputUnitProperty_SetInputCallback, kAudioUnitScope_Global, kInputBus, &renderCallbackStruct, sizeof(renderCallbackStruct)), @"设置录音回调", NO);
}

- (void)setupPlayCallback {
    AURenderCallbackStruct playCallbackStruct;
    playCallbackStruct.inputProcRefCon = (__bridge void *)self;
    playCallbackStruct.inputProc = playbackCallback;
    CheckStatus(AudioUnitSetProperty(audioUnit, kAudioUnitProperty_SetRenderCallback, kAudioUnitScope_Group, 0, &playCallbackStruct, sizeof(playCallbackStruct)), @"设置播放回调", NO);
}

//录音准备的辅助函数
- (AVAudioSessionPortDescription*)audioDeviceFromTypes:(NSArray*)types
{
    NSArray* routes = [[AVAudioSession sharedInstance] availableInputs];
    for (AVAudioSessionPortDescription* route in routes)
    {
        if ([types containsObject:route.portType])
        {
            return route;
        }
    }
    return nil;
}
- (AVAudioSessionPortDescription*)bluetoothAudioDevice {
    NSArray* bluetoothRoutes = @[AVAudioSessionPortBluetoothA2DP, AVAudioSessionPortBluetoothLE, AVAudioSessionPortBluetoothHFP];
    return [self audioDeviceFromTypes:bluetoothRoutes];
}
static OSStatus playbackCallback(void *inRefCon,
                                 AudioUnitRenderActionFlags *ioActionFlags,
                                 const AudioTimeStamp *inTimeStamp,
                                 UInt32 inBusNumber,
                                 UInt32 inNumberFrames,
                                 AudioBufferList *ioData) {
    AudioRecord *controller = (__bridge AudioRecord *)inRefCon;
    AudioUnitRender(controller->audioUnit, ioActionFlags, inTimeStamp, 1, inNumberFrames, ioData);
    return noErr;
}
static OSStatus recordingCallback(void *inRefCon,
                                  AudioUnitRenderActionFlags *ioActionFlags,
                                  const AudioTimeStamp *inTimeStamp,
                                  UInt32 inBusNumber,
                                  UInt32 inNumberFrames,
                                  AudioBufferList *ioData) {
    AudioRecord *controller = (__bridge AudioRecord *)inRefCon;
    
    AudioBufferList bufferList;
    bufferList.mNumberBuffers = 1;
    bufferList.mBuffers[0].mNumberChannels = 1;
    bufferList.mBuffers[0].mDataByteSize = 2 * inNumberFrames;
    bufferList.mBuffers[0].mData = malloc(inNumberFrames * 2);
    
    CheckStatus(AudioUnitRender(controller->audioUnit, ioActionFlags, inTimeStamp, inBusNumber, inNumberFrames, &(bufferList)), @"获取数据成", NO);
    
    return noErr;
    
}
static void CheckStatus(OSStatus status, NSString *errMsg, BOOL fatal) {
    if (status != noErr) {
        char fourcc[16];
        *fourcc = CFSwapInt32HostToBig(status);
        fourcc[4] = '\0';
        if (isprint(fourcc[0]) && isprint(fourcc[1]) && isprint(fourcc[2]) && isprint(fourcc[3])) {
            NSLog(@"%@ : %s", errMsg, fourcc);
        } else {
            NSLog(@"%@ : %d", errMsg, (int)status);
        }
        if (fatal) {
            exit(-1);
        }
    }
}
- (void)stopAudioSession {
    AVAudioSession *audioSession = [AVAudioSession sharedInstance];
    //通过重写audio route属性来重定向音频。
    [audioSession setCategory:AVAudioSessionCategoryAmbient
                  withOptions:AVAudioSessionCategoryOptionDefaultToSpeaker
                        error:nil];
}
@end

