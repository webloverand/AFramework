//  BLEForUnity.h


#import <CoreBluetooth/CoreBluetooth.h>
#import "AudioRecord.h"

@interface BLEForUnity : NSObject <CBCentralManagerDelegate, CBPeripheralManagerDelegate, CBPeripheralDelegate>
{
    bool isBLEInit;
    bool isListenA2DP;
    bool isNeedScan;
    bool isConnect;
    bool isSendingMsg;
    NSDictionary* option;
    
    //录音相关
    AudioRecord* audioRecord;
    NSString* StartRecordCommandStr;
    NSString* StopRecordCommandStr;
    bool isRecording;
    NSString* ServiceUUID;
    NSString* WriteCharacterUUID;
}

+ (BLEForUnity *)sharedInstance;
- (void)exitBLEForUnity;

//提供给上层调用
- (void)BlueInit : (bool)islistenA2DP;
-(void)RegitsterA2DPListen;
-(void)UnRegitsterA2DPListen;
- (void)GetAudioName;
- (void)startScanDevice:(NSString*)serviceUUIDsStringRaw ClearPeripheralList:(bool) clearPeripheralList AllowDuplicates:(bool)allowDuplicates;
- (void)stopScanDevice;
- (void)connectToPeripheral:(NSString*)macAddress;
- (void)readCharacteristic:(NSString *)serviceString characteristic:(NSString *)characteristicString;
- (void)writeCharacteristic:(NSString *)serviceString characteristic:(NSString *)characteristicString data:(NSString *)DataStr SendInternalTime:(float)sendInternalTime;
- (void)subscribeCharacteristic:(NSString *)serviceString characteristic:(NSString *)characteristicString;
- (void)unsubscribeCharacteristic:(NSString *)serviceString characteristic:(NSString *)characteristicString;
- (void)disconnetPeripheral;
- (void)disconnectPeripheralByMacAddress:(NSString *)macAddress;
- (void)disconnectPeripheralByName:(NSString *)deviceName;
-(void)ApplicationPause:(bool)pause isControllScan:(bool)isControllScan;

-(void)CheckRecordPermission;
-(void)SetRecordParameter:(NSString*)startRecordCommand stopRecordCommand:(NSString*) stopRecordCommand
              serviceUUID:(NSString*) serviceUUID writeCharacterUUID:(NSString*) writeCharacterUUID;
-(void)StartRecord;
- (void)stopRecord;
- (void)stopRecordForce ;

-(void)writeNextMsg;
@end
