//
//  UIImagePickerController+MyImagePicker.m
//  Photo-album-horizontal-screen
//

#import "UIImagePickerController.h"

@implementation UIImagePickerController (MyImagePicker)
- (BOOL)shouldAutorotate {
    return YES;
}

- (NSUInteger)supportedInterfaceOrientations{
    return UIInterfaceOrientationMaskLandscape;
}

@end
