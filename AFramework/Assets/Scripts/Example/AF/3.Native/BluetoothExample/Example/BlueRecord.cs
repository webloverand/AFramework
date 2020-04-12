
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: BlueRecord.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using AFramework;
using UnityEngine;
using UnityEngine.UI;
	
	public class BlueRecord : MonoBehaviour
	{
	    CharacteristicInfo characteristicInfo;
	    BLEExample bLEExample;
	
	    public InputField startCommand;
	    public InputField stopCommand;
	    public InputField startCommandDelay;
	    public InputField stopCommandDelay;
	    public InputField playCommand;
	    public InputField MaxRecordTimeInput;
	    public Text timeText;
	    public Text statusText;
	
	    string playCommandStr = "";
	    string stopCommandStr = "";
	    string startCommandStr = "";
	    int startCommandDelayInt = 500;
	    int stopCommandDelayInt = 2000;
	
	    float recordTime;
	    int MaxRecordTime;
	
	    bool isStartRecord = false;
	    bool isRealStartRecord = false;
	    bool isStopRecord = false;
	    bool isRealStopRecord = false;
	    bool isRecordInit = false;
	    bool isCanPlay = true;
	
	    public void Init(CharacteristicInfo characteristicInfo, BLEExample bLEExample)
	    {
	        this.characteristicInfo = characteristicInfo;
	        this.bLEExample = bLEExample;
	    }
	    public void Back()
	    {
	        if(isStartRecord)
	        {
	            StopRecordForce();
	        }
	        isStartRecord = false;
	        isRealStartRecord = false;
	        isStopRecord = false;
	        isRealStopRecord = false;
	        isRecordInit = false;
	        isCanPlay = false;
	        recordTime = 0;
	        MaxRecordTime = 0;
	        bLEExample.RecordToDeviceInfo();
	    }
	    public void RecordInit()
	    {
	        //权限申请要注意,申请完成要区分第一次还是之前就有权限,返回有区分,第一次需要延时一会开始录音,因为案例是按钮控制,因此本身就有缓冲时间,这里未写延时
	        NativeInterface.Instance.GetAudioRecordPermission(PermissionCallBack);
	    }
	    public void PermissionCallBack(string result)
	    {
	        if (result.Equals("0"))
	        {
	            bLEExample.TipPage.ShowTip("没有录音权限", TipType.OneBnt);
	            return;
	        }
	        if (startCommand.text == "")
	        {
	            bLEExample.TipPage.ShowTip("开始录音命令为空", TipType.OneBnt);
	            return;
	        }
	        if (stopCommand.text == "")
	        {
	            bLEExample.TipPage.ShowTip("停止录音为空", TipType.OneBnt);
	            return;
	        }
	        if (playCommand.text == "")
	        {
	            bLEExample.TipPage.ShowTip("播放命令为空", TipType.OneBnt);
	            return;
	        }
	        if (MaxRecordTimeInput.text == "")
	        {
	            bLEExample.TipPage.ShowTip("录音时间上限为空", TipType.OneBnt);
	            return;
	        }
	        if (startCommandDelay.text == "")
	        {
	            bLEExample.TipPage.ShowTip("开始命令延迟为空", TipType.OneBnt);
	            return;
	        }
	        if (stopCommandDelay.text == "")
	        {
	            bLEExample.TipPage.ShowTip("停止命令延迟为空", TipType.OneBnt);
	            return;
	        }
	        startCommandStr = startCommand.text;
	        stopCommandStr = stopCommand.text;
	
	        startCommandDelayInt = int.Parse(startCommandDelay.text);
	        stopCommandDelayInt = int.Parse(stopCommandDelay.text);
	
	        playCommandStr = playCommand.text;
	        MaxRecordTime = int.Parse(MaxRecordTimeInput.text);
	        BLEManager.Instance.SetRecordParameter(startCommandStr, stopCommandStr, characteristicInfo.ServiceUUID, characteristicInfo.UUID);
	        statusText.text = "录音状态:已初始化";
	        isRecordInit = true;
	    }
	    public void StartRecord()
	    {
	        if(!isRecordInit)
	        {
	            bLEExample.TipPage.ShowTip("录音参数未初始化", TipType.OneBnt);
	            return;
	        }
	        if (isStartRecord)
	        {
	            bLEExample.TipPage.ShowTip("已经在录音了", TipType.OneBnt);
	            return;
	        }
	        NativeInterface.Instance.SetVolumn(bLEExample.maxVolumn);
	        isCanPlay = false;
	        isStartRecord = true;
	        statusText.text = "录音状态:正在启动";
	        BLEManager.Instance.StartRecord(RealStartRecord, startCommandDelayInt);
	    }
	    public void StopRecord()
	    {
	        if (isStopRecord)
	            return;
	        NativeInterface.Instance.SetVolumn(bLEExample.currentVolumn);
	        isRealStartRecord = false;
	        statusText.text = "录音状态:正在保存";
	        isStopRecord = true;
	        BLEManager.Instance.StopRecord(RealStopRecord, stopCommandDelayInt);
	    }
	    public void StopRecordForce()
	    {
	        if (isStopRecord)
	            return;
	        NativeInterface.Instance.SetVolumn(bLEExample.currentVolumn);
	        isStopRecord = true;
	        BLEManager.Instance.stopRecordForce();
	    }
	    public void PlayRecord()
	    {
	        if(isCanPlay)
	        {
	            if (playCommand.text == "")
	            {
	                bLEExample.TipPage.ShowTip("播放命令为空", TipType.OneBnt);
	                return;
	            }
	            statusText.text = "录音状态:播放录音";
	            BLEManager.Instance.WriteCharacteristic(characteristicInfo, playCommand.text, playCommandCallBack);
	        }
	        else
	        {
	            bLEExample.TipPage.ShowTip("isCanPlay为false", TipType.OneBnt);
	        }
	    }
	    public void playCommandCallBack(bool isSuccess, string error, string sendMsg)
	    {
	        Debug.Log("播放命令是否发送成功:" + isSuccess + " " + error + " " + sendMsg);
	    }
	    public void RealStopRecord(bool isFinish)
	    {
	        if(isFinish)
	        {
	            statusText.text = "录音状态:完成";
	            isCanPlay = true;
	            isStartRecord = false;
	            isStopRecord = false;
	            isRealStartRecord = false;
	            isRealStopRecord = false;
	        }
	    }
	    //回调函数
	    public void RealStartRecord(bool isStart,string error)
	    {
	        Debug.Log("是否能开始录音:" + isStart + " " + error);
	        if (isStart)
	        {
	            statusText.text = "录音状态:录音中";
	            //slider进度开始走
	            recordTime = 0;
	            isRealStartRecord = true;
	        }
	        else
	        {
	            statusText.text = "录音状态:启动失败";
	            isStart = false;
	            bLEExample.TipPage.ShowTip("开始录音失败:" + error, TipType.OneBnt);
	        }
	    }
	    private void Update()
	    {
	        if(isRealStartRecord)
	        {
	            recordTime += Time.deltaTime;
	            if(recordTime >= MaxRecordTime)
	            {
	                timeText.text = "录音时间:" + MaxRecordTime.ToString();
	                StopRecord();
	            }
	            else
	            {
	                timeText.text = "录音时间:" + recordTime.ToString();
	            }
	        }
	    }
	}
}
