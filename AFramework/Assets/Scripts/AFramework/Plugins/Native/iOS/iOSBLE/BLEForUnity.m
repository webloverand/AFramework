//
//  BLEForUnity.m
//  Unity-iPhone
//
//  Created by 邓顺好 on 2019/12/27.
//

#import <Foundation/Foundation.h>
#import "BLEForUnity.h"
#import "SendToUnity.h"
#import <MediaPlayer/MediaPlayer.h>
#import <CoreBluetooth/CoreBluetooth.h>
#import <AVFoundation/AVFoundation.h>
#import "BLESendMsg.h"
#import "UnityAdapter.h"

#define BLE_SEND_MAX_LEN 40

@interface CBUUID (StringExtraction)
- (NSString *)UUIDStringSafe;
@end

@implementation CBUUID (StringExtraction)
- (NSString *)UUIDStringSafe;
{
    SEL sel = @selector (UUIDString);
    if ([super respondsToSelector:sel]) {
        return self.UUIDString;
    }
    
    
    NSData *data = [self data];
    
    NSUInteger bytesToConvert = [data length];
    const unsigned char *uuidBytes = [data bytes];
    NSMutableString *outputString = [NSMutableString stringWithCapacity:16];
    
    for (NSUInteger currentByteIndex = 0; currentByteIndex < bytesToConvert; currentByteIndex++)
    {
        switch (currentByteIndex)
        {
            case 3:
            case 5:
            case 7:
            case 9:[outputString appendFormat:@"%02X-", uuidBytes[currentByteIndex]]; break;
            default:[outputString appendFormat:@"%02X", uuidBytes[currentByteIndex]];
        }
        
    }
    
    return outputString;
}
@end

@interface BLEForUnity() <CBCentralManagerDelegate, CBPeripheralManagerDelegate, CBPeripheralDelegate>{
    CBCentralManager* _centralManager;
    //扫描蓝牙参数
    NSMutableDictionary* scanDevices;
    //连接蓝牙参数
    CBPeripheral* connectPeripheral;
    //发送所需参数
    BLESendMsg* currentSendMsg;
}
@property(nonatomic,retain) NSMutableArray *sendMsgArray;
@end


@implementation BLEForUnity
static BLEForUnity * s_instance_dj_singleton = nil ;
//单例
+ (BLEForUnity *)sharedInstance{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        if (s_instance_dj_singleton == nil) {
            s_instance_dj_singleton = [BLEForUnity alloc];
            
        }
    });
    return (BLEForUnity *)s_instance_dj_singleton;
}

//蓝牙初始化
- (void)BlueInit : (bool)islistenA2DP
{
    //程序退出调用
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationWillTerminate) name:UIApplicationWillTerminateNotification object:nil];
    
    if(_sendMsgArray == nil)
        _sendMsgArray = [NSMutableArray array];
    isSendingMsg = false;
    [self GetBLEManager];
    if(islistenA2DP)
    {
        [self RegitsterA2DPListen];
    }
    else
    {
        [self UnRegitsterA2DPListen];
    }
    isBLEInit = true;
}
-(CBCentralManager*)GetBLEManager
{
    if(!_centralManager)
    {
        _centralManager = [[CBCentralManager alloc] initWithDelegate:self queue:nil];
        [self centralManagerDidUpdateState:_centralManager];
    }
    return _centralManager;
}
//注册音频蓝牙监听
-(void)RegitsterA2DPListen
{
    if(!isListenA2DP)
    {
        [self A2DPListen];
    }
    isListenA2DP = true;
}
//是否监听音频蓝牙
-(void)UnRegitsterA2DPListen
{
    if(isListenA2DP)
    {
        [[NSNotificationCenter defaultCenter] removeObserver:self
                                                        name:AVAudioSessionRouteChangeNotification
                                                      object:nil];
    }
    isListenA2DP = false;
}
-(void)A2DPListen
{
    [self GetAudioName];
    //音频蓝牙的监听回调
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(myRouteChangeSelector:)
                                                 name:AVAudioSessionRouteChangeNotification
                                               object:nil];
}
//获取音频蓝牙连接状态以及名称
- (void)GetAudioName {
    AVAudioSessionRouteDescription *currentRoute = [[AVAudioSession sharedInstance] currentRoute];
    NSArray *outputsForRoute = currentRoute.outputs;
    if (outputsForRoute.count > 0) {
        AVAudioSessionPortDescription *outPortDesc = [outputsForRoute objectAtIndex:0];
        [SendToUnity SendBlueMsg:[NSString stringWithFormat: @"A2DPBlueInfo~1~%@~%@", outPortDesc.portName,@""]];
    }
    else
    {
        [SendToUnity SendBlueMsg:@"A2DPBlueInfo~0" ];
    }
}
- (void)startScanDevice:(NSString*)serviceUUIDsString ClearPeripheralList:(bool) clearPeripheralList AllowDuplicates:(bool)allowDuplicates
{
    NSLog(@"%@", [NSString stringWithFormat:@"开始扫描:%@,%d",serviceUUIDsString,[serviceUUIDsString isEqualToString:@""]]);
    NSDictionary *options = nil;
    if (allowDuplicates)
        options = @{ CBCentralManagerScanOptionAllowDuplicatesKey:@YES };
    if([serviceUUIDsString isEqualToString:@""])
    {
        [self startScan:options clearPeripheral:clearPeripheralList];
    }
    else
    {
        NSMutableArray *actualUUIDs = nil;
        NSArray *serviceUUIDs = [serviceUUIDsString componentsSeparatedByString:@"|"];
        if (serviceUUIDs.count > 0)
        {
            actualUUIDs = [[NSMutableArray alloc] init];
            
            for (NSString* sUUID in serviceUUIDs)
                [actualUUIDs addObject:[CBUUID UUIDWithString:sUUID]];
        }
        [self startScanWithService:actualUUIDs option:options clearPeripheral:clearPeripheralList];
    }
}
//扫描全部蓝牙设备
- (void)startScan:(NSDictionary *)options clearPeripheral:(bool)clearPeripheralList{
    option = options;
    if(clearPeripheralList && scanDevices)
    {
        [scanDevices removeAllObjects];
    }
    else if(!scanDevices)
    {
        scanDevices = [[NSMutableDictionary alloc] init];
    }
    if ([self GetBLEManager] &&[self GetBLEManager].state == CBCentralManagerStatePoweredOn && ![self GetBLEManager].isScanning) {
        NSLog(@"开始扫描全部设备...");
        [[self GetBLEManager] scanForPeripheralsWithServices:nil
                                                     options:options];
    }
}
- (void)startScanWithService:(NSArray *)serviceUUIDs option:(NSDictionary *)options clearPeripheral:(bool)clearPeripheralList{
    if(clearPeripheralList && scanDevices)
    {
        [scanDevices removeAllObjects];
    }
    else if(!scanDevices)
    {
        scanDevices = [[NSMutableDictionary alloc] init];
    }
    if ([self GetBLEManager] &&[self GetBLEManager].state == CBCentralManagerStatePoweredOn && ![self GetBLEManager].isScanning) {
        NSLog(@"开始扫描设备(有限制)...");
        [[self GetBLEManager] scanForPeripheralsWithServices:serviceUUIDs options:options];
    }
}
//停止扫描蓝牙
- (void)stopScanDevice {
    if([[self GetBLEManager] isScanning])
        [[self GetBLEManager] stopScan];
}

//连接蓝牙
- (void)connectToPeripheral:(NSString*)macAddress {
    if (scanDevices != nil && macAddress != nil)
    {
        CBPeripheral *peripheral = [scanDevices objectForKey:macAddress];
        if (peripheral != nil)
            [[self GetBLEManager] connectPeripheral:peripheral options:nil];
    }
}
//读取Characteristic
- (void)readCharacteristic:(NSString *)serviceString characteristic:(NSString *)characteristicString
{
    if (serviceString != nil && characteristicString != nil && connectPeripheral != nil)
    {
        CBCharacteristic *characteristic = [self getCharacteristic:serviceString characteristic:characteristicString];
        if (characteristic != nil)
            [connectPeripheral readValueForCharacteristic:characteristic];
    }
}
//写数据
- (void)writeCharacteristic:(NSString *)serviceString characteristic:(NSString *)characteristicString data:(NSString *)DataStr SendInternalTime:(float)sendInternalTime
{
    //数据分包
    NSData *msgData = [DataStr dataUsingEncoding:NSUTF8StringEncoding];
    NSMutableArray *sendArray =  [NSMutableArray array];
    for (int i = 0; i < [msgData length]; i += BLE_SEND_MAX_LEN) {
        if ((i + BLE_SEND_MAX_LEN) < [msgData length]) {
            NSString *rangeStr = [NSString stringWithFormat:@"%i,%i", i, BLE_SEND_MAX_LEN];
            NSData *subData = [msgData subdataWithRange:NSRangeFromString(rangeStr)];
            NSString *hexStr = [[NSString alloc] initWithData:subData encoding:NSUTF8StringEncoding];
            
            [sendArray insertObject:[self dataWithHexString : hexStr] atIndex : sendArray.count];
        } else {
            NSString *rangeStr = [NSString stringWithFormat:@"%i,%i", i, (int)([msgData length] - i)];
            NSData *subData = [msgData subdataWithRange:NSRangeFromString(rangeStr)];
            NSString *hexStr = [[NSString alloc] initWithData:subData encoding:NSUTF8StringEncoding];
            [sendArray insertObject:[self dataWithHexString : hexStr] atIndex : sendArray.count];
        }
    }
    NSLog(@"发送数据servicestring:%@,characteristicstring:%@,Datastr:%@",serviceString,characteristicString,DataStr);
    if ( serviceString != nil && characteristicString != nil && connectPeripheral != nil && DataStr != nil)
    {
        NSLog(@"serviceString:%@,characteristicString:%@",
              serviceString,characteristicString);
        CBCharacteristic *characteristic = [self getCharacteristic:serviceString characteristic:characteristicString];
        if (characteristic != nil)
        {
            BLESendMsg* bleSendMsg =[[BLESendMsg alloc] init:DataStr sendArray:sendArray WriteCharacteristic:characteristic Peripheral:connectPeripheral  SendInternalTime:sendInternalTime BLEForUnity : self] ;
            [_sendMsgArray insertObject:bleSendMsg atIndex:_sendMsgArray.count];
            [self writeMsg];
        }
    }
    else
    {
        NSLog(@"connectPeripheral为空");
    }
}

//订阅通知
- (void)subscribeCharacteristic:(NSString *)serviceString characteristic:(NSString *)characteristicString
{
    if (serviceString != nil && characteristicString != nil && connectPeripheral != nil)
    {
        CBCharacteristic *characteristic = [self getCharacteristic:serviceString characteristic:characteristicString];
        if (characteristic != nil)
            [connectPeripheral setNotifyValue:YES forCharacteristic:characteristic];
        
    }
}
//取消订阅
- (void)unsubscribeCharacteristic:(NSString *)serviceString characteristic:(NSString *)characteristicString
{
    if (serviceString != nil && characteristicString != nil && connectPeripheral != nil)
    {
        CBCharacteristic *characteristic = [self getCharacteristic:serviceString characteristic:characteristicString];
        if (characteristic != nil)
            [connectPeripheral setNotifyValue:NO forCharacteristic:characteristic];
    }
}
//断开蓝牙连接
- (void)disconnetPeripheral {
    scanDevices = nil;
    if ([self GetBLEManager] &&[self GetBLEManager].state == CBCentralManagerStatePoweredOn && ![self GetBLEManager].isScanning)
    {
        [self stopScanDevice];
    }
    if ([self GetBLEManager] && connectPeripheral && connectPeripheral) {
        [[self GetBLEManager] cancelPeripheralConnection:connectPeripheral];
    }
}
- (void)disconnectPeripheralByMacAddress:(NSString *)macAddress
{
    [_sendMsgArray removeAllObjects];
    isConnect = false;
    isSendingMsg = false;
    if (scanDevices != nil && macAddress != nil)
    {
        NSArray* keys = [scanDevices allKeys];
        for(NSString* key in keys)
        {
            CBPeripheral *peripheral = [scanDevices objectForKey:key];
            if (peripheral != nil)
            {
                NSString* identifier = [[peripheral identifier] UUIDString];
                if([identifier isEqualToString:macAddress])
                {
                    [[self GetBLEManager] cancelPeripheralConnection:peripheral];
                    return;
                }
            }
        }
    }
}
- (void)disconnectPeripheralByName:(NSString *)deviceName
{
    if (scanDevices != nil && deviceName != nil)
    {
        CBPeripheral *peripheral = [scanDevices objectForKey:deviceName];
        if (peripheral != nil)
        {
            [[self GetBLEManager] cancelPeripheralConnection:peripheral];
        }
    }
}
- (void)disconnectAll
{
    if (scanDevices != nil && [scanDevices count] > 0)
    {
        NSArray* keys = [scanDevices allKeys];
        for(NSString* key in keys)
        {
            CBPeripheral *peripheral = [scanDevices objectForKey:key];
            if (peripheral != nil)
                [[self GetBLEManager] cancelPeripheralConnection:peripheral];
        }
    }
    connectPeripheral = nil;
}
-(void)ApplicationPause:(bool)pause isControllScan:(bool)isControllScan
{
    if(pause)
    {
        if(isControllScan)
        {
            isNeedScan = true;
            [self stopScanDevice];
        }
    }
    else
    {
        [self GetNewBlueState];
        if(isControllScan && isNeedScan)
        {
            isNeedScan = false;
            NSLog(@"StartScan: 进入前台启动扫描");
            [self startScan:option clearPeripheral:true];
        }
    }
}
-(void)CheckRecordPermission
{
    [[self GetAudioRecord] CheckRecordPermission];
}
-(void)SetRecordParameter:(NSString*)startRecordCommand stopRecordCommand:(NSString*) stopRecordCommand
              serviceUUID:(NSString*) serviceUUID writeCharacterUUID:(NSString*) writeCharacterUUID
{
    StartRecordCommandStr = startRecordCommand;
    StopRecordCommandStr = stopRecordCommand;
    ServiceUUID = serviceUUID;
    WriteCharacterUUID = writeCharacterUUID;
    isRecording = false;
    [[self GetAudioRecord] readyForRecord];
}
//开始录音
-(void)StartRecord
{
    [[UnityAdapter sharedInstance] SetIngoreAudioChange:true];
   if(!isRecording && [[self GetAudioRecord] isReadyToRecord])
   {
       isRecording = true;
       //发送录音命令
       [[BLEForUnity sharedInstance] writeCharacteristic:ServiceUUID characteristic:WriteCharacterUUID data:StartRecordCommandStr SendInternalTime:0.01];
       [[self GetAudioRecord] StartRecord];
        [SendToUnity SendBlueMsg: @"RealStartRecord~1"];
   }
    else
    {
        [SendToUnity SendBlueMsg: @"RealStartRecord~0~录音初始化错误"];
    }
}
//停止录音
- (void)stopRecord {
    if(!isRecording)
    {
        return;
    }
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(3 * NSEC_PER_SEC)), dispatch_get_global_queue(0, 0), ^{
        
        [[AVAudioSession sharedInstance] setActive:YES error:nil];
        [[BLEForUnity sharedInstance] writeCharacteristic:ServiceUUID characteristic:WriteCharacterUUID data:StopRecordCommandStr SendInternalTime:0.01];
        [[self GetAudioRecord] stopRecordAudioUnit];
        [[UnityAdapter sharedInstance] SetIngoreAudioChange:false];
        [SendToUnity SendBlueMsg:@"RealFinishRecord~1"];
    });
}
//提供给返回按钮使用
- (void)stopRecordForce {
    if(!isRecording)
    {
        return;
    }
    [[self GetAudioRecord] stopRecordAudioUnit];
    [[BLEForUnity sharedInstance] writeCharacteristic:ServiceUUID characteristic:WriteCharacterUUID data:StopRecordCommandStr SendInternalTime:0.01];
    [[UnityAdapter sharedInstance] SetIngoreAudioChange:false];
}

-(AudioRecord*)GetAudioRecord
{
    if(!audioRecord)
    {
        audioRecord = [AudioRecord alloc];
    }
    return audioRecord;
}
-(void)GetNewBlueState
{
    [self JudgeBlueState:[self GetBLEManager].state];
    if(isListenA2DP)
    {
        [self GetAudioName];
    }
    if(isConnect)
    {
        NSString* identifier = [[connectPeripheral identifier] UUIDString];
        [SendToUnity SendBlueMsg:[NSString stringWithFormat:@"BlueConnectState~1~%@~%@",connectPeripheral.name,identifier]];
    }
    else
    {
        [SendToUnity SendBlueMsg:@"BlueConnectState~0~null~null"];
    }
}
- (CBCharacteristic *)getCharacteristic:(NSString *)serviceString characteristic:(NSString *)characteristicString
{
    CBCharacteristic *returnCharacteristic = nil;
    
    if (serviceString != nil && characteristicString != nil && connectPeripheral != nil)
    {
        CBUUID *serviceUUID = [CBUUID UUIDWithString:serviceString];
        CBUUID *characteristicUUID = [CBUUID UUIDWithString:characteristicString];
        
        for (CBService *service in connectPeripheral.services)
        {
            if ([service.UUID isEqual:serviceUUID])
            {
                for (CBCharacteristic *characteristic in service.characteristics)
                {
                    if ([characteristic.UUID isEqual:characteristicUUID])
                    {
                        returnCharacteristic = characteristic;
                    }
                }
            }
        }
    }
    return returnCharacteristic;
}
//根据
- (CBCharacteristic *)getCharacteristic:(NSString *)macAddress service:(NSString *)serviceString characteristic:(NSString *)characteristicString
{
    CBCharacteristic *returnCharacteristic = nil;
    
    if ( macAddress!= nil && serviceString != nil && characteristicString != nil && scanDevices != nil)
    {
        CBPeripheral *peripheral = [scanDevices objectForKey:macAddress];
        if (peripheral != nil)
        {
            CBUUID *serviceUUID = [CBUUID UUIDWithString:serviceString];
            CBUUID *characteristicUUID = [CBUUID UUIDWithString:characteristicString];
            
            for (CBService *service in peripheral.services)
            {
                if ([service.UUID isEqual:serviceUUID])
                {
                    for (CBCharacteristic *characteristic in service.characteristics)
                    {
                        if ([characteristic.UUID isEqual:characteristicUUID])
                        {
                            returnCharacteristic = characteristic;
                        }
                    }
                }
            }
        }
    }
    return returnCharacteristic;
}
//Characteristic改变回调
- (void)peripheral:(CBPeripheral *)peripheral didUpdateValueForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error
{
    if (error)
    {
        NSLog(@"%@",[NSString stringWithFormat:@"Characteristic改变回调Error~%@", error.description]);
    }
    else
    {
        NSString *orStr = characteristic.value.description;
        NSString *dataStr = @"";
        if (orStr) {
            if([SendToUnity JudgeSystemVersion:13.0])
            {
                const unsigned *dataBytes = [characteristic.value bytes];
                NSString *str = [[orStr substringWithRange:NSMakeRange(1, orStr.length - 2)] stringByReplacingOccurrencesOfString:@" " withString:@""];
                NSUInteger datalength = [[[[[str componentsSeparatedByString:@","] objectAtIndex:0] componentsSeparatedByString:@"="] objectAtIndex:1] intValue];
                NSUInteger length = orStr.length;
                NSLog(@"length = %lu",(unsigned long)length);
                for(int i = 0;i<length;i++)
                {
                    dataStr = [dataStr stringByAppendingString:[NSString stringWithFormat:@"%08x",
                                                                ntohl(dataBytes[i])]];
                }
                dataStr = [dataStr substringToIndex:datalength*2];
            }
            else
            {
                NSString *str = [orStr substringWithRange:NSMakeRange(1, orStr.length - 2)];
                dataStr = [str stringByReplacingOccurrencesOfString:@" " withString:@""];
            }
            NSString* identifier = [[peripheral identifier] UUIDString];
            if(dataStr && ![dataStr isEqualToString:@""] )
            {
                [SendToUnity SendBlueMsg:[NSString stringWithFormat:@"NotifyCharacterData~%@~%@~%@~%@",peripheral.name,identifier,[characteristic UUID],dataStr]];
            }
        }
    }
}
//写入回调
- (void)peripheral:(CBPeripheral *)peripheral didWriteValueForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error
{
    NSLog(@"发送结果:%d",isSendingMsg);
    if(!isSendingMsg)
    {
        return;
    }
    if (error)
    {
        [currentSendMsg SendMsgResult:false Error:error.description];
    }
    else
    {
        [currentSendMsg SendMsgResult:true Error:@""];
    }
}
//更新Notification Characteristic回调
- (void)peripheral:(CBPeripheral *)peripheral didUpdateNotificationStateForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error
{
    if (error)
    {
        NSLog(@"%@", [NSString stringWithFormat:@"Error~%@", error.description]);
        [SendToUnity SendBlueMsg: [NSString stringWithFormat:@"NotifyCharacter~%@~%@",@"0",error.description]];
    }
    else
    {
        [SendToUnity SendBlueMsg: [NSString stringWithFormat:@"NotifyCharacter~%@~%@",@"1",@"Success"]];
    }
}
//扫描服务回调
- (void)peripheral:(CBPeripheral *)peripheral didDiscoverServices:(NSError *)error
{
    if (error)
    {
        NSLog(@"%@", [NSString stringWithFormat:@"扫描服务Error~%@", error.description]);
    }
    else
    {
        for (CBService *service in peripheral.services)
        {
            [peripheral discoverCharacteristics:nil forService:service];
        }
    }
}
//扫描服务的特征回调回调
- (void)peripheral:(CBPeripheral *)peripheral didDiscoverCharacteristicsForService:(CBService *)service error:(NSError *)error
{
    if (error)
    {
        NSLog(@"%@", [NSString stringWithFormat:@"扫描服务Error~%@", error.description]);
    }
    else
    {
        for (CBCharacteristic *characteristic in service.characteristics)
        {
            NSString* identifier = [[peripheral identifier] UUIDString];
            [SendToUnity SendBlueMsg:[NSString stringWithFormat:@"DiscoveryCharacteristic~%@~%@~%@~%@~%@",
                                      peripheral.name,identifier,[service UUID],[characteristic UUID],
                                      [self JudgeCharacteristicProperties:characteristic.properties]]];
        }
    }
}
//判断Characteristic读写属性
-(NSString*)JudgeCharacteristicProperties:(CBCharacteristicProperties)properties
{
    NSString* PropertiesStr = @"";
    if((properties & CBCharacteristicPropertyRead) != 0)
    {
        PropertiesStr = @"1~";
    }
    else
    {
        PropertiesStr = @"0~";
    }
    if((properties & CBCharacteristicPropertyWrite) != 0)
    {
        PropertiesStr = [PropertiesStr stringByAppendingString:@"1~"];
    }
    else
    {
        PropertiesStr = [PropertiesStr stringByAppendingString:@"0~"];
    }
    if((properties & CBCharacteristicPropertyNotify) != 0)
    {
        PropertiesStr = [PropertiesStr stringByAppendingString:@"1"];
    }
    else
    {
        PropertiesStr = [PropertiesStr stringByAppendingString:@"0"];
    }
    return PropertiesStr;
}
//连接回调
- (void)centralManager:(CBCentralManager *)central
  didConnectPeripheral:(CBPeripheral *)peripheral {
    peripheral.delegate = self;
    NSLog(@"和周边设备连接成功,开始扫描服务...");
    isConnect = true;
    connectPeripheral = peripheral;
    NSString* identifier = [[peripheral identifier] UUIDString];
    [SendToUnity SendBlueMsg:[NSString stringWithFormat:@"BlueConnectState~1~%@~%@",peripheral.name,identifier]];
    //扫描服务
    [peripheral discoverServices:nil];
}
//连接断开回调
- (void)centralManager:(CBCentralManager *)central
didDisconnectPeripheral:(CBPeripheral *)peripheral
                 error:(nullable NSError *)error {
    if(error)
    {
        NSLog(@"断开连接error:%@",error.description);
    }
    else
    {
        isConnect = false;
        connectPeripheral = nil;
        NSString* identifier = [[peripheral identifier] UUIDString];
        [SendToUnity SendBlueMsg:[NSString stringWithFormat:@"BlueConnectState~0~%@~%@",peripheral.name,identifier]];
    }
}
//扫描蓝牙设备回调
- (void)centralManager:(CBCentralManager *)central didDiscoverPeripheral:(CBPeripheral *)peripheral advertisementData:(NSDictionary *)advertisementData RSSI:(NSNumber *)RSSI
{
    if(peripheral == nil)
    {
        return;
    }
    NSString *identifier = nil;
    NSString *foundPeripheral = [self findPeripheralName:peripheral];
    if (foundPeripheral == nil)
        identifier = [[peripheral identifier] UUIDString];
    else
        identifier = foundPeripheral;
    NSLog(@"~~~扫描到周边设备，name:%@ id:%@, rssi: %@", peripheral.name, identifier,RSSI);
    peripheral.delegate = self;
    //添加到字典
    [scanDevices setObject:peripheral forKey:identifier];
    //回调
    [SendToUnity SendBlueMsg:[NSString stringWithFormat:@"ScanBlueResult~%@~%@~%@", peripheral.name,[peripheral.identifier UUIDString],RSSI]];
}
- (CBPeripheral *) findPeripheralInList:(CBPeripheral*)peripheral
{
    CBPeripheral *foundPeripheral = nil;
    
    NSEnumerator *enumerator = [scanDevices keyEnumerator];
    id key;
    while ((key = [enumerator nextObject]))
    {
        CBPeripheral *tempPeripheral = [scanDevices objectForKey:key];
        if ([tempPeripheral isEqual:peripheral])
        {
            foundPeripheral = tempPeripheral;
            break;
        }
    }
    
    return foundPeripheral;
}
- (NSString *) findPeripheralName:(CBPeripheral*)peripheral
{
    NSString *foundPeripheral = nil;
    
    NSEnumerator *enumerator = [scanDevices keyEnumerator];
    id key;
    while ((key = [enumerator nextObject]))
    {
        CBPeripheral *tempPeripheral = [scanDevices objectForKey:key];
        if ([tempPeripheral isEqual:peripheral])
        {
            foundPeripheral = key;
            break;
        }
    }
    
    return foundPeripheral;
}
//音频蓝牙监听回调(系统的音频路由发生改变时)
- (void)myRouteChangeSelector:(NSNotification*)notification {
    if(isListenA2DP)
    {
        [self GetAudioName];
    }
}
//蓝牙监听函数(蓝牙状态改变时调用)
- (void)centralManagerDidUpdateState:(CBCentralManager *)central {
    [self JudgeBlueState:central.state];
}
-(void)JudgeBlueState:(CBManagerState)state
{
    if([SendToUnity JudgeSystemVersion:10.0])
    {
        switch (state) {
                //蓝牙关闭
            case CBManagerStatePoweredOff:{
                NSLog(@"蓝牙没有开启，在设置中打开蓝牙");
                [self BluePoweredOff ];
            }
                break;
            case CBManagerStatePoweredOn: {
                NSLog(@"蓝牙连接正常");
                [self BluePoweredOn];
            }
                break;
            case CBManagerStateUnauthorized:
                NSLog(@"未开启蓝牙权限");
                [SendToUnity SendBlueMsg:@"SupportBLE~1"];
                [SendToUnity SendBlueMsg:@"BluetoothLEPermission~0"];
                break;
            case CBManagerStateUnsupported:
                NSLog(@"手机不支持蓝牙");
                [SendToUnity SendBlueMsg:@"SupportBLE~0"];
                break;
            case CBManagerStateUnknown:
                
                break;
            case CBManagerStateResetting:
                
                break;
        }
    }
    else
    {
        switch (state) {
                //蓝牙关闭
            case CBCentralManagerStatePoweredOff:{
                NSLog(@"蓝牙没有开启，在设置中打开蓝牙");
                [self BluePoweredOff ];
            }
                break;
            case CBCentralManagerStatePoweredOn: {
                NSLog(@"蓝牙连接正常");
                [self BluePoweredOn];
            }
                break;
            case CBCentralManagerStateUnauthorized:
                NSLog(@"未开启蓝牙权限");
                [SendToUnity SendBlueMsg:@"SupportBLE~1"];
                [SendToUnity SendBlueMsg:@"BluetoothLEPermission~0"];
                break;
            case CBCentralManagerStateUnsupported:
                NSLog(@"手机不支持蓝牙");
                [SendToUnity SendBlueMsg:@"SupportBLE~0"];
                break;
            case CBManagerStateUnknown:
                
                break;
            case CBManagerStateResetting:
                
                break;
        }
    }
}
-(void)BluePoweredOff{
    if(!isBLEInit)
    {
        [SendToUnity SendBlueMsg:@"SupportBLE~1"];
        [SendToUnity SendBlueMsg:@"BluetoothLEPermission~1"];
    }
    [self disconnectAll];
    [SendToUnity SendBlueMsg:@"DeviceBlueState~0"];
    [SendToUnity SendBlueMsg:@"SystemBlueState~0"];
    if(isListenA2DP)
    {
        [SendToUnity SendBlueMsg:@"A2DPBlueInfo~0"];
    }
}
-(void)BluePoweredOn{
    if(!isBLEInit)
    {
        [SendToUnity SendBlueMsg:@"SupportBLE~1"];
        [SendToUnity SendBlueMsg:@"BluetoothLEPermission~1"];
    }
    [SendToUnity SendBlueMsg:@"SystemBlueState~1"];
    if(isListenA2DP)
    {
        [self A2DPListen];
    }
}
/**
 *    @brief    将字符表示的16进制数转化为二进制数据
 *
 *    @param     hexString     字符表示的16进制数，长度必须为偶数
 *
 *    @return    二进制数据
 */
- (NSData *)dataWithHexString:(NSString *)hexString
{
    // hexString的长度应为偶数
    if ([hexString length] % 2 != 0)
        return nil;
    
    NSUInteger len = [hexString length];
    NSMutableData *retData = [[NSMutableData alloc] init] ;
    const char *ch = [[hexString dataUsingEncoding:NSASCIIStringEncoding] bytes];
    for (int i=0 ; i<len ; i+=2) {
        
        int height=0;
        if (ch[i]>='0' && ch[i]<='9')
            height = ch[i] - '0';
        else if (ch[i]>='A' && ch[i]<='F')
            height = ch[i] - 'A' + 10;
        else if (ch[i]>='a' && ch[i]<='f')
            height = ch[i] - 'a' + 10;
        else
            // 错误数据
            return nil;
        
        int low=0;
        if (ch[i+1]>='0' && ch[i+1]<='9')
            low = ch[i+1] - '0';
        else if (ch[i+1]>='A' && ch[i+1]<='F')
            low = ch[i+1] - 'A' + 10;
        else if (ch[i+1]>='a' && ch[i+1]<='f')
            low = ch[i+1] - 'a' + 10;
        else
            // 错误数据
            return nil;
        
        int byteValue = height*16 + low;
        [retData appendBytes:&byteValue length:1];
    }
    
    return retData;
}
-(void)writeMsg
{
    //正在发送数据
    if(isSendingMsg)
    {
        return;
    }
    NSLog(@"writeMsg:数量%d",_sendMsgArray.count);
    if(_sendMsgArray.count > 0)
    {
        isSendingMsg = true;
        currentSendMsg = [_sendMsgArray objectAtIndex : 0];
        [_sendMsgArray removeObjectAtIndex : 0];
        [currentSendMsg WriteMsg];
    }
    else
    {
        isSendingMsg = false;
        currentSendMsg = false;
    }
}
-(void)writeNextMsg
{
    isSendingMsg = false;
    [self writeMsg];
}
//读取测试,看能否解析全部数据而不是...
char base64EncodingTable[64] =
{
    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
    'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f',
    'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
    'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/'
};
- (NSString *) base64StringFromData: (NSData *)data length: (int)length
{
    unsigned long ixtext, lentext;
    long ctremaining;
    unsigned char input[3], output[4];
    short i, charsonline = 0, ctcopy;
    const unsigned char *raw;
    NSMutableString *result;
    
    lentext = [data length];
    if (lentext < 1)
        return @"";
    result = [NSMutableString stringWithCapacity: lentext];
    raw = (const unsigned char *)[data bytes];
    ixtext = 0;
    
    while (true) {
        ctremaining = lentext - ixtext;
        if (ctremaining <= 0)
            break;
        for (i = 0; i < 3; i++) {
            unsigned long ix = ixtext + i;
            if (ix < lentext)
                input[i] = raw[ix];
            else
                input[i] = 0;
        }
        output[0] = (input[0] & 0xFC) >> 2;
        output[1] = ((input[0] & 0x03) << 4) | ((input[1] & 0xF0) >> 4);
        output[2] = ((input[1] & 0x0F) << 2) | ((input[2] & 0xC0) >> 6);
        output[3] = input[2] & 0x3F;
        ctcopy = 4;
        switch (ctremaining) {
            case 1:
                ctcopy = 2;
                break;
            case 2:
                ctcopy = 3;
                break;
        }
        
        for (i = 0; i < ctcopy; i++)
            [result appendString: [NSString stringWithFormat: @"%c", base64EncodingTable[output[i]]]];
        
        for (i = ctcopy; i < 4; i++)
            [result appendString: @"="];
        
        ixtext += 3;
        charsonline += 4;
        
        if ((length > 0) && (charsonline >= length))
            charsonline = 0;
    }
    return result;
}
//APP退出时调用
- (void)applicationWillTerminate {
    [self exitBLEForUnity];
}
-(void)exitBLEForUnity
{
    [self stopScanDevice];
    [self disconnectAll];
    isConnect = false;
    connectPeripheral = false;
    isNeedScan = false;
    isSendingMsg = false;
    isListenA2DP = false;
    isBLEInit = false;
    scanDevices = nil;
    _sendMsgArray = nil;
    currentSendMsg = nil;
}
@end
