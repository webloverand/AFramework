//
//  ImageController.m
//  Unity-iPhone
//
//  Created by Wili on 2018/1/6.
//

#import <Foundation/Foundation.h>
#import "ImageSave.h"

extern "C" {
    

    void SaveImageToAlbum(const char* path) {
        [ImageSave saveScreenshot:[NSString stringWithUTF8String:path]];
    }
    
}
