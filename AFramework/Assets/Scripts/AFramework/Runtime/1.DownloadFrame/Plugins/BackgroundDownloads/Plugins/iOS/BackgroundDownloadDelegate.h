 NS_ASSUME_NONNULL_BEGIN
typedef void (^CompletionHandler)();
 
@interface BackgroundDownloadDelegate : NSObject <NSURLSessionTaskDelegate, NSURLSessionDownloadDelegate>
 
@end
NS_ASSUME_NONNULL_END