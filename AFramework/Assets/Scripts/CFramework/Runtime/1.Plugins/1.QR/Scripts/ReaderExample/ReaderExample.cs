using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReaderExample : MonoBehaviour {
	
	public Texture2D targetTex;
	public CodeReader reader;
	string str = "";
	public Text datatext;
	public AudioClip tipclip;
	AudioSource audio;
	// Use this for initialization
	void Start () {
		CodeReader.OnCodeFinished += getDataFromReader;
		audio = Camera.main.gameObject.AddComponent<AudioSource> ();
		audio.clip = tipclip;
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void readCode()
	{
		str = reader.ReadCode (targetTex);
	}

	public void StartReader()
	{
		if (reader != null) {
			reader.StartWork ();
		}
	}

	public void StopReader()
	{
		if (reader != null) {
			reader.StopWork ();
		}
	}

	public void getDataFromReader(string dataStr)
	{
		if (datatext != null) {
			datatext.text = " "+ dataStr;
		}
		if (audio != null) {
			audio.Play ();
		}
		//Debug.Log ("data Str is " + dataStr);
	}

	public void GotoCreator()
	{
		StopReader ();
		SceneManager.LoadScene ("CreatorExample");
	}

}
