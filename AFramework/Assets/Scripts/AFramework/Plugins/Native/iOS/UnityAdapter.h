
#import "IOSAlbumOperate.h"
#import "IOSSystemInfo.h"
#import "SendToUnity.h"
#import "iOSSystemSetting.h"

@interface UnityAdapter : NSObject
{
    IOSAlbumOperate* iOSAlbumOperate;
    IOSSystemInfo* iosSysteninfo;
    IOSSystemSetting* iOSSystemSetting;
}
+ (UnityAdapter *)sharedInstance;
- (void)exitUnityAdapter;

-(void)SetIngoreAudioChange:(bool)isIngore;
@end
