package com.qianxi.afnative;

import android.annotation.TargetApi;
import android.bluetooth.BluetoothA2dp;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattDescriptor;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.BluetoothManager;
import android.bluetooth.BluetoothProfile;
import android.bluetooth.le.BluetoothLeScanner;
import android.bluetooth.le.ScanCallback;
import android.bluetooth.le.ScanFilter;
import android.bluetooth.le.ScanResult;
import android.bluetooth.le.ScanSettings;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Build;
import android.os.Handler;
import android.os.ParcelUuid;
import android.util.Log;

import com.qianxi.afnative.Function.AudioRecordOperate;
import com.qianxi.afnative.Tool.ActivityResult.ActResultRequest;
import com.qianxi.afnative.Tool.Hex.HexUtil;
import com.qianxi.afnative.Tool.Permission.OnPermission;
import com.qianxi.afnative.Tool.Permission.Permission;
import com.qianxi.afnative.Tool.Permission.XXPermissions;
import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.UUID;
/*
蓝牙功能封装
 */

class AndroidBlueSendMsg
{
    public String CompleteMsg;
    public LinkedList<byte[]> dataList;
    public BluetoothGattCharacteristic WriteCharacteristic;
    public BluetoothGatt WriteGatt;
    public Handler handler;
    public int sendInternalTime;

    public AndroidBlueSendMsg(String CompleteMsg,LinkedList<byte[]> data,BluetoothGattCharacteristic WriteCharacteristic,
                              BluetoothGatt WriteGatt,Handler handler,int sendInternalTime)
    {
        this.CompleteMsg = CompleteMsg;
        this.dataList = data;
        this.WriteCharacteristic = WriteCharacteristic;
        this.WriteGatt = WriteGatt;
        this.handler = handler;
        this.sendInternalTime = sendInternalTime;
    }
    //真正发送
    public boolean WriteMsg() {
        boolean success = false;
        if (dataList != null && !dataList.isEmpty()) {
            byte[] t = dataList.getFirst();
            success  = WriteData(t);
            dataList.poll();
            if (success) {
                if(!dataList.isEmpty() && dataList.getFirst() != null)
                {
                    handler.postDelayed(new Runnable() {
                        @Override
                        public void run() {
                            WriteMsg();
                        }
                    }, sendInternalTime); //20-40
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public boolean WriteData(byte[] data)
    {
        if (data == null || data.length > 20) {
            Log.e("AndroidBlueSendMsg", "数据为空或者超出20字节");
            return false;
        }
        WriteCharacteristic.setValue(data);
        if(WriteGatt != null)
        {
            boolean success = WriteGatt.writeCharacteristic(WriteCharacteristic);
            return success;
        }
        else
        {
            return false;
        }
    }
}

public class BLEForUnity {
    //全局使用的参数
    public Context mContext;
    //单例
    public static BLEForUnity Instance;
    //初始化
    boolean isBLEInit = false;
    //蓝牙相关参数
    private BluetoothAdapter mBluetoothAdapter;
    //扫描器
    private BluetoothLeScanner mBluetoothLeScanner;
    //扫描过滤UUID
    private ArrayList<UUID> ServiceUUIDList = null;

    public String TAG = "BLEForUnity";
    public Handler mMainHandler;   //用来进入主程序流程

    //蓝牙需要使用的参数
    private boolean isBlueScaning = false;  //当前是否正在扫描
    private boolean isNeedScan = false;  //是否需要扫描

    public boolean isConnect = false;
    //当前连接的数据蓝牙
    public BluetoothDevice mConnectDataBlue;
    //当前数据蓝牙服务
    public Map<String,BluetoothGatt> mDataBluetoothGatt;
    //服务列表
    private List<BluetoothGattService> mGattServices;
    //搜索到的数据列表
    Map<String,BluetoothDevice> BLEDeviceMap= null;

    //是否监听音频蓝牙
    public boolean isListenA2DP = false;
    //设备上连接的音频蓝牙
    public BluetoothA2dp mA2DPBLE;
    //发送数据队列
    LinkedList<AndroidBlueSendMsg> dataInfoList = new LinkedList<AndroidBlueSendMsg>();
    //注册的通知列表
    List<String> NotifyList;
    //录音辅助参数
    AudioRecordOperate audioRecordOperate;
    String StartRecordCommandStr = "";
    String StopRecordCommandStr = "";
    boolean isRecording = false;
    String ServiceUUID;
    String writeCharacterUUID;

    public static BLEForUnity GetInstance() {
        if (Instance == null) {
            Instance = new BLEForUnity();
        }
        return Instance;
    }

    public static void SendToUnityMsg(String message)
    {
        UnityPlayer.UnitySendMessage("BLEReceiver", "BLEMsgDispose", message);
    }
    public void BLELog(String message)
    {
        Log.i("BLEForUnity", message);
    }


    //供unity调用
    public void BlueInit(boolean islistenA2DP)
    {
        //请求Androidmanifest中必须要有的权限
        mContext  = UnityPlayer.currentActivity.getApplicationContext();
        //判断设备有没有低功耗蓝牙的支持
        if (!this.mContext.getPackageManager().hasSystemFeature("android.hardware.bluetooth_le")) {
            Log.i(TAG, "错误 : 设备不支持Bluetooth Low Energy");
            SendToUnityMsg("SupportBLE~0");
        }
        else {
            SendToUnityMsg("SupportBLE~1");
            ServiceUUIDList = null;
            BLEDeviceMap = null;

            mMainHandler = new Handler(mContext.getMainLooper());
            GetBlueAdapter();
            if(Build.VERSION.SDK_INT >= 21)
            {
                GetBluetoothLeScanner();
            }
            //注册系统蓝牙状态改变监听
            IntentFilter SysStatusFilter = new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
            mContext.registerReceiver(mBlueStatusChanged, SysStatusFilter);
            if(islistenA2DP)
            {
                RegitsterA2DPListen();
            }
            else
            {
                UnRegitsterA2DPListen();
            }
            isNeedScan = false;
            if(isHasPermission(Permission.Group.BLUETOOTH)) {
                //判断蓝牙是否开启
                JudgeSysBlueState();
            }
            else
            {
                RequestLocationPermission();
            }
        }
    }
    public void RegitsterA2DPListen()
    {
        if(!isListenA2DP)
        {
            A2DPListen();
        }
        isListenA2DP = true;
    }
    public void UnRegitsterA2DPListen()
    {
        if(isListenA2DP)
        {
            mContext.unregisterReceiver(mA2DPBlueStatusChanged);
        }
        isListenA2DP = false;
    }
    public void A2DPListen()
    {
        Log.d(TAG, "A2DPListen: 启动音频蓝牙监听:" +  isListenA2DP);
        IntentFilter A2DPStatusFilter = new IntentFilter(BluetoothA2dp.ACTION_CONNECTION_STATE_CHANGED);
        mContext.registerReceiver(mA2DPBlueStatusChanged, A2DPStatusFilter);
        JudgeConAudioBlueA2DP();
    }
    //蓝牙状态判断处理
    public void JudgeSysBlueState()
    {
        if(GetBlueAdapter() != null && !GetBlueAdapter().isEnabled() )//判断手机是否开启蓝牙
        {
            SendToUnityMsg("SystemBlueState~0");
            EnableBlueLE();
        }
        else
        {
            SendToUnityMsg("SystemBlueState~1");
            SendToUnityMsg("BLEInitFinished");
            isBLEInit = true;
        }
    }
    //启动设备蓝牙
    public void EnableBlueLE()
    {
        mMainHandler.post(new Runnable() {
            @Override
            public void run() {
                Log.d(TAG, "手机未开启蓝牙,正在请求开启！");
                Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
                // 启动Activity
                ActResultRequest.init(UnityPlayer.currentActivity)
                        .startForResult(enableBtIntent, new ActResultRequest.ACTRequestCallback() {
                            @Override
                            public void onActivityResult(int resultCode, Intent data) {
                                Log.d(TAG, "请求蓝牙回调,是否需要重新开始扫描:" + isNeedScan);
                                // 处理回调信息
                                if (resultCode ==  UnityPlayer.currentActivity.RESULT_OK) {
                                    SendToUnityMsg("SystemBlueState~1");
                                    if(!isBLEInit)
                                    {
                                        SendToUnityMsg("BLEInitFinished");
                                        isBLEInit = true;
                                    }
                                    mMainHandler.postDelayed(new Runnable() {
                                        @Override
                                        public void run() {
                                            if(isNeedScan)
                                            {
                                                isNeedScan = false;
                                                Log.d(TAG, "run: 开启系统蓝牙后启动扫描");
                                                GetPermissionBeforeScan();
                                            }
                                        }
                                    },1);
                                } else if (resultCode ==  UnityPlayer.currentActivity.RESULT_CANCELED) {
                                    SendToUnityMsg("SystemBlueState~0");
                                    //重新请求开启蓝牙
                                    EnableBlueLE();
                                }
                            }
                        });
            }
        });
    }
    public void StartScan(String serviceUUIDsStr,boolean clearPeripheralList)
    {
        try {
            //根据传过来的ServiceUUIDStr解析成ArrayList
            ServiceUUIDList = new ArrayList<UUID>();
            if(BLEDeviceMap == null)
            {
                BLEDeviceMap = new HashMap<>();
            }
            if (serviceUUIDsStr != null && serviceUUIDsStr != "") {
                if (serviceUUIDsStr.contains("|")) {
                    String[] serviceUUIDs = serviceUUIDsStr.split("|");
                    if (serviceUUIDs != null && serviceUUIDs.length > 0)
                    {
                        for (int i = 0; i < serviceUUIDs.length; i++) {
                            if (serviceUUIDs[i] != null && serviceUUIDs[i].length() >= 4) {
                                ServiceUUIDList.add(GetFullBLEUUID(serviceUUIDs[i]));
                            }
                        }
                    }
                }
                //只有一个ServiceID
                else if (serviceUUIDsStr.length() > 0) {
                    ServiceUUIDList.add(GetFullBLEUUID(serviceUUIDsStr));
                }
            }
            if(clearPeripheralList &&  BLEDeviceMap != null &&  BLEDeviceMap.size() > 0)
            {
                BLEDeviceMap.clear();
            }
            Log.d(TAG, "StartScan: 请求权限");
            GetBLEBeforeScan();
        }
        catch (Exception e)
        {
            Log.d(TAG, "StartScan出现错误: " + e);
        }
    }
    public void GetBLEBeforeScan()
    {
        Log.d(TAG, "GetBLEBeforeScan,是否为空:" + (GetBlueAdapter() == null) );
        //判断蓝牙是否开启
        if(GetBlueAdapter() != null && GetBlueAdapter().isEnabled())
        {
            Log.d(TAG, "GetPermissionBeforeScan: 请求权限");
            GetPermissionBeforeScan();
        }
        else
        {
            Log.d(TAG, "GetBLEBeforeScan: 打开系统蓝牙");
            isNeedScan = true;
            EnableBlueLE();
        }
    }
    public void GetPermissionBeforeScan()
    {
        if(isHasPermission(Permission.Group.LOCATION))
        {
            Log.d(TAG, "GetPermissionBeforeScan: 已有权限调用启动扫描");
            StartScanRequest(ServiceUUIDList);
        }
        else
        {
            isNeedScan = true;
            RequestLocationPermission();
        }
    }
    //判断是否连接上音频蓝牙
    public void JudgeConAudioBlueA2DP() {
        if (GetBlueAdapter() != null)
            GetBlueAdapter().getProfileProxy(mContext, AudioBlueListener, BluetoothProfile.A2DP);
    }
    public void StartScanRequest(ArrayList<UUID> ServiceUUIDs) {
        if (!isBlueScaning) {
            Log.d(TAG, "开始扫描蓝牙设备...");
            if (GetBlueAdapter() != null) {
                isBlueScaning = true;
                //根据ServiceUUID过滤搜索
                if (ServiceUUIDs != null && ServiceUUIDs.size() > 0) {
                    UUID[] uuids = new UUID[ServiceUUIDs.size()];
                    uuids = ServiceUUIDs.toArray(uuids);

                    for (int i = 0; i < uuids.length; i++) {
                        Log.d(TAG, "Scan Service:" + uuids[i].toString());
                    }
                    if (Build.VERSION.SDK_INT >= 23) {
                        Log.d(TAG, "使用API 23位以上扫描,哟局条件限制...");
                        ScanForAPI23(ServiceUUIDs);
                    } else {
                        Log.d(TAG, "使用Legacy scan");
                        GetBlueAdapter().startLeScan(uuids, mLeScanCallback);
                    }
                }
                //搜索全部的设备
                else if (Build.VERSION.SDK_INT >= 23) {
                    if (GetBluetoothLeScanner() != null) {
                        Log.d(TAG, "使用API 23位以上扫描,搜索全部的设备");
                        GetBluetoothLeScanner().startScan(mNewScanCallBack);
                    }
                    else
                    {
                        Log.d(TAG, "GetBluetoothLeScanner()为空");
                    }
                } else {
                    Log.d(TAG, "使用Legacy scan");
                    GetBlueAdapter().startLeScan(mLeScanCallback);
                }
            }
        }
    }
    @TargetApi(23)
    public void StopScanForAPI23()
    {
        if(GetBlueAdapter() != null &&  GetBluetoothLeScanner() != null)
        {
            GetBluetoothLeScanner().stopScan(mNewScanCallBack);
            Log.d(TAG, "StopScan : 停止扫描回调");
        }
    }

    public void StopScan() {
        isBlueScaning = false;
        isNeedScan = false;
        if (GetBlueAdapter() != null)
        {
            if (Build.VERSION.SDK_INT >= 23) {
                StopScanForAPI23();
            } else {
                GetBlueAdapter().stopLeScan(mLeScanCallback);
            }
        }
    }
    public void ConnectBLEByAddress(String macAddress)
    {
        if(mContext != null && GetBlueAdapter() != null)
        {
            if(BLEDeviceMap.containsKey(macAddress))
            {
                mConnectDataBlue = BLEDeviceMap.get(macAddress);
                mConnectDataBlue.connectGatt(mContext,false,coreGattCallback);
            }
            else
            {
                Log.d(TAG, "ConnectBlue : 搜索到的蓝牙设备未包含此蓝牙地址!");
            }
        }
    }
    public void ConnectBLEByName(String BLEName)
    {
        if(mContext != null && GetBlueAdapter() != null)
        {
            for(BluetoothDevice blueDevice : BLEDeviceMap.values())
            {
                if(blueDevice!=null && blueDevice.getName() != null  && blueDevice.getName().equals(BLEName))
                {
                    mConnectDataBlue = blueDevice;
                    Log.d(TAG, "ConnectBlue : 开始连接蓝牙~");
                    mConnectDataBlue.connectGatt(mContext,false,coreGattCallback);
                    return;
                }
            }
            Log.d(TAG, "ConnectBlue : 搜索到的蓝牙设备未包含此蓝牙名称!");
        }
        else
        {

            Log.d(TAG, "ConnectBlue : mContext或者GetBlueAdapter为空!");
        }
    }
    public void ReadCharacteristic(String MACAddress,String ServiceUUID,String CharacteristicUUID)
    {
        BluetoothGatt gatt = mDataBluetoothGatt.get(MACAddress);
        if(gatt != null)
        {
            UUID serviceUUID = GetFullBLEUUID(ServiceUUID);
            if (serviceUUID != null) {

                BluetoothGattService service = gatt.getService(serviceUUID);
                if (service != null) {
                    UUID characteristicUUID = GetFullBLEUUID(CharacteristicUUID);
                    if (characteristicUUID != null) {

                        BluetoothGattCharacteristic characteristic = service.getCharacteristic(characteristicUUID);
                        if (characteristic != null) {
                            if (!gatt.readCharacteristic(characteristic)) {
                                SendToUnityMsg("ReadCharacter~0~Failed to read characteristic");
                            }
                        } else {
                            SendToUnityMsg("ReadCharacter~0~Characteristic not found for Read characteristic");
                        }
                    } else {
                        SendToUnityMsg("ReadCharacter~0~Invalid Characteristic UUID for Read characteristic");
                    }
                } else {
                    SendToUnityMsg("ReadCharacter~0~Service not found for Read characteristic");
                }
            } else {
                SendToUnityMsg("ReadCharacter~0~Invalid Service UUID for Read characteristic");
            }
        }
    }

    boolean isSending = false;

    public void WriteCharacteristic(String MACAddress,String ServiceUUID,String CharacteristicUUID,String sendData,int sendInternalTime)
    {
        if (GetBlueAdapter() != null && mDataBluetoothGatt != null && mContext != null) {

            BluetoothGatt gatt = mDataBluetoothGatt.get(MACAddress);
            if (gatt != null) {
                UUID serviceUUID = GetFullBLEUUID(ServiceUUID);
                if (serviceUUID != null) {
                    BluetoothGattService service = gatt.getService(serviceUUID);
                    if (service != null) {
                        UUID characteristicUUID = GetFullBLEUUID(CharacteristicUUID);
                        if (characteristicUUID != null) {
                            BluetoothGattCharacteristic characteristic = service.getCharacteristic(characteristicUUID);
                            if (characteristic != null) {
                                if(dataInfoList.isEmpty() || dataInfoList.getFirst() != null)
                                {
                                    isSending = false;
                                }
                                splitPacketFor20Byte(sendData,characteristic,gatt,sendInternalTime);
                                mMainHandler.post(new Runnable() {
                                    @Override
                                    public void run() {
                                        WriteMsg();
                                    }
                                });
                            } else {
                                SendToUnityMsg("WriteCharacter~0~Characteristic not found for Write~" + sendData);
                            }
                        } else {
                            SendToUnityMsg("WriteCharacter~0~Not a Valid Characteristic UUID for Write~" + sendData);
                        }
                    } else {
                        SendToUnityMsg("WriteCharacter~0~Service not found for Write~"+ sendData);
                    }
                } else {
                    SendToUnityMsg("WriteCharacter~0~Not a Valid Service UUID for Write~"+ sendData);
                }
            }
        }
    }
    public void SubscribeCharacteristic(String MACAddress,String ServiceUUID,String CharacteristicUUID)
    {
        NotifyCharacteristic(MACAddress,ServiceUUID,CharacteristicUUID,true);
    }
    public void UnSubscribeCharacteristic(String MACAddress,String ServiceUUID,String CharacteristicUUID)
    {
        NotifyCharacteristic(MACAddress,ServiceUUID,CharacteristicUUID,false);

    }
    protected static final UUID Characteristic_Descriptor_UUID = UUID.fromString("00002902-0000-1000-8000-00805f9b34fb");
    public void NotifyCharacteristic(String MACAddress,String ServiceUUID,String CharacteristicUUID,boolean isEnable)
   {
       BluetoothGatt gatt = mDataBluetoothGatt.get(MACAddress);
       String callbackStr = "UnNotifyCharacter~";
       if(isEnable)
       {
           callbackStr = "NotifyCharacter~";
       }
       if (gatt != null) {
           if(NotifyList == null) {
               NotifyList = new LinkedList<>();
           }
           if(isEnable && NotifyList.contains(MACAddress))
           {
               SendToUnityMsg(callbackStr+"1~Success");
               return;
           }
           else if(!isEnable && NotifyList.contains(MACAddress))
           {
               NotifyList.remove(MACAddress);
           }
           UUID serviceUUID = GetFullBLEUUID(ServiceUUID);
           if (serviceUUID != null) {

               BluetoothGattService service = gatt.getService(serviceUUID);
               if (service != null) {

                   UUID characteristicUUID = GetFullBLEUUID(CharacteristicUUID);
                   if (characteristicUUID != null) {

                       BluetoothGattCharacteristic characteristic = service.getCharacteristic(characteristicUUID);
                       if (characteristic != null) {
                           if (gatt.setCharacteristicNotification(characteristic, true)) {
                               for(BluetoothGattDescriptor dp: characteristic.getDescriptors()) {

                                   BluetoothGattDescriptor descriptor = characteristic.getDescriptor(Characteristic_Descriptor_UUID);
                                   if (descriptor != null) {
                                       int characteristicProperties = characteristic.getProperties();
                                       byte[] descriptorStr = BluetoothGattDescriptor.DISABLE_NOTIFICATION_VALUE;
                                       if (isEnable) {
                                           if ((characteristicProperties & 0x10) == 16) {
                                               descriptorStr = BluetoothGattDescriptor.ENABLE_NOTIFICATION_VALUE;
                                           } else if ((characteristicProperties & 0x20) == 32) {
                                               descriptorStr = BluetoothGattDescriptor.ENABLE_INDICATION_VALUE;
                                           }
                                       }
                                       if(descriptorStr != null)
                                       {
                                           dp.setValue(descriptorStr);
                                           gatt.writeDescriptor(dp);
                                           if(isEnable)
                                           {
                                               NotifyList.add(MACAddress);
                                           }
                                           SendToUnityMsg(callbackStr+"1~Success");
                                           Log.d(TAG, "蓝牙通知回调: " + callbackStr+"1~Success");
                                       }
                                       else
                                       {
                                           SendToUnityMsg(callbackStr+"0~Failed to set notification type");
                                       }
                                   }
                                   else
                                   {
                                       SendToUnityMsg(callbackStr+"0~Failed to get notification descriptor");
                                   }
                               }
                           } else {
                               SendToUnityMsg(callbackStr+"0~Failed to set characteristic notification");
                           }
                       } else {
                           SendToUnityMsg(callbackStr+"0~Characteristic not found for Subscribe");
                       }
                   } else {
                       SendToUnityMsg(callbackStr+"0~Not a Valid Characteristic UUID for Subscribe");
                   }
               } else {
                   SendToUnityMsg(callbackStr+"0~Service not found for Subscribe");
               }
           } else {
               SendToUnityMsg(callbackStr+"0~Not a Valid Service UUID for Subscribe");
           }
       }
   }
   public void DisConnect() {
       DisConnectByMacAddress(mConnectDataBlue.getAddress());
   }
    public void DisConnectByMacAddress(String MACAddress) {
        //断开
        if (mDataBluetoothGatt != null && mDataBluetoothGatt.containsKey(MACAddress)) {
            SendToUnityMsg("BlueConnectState~0~"+ mDataBluetoothGatt.get(MACAddress).getDevice().getName()+"~MACAddress");
            mDataBluetoothGatt.get(MACAddress).disconnect();
            mDataBluetoothGatt.remove(MACAddress);
        }

        if (NotifyList != null && NotifyList.contains(MACAddress))
            NotifyList.remove(MACAddress);
        if (mConnectDataBlue != null && mConnectDataBlue.getAddress().equals(MACAddress)) {
            if (mGattServices != null)
                mGattServices.clear();
            mConnectDataBlue = null;
            isConnect = false;
            ServiceUUIDList = null;
        }
    }
    public void DisConnectByName(String BLEName) {
        if(mContext != null && GetBlueAdapter() != null)
        {
            for(BluetoothDevice blueDevice : BLEDeviceMap.values())
            {
                if(blueDevice.getName().equals(BLEName))
                {
                    DisConnectByMacAddress(blueDevice.getAddress());
                }
                return;
            }
        }
    }
    public void BlueDeInit()
    {
        if(isBlueScaning)
        {
            StopScan();
            BLEDeviceMap.clear();
            isBlueScaning = false;
        }
        if(mConnectDataBlue != null)
        {
            DisConnect();
        }
        isListenA2DP = false;
        mA2DPBLE = null;
        NotifyList = null;
        isNeedScan = false;
        isBLEInit = false;
        mContext.unregisterReceiver(mBlueStatusChanged);
        mContext.unregisterReceiver(mA2DPBlueStatusChanged);
    }
    //设置录音参数,包含录音命令,录音命令延时,停止录音命令,停止录音命令延时
    public void SetRecordParameter(String startRecordCommand,String stopRecordCommand,
                                   String serviceUUID,String writeCharacterUUID)
    {
        StartRecordCommandStr = startRecordCommand;
        StopRecordCommandStr = stopRecordCommand;
        this.ServiceUUID = serviceUUID;
        this.writeCharacterUUID = writeCharacterUUID;
        isRecording = false;
        GetAudioRecord().isRecording = false;
        Log.d(TAG, "SetRecordParameter: " + startRecordCommand + " " + stopRecordCommand);
        Log.d(TAG, "SetRecordParameter: " + serviceUUID + " " + writeCharacterUUID);
    }
    public void CheckRecordPermission()
    {
        if(GetAudioRecord().isHasRecordPermission(mContext))
        {
            SendToUnityMsg("CheckRecordPermission~1");
        }
        else
        {
            SendToUnityMsg("CheckRecordPermission~0");
        }
    }
    Thread RecordThread;
    public void StartRecord(final int startCommandDelay)
    {
        if(GetAudioRecord().isHasRecordPermission(mContext)) {
            if (!isRecording) {
                SendToUnityMsg("RealStartRecord~1~");
                isRecording = true;
                RecordThread = new Thread(new Runnable() {
                    @Override
                    public void run() {
                        mMainHandler.postDelayed(new Runnable() {
                            @Override
                            public void run() {
                                Log.d(TAG, "StartRecord: 发送开始录音命令" + StartRecordCommandStr + " " + startCommandDelay);
                                WriteCharacteristic(mConnectDataBlue.getAddress(), ServiceUUID, writeCharacterUUID, StartRecordCommandStr, 30);
                            }
                        }, startCommandDelay); //延迟一秒发送开始录音命令
                        GetAudioRecord().StartRecord();
                    }
                });
                RecordThread.start();
            }
        }
        else
        {
            SendToUnityMsg("RealStartRecord~0~没有录音权限");
        }
    }
    public void StopRecord(final int stopCommandDelay)
    {
        isRecording = false;
        mMainHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                Log.d(TAG, "StopRecord: 发送停止录音命令" + StopRecordCommandStr);
                WriteCharacteristic(mConnectDataBlue.getAddress(),ServiceUUID,writeCharacterUUID,StopRecordCommandStr,30);
               SendToUnityMsg("RealFinishRecord~1");
            }
        },stopCommandDelay); //延迟一秒发送停止命令
        GetAudioRecord().StopRecord();
    }
    public void stopRecordForce()
    {
        mMainHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                WriteCharacteristic(mConnectDataBlue.getAddress(),ServiceUUID,writeCharacterUUID,StopRecordCommandStr,30);
            }
        },500); //延迟一秒发送停止命令
        isRecording = false;
        GetAudioRecord().StopRecord();
    }
    //数据分包
    private void splitPacketFor20Byte(String sendData,  BluetoothGattCharacteristic characteristic, BluetoothGatt gatt,int sendInternalTime ) {
        LinkedList dataSplite = new LinkedList<>();
        byte[] data = HexUtil.decodeHex(sendData.toCharArray());
        if (data != null) {
            int index = 0;
            do {
                byte[] surplusData = new byte[data.length - index];
                byte[] currentData;
                System.arraycopy(data, index, surplusData, 0, data.length - index);
                if (surplusData.length <= 20) {
                    currentData = new byte[surplusData.length];
                    System.arraycopy(surplusData, 0, currentData, 0, surplusData.length);
                    index += surplusData.length;
                } else {
                    currentData = new byte[20];
                    System.arraycopy(data, index, currentData, 0, 20);
                    index += 20;
                }
                dataSplite.addLast(currentData);
            } while (index < data.length);
            dataInfoList.addLast(new AndroidBlueSendMsg(sendData,dataSplite,characteristic,gatt,mMainHandler,sendInternalTime));
        }
    }
    //上一层真正发送
    private void WriteMsg() {
        if(isSending)
        {
            return;
        }
        boolean success = false;
        if (dataInfoList != null && !dataInfoList.isEmpty()) {
            AndroidBlueSendMsg t =  dataInfoList.getFirst();
            success = t.WriteMsg();
            if (success) {
                Log.d(TAG, "WriteMsg: 发送成功" + t.CompleteMsg);
                //一条完整信息发送成功
                SendToUnityMsg("WriteCharacter~1~" + t.CompleteMsg);
            }
            else
            {
                Log.d(TAG, "WriteMsg: 发送失败");
                //发送失败
                SendToUnityMsg("WriteCharacter~0~Sending Process Error~" + t.CompleteMsg);
            }
            dataInfoList.poll();
            if(!dataInfoList.isEmpty() && dataInfoList.getFirst() != null)
            {
                mMainHandler.postDelayed(new Runnable() {
                    @Override
                    public void run() {
                        WriteMsg();
                    }
                },20);
            }
            else
            {
                isSending = false;
            }
        }
    }

    //API 23以下的扫描回调
    BluetoothAdapter.LeScanCallback mLeScanCallback = new BluetoothAdapter.LeScanCallback()
    {
        public void onLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            if(device.getBondState() != BluetoothDevice.BOND_BONDED)
            {
                BLEDeviceMap.put(device.getAddress(),device);
                SendToUnityMsg("ScanBlueResult~" + device.getName() + "~" + device.getAddress() +
                        "~" + rssi);
            }
        }
    };
    //蓝牙扫描设备回调
    @TargetApi(23)
    ScanCallback mNewScanCallBack = new ScanCallback() {
        @Override
        public void onScanResult(int callbackType, ScanResult result) {
            if(result.getDevice().getBondState() != BluetoothDevice.BOND_BONDED)
            {
                Log.d(TAG, "扫描结果: " + result.getDevice().getName());
                BLEDeviceMap.put(result.getDevice().getAddress(),result.getDevice());
                SendToUnityMsg("ScanBlueResult~"+result.getDevice().getName() + "~" + result.getDevice().getAddress() + "~" + result.getRssi());
            }
            else
            {
                Log.d(TAG, "扫描结果: " + result.getDevice().getName() + "已经绑定");
            }
        }
        @Override
        public void onScanFailed(int errorCode) {
            super.onScanFailed(errorCode);
        }
    };
    @TargetApi(23)
    private void ScanForAPI23(List<UUID> uuids) {
        if(GetBluetoothLeScanner() == null)
        {
            return;
        }
        List<ScanFilter> scanFilters = new ArrayList<>();
        for (int i = 0; i < uuids.size(); i++) {
            scanFilters.add((new ScanFilter.Builder()).setServiceUuid(ParcelUuid.fromString(((UUID)uuids.get(i)).toString())).build());
        }
        ScanSettings settings = (new ScanSettings.Builder()).setCallbackType(1).setScanMode(1).build();
        if (uuids != null) {
            GetBluetoothLeScanner().startScan(scanFilters,settings, mNewScanCallBack);
        } else {
           GetBluetoothLeScanner().startScan(null, settings, mNewScanCallBack);
        }
    }
    //获取系统连接蓝牙中是否有音频蓝牙回调
    private BluetoothProfile.ServiceListener AudioBlueListener = new BluetoothProfile.ServiceListener() {
        @Override
        public void onServiceDisconnected(int profile) {
            if(profile == BluetoothProfile.A2DP){
                mA2DPBLE = null;
                SendToUnityMsg("A2DPBlueInfo~0");
            }
        }
        @Override
        public void onServiceConnected(int profile, BluetoothProfile proxy) {
            if (profile == BluetoothProfile.A2DP) {
                mA2DPBLE = (BluetoothA2dp) proxy; //转换
                List<BluetoothDevice> a2dpList = mA2DPBLE.getConnectedDevices();
                if (a2dpList.size() > 0) {
                    for (BluetoothDevice device : a2dpList) {
                        SendToUnityMsg("A2DPBlueInfo~1~" + device.getName() + "~" + device.getAddress());
                    }
                    Log.d(TAG, "onServiceConnected: 存在音频蓝牙");
                }
                else
                {
                    SendToUnityMsg("A2DPBlueInfo~0");
                    Log.d(TAG, "onServiceConnected: 不存在音频蓝牙");
                }
            }
        }
    };
    //连接蓝牙的回调类
    private BluetoothGattCallback coreGattCallback = new BluetoothGattCallback() {
        /**
         * 连接状态改变，主要用来分析设备的连接与断开
         * @param gatt GATT
         * @param status 改变前状态
         * @param newState 改变后状态
         */
        @Override
        public void onConnectionStateChange(final BluetoothGatt gatt, final int status, final int newState) {
            if (newState == BluetoothGatt.STATE_CONNECTED) {
                Log.d(TAG, "蓝牙连接成功,开始扫描服务");
                SendToUnityMsg("BlueConnectState~1~" + gatt.getDevice().getName() + "~"+ gatt.getDevice().getAddress());
                if(mDataBluetoothGatt == null)
                    mDataBluetoothGatt = new HashMap<>();
                mDataBluetoothGatt.put(gatt.getDevice().getAddress(),gatt);

                if(!gatt.discoverServices())
                {
                    mMainHandler.postDelayed(new Runnable() {
                        @Override
                        public void run() {
                            Log.d(TAG, "蓝牙扫描服务失败,因此重新开始扫描");
                            gatt.discoverServices();
                        }
                    },1000);
                }
            } else if (newState == BluetoothGatt.STATE_DISCONNECTED) {
                Log.d(TAG, "蓝牙断开连接");
                SendToUnityMsg("BlueConnectState~0~null~null");
                gatt.close();
            } else if (newState == BluetoothGatt.STATE_CONNECTING) {
                Log.d(TAG, "蓝牙正在连接");
                isConnect = false;
            }
        }

        /**
         * 发现服务，主要用来获取设备支持的服务列表
         * @param gatt GATT
         * @param status 当前状态
         */
        @Override
        public void onServicesDiscovered(final BluetoothGatt gatt, final int status) {
            if(gatt == null)
            {
                return;
            }
            mGattServices = gatt.getServices();
            if(mGattServices.size() > 0)
            {
                Log.d(TAG, "onServicesDiscovered:" + status + " 搜索服务长度为:" + mGattServices.size());
                for (BluetoothGattService bluetoothGattService:mGattServices){
                    String ServiceUUID = bluetoothGattService.getUuid().toString();
                    //1800和1801分别表示Generic Access和Generic Attribute，描述了设备连接相关属性。蓝牙联盟预定的服务
                    if(ServiceUUID.startsWith("00001800") || ServiceUUID.startsWith("00001801"))
                    {
                        continue;
                    }
                    SendToUnityMsg("DiscoveryService~"+ServiceUUID);
                    List<BluetoothGattCharacteristic> gattCharacteristics = bluetoothGattService.getCharacteristics();
                    if(gattCharacteristics.size() > 0)
                    {
                        for (BluetoothGattCharacteristic gattCharacteristic : gattCharacteristics) {
                            int charaProp = gattCharacteristic.getProperties();
                            int canRead = 0;
                            int canWrite = 0;
                            int canNotify = 0;
                            // 可读
                            if ((charaProp & BluetoothGattCharacteristic.PROPERTY_READ) > 0) {
                                canRead = 1;
                            }
                            // 可写，注：要 & 其可写的两个属性
                            if ((charaProp & BluetoothGattCharacteristic.PROPERTY_WRITE_NO_RESPONSE) > 0
                                    || (charaProp & BluetoothGattCharacteristic.PROPERTY_WRITE) > 0) {
                                canWrite = 1;
                            }
                            // 可通知，可指示
                            if ((charaProp & BluetoothGattCharacteristic.PROPERTY_NOTIFY) > 0
                                    || (charaProp & BluetoothGattCharacteristic.PROPERTY_INDICATE) > 0) {
                                canNotify = 1;
                            }
                            SendToUnityMsg("DiscoveryCharacteristic~"+ gatt.getDevice().getName()+"~"+ gatt.getDevice().getAddress() +"~"+ ServiceUUID +
                                    "~" + gattCharacteristic.getUuid() + "~" + canRead + "~" +canWrite + "~" + canNotify);
                        }
                    }
                }

            }
            else
            {
                Log.d(TAG, "搜索到的服务长度为0,因此重新开始扫描");
                gatt.discoverServices();
            }
        }

        /**
         * 读取特征值，主要用来读取该特征值包含的可读信息
         * @param gatt GATT
         * @param characteristic 特征值
         * @param status 当前状态
         */
        @Override
        public void onCharacteristicRead(BluetoothGatt gatt, final BluetoothGattCharacteristic characteristic, final int status) {
            if (status == BluetoothGatt.GATT_SUCCESS) {
               String data =HexUtil.encodeHexStr(characteristic.getValue());
               if(data != null)
               {
                   SendToUnityMsg("ReadCharacter~1~" + gatt.getDevice().getName()+"~"+ gatt.getDevice().getAddress()+"~"+data);
               }
            }
        }

        /**
         * 特征值改变，主要用来接收设备返回的数据信息
         * @param gatt GATT
         * @param characteristic 特征值
         */
        @Override
        public void onCharacteristicChanged(BluetoothGatt gatt, final BluetoothGattCharacteristic characteristic) {
            Log.d(TAG, "特征值改变:" + characteristic .getUuid()+ "内容:" + HexUtil.encodeHexStr(characteristic.getValue()));
            //读取到数据
            SendToUnityMsg("NotifyCharacterData~" +  gatt.getDevice().getName()+"~"+ gatt.getDevice().getAddress()+"~"+ characteristic .getUuid()+"~"+
                    HexUtil.encodeHexStr(characteristic.getValue()));
        }

        /**
         * 写入特征值，主要用来发送数据到设备
         * @param gatt GATT
         * @param characteristic 特征值
         * @param status 当前状态
         */
        @Override
        public void onCharacteristicWrite(BluetoothGatt gatt, final BluetoothGattCharacteristic characteristic, final int status) {

        }
        /**
         * 写入属性描述值，主要用来根据当前属性描述值写入数据到设备
         * @param gatt GATT
         * @param descriptor 属性描述值
         * @param status 当前状态
         */
        @Override
        public void onDescriptorWrite(BluetoothGatt gatt, final BluetoothGattDescriptor descriptor, final int status) {
            if (status == BluetoothGatt.GATT_SUCCESS) {
                Log.d(TAG, "绑定写通道: " + HexUtil.encodeHexStr(descriptor.getValue()));
            }
            else
            {

            }
        }

        /**
         * 读取属性描述值，主要用来获取设备当前属性描述的值
         * @param gatt GATT
         * @param descriptor 属性描述
         * @param status 当前状态
         */
        @Override
        public void onDescriptorRead(BluetoothGatt gatt, final BluetoothGattDescriptor descriptor, final int status) {
            if (status == BluetoothGatt.GATT_SUCCESS) {
                Log.d(TAG, "绑定读通道: " + HexUtil.encodeHexStr(descriptor.getValue()));
            } else {
            }
        }
        /**
         * 阅读设备信号值
         * @param gatt GATT
         * @param rssi 设备当前信号
         * @param status 当前状态
         */
        @Override
        public void onReadRemoteRssi(BluetoothGatt gatt, int rssi, int status) {

        }
    };
    // 系统蓝牙的打开和关闭监听
    private BroadcastReceiver mBlueStatusChanged = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            switch (intent.getAction()) {
                case BluetoothAdapter.ACTION_STATE_CHANGED: //系统蓝牙的开关
                    int blueState = intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, 0);
                    int bluePreState = intent.getIntExtra(BluetoothAdapter.EXTRA_PREVIOUS_STATE, 0);
                    //系统蓝牙打开
                    if (blueState == BluetoothAdapter.STATE_ON && bluePreState == BluetoothAdapter.STATE_TURNING_ON) {
                        SendToUnityMsg("SystemBlueState~1");
                        if(isListenA2DP)
                        {
                            A2DPListen();
                        }
                    }
                    //系统蓝牙关闭
                    else if (blueState == BluetoothAdapter.STATE_OFF && bluePreState == BluetoothAdapter.STATE_TURNING_OFF) {
                        mBluetoothLeScanner = null;
                        mBluetoothAdapter = null;
                        if(isBlueScaning)
                        {
                            isBlueScaning = false;
                            isNeedScan = false;
                        }
                        DisConnect();
                        SendToUnityMsg("DeviceBlueState~0");
                        SendToUnityMsg("SystemBlueState~0");
                        if(isListenA2DP)
                        {
                            SendToUnityMsg("A2DPBlueInfo~0");
                        }
                    }
                    break;
            }
        }
    };
    // 音频蓝牙连接与断开的监听
    private BroadcastReceiver mA2DPBlueStatusChanged = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            switch (intent.getAction()) {
                case BluetoothA2dp.ACTION_CONNECTION_STATE_CHANGED: //音频蓝牙的监听
                    if(isListenA2DP)
                    {
                        int CurState = intent.getIntExtra(BluetoothA2dp.EXTRA_STATE, BluetoothA2dp.STATE_DISCONNECTED);
                        if(CurState == BluetoothA2dp.STATE_DISCONNECTED || CurState == BluetoothA2dp.STATE_CONNECTED)
                        {
                            //判断是否有音频蓝牙连接
                            JudgeConAudioBlueA2DP();
                        }
                    }
                    break;
            }
        }
    };

    public void ApplicationPause(boolean pause,boolean isControlScan)
    {
        if (pause)
        {
            //程序进入后台
            Log.d(TAG, "ApplicationPause: 进入后台");

            //停止识别
            if(isControlScan && isBlueScaning)
            {
                StopScan();
                isNeedScan = true;
            }
        }
        else
        {
            Log.d(TAG, "ApplicationPause: 进入前台,是否需要启动扫描:" + isNeedScan );
            GetNewBlueState();
            if(isControlScan && isNeedScan)
            {
                isNeedScan = false;
                Log.d(TAG, "StartScan: 进入前台启动扫描");
                GetBLEBeforeScan();
            }
        }
    }
    public AudioRecordOperate GetAudioRecord()
    {
        if(audioRecordOperate == null)
        {
            audioRecordOperate = new AudioRecordOperate();
        }
//        if(audioRecordOperate == null)
//        {
//            Log.d(TAG, "GetAudioRecord: 新建为空?????");
//        }
        return audioRecordOperate;
    }

    BluetoothAdapter GetBlueAdapter()
    {
        if (mBluetoothAdapter == null) {
            //蓝牙初始化
            final BluetoothManager bluetoothManager = (BluetoothManager) UnityPlayer.currentActivity.getSystemService(Context.BLUETOOTH_SERVICE);
            mBluetoothAdapter = bluetoothManager.getAdapter();
        }
        if(mBluetoothAdapter == null)
        {
            Log.e(TAG, "GetBlueAdapter: 获取BluetoothAdapter为空");
        }
        return mBluetoothAdapter;
    }
    BluetoothLeScanner GetBluetoothLeScanner()
    {
        if(Build.VERSION.SDK_INT >= 21)
        {
            if(GetBlueAdapter() != null && mBluetoothLeScanner == null)
            {
                mBluetoothLeScanner = mBluetoothAdapter.getBluetoothLeScanner();
            }
        }
        if(mBluetoothAdapter == null)
        {
            Log.e(TAG, "GetBlueAdapter: 获取BluetoothLeScanner为空");
        }
        return mBluetoothLeScanner;
    }
    public void GetNewBlueState()
    {
        //更新unity蓝牙信息,因为在进入后台的时候有可能断开蓝牙或者其他情况
        if( GetBlueAdapter().isEnabled())
        {
            SendToUnityMsg("SystemBlueState~1");
        }
        else{
            SendToUnityMsg("SystemBlueState~0");
        }
        if(isListenA2DP)
        {
            JudgeConAudioBlueA2DP();
        }
        if(isConnect)
        {
            SendToUnityMsg("BlueConnectState~1"+ mConnectDataBlue.getName() + "~"+ mConnectDataBlue.getAddress());
        }
        else
        {
            SendToUnityMsg("BlueConnectState~0~null~null");
        }
    }

    private UUID GetFullBLEUUID(String uuidString) {
        UUID uuid;
        if (uuidString.length() == 4) {
            uuid = UUID.fromString("0000" + uuidString + "-0000-1000-8000-00805F9B34FB");
        } else {
            uuid = UUID.fromString(uuidString);
        }
        return uuid;
    }

    public boolean isHasPermission(String[] pemission) {
        if (XXPermissions.isHasPermission(UnityPlayer.currentActivity, pemission)) {
            return true;
        }else {
            return false;
        }
    }
    //跳转到权限设置页面
    public void gotoPermissionSettings() {
        XXPermissions.gotoPermissionSettings(UnityPlayer.currentActivity);
    }
    public void RequestLocationPermission()
    {
        XXPermissions.with(UnityPlayer.currentActivity)
                // 可设置被拒绝后继续申请，直到用户授权或者永久拒绝
                //.constantRequest()
                // 支持请求6.0悬浮窗权限8.0请求安装权限
                //.permission(Permission.REQUEST_INSTALL_PACKAGES)
                // 不指定权限则自动获取清单中的危险权限
                .permission(Permission.Group.BLUETOOTH)
                .request(new OnPermission() {
                    @Override
                    public void hasPermission(List<String> granted, boolean isAll) {
                        if (isAll) {
                            if(!isBLEInit)
                            {
                                SendToUnityMsg("BluetoothLEPermission~1");
                                JudgeSysBlueState();
                            }
                            else if(isNeedScan)
                            {
                                isNeedScan = false;
                                Log.d(TAG, "hasPermission: 获取权限后开启扫描");
                                StartScanRequest(ServiceUUIDList);
                            }
                        }else {
                            RequestLocationPermission();
                        }
                    }

                    @Override
                    public void noPermission(List<String> denied, boolean quick) {
                        if(quick) {
                            //如果是被永久拒绝就跳转到应用权限系统设置页面(unity逻辑层设置)
                            SendToUnityMsg("BluetoothLEPermission~0");
                        }else {
                            RequestLocationPermission();
                        }
                    }
                });
    }
}
