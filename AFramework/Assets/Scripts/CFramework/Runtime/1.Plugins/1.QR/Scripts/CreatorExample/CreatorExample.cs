using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System;

public class CreatorExample : MonoBehaviour {

	public CodeWriter codeWtr;// drag the codewriter into this
	public InputField input; // content input
	public RawImage previewImg; // code image preview
	public Text errorText;// tip:error tips
	public CodeWriter.CodeType codetype;

	string androidPaths = "";
	public Texture2D targetTex;
	// Use this for initialization
	void Start () {
		CodeWriter.onCodeEncodeFinished += GetCodeImage;
		CodeWriter.onCodeEncodeError += errorInfo;
	}

	public void SaveIamgeToGallery()
	{
		if ( targetTex != null) {
			MediaController.SaveImageToGallery (targetTex);
		}
	}

	/// <summary>
	/// Creates the code.
	/// </summary>
	public void create_Code()
	{
		if (codeWtr != null) {
			codeWtr.CreateCode (codetype,input.text);
		}
	}

	/// <summary>
	/// Sets the type of the code by dropdown list.
	/// </summary>
	/// <param name="typeId">Type identifier.</param>
	public void setCodeType(int typeId)
	{
		codetype = (CodeWriter.CodeType)(typeId);
		Debug.Log ("clicked typeid is " + typeId);
	}

	/// <summary>
	/// Gets the code image.
	/// </summary>
	/// <param name="tex">Tex.</param>
	public void GetCodeImage(Texture2D tex)
	{
        if(targetTex != null)
        {
			DestroyImmediate(targetTex,true);
        }
        targetTex = tex;
		RectTransform component = this.previewImg.GetComponent<RectTransform>();
		float y = component.sizeDelta.x * (float)tex.height / (float)tex.width;
		component.sizeDelta = new Vector2(component.sizeDelta.x, y);
		previewImg.texture = targetTex;
		errorText.text = "";
	}

	/// <summary>
	/// Errors the info.
	/// </summary>
	/// <param name="str">String.</param>
	public void errorInfo(string str)
	{
		errorText.text = str;
	}

	public void GotoReader()
	{
		Application.LoadLevel ("ReaderExample");
	}

	public void CreatePhone()
	{
		if (codeWtr != null) {
			codeWtr.CreateQRCode_PhoneNumber ("123854698",CodeWriter.CodeType.QRCode);
		}
	}

	public void CreateWifi()
	{
		if (codeWtr != null) {
			codeWtr.CreateQRCode_WiFi("Wili","12345678",CreateCodeManager.WIFIMode.WEP,false, CodeWriter.CodeType.QRCode);
		}
	}

	public void CreateSkype()
	{
		if (codeWtr != null) {
			codeWtr.CreateQRCode_SkypeCall ("Wili Skype",CodeWriter.CodeType.QRCode);
		}
	}

	public void CreateMMS()
	{
		if (codeWtr != null) {
			codeWtr.CreateQRCode_MMS("546213","MMSSubject",CreateCodeManager.MMS_Mode.MMS,CodeWriter.CodeType.QRCode);
		}
	}



	public void CreateSMS()
	{
		if (codeWtr != null) {
			codeWtr.CreateQRCode_SMS("546213","MMSSubject",CreateCodeManager.SMS_Mode.SMS,CodeWriter.CodeType.QRCode);
		}
	}

	public void CreatebussCard()
	{
		if (codeWtr != null) {
			codeWtr.CreateQRCode_BusinessCard("wili team","856452","wiliam@gmail.com","https://www.unity3d.com","Wili Team","Nanyang Polytechnic 180 Ang Mo Kio Avenue 8 Singapore",CodeWriter.CodeType.QRCode);
		}
	}

	public void CreateQRCode_Geolocation()
	{
		if (codeWtr != null) {
			codeWtr.CreateQRCode_Geolocation("73.234","33.234",CreateCodeManager.GeoMode.GEO,CodeWriter.CodeType.QRCode);
		}
	}


}
