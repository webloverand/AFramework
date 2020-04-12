//
//  UnityAdapter.h
//  Unity-iPhone
//
//  Created by 邓顺好 on 2019/12/25.
//

#import "UnityAdapter.h"

@implementation UnityAdapter
static UnityAdapter * s_instance_dj_singleton = nil ;
//单例
+ (UnityAdapter *)sharedInstance{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        if (s_instance_dj_singleton == nil) {
            s_instance_dj_singleton = [[UnityAdapter alloc] initSingle];
            
        }
    });
    return (UnityAdapter *)s_instance_dj_singleton;
}

- (id)initSingle{
    self = [super init];
    if (self) {
        iosSysteninfo = [IOSSystemInfo alloc];
        iOSAlbumOperate = [IOSAlbumOperate alloc];
        iOSSystemSetting = [[IOSSystemSetting alloc] init];
    }
    return self;
}
//类之间交互函数
-(void)SetIngoreAudioChange:(bool)isIngore
{
    [iOSSystemSetting SetIngoreAudioChange:isIngore];
}
//供C调用函数
-(void)stopOtherAudio
{
    [iOSSystemSetting stopOtherAudio];
}
-(void)resumeOtherAudio
{
    [iOSSystemSetting resumeOtherAudio];
}
-(void)RegisterVolumeChangeListene
{
    [iOSSystemSetting RegisterVolumeChangeListener];
}
-(void)UnRegisterVolumeChangeListene
{
    [iOSSystemSetting UnRegisterVolumeChangeListener];
}
-(void)GetCurrentVolum
{
    [iOSSystemSetting getCurrentVolum];
}
-(void)setVolume:(float)volume
{
    [iOSSystemSetting setVolume:volume];
}

-(void)GetUUIDInKeychai
{
    NSString *s = [iosSysteninfo getUUIDInKeychain];
    [SendToUnity SendMsg: [@"GetUUID~" stringByAppendingString:s]];
}
-(void)DeleteKeyChain
{
    [iosSysteninfo deleteKeyChain];
}
-(void)GetRegion
{
    [iosSysteninfo getRegion];
}
-(void)GetIphoneName
{
    [SendToUnity SendMsg: [@"ManufacturerNative~"  stringByAppendingString:[iosSysteninfo getIphoneName]]];
}

// 打开相册
-(void)OpenAlbum
{
    UIViewController *vc = UnityGetGLViewController();
    [vc.view addSubview:[iOSAlbumOperate getView]];
    [iOSAlbumOperate OpenAlbum:UIImagePickerControllerSourceTypePhotoLibrary];
}
-(void)SaveImageToAlbum: (NSString*) sourcePath
{
    [iOSAlbumOperate  saveImageToAlbum:sourcePath];
}
-(void)SaveVideoToAlbum: (NSString*) sourcePath
{
    [iOSAlbumOperate saveVideo:sourcePath];
}
//改成传string值
-(void)GetPhotoPermission : (NSString*) msgPrefix
{
    [iOSAlbumOperate  CheckPhotoPermission:msgPrefix];
}
-(void)GetRecordPermission
{
    [iOSAlbumOperate  GetAudioRecordPermission : @"CheckAudioPermission~"];
}
-(void)GetVideoRecordPermission
{
    [iOSAlbumOperate  GetAudioRecordPermission : @"CheckRecordVedioPermission~"];
}
-(void)ToAPPSetting
{
    [iOSSystemSetting  ToAPPSetting];
}
- (void)exitUnityAdapter
{
    iOSSystemSetting = nil;
    iOSAlbumOperate = nil;
    iosSysteninfo = nil;
    s_instance_dj_singleton = nil;
}
@end
#if defined (__cplusplus)
extern "C" {
#endif
void exitUnityAdapter()
{
    [[UnityAdapter sharedInstance] exitUnityAdapter];
}
void stopOtherAudio()
{
    [[UnityAdapter sharedInstance] stopOtherAudio];
}
void resumeOtherAudio()
{
    [[UnityAdapter sharedInstance] resumeOtherAudio];
}
void RegisterVolumeChangeListener()
{
    [[UnityAdapter sharedInstance] RegisterVolumeChangeListene];
}
void UnRegisterVolumeChangeListener()
{
    [[UnityAdapter sharedInstance] UnRegisterVolumeChangeListene];
}
void GetCurrentVolume()
{
    [[UnityAdapter sharedInstance] GetCurrentVolum];
}
void setVolume(float volume)
{
    [[UnityAdapter sharedInstance] setVolume:volume];
}

void GetUUIDInKeychain()
{
    [[UnityAdapter sharedInstance] GetUUIDInKeychai];
}
void DeleteKeyChain()
{
    [[UnityAdapter sharedInstance] DeleteKeyChain];
}

void GetRegion()
{
    [[UnityAdapter sharedInstance] GetRegion];
}
void GetIphoneName()
{
    [[UnityAdapter sharedInstance] GetIphoneName];
}

// 打开相册
void iosOpenAlbum()
{
    [[UnityAdapter sharedInstance] OpenAlbum];
}
void iosSaveImageToAlbum(char* sourcePathP)
{
    NSString* sourcePath = [NSString stringWithUTF8String:sourcePathP];
    [[UnityAdapter sharedInstance] SaveImageToAlbum:sourcePath];
}
void iosSaveVideoToAlbum(char* sourcePathP)
{
    NSString* sourcePath = [NSString stringWithUTF8String:sourcePathP];
    [[UnityAdapter sharedInstance] SaveVideoToAlbum:sourcePath];
}
//改成传string值
void iosGetPhotoPermission(char* msgPrefix)
{
    NSString* msgPre = [NSString stringWithUTF8String:msgPrefix];
    [[UnityAdapter sharedInstance] GetPhotoPermission:msgPre];
}
void iosGetRecordPermission()
{
    [[UnityAdapter sharedInstance] GetRecordPermission];
}
void iosGetVideoRecordPermission()
{
    [[UnityAdapter sharedInstance] GetVideoRecordPermission];
}
void iOSToAPPSetting()
{
    [[UnityAdapter sharedInstance] ToAPPSetting];
}
# if defined (__cplusplus)
}
#endif
