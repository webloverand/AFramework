using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class PreviewControllerFullScene : MonoBehaviour {

	public DeviceCamera devicecamera
	{
		get
		{
			return mCamera;
		}
	}
	//public RawImage rawimg;
	DeviceCamera mCamera;
    private void Awake()
    {

    }
    // Use this for initialization
    IEnumerator Start()
	{
		yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

		if (Application.HasUserAuthorization(UserAuthorization.WebCam))
		{
			mCamera = new DeviceCamera ();
            StartWork();
        }
    }

    public GameObject e_CameraPlaneObj;
    public void StartWork()
	{
		if (mCamera != null) {
			mCamera.Play ();
            e_CameraPlaneObj.GetComponent<Renderer>().material.mainTexture = mCamera.webcamera;
		}
	}

  
	/// <summary>
	/// Stops the work.
	/// </summary>
	public void StopWork()
	{
		if (mCamera != null) {
			mCamera.Stop ();
            e_CameraPlaneObj.GetComponent<Renderer>().material.mainTexture = null;
        }

	}

	/// <summary>
	/// open the rear camera.
	/// </summary>
	/// <param name="val">If set to <c>true</c> value.</param>
	public void rearCamera(bool val = false)
	{
		if (val && mCamera != null) {
			mCamera.ActiveRearCamera ();
            e_CameraPlaneObj.GetComponent<Renderer>().material.mainTexture = mCamera.webcamera;
        }
	}
	/// <summary>
	/// open the front camera.
	/// </summary>
	/// <param name="val">If set to <c>true</c> value.</param>
	public void frontCamera(bool val = false)
	{
		if (val && mCamera != null)
        {
            mCamera.ActiveFrontCamera ();
            e_CameraPlaneObj.GetComponent<Renderer>().material.mainTexture = mCamera.webcamera;
        }
    }
    bool isCorrected = false;
    float screenVideoRatio = 1.0f;
    public void Update()
    {
        if (mCamera != null && mCamera.webcamera != null && mCamera.webcamera.isPlaying)
        {
            if (mCamera.webcamera.width > 200 && !isCorrected)
            {
                correctScreenRatio();
            }
        }
    }
    void correctScreenRatio()
    {
        int videoWidth = 640;
        int videoHeight = 480;
        int ScreenWidth = 640;
        int ScreenHeight = 480;

        float videoRatio = 1;
        float screenRatio = 1;

        if (this.mCamera.webcamera != null)
        {
            videoWidth = mCamera.webcamera.width;
            videoHeight = mCamera.webcamera.height;
        }
        videoRatio = videoWidth * 1.0f / videoHeight;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        ScreenWidth = Mathf.Max (Screen.width, Screen.height);
		ScreenHeight = Mathf.Min (Screen.width, Screen.height);
#else
        ScreenWidth = Screen.width;
        ScreenHeight = Screen.height;
#endif
        screenRatio = ScreenWidth * 1.0f / ScreenHeight;

        screenVideoRatio = screenRatio / videoRatio;
        isCorrected = true;

        if (e_CameraPlaneObj != null)
        {
            e_CameraPlaneObj.GetComponent<CameraPlaneController>().correctPlaneScale(screenVideoRatio);
        }
    }
}
