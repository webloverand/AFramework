//
//  BLESendMsg.h
//  Unity-iPhone
//
//  Created by 邓顺好 on 2019/12/30.
//

#import <CoreBluetooth/CoreBluetooth.h>
#import "BLEForUnity.h"
@interface BLESendMsg : NSObject
{
    NSString* CompleteMsg;
    NSMutableArray *SendArray;
    CBCharacteristic * WriteCharacteristic;
    CBPeripheral *Peripheral;
    float SendInternalTime;
    BLEForUnity* bleForUnity;
    
    dispatch_queue_t serialQueue;
}
-(BLESendMsg*)init:(NSString*)completeMsg sendArray:(NSMutableArray *)sendArray WriteCharacteristic:(CBCharacteristic *) writeCharacteristic
Peripheral:(CBPeripheral *)peripheral SendInternalTime:(float)sendInternalTime BLEForUnity:(BLEForUnity*)blEForUnity;

-(void)WriteMsg;
-(void)SendMsgResult:(bool)isSuccess Error:(NSString*)error;
@end
