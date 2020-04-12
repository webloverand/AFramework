#import <Foundation/Foundation.h>
#import "BackgroundDownloadDelegate.h"

@interface BackgroundDownloadManager : NSObject {
    NSMutableDictionary *pendingDownloadTasks;
}

+ (BackgroundDownloadManager *) instance;

- (long) startDownload:(NSString *) url storagePath:(NSString *) storagePath;
- (void) cancelDownloadTask:(long) identifier;

- (NSURLSessionTask *) getDownloadTask:(long) identifier;
- (NSURLSessionTask *) getDownloadTaskWithURL:(NSString *) url;

- (void) moveDownloadToDestination:(NSURL *) tempFileURL downloadedFilename:(NSURLSessionDownloadTask *) filename;

- (void) initDownloadSession;

@end
