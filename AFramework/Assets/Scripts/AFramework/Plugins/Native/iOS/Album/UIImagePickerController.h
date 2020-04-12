//
//  UIImagePickerController+MyImagePicker.h
//  Photo-album-horizontal-screen
//横屏相册适配

#import <UIKit/UIKit.h>

@interface UIImagePickerController (MyImagePicker)

- (BOOL)shouldAutorotate;
- (NSUInteger)supportedInterfaceOrientations;

@end
