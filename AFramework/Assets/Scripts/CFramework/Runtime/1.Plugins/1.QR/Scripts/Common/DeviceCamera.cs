using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class DeviceCamera {

	public enum CameraMode
	{
		Rear=0,
		Front,
		NONE
	}

	public Texture preview
	{
		get
		{
			return webcamera;
		}
	}

    public WebCamTexture webcamera;
	bool isRunning = false;
	int previewWidth = 0;
	int previewHeight = 0;

	public CameraMode mCamMode = CameraMode.Rear;
	string deviceName= "";

	public int centerBlockWidth;
	bool isCalc = false;

	Color32[] centerColorArr;

	public DeviceCamera (CameraMode mode = CameraMode.Rear)
	{
		previewWidth = 640;
		previewHeight = 480;

		if (!GetDeviceName (mode)) {
			Debug.Log ("current device have no device camera !");
			return;
		}
		mCamMode = mode;
		webcamera = new WebCamTexture (this.deviceName,640, 480);
	}

	public DeviceCamera (int width,int height,CameraMode mode = CameraMode.Rear)
	{
		previewWidth = width;
		previewHeight = height;
		if (!GetDeviceName (mode)) {
			Debug.Log ("current device have no device camera !");
			return;
		}
		mCamMode = mode;
		webcamera = new WebCamTexture (this.deviceName,width, height);
	}

	public void ActiveRearCamera()
	{
		if (mCamMode == CameraMode.Front) {
			if (webcamera != null) {
				webcamera.Stop ();
				webcamera = null;
				if (!GetDeviceName (CameraMode.Rear)) {
					Debug.Log ("current device have no rear camera !");
				}
				webcamera = new WebCamTexture (this.deviceName,previewWidth, previewHeight);
				if (isRunning) {
					webcamera.Play ();
				}
				mCamMode = CameraMode.Rear;
			}
		}
	}

	public void ActiveFrontCamera()
    {
        if (mCamMode == CameraMode.Rear)
        {
            if (webcamera != null) {
				webcamera.Stop ();
				webcamera = null;
				if (!GetDeviceName (CameraMode.Front)) {
					Debug.Log ("current device have no front camera !");
				}
				webcamera = new WebCamTexture (this.deviceName,previewWidth, previewHeight);
				if (isRunning) {
					webcamera.Play ();
				}
				mCamMode = CameraMode.Front;
			}
		}
	}

	public void Play()
	{
		if (isRunning) {
			return;
		}
		if (webcamera != null) {
			webcamera.Play ();		
			isRunning = true;
		}
	}

	public void Stop()
	{
		if (!isRunning) {
			return;
		}
		if (webcamera != null) {
			webcamera.Stop ();
		}
		isRunning = false;
	}

	public Vector2 getSize()
	{
		return new Vector2(webcamera.width, webcamera.height); 
	}

	public int Width()
	{
		if (webcamera != null) {
			return webcamera.width;
		} else {
			return -1;
		}

	}

	public int Height()
	{
		if (webcamera != null) {
			return webcamera.height;
		} else {
			return -1;
		}
	}

	public bool isPlaying()
	{
		if (webcamera != null) {
			return webcamera.isPlaying; 	
		} else {
			return false;
		}
	}

	public Color[] GetPixels()
	{
		if (webcamera != null) {
			return webcamera.GetPixels ();
		} else {
			return null;
		}
	}

	public Color[] GetPixels(int x,int y,int targetWidth,int targetHeight)
	{
		if (webcamera != null) {
		
			return webcamera.GetPixels (x, y, targetWidth, targetHeight); 
		} else {
			return null;
		}
	}

	public Color32[] GetPixels32()
	{
		if (webcamera != null) {
			return webcamera.GetPixels32 (); 
		} else {
			return null;
		}
	}


	public Color32[] GetCenterPixels32()
	{
		if (!isCalc && Width () > 100 && Height () > 100) {
			centerBlockWidth = (int)((Math.Min (Width (), Height ()) / 3f) * 2);
			isCalc = true;
		} 
		if (!isCalc) {
			return null;
		}

		if(centerColorArr == null)
		{
			centerColorArr= new Color32[centerBlockWidth * centerBlockWidth];
		}

		int posx = ((Width()-centerBlockWidth)>>1);//
		int posy = ((Height()-centerBlockWidth)>>1);

		Color[] orginalc = GetPixels(posx,posy,centerBlockWidth,centerBlockWidth);// get the webcam image colors

		//convert the color(float) to color32 (byte)
		for(int i=0;i!= centerBlockWidth;i++)
		{
			for(int j = 0;j!=centerBlockWidth ;j++)
			{
				centerColorArr[i + j*centerBlockWidth].r = (byte)( orginalc[i + j*centerBlockWidth].r*255);
				centerColorArr[i + j*centerBlockWidth].g = (byte)(orginalc[i + j*centerBlockWidth].g*255);
				centerColorArr[i + j*centerBlockWidth].b = (byte)(orginalc[i + j*centerBlockWidth].b*255);
				centerColorArr[i + j*centerBlockWidth].a = 255;
			}
		}

		return centerColorArr;

	}




	public float GetRotationAngle()
	{
		return (float)(-(float)this.webcamera.videoRotationAngle);
	}

	public Vector3 GetRotation()
	{
		return new Vector3(0,0,GetRotationAngle());
	}


	public Vector3 getVideoScale()
	{
		return new Vector3(1,this.webcamera.videoVerticallyMirrored?-1:1,1);
	}

	public bool GetDeviceName(CameraMode mode)
	{
        Debug.Log(WebCamTexture.devices.Length);
		if (mode == CameraMode.Front) {
			int i = 0;
			for (i = 0; i != WebCamTexture.devices.Length; i++)
            {
                Debug.Log(WebCamTexture.devices[i].isFrontFacing + " " + WebCamTexture.devices[i].name);
                if (i!= 0 && WebCamTexture.devices [i].isFrontFacing) {
					this.deviceName = WebCamTexture.devices [i].name;
					break;
				}
			}
			if (i == WebCamTexture.devices.Length) {
				return false;
			}
		} else {
			
			if ( WebCamTexture.devices.Length < 1) {
				return false;
			}
			this.deviceName = WebCamTexture.devices [0].name;
		
		}

		return true;
	}


}
