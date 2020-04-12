//
//  ImageSave.h
//  Unity-iPhone
//
//  Created by Wili on 2018/1/6.
//

#ifndef ImageSave_h
#define ImageSave_h

#import <UIKit/UIKit.h>

@interface ImageSave:NSObject<UIImagePickerControllerDelegate, UINavigationControllerDelegate>
+ (void)saveScreenshot:(NSString *)path;
@end


#endif /* NativeGallery_h */
