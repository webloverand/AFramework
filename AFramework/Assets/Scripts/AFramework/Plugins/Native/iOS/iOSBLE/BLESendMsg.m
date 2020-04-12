//
//  BLESendMsg.m
//  Unity-iPhone
//
//  Created by 邓顺好 on 2019/12/30.
//

#import <Foundation/Foundation.h>
#import "BLESendMsg.h"
#import "SendToUnity.h"
@implementation BLESendMsg
-(BLESendMsg*)init:(NSString*)completeMsg sendArray:(NSMutableArray *)sendArray WriteCharacteristic:(CBCharacteristic *) writeCharacteristic
Peripheral:(CBPeripheral *)peripheral SendInternalTime:(float)sendInternalTime BLEForUnity:(BLEForUnity*)blEForUnity
{
    CompleteMsg = completeMsg;
    SendArray = sendArray;
    WriteCharacteristic = writeCharacteristic;
    Peripheral = peripheral;
    SendInternalTime = sendInternalTime;
    bleForUnity =blEForUnity;
    serialQueue = dispatch_queue_create("com.wql.www", DISPATCH_QUEUE_SERIAL);
    return self;
}

-(void)WriteMsg
{
    NSLog(@"writeMsg1111:是否为空%d,数量%d",(SendArray != nil),SendArray.count);
    if (SendArray != nil && SendArray.count > 0) {
        dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(SendInternalTime * NSEC_PER_SEC)), serialQueue, ^{
             if(SendArray.count > 0)
             {
                 NSData *subData =[SendArray objectAtIndex : 0];
                 [SendArray removeObjectAtIndex : 0];
                 [self writeData:subData];
             }
         });
    }
}
- (void)writeData:(NSData *)data {
    NSLog(@"writeData:是否为空%d,长度%d",(Peripheral != nil),data.length );
    if (Peripheral && data.length > 0) {
        [Peripheral writeValue:data
              forCharacteristic:WriteCharacteristic
                           type:CBCharacteristicWriteWithResponse];
    }
}
-(void)SendMsgResult:(bool)isSuccess Error:(NSString*)error
{
    if(isSuccess)
    {
        //继续发送本条信息
        if(SendArray.count > 0)
        {
            [self WriteMsg];
        }
        else
        {
            //本条数据发送成功
            [SendToUnity SendBlueMsg:[NSString stringWithFormat:@"WriteCharacter~1~%@",CompleteMsg]];
           //继续发送下一条信息
            [bleForUnity writeNextMsg];
        }
    }
    else
    {
        [SendToUnity SendBlueMsg:[NSString stringWithFormat:@"WriteCharacter~0~%@",error]];
        //继续发送下一条信息
        [bleForUnity writeNextMsg];
    }
}
@end

