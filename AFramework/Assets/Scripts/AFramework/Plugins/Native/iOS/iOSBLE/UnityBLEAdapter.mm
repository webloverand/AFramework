//
//  UnityBLEAdapter.m
//  Unity-iPhone
//
//  Created by 邓顺好 on 2019/12/30.
//
#import "BLEForUnity.h"

#if defined (__cplusplus)
extern "C" {
#endif

void BlueInit(bool islistenA2DP)
{
    [[BLEForUnity sharedInstance] BlueInit:islistenA2DP];
}
void RegitsterA2DPListen()
{
    [[BLEForUnity sharedInstance] RegitsterA2DPListen];
}
void UnRegitsterA2DPListen()
{
    [[BLEForUnity sharedInstance] UnRegitsterA2DPListen];
}
void GetAudioName()
{
    [[BLEForUnity sharedInstance] GetAudioName];
}
//扫描蓝牙
void ScanDevice(char *serviceUUIDsStringRaw,bool clearPeripheralList,bool allowDuplicates) {
      NSString* serviceUUIDsString = [NSString stringWithFormat:@"%s", serviceUUIDsStringRaw];
     [[BLEForUnity sharedInstance] startScanDevice:serviceUUIDsString ClearPeripheralList:clearPeripheralList AllowDuplicates:allowDuplicates];
}
void stopScanDevice()
{
    [[BLEForUnity sharedInstance] stopScanDevice];
}
void connectToPeripheral(char* macAddress)
{
    NSString* macaddress = [NSString stringWithUTF8String:macAddress];
    [[BLEForUnity sharedInstance] connectToPeripheral:macaddress];
}
void readCharacteristic(char* serviceString,char* characteristicString)
{
    NSString* servicestring = [NSString stringWithUTF8String:serviceString];
    NSString* characteristicstring = [NSString stringWithUTF8String:characteristicString];
    [[BLEForUnity sharedInstance] readCharacteristic:servicestring characteristic:characteristicstring];
}
void writeCharacteristic(char* serviceString,char* characteristicString ,char* DataStr,float sendInternalTime)
{
    NSString* servicestring = [NSString stringWithUTF8String:serviceString];
    NSString* characteristicstring = [NSString stringWithUTF8String:characteristicString];
    NSString* Datastr = [NSString stringWithUTF8String:DataStr];
   
    [[BLEForUnity sharedInstance] writeCharacteristic:servicestring characteristic:characteristicstring data:Datastr SendInternalTime:sendInternalTime];
}
void subscribeCharacteristic(char* serviceString ,char*  characteristicString)
{
    NSString* servicestring = [NSString stringWithUTF8String:serviceString];
    NSString* characteristicstring = [NSString stringWithUTF8String:characteristicString];
    [[BLEForUnity sharedInstance] subscribeCharacteristic:servicestring characteristic:characteristicstring];
}
void unsubscribeCharacteristic(char* serviceString, char* characteristicString)
{
    NSString* servicestring = [NSString stringWithUTF8String:serviceString];
    NSString* characteristicstring = [NSString stringWithUTF8String:characteristicString];
    [[BLEForUnity sharedInstance] unsubscribeCharacteristic:servicestring characteristic:characteristicstring];
}
void disconnetPeripheral()
{
    [[BLEForUnity sharedInstance] disconnetPeripheral];
}
void disconnectPeripheralByMacAddress(char*  macAddress)
{
    NSString* macaddress = [NSString stringWithUTF8String:macAddress];
    [[BLEForUnity sharedInstance] disconnectPeripheralByMacAddress:macaddress];
}
void disconnectPeripheralByName(char *deviceName)
{
     NSString* devicename = [NSString stringWithUTF8String:deviceName];
    [[BLEForUnity sharedInstance] disconnectPeripheralByName:devicename];
}
void ApplicationPause(bool pause,bool isControllScan)
{
    [[BLEForUnity sharedInstance] ApplicationPause:pause isControllScan:isControllScan];
}
void CheckRecordPermission()
{
    [[BLEForUnity sharedInstance] CheckRecordPermission];
}
void SetRecordParameter(char* startRecordCommand ,char* stopRecordCommand,
              char* serviceUUID,char* writeCharacterUUID)
{
    NSString* startrecordCommand = [NSString stringWithUTF8String:startRecordCommand];
    NSString* stoprecordCommand = [NSString stringWithUTF8String:stopRecordCommand];
    NSString* serviceuuid = [NSString stringWithUTF8String:serviceUUID];
    NSString* writecharacterUUID = [NSString stringWithUTF8String:writeCharacterUUID];
    [[BLEForUnity sharedInstance] SetRecordParameter:startrecordCommand stopRecordCommand:stoprecordCommand serviceUUID:serviceuuid writeCharacterUUID:writecharacterUUID];
}
void StartRecord()
{
    [[BLEForUnity sharedInstance] StartRecord];
}
void stopRecord()
{
    [[BLEForUnity sharedInstance] stopRecord];
}
void stopRecordForce()
{
    [[BLEForUnity sharedInstance] stopRecordForce];
}
void exitBLEForUnity()
{
    [[BLEForUnity sharedInstance] exitBLEForUnity];
}
# if defined (__cplusplus)
}
#endif
