//
//  AudioRecord.h
//  Unity-iPhone
//
//  Created by 邓顺好 on 2020/1/2.
//

#import <AVFoundation/AVFoundation.h>
#import <MediaPlayer/MediaPlayer.h>

@interface AudioRecord : NSObject
{
    AudioComponentInstance audioUnit;
    AudioComponentInstance ioUnit;
    AudioComponent ioUnitRef;
    AudioStreamBasicDescription streamDescription;
    ExtAudioFileRef audioFileRef;
    bool isAudioReady;
}

- (void)CheckRecordPermission;
- (void)readyForRecord;
-(bool)isReadyToRecord;
- (void)StartRecord;
- (void)stopRecordAudioUnit;
@end
