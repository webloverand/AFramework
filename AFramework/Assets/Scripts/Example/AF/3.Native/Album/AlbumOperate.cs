
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: AlbumOperate.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using System;
using AFramework;
using NatCorder;
using NatCorder.Clocks;
using NatCorder.Inputs;
using UnityEngine;
using UnityEngine.UI;
	
	public class AlbumOperate : MonoBehaviour
	{
	    public Tip tip;
	    public Text PhotoPathText;
	    void PhotoPathCallBack(string s)
	    {
	        PhotoPathText.text = "Photo Path : " + s;
	    }
	
	    public void OpenAlbum()
	    {
	        AfterPermissionCallBack = RealOpenAlbum;
	        NativeInterface.Instance.GetAlbumPermission(PermissionCallBack);
	    }
	    public void RealOpenAlbum(bool isNeedDelay)
	    {
	        NativeInterface.Instance.OpenAlbum(PhotoPathCallBack);
	    }
	    Action<bool> AfterPermissionCallBack;
	    void PermissionCallBack(string result)
	    {
	        switch (result)
	        {
	            case "1":
	                AfterPermissionCallBack?.Invoke(true);
	                break;
	            case "2":
	                AfterPermissionCallBack?.Invoke(false);
	                AfterPermissionCallBack = null;
	                break;
	            case "0":
	                //没有权限,显示提示,android会一直申请权限,iOS才需要这个
	                tip.ShowTip("没有相册权限,点击前往设置面板!", TipType.OneBnt, ToAPPSetting);
	                break;
	        }
	    }
	    void ToAPPSetting()
	    {
	        NativeInterface.Instance.gotoPermissionSettings();
	    }
	    public Camera[] CaptureCamera;
	    public void StartCapture()
	    {
	        AfterPermissionCallBack = CaptureTexture;
	        NativeInterface.Instance.GetCapturePermission(PermissionCallBack);
	    }
	    void CaptureTexture(bool isNeedDelay)
	    {
	        if (isNeedDelay)
	        {
	            Invoke("CaptureTextureF", 1);
	        }
	        else
	        {
	            CaptureTextureF();
	        }
	    }
	    void CaptureTextureF()
	    {
	        Texture2D texture2D = NativeInterface.Instance.CaptureScreen(CaptureCamera[2], new Rect(0, 0, Screen.width, Screen.height));
	        if (texture2D != null)
	        {
	            DateTime now = DateTime.Now;
	            string filename = string.Format("image{0}{1}{2}{3}.png", now.Day, now.Hour, now.Minute, now.Second);
	            NativeInterface.Instance.SaveTextureToAlbum(filename, texture2D, SaveCallBack);
	        }
	        else
	        {
	            tip.ShowTip("正在连接,请稍候...", TipType.NoBtn);
	        }
	    }
	
	    public void StartManyCapture()
	    {
	        AfterPermissionCallBack = CaptureTexture;
	        NativeInterface.Instance.GetCapturePermission(PermissionCallBack);
	    }
	    void CaptureManyTexture(bool isNeedDelay)
	    {
	        if (isNeedDelay)
	        {
	            Invoke("CaptureManyTexture", 1);
	        }
	        else
	        {
	            CaptureManyTexture();
	        }
	    }
	    public void CaptureManyTexture()
	    {
	        Texture2D texture2D = NativeInterface.Instance.CaptureScreen(CaptureCamera, new Rect(0, 0, Screen.width, Screen.height));
	        if (texture2D != null)
	        {
	            DateTime now = DateTime.Now;
	            string filename = string.Format("image{0}{1}{2}{3}.png", now.Day, now.Hour, now.Minute, now.Second);
	            NativeInterface.Instance.SaveTextureToAlbum(filename, texture2D, SaveCallBack);
	        }
	        else
	        {
	            tip.ShowTip("正在连接,请稍候...", TipType.NoBtn);
	        }
	    }
	
	    public Text textureSavePath;
	    public void SaveCallBack(bool isSuccess, string pathOrError)
	    {
	        textureSavePath.text = "截图路径 : " + pathOrError;
	    }
	    public bool isNeedRecord = false;
	    public bool isStartRecordVedio = false;
	    public void StartRecordVedio(bool isRecord)
	    {
	        if (isRecord)
	        {
	            if (Application.isEditor)
	            {
	                isStartRecordVedio = true;
	                RealRecordVedioF();
	            }
	            else
	            {
	                isNeedRecord = true;
	                AfterPermissionCallBack = RealRecordVedio;
	                NativeInterface.Instance.GetVedioRecordPermission(PermissionCallBack);
	            }
	        }
	        else
	        {
	            isNeedRecord = false;
	            if (isStartRecordVedio)
	            {
	                //停止录音
	                StopRecord();
	            }
	        }
	    }
	    void StopRecord()
	    {
	        NativeInterface.Instance.StopRecordVedio();
	    }
	    void RealRecordVedio(bool isNeedDelay)
	    {
	        if (!isNeedRecord)
	        {
	            return;
	        }
	        if (isNeedDelay)
	        {
	            Invoke("RealRecordVedioF", 1);
	        }
	        else
	        {
	            RealRecordVedioF();
	        }
	    }
	    public int recordTime = 10;
	    public AudioSource audioSource;
	    void RealRecordVedioF()
	    {
	        isStartRecordVedio = true;
	        NativeInterface.Instance.StartRecordVedio(CaptureCamera, Screen.width, Screen.height, true, audioSource,
	             recordVedioCallBack: OnReplay, recordTime: recordTime);
	    }
	    private void OnReplay(string path)
	    {
	        currentRecordTime = 0;
	        if (isStartRecordVedio && recordToggle.isOn)
	        {
	            recordToggle.isOn = false;
	        }
	        isStartRecordVedio = false;
	        if (Application.isEditor)
	        {
	            SaveVedioCallBakc(true, path);
	        }
	        else
	        {
	            NativeInterface.Instance.SaveVedioToAlbum(path, SaveVedioCallBakc);
	        }
	    }
	    public Text VedioPath;
	    public void SaveVedioCallBakc(bool isSuccess, string realPath)
	    {
	        if (isSuccess)
	        {
	            VedioPath.text = "录屏路径 : " + realPath;
	        }
	        else
	        {
	            VedioPath.text = "录屏保存 : 失败";
	        }
	    }
	    public Image fillImage;
	    float currentRecordTime = 0;
	    public Toggle recordToggle;
	    private void Update()
	    {
	        if (isStartRecordVedio)
	        {
	            currentRecordTime += Time.deltaTime;
	            fillImage.fillAmount = currentRecordTime / recordTime;
	        }
	    }
	}
}
