using System;
using System.Collections;
using System.Collections.Generic;
using AFramework;
using UnityEngine;
using ZXing;

public class CoderReaderFullScene : MonoBehaviour
{
    float intervalTime = 1f;    //set the interval time for wait next scan
    float tempTime = 1;

    bool isWorking = true;

    BarcodeReader barReader;

    public delegate void QRScanFinished(string str);            //declare a delegate to deal with the QRcode decode complete
    public static event QRScanFinished OnCodeFinished;          //declare a event with the delegate to trigger the complete event

    public PreviewControllerFullScene previewctr;// the preview controller
    int frameCount = 0;
    Result data = null;
    bool isReadyForRead = true;
    Color32[] dataColor;

    int scanWidth = 0;
    int scanHeight = 0;
    // Use this for initialization
    void Start()
    {
        barReader = new BarcodeReader();
        barReader.TryInverted = true;
        barReader.AutoRotate = true;
        if (previewctr == null)
        {
            previewctr = GameObject.FindObjectOfType<PreviewControllerFullScene>();
        }
        tempTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

        if (isWorking)
        {

            //if (frameCount++ % 20 == 0) 
            {
                if (previewctr.devicecamera != null && !previewctr.devicecamera.isPlaying())
                {
                    return;
                }

                if (data != null)
                {
                    OnCodeFinished(data.Text);// get data send the message to other module
                    tempTime = Time.time;
                    data = null;
                    frameCount = 0;
                    isReadyForRead = false;
                    intervalTime = 1f;
                    return;
                }

                if (!isReadyForRead)
                {
                    intervalTime = intervalTime - Time.deltaTime;
                    if (intervalTime < 0)
                    {
                        isReadyForRead = true;
                    }
                    return;
                }

                isReadyForRead = true;

                if (frameCount++ % 20 == 0)
                {
                    DecodeQR();
                }

            }
        }

    }

    public void DecodeQR()
    {

        if (!isWorking || !isReadyForRead || previewctr.devicecamera == null || previewctr.devicecamera.Width() < 100)
        {
            return;
        }
        try
        {
            dataColor = previewctr.devicecamera.GetCenterPixels32();// get the camera pixels

            scanWidth = previewctr.devicecamera.centerBlockWidth;
            scanHeight = previewctr.devicecamera.centerBlockWidth;

            CodeDecodeThread.RunAsync(() =>
            {
                try
                {
                    data = barReader.Decode(dataColor,
                        scanWidth,
                        scanHeight);//start decode
                }
                catch (Exception e)
                {
                    AFLogger.log(e);
                }
            });
        }

        catch (Exception e)
        {
            AFLogger.log(e);
        }

    }



    /// <summary>
    /// Starts the work.
    /// </summary>
    public void StartWork()
    {
        if (previewctr != null)
        {
            isWorking = true;
        }
    }

    /// <summary>
    /// Stops the work.
    /// </summary>
    public void StopWork()
    {
        isWorking = false;
        data = null;
    }

    public void StartWorkAndPreview()
    {
        if (previewctr != null)
        {
            isWorking = true;
            previewctr.StartWork();
        }
    }
    /// <summary>
    /// Stops the work.
    /// </summary>
    public void StopWorkAndPreview()
    {
        isWorking = false;
        data = null;

        previewctr.StopWork();
    }

    /// <summary>
    /// Reads the code by static texture
    /// </summary>
    /// <returns>The code.</returns>
    /// <param name="targetTex">Target tex.</param>
    public string ReadCode(Texture2D targetTex)
    {
        try
        {
            if (barReader == null)
            {
                barReader = new BarcodeReader();
                barReader.TryInverted = true;
                barReader.AutoRotate = true;
            }
            Result data;
            data = barReader.Decode(targetTex.GetPixels32(),
                targetTex.width,
                targetTex.height);//start decode

            if (data != null) // if get the result success
            {
                return data.Text;
                //dataText = data.Text;	// use the variable to save the code result
            }
        }
        catch (Exception e)
        {

        }
        return "";
    }
}
