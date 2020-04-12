//
//  iOSSystemSetting.h
//  Unity-iPhone
//
//  Created by 邓顺好 on 2019/12/25.
//


@interface IOSSystemSetting : NSObject
{
    bool isIngoreVolumnChange;
    bool isIngoreAudioChange;
    bool isStopManual;
    bool hasOtherAudio;
}
- (IOSSystemSetting*)init;
- (void)RegisterVolumeChangeListener;
- (void)UnRegisterVolumeChangeListener;
- (void) getCurrentVolum;
- (void)setVolume: (float)value;
- (float)currentVolume;
- (void)stopOtherAudio;
- (void)resumeOtherAudio;
-(void)ToAPPSetting;

-(void)SetIngoreAudioChange:(bool)isIngore;
@end
