//
//  SendToUnity.m
//  Unity-iPhone
//
//  Created by 邓顺好 on 2019/12/10.
//
#import "SendToUnity.h"

@implementation SendToUnity
//向Unity发送数据
+(void)SendMsg:(NSString*)msg
{
    UnitySendMessage("AFReceiver", "MsgDispose", msg.UTF8String);
}
//向Unity发送数据
+(void)SendBlueMsg:(NSString*)msg
{
    UnitySendMessage("BLEReceiver", "BLEMsgDispose", msg.UTF8String);
}

+(bool)JudgeSystemVersion:(double)version
{
    return [[UIDevice currentDevice].systemVersion doubleValue] >= version;
}
@end
