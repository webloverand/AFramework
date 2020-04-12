//
//  SendToUnity.h
//  Unity-iPhone
//
//  Created by 邓顺好 on 2019/12/10.
//

@interface SendToUnity
+(void)SendMsg:(NSString*)msg;
+(void)SendBlueMsg:(NSString*)msg;

+(bool)JudgeSystemVersion:(double)version;
@end
