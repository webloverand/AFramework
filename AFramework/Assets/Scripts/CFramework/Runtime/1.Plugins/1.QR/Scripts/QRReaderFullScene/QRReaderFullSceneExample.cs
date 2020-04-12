using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QRReaderFullSceneExample : MonoBehaviour
{
    public CoderReaderFullScene reader;
    string str = "";
    public Text datatext;
    public AudioClip tipclip;
    AudioSource audio;
    // Use this for initialization
    void Start()
    {
        CoderReaderFullScene.OnCodeFinished += getDataFromReader;
        audio = Camera.main.gameObject.AddComponent<AudioSource>();
        audio.clip = tipclip;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartReader()
    {
        if (reader != null)
        {
            reader.StartWorkAndPreview();
        }
    }

    public void StopReader()
    {
        if (reader != null)
        {
            reader.StopWorkAndPreview();
        }
    }

    public void getDataFromReader(string dataStr)
    {
        if (datatext != null)
        {
            datatext.text = " " + dataStr;
        }
        if (audio != null)
        {
            audio.Play();
        }
        //Debug.Log ("data Str is " + dataStr);
    }

}
