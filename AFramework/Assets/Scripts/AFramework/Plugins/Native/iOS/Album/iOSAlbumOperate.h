//iOS相册操作 : 打开相册,保存截图/视频到相册

#import<QuartzCore/CADisplayLink.h>
//用CADisplayLink可以实现不停重绘。

//UIPopoverPresentationControllerDelegate : 弹出页神器
//UINavigationControllerDelegate : 转场动画
//UIImagePickerControllerDelegate : 选择图片

@interface IOSAlbumOperate : UIViewController<UIImagePickerControllerDelegate,UINavigationControllerDelegate,UIPopoverPresentationControllerDelegate>
{
    NSString* RecordMsgPrefix;
    NSString* AlbumMsgPrefix;
    bool isFirstRequestPermission;
}
-(void)OpenAlbum:(UIImagePickerControllerSourceType)type;
-(void)imagePickerController:(UIImagePickerController *)picker;
-(void)SaveFileToDoc:(UIImage *)image path:(NSString *)path;
- (void)imagePickerControllerDidCancel:(UIImagePickerController*)picker;
-(NSString*)GetSavePath:(NSString *)filename;
- (UIImage *)fixOrientation:(UIImage *)aImage;
-(void) saveImageToAlbum:(NSString*) sourcePath;
-(void) image:(UIImage*)image didFinishSavingWithError:(NSError*)error contextInfo:(void*)contextInfo;
-(void)saveVideo:(NSString *)sourcePath;
-(void) savedVedioImage:(UIImage*)image didFinishSavingWithError: (NSError *)error contextInfo: (void *)contextInfo;
-(void)GetAudioRecordPermission :(NSString*)msgPrefix;
-(void)CheckPhotoPermission : (NSString*)msgPrefix;

-(UIView *)getView;
@end
