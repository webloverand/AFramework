#include "UnityNativeBridge.h"
#include "BackgroundDownloadDelegate.h"
#include "BackgroundDownloadManager.h"

long startDownload(char *url, char *savePath)
{
    return [[BackgroundDownloadManager instance] startDownload:[NSString stringWithUTF8String:url] storagePath:[NSString stringWithUTF8String:savePath]];
}

void cancelDownload(long id)
{
    [[BackgroundDownloadManager instance] cancelDownloadTask:id];
}

long getDownloadTask(const char* url)
{
    NSURLSessionTask *task = [[BackgroundDownloadManager instance] getDownloadTaskWithURL:[NSString stringWithUTF8String:url]];
    
    if (task == NULL)
    {
        // TODO: log ??
        
        return -1;
    }
    
    return task.taskIdentifier;
}

float getDownloadProgress(long id)
{
    NSURLSessionTask *task = [[BackgroundDownloadManager instance] getDownloadTask:id];
    
    if (task == NULL)
    {
        return 0.0f;
    }
    
    double received = (double)[task countOfBytesReceived];
    double total = (double)[task countOfBytesExpectedToReceive];
    
    return (total == 0) ? 0.0f : received / total;
}

int getDownloadStatus(long id)
{
    NSURLSessionTask *task = [[BackgroundDownloadManager instance] getDownloadTask:id];
    
    if (task == NULL)
    {
        return NSURLSessionTaskStateCanceling;
    }

    return [task state];
}

const char *getError(long id)
{
    NSURLSessionTask *task = [[BackgroundDownloadManager instance] getDownloadTask:id];
    
    if (task == NULL)
    {
        return "";
    }

    if (task.error == NULL)
    {
        return "";
    }

    return [[[task error] localizedDescription] UTF8String];
}
