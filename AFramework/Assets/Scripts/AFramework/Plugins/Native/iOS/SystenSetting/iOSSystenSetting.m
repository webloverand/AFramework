//
//  iOSSystenSetting.m
//  Unity-iPhone
//
//  Created by 邓顺好 on 2019/12/25.
//

#import <Foundation/Foundation.h>
#import "iOSSystemSetting.h"
#import <MediaPlayer/MediaPlayer.h>
#import <AVFoundation/AVFoundation.h>
#import "SendToUnity.h"

@interface IOSSystemSetting ()
@property (nonatomic, strong) MPVolumeView *volumeView;
@property (nonatomic, strong) UISlider* volumeViewSlider;
@end

@implementation IOSSystemSetting
- (IOSSystemSetting*)init
{
    
    return self;
}

- (UISlider *)volumeViewSlider {
    if (!_volumeViewSlider) {
        if (!_volumeView) {
            _volumeView = [[MPVolumeView alloc] init];
            _volumeView.backgroundColor = [UIColor whiteColor];
            _volumeView.layer.cornerRadius = 10.0;
            
            UIWindow *keyWindow = [UIApplication sharedApplication].keyWindow;
            _volumeView.frame = CGRectMake(10, 30, keyWindow.frame.size.width - 20, 44);
            
            _volumeView.hidden = YES;
            [[UIApplication sharedApplication].keyWindow addSubview:_volumeView];
        }
        
        UISlider* volumeViewSlider = nil;
        for (UIView *view in [self.volumeView subviews]) {
            if ([view.class.description isEqualToString:@"MPVolumeSlider"]) {
                volumeViewSlider = (UISlider*)view;
                break;
                
            }
        }
        if (volumeViewSlider) {
            volumeViewSlider.frame = CGRectMake(10, 44 / 2, self.volumeView.frame.size.width - 40, 44.0);
            _volumeViewSlider = volumeViewSlider;
            self.volumeView.showsVolumeSlider = YES;
            self.volumeView.showsRouteButton = NO;
        }
    }
    return _volumeViewSlider;
}


-(void)RegisterVolumeChangeListener
{
    //音量更改监听
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(volumeChanged:) name:@"AVSystemController_SystemVolumeDidChangeNotification" object:nil];
}
-(void)UnRegisterVolumeChangeListener
{
    //音量更改监听
    [[NSNotificationCenter defaultCenter] removeObserver:self  name:@"AVSystemController_SystemVolumeDidChangeNotification" object:nil];
}
//音量更改回调
- (void)volumeChanged:(NSNotification *)notification {
    if(!isIngoreVolumnChange && !isIngoreAudioChange)
    {
        [self getCurrentVolum];
    }
    else
    {
        isIngoreVolumnChange = false;
    }
}
- (void)setVolume: (float)value {
    isIngoreVolumnChange = true;
    NSLog(@"APP更改音量,忽略:%f",value);
    [self.volumeViewSlider setValue:value animated:YES];
    [self.volumeViewSlider sendActionsForControlEvents:UIControlEventTouchUpInside];
}
- (void) getCurrentVolum
{
    NSString *v = [NSString stringWithFormat:@"%0.1f", [self currentVolume]];
    NSLog(@"系统当前音量回调: %@",v);
    [SendToUnity SendMsg: [@"GetCurrentVolumn~" stringByAppendingString:v]];
}
- (float)currentVolume {
    if (self.volumeViewSlider) {
        if(self.volumeViewSlider.value!=0)
        {
            return self.volumeViewSlider.value;
        }
        else
        {
            return [AVAudioSession sharedInstance].outputVolume;
        }
    }
    return 0.0;
}

- (void)stopOtherAudio {
    isStopManual = true;
    if ([AVAudioSession sharedInstance].isOtherAudioPlaying) {
        hasOtherAudio = false;
        [[AVAudioSession sharedInstance] setActive:YES error:nil];
    }
}
- (void)resumeOtherAudio {
    if (hasOtherAudio && isStopManual) {
        isStopManual = false;
        [[AVAudioSession sharedInstance] setActive:NO withOptions:AVAudioSessionSetActiveOptionNotifyOthersOnDeactivation
                                             error:nil];
    }
}
-(void)ToAPPSetting {
    if ([[UIDevice currentDevice].systemVersion doubleValue]< 10.0){
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:UIApplicationOpenSettingsURLString]];
    }
    else{
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:UIApplicationOpenSettingsURLString] options:@{} completionHandler:nil];
    }
}

-(void)SetIngoreAudioChange:(bool)isIngore
{
    isIngoreAudioChange = isIngore;
}
@end

