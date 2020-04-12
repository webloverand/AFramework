#import <Foundation/Foundation.h>
#import "ImageSave.h"

@implementation ImageSave

+ (void)saveScreenshot:(NSString *)path {
	UIImage *image = [UIImage imageWithContentsOfFile:path];
    UIImageWriteToSavedPhotosAlbum(image, self,
	   @selector(image:finishedSavingWithError:contextInfo:),
	   (__bridge_retained void *) path);
    
}


+ (void)image:(UIImage *)image finishedSavingWithError:(NSError *)error contextInfo:(void *)contextInfo {
    NSString* path = (__bridge_transfer NSString *)(contextInfo);
	//[[NSFileManager defaultManager] removeItemAtPath:path error:nil];
}

@end

