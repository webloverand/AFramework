//iOS相册操作 : 打开相册,保存截图/视频到相册

#import "IOSAlbumOperate.h"
#import <PhotosUI/PhotosUI.h>
#import <AssetsLibrary/AssetsLibrary.h>
#import <Photos/PHPhotoLibrary.h>
#import <CoreLocation/CoreLocation.h>
#import "SendToUnity.h"

@implementation IOSAlbumOperate
-(UIView *)getView
{
    return self.view;
}
//打开相册 : 无法多选
-(void)OpenAlbum:(UIImagePickerControllerSourceType)type{
    //创建UIImagePickerController实例
    UIImagePickerController * picker = [[UIImagePickerController alloc] init];
    picker.sourceType = UIImagePickerControllerSourceTypeCamera;
    //设置代理
    picker.delegate = self;
    //是否允许编辑 (默认为NO)
    picker.allowsEditing = YES;
    //设置照片的来源
    picker.sourceType = type;
    //展示选取照片控制器
    picker.modalPresentationStyle = UIModalPresentationPopover;
    UIPopoverPresentationController *popover = picker.popoverPresentationController;
    popover.delegate = self;
    popover.sourceRect = CGRectMake(0, 0, 0, 0);
    popover.sourceView = self.view;
    popover.canOverlapSourceViewRect = true;
    popover.permittedArrowDirections = UIPopoverArrowDirectionAny;
    [self presentViewController:picker animated:YES completion:nil];
}
//选择图片响应函数
-(void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary<NSString *,id> *)info{
    [picker dismissViewControllerAnimated:YES completion:^{}];
    UIImage *image = [info objectForKey:@"UIImagePickerControllerEditedImage"];
    if (image == nil) {
        image = [info objectForKey:@"UIImagePickerControllerOriginalImage"];
    }
    //图片旋转
    if (image.imageOrientation != UIImageOrientationUp) {
        //图片旋转
        image = [self fixOrientation:image];
    }
    NSString *imagePath = [self GetSavePath:@"Temp.jpg"];
    [self SaveFileToDoc:image path:imagePath];
}
-(void)SaveFileToDoc:(UIImage *)image path:(NSString *)path{
    NSData *data;
    if (UIImagePNGRepresentation(image)==nil) {
        data = UIImageJPEGRepresentation(image, 1);
    }else{
        data = UIImagePNGRepresentation(image);
    }
    [data writeToFile:path atomically:YES];
    
    [SendToUnity SendMsg:@"AlbumPicturePath~"];
}
// 打开相册后点击“取消”的响应
- (void)imagePickerControllerDidCancel:(UIImagePickerController*)picker
{
    [SendToUnity SendMsg:@"AlbumPicturePath~Temp.jpg"];
    [self dismissViewControllerAnimated:YES completion:nil];
}
//获取图片完整路径
-(NSString*)GetSavePath:(NSString *)filename{
    NSArray *pathArray = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *docPath = [pathArray objectAtIndex:0];
    return [docPath stringByAppendingPathComponent:filename];
}
//图片旋转处理
- (UIImage *)fixOrientation:(UIImage *)aImage {
    CGAffineTransform transform = CGAffineTransformIdentity;
    
    switch (aImage.imageOrientation) {
        case UIImageOrientationDown:
        case UIImageOrientationDownMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.width, aImage.size.height);
            transform = CGAffineTransformRotate(transform, M_PI);
            break;
            
        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.width, 0);
            transform = CGAffineTransformRotate(transform, M_PI_2);
            break;
            
        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            transform = CGAffineTransformTranslate(transform, 0, aImage.size.height);
            transform = CGAffineTransformRotate(transform, -M_PI_2);
            break;
        default:
            break;
    }
    switch (aImage.imageOrientation) {
        case UIImageOrientationUpMirrored:
        case UIImageOrientationDownMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.width, 0);
            transform = CGAffineTransformScale(transform, -1, 1);
            break;
            
        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRightMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.height, 0);
            transform = CGAffineTransformScale(transform, -1, 1);
            break;
        default:
            break;
    }
    
    // Now we draw the underlying CGImage into a new context, applying the transform
    // calculated above.
    CGContextRef ctx = CGBitmapContextCreate(NULL, aImage.size.width, aImage.size.height,
                                             CGImageGetBitsPerComponent(aImage.CGImage), 0,
                                             CGImageGetColorSpace(aImage.CGImage),
                                             CGImageGetBitmapInfo(aImage.CGImage));
    CGContextConcatCTM(ctx, transform);
    switch (aImage.imageOrientation) {
        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            CGContextDrawImage(ctx, CGRectMake(0,0,aImage.size.height,aImage.size.width), aImage.CGImage);
            break;
            
        default:
            CGContextDrawImage(ctx, CGRectMake(0,0,aImage.size.width,aImage.size.height), aImage.CGImage);
            break;
    }
    // And now we just create a new UIImage from the drawing context
    CGImageRef cgimg = CGBitmapContextCreateImage(ctx);
    UIImage *img = [UIImage imageWithCGImage:cgimg];
    CGContextRelease(ctx);
    CGImageRelease(cgimg);
    return img;
}

//保存图片到相册
-(void) saveImageToAlbum:(NSString*) sourcePath
{
    if (sourcePath) {
        UIImage* image = [UIImage imageWithContentsOfFile:sourcePath];
        UIImageWriteToSavedPhotosAlbum(image,
                                       self,
                                       @selector(image:didFinishSavingWithError:contextInfo:),
                                       NULL);
    }
    else
    {
        [SendToUnity SendMsg: [[@"SavePhotoOrVideo~" stringByAppendingString:@"0"] stringByAppendingString:@"~视频原路径或者目标路径为空"]];
    }
}
//保存图片回调
-(void) image:(UIImage*)image didFinishSavingWithError:(NSError*)error contextInfo:(void*)contextInfo
{
    NSString* result;
    if(error)
    {
        result = @"0";
    }
    else
    {
        result = @"1";
    }
    
    [SendToUnity SendMsg: [[@"SavePhotoOrVideo~" stringByAppendingString:result] stringByAppendingString:@"~"]];
}
// videoPath为视频下载到本地之后的本地路径
-(void)saveVideo:(NSString *)sourcePath{
    if (sourcePath) {
        NSURL *url = [NSURL URLWithString:sourcePath];
        if (UIVideoAtPathIsCompatibleWithSavedPhotosAlbum(url.path) == NO) {
            NSLog(@"视频可以保存 ");
        }
        else
        {  NSLog(@"视频不可以保存 ");
        }
        //保存相册核心代码
        UISaveVideoAtPathToSavedPhotosAlbum(url.path, self, @selector(savedVedioImage:didFinishSavingWithError:contextInfo:), nil);
    }
    else
    {
        [SendToUnity SendMsg: [[@"SavePhotoOrVideo~" stringByAppendingString:@"0"] stringByAppendingString:@"~视频原路径或者目标路径为空"]];
    }
}
//保存视频完成之后的回调
-(void) savedVedioImage:(UIImage*)image didFinishSavingWithError: (NSError *)error contextInfo: (void *)contextInfo {
    NSString* result;
    if (error) {
        result =@"0";
    }
    else {
        result =@"1";
    }
    [SendToUnity SendMsg: [[@"SavePhotoOrVideo~" stringByAppendingString:result] stringByAppendingString:@"~"]];
}
-(void)GetAudioRecordPermission :(NSString*)msgPrefix
{
    RecordMsgPrefix = msgPrefix;
    NSString *mediaType = AVMediaTypeAudio;
    AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:mediaType];
    
    //用户尚未授权->申请权限
    if (authStatus == AVAuthorizationStatusNotDetermined)
    {
        [AVCaptureDevice requestAccessForMediaType:mediaType completionHandler : ^(BOOL granted){
            if(granted)
            {
                if([RecordMsgPrefix isEqualToString:@"CheckAudioPermission~"])
                {
                    [SendToUnity SendMsg:[RecordMsgPrefix stringByAppendingString:@"1"]];
                }
                else if([RecordMsgPrefix isEqualToString:@"CheckRecordVedioPermission~"])
                {
                    isFirstRequestPermission = true;
                    [self CheckPhotoPermission:RecordMsgPrefix];
                }
            }
            else
            {
                [SendToUnity SendMsg:[RecordMsgPrefix stringByAppendingString:@"0"]];
            }
        }];
    }
    //用户已经授权
    else  if (authStatus == AVAuthorizationStatusAuthorized){
        if([RecordMsgPrefix isEqualToString:@"CheckAudioPermission~"])
        {
            [SendToUnity SendMsg:[RecordMsgPrefix stringByAppendingString:@"2"]];
        }
        else  if([RecordMsgPrefix isEqualToString:@"CheckRecordVedioPermission~"])
        {
            isFirstRequestPermission = false;
            [self CheckPhotoPermission:RecordMsgPrefix];
        }
    }
    //用户拒绝授权
    else
    {
        [SendToUnity SendMsg:[RecordMsgPrefix stringByAppendingString:@"0"]];
    }
}
-(void)CheckPhotoPermission : (NSString*)msgPrefix
{
    AlbumMsgPrefix = msgPrefix;
    if ([SendToUnity JudgeSystemVersion:8.0])
    {
        PHAuthorizationStatus authStatus = [PHPhotoLibrary authorizationStatus];
        
        //用户尚未授权
        if (authStatus == PHAuthorizationStatusNotDetermined)
        {
            [PHPhotoLibrary requestAuthorization:^(PHAuthorizationStatus status) {
                
                if (status == PHAuthorizationStatusAuthorized)
                {
                    [SendToUnity SendMsg:[AlbumMsgPrefix stringByAppendingString:@"1"]];
                }
                else
                {
                    [SendToUnity SendMsg:[AlbumMsgPrefix stringByAppendingString:@"0"]];
                }
            }];
        }
        //用户已经授权
        else  if (authStatus == PHAuthorizationStatusAuthorized)
        {
            if( ([RecordMsgPrefix isEqualToString:@"CheckRecordVedioPermission~"] && !isFirstRequestPermission) ||
               ![RecordMsgPrefix isEqualToString:@"CheckRecordVedioPermission~"])
            {
                [SendToUnity SendMsg:[AlbumMsgPrefix stringByAppendingString:@"2"]];
            }
            else if([RecordMsgPrefix isEqualToString:@"CheckRecordVedioPermission~"] && isFirstRequestPermission)
            {
                [SendToUnity SendMsg:[AlbumMsgPrefix stringByAppendingString:@"1"]];
            }
        }
        //用户拒绝授权
        else
        {
            [SendToUnity SendMsg:[AlbumMsgPrefix stringByAppendingString:@"0"]];
        }
    }
    else
    {
        ALAuthorizationStatus authStatus = [ALAssetsLibrary authorizationStatus];
        //用户已经授权
        if (authStatus == ALAuthorizationStatusAuthorized)
        {
            if( ([RecordMsgPrefix isEqualToString:@"CheckRecordVedioPermission~"] && !isFirstRequestPermission) ||
               ![RecordMsgPrefix isEqualToString:@"CheckRecordVedioPermission~"])
            {
                [SendToUnity SendMsg:[AlbumMsgPrefix stringByAppendingString:@"2"]];
            }
            else if([RecordMsgPrefix isEqualToString:@"CheckRecordVedioPermission~"] && isFirstRequestPermission)
            {
                [SendToUnity SendMsg:[AlbumMsgPrefix stringByAppendingString:@"1"]];
            }
        }
        //用户拒绝授权
        else
        {
            [SendToUnity SendMsg:[AlbumMsgPrefix stringByAppendingString:@"0"]];
        }
    }
}
@end

