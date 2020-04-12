using UnityEngine;
using System.Collections.Generic;
using System; 

using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Common;


public class CodeWriter : MonoBehaviour {

	/// <summary>
	/// Code type.
	/// </summary>
	public enum CodeType
	{
		QRCode=0,
		CODE_39=1,
		CODE_128=2,
		EAN_8=3,
		EAN_13=4
	}

	private Texture2D mCodeTex;
	public delegate void QREncodeFinished(Texture2D tex);  
	public static event QREncodeFinished onCodeEncodeFinished;  

	public delegate void QREncodeError(string ErrorInfo);  
	public static event QREncodeError onCodeEncodeError;  

	BitMatrix byteMatrix;
	private int CodeWidth  =512;

	CodeType codetype = CodeType.QRCode;
	public Texture2D e_LogoTex;
	public float e_EmbedLogoRatio;

	Texture2D tempLogoTex = null;

	void Start ()
	{
		
	}

	/// <summary>
	/// Creates the code by codetype and code content
	/// </summary>
	/// <returns><c>true</c>, if code was created, <c>false</c> otherwise.</returns>
	/// <param name="type">Type.</param>
	/// <param name="content">Content.</param>
	public bool CreateCode(CodeType type, string content)
	{
		int imgWidth = CodeWidth;
		int imgHeight = CodeWidth;
		codetype = type;
		BarcodeFormat codeFormat = BarcodeFormat.QR_CODE;
		switch (type) {
		case CodeType.QRCode:
			{
				codeFormat = BarcodeFormat.QR_CODE;
			}
			break;
		case CodeType.CODE_39:
			{
				imgWidth = CodeWidth;
				imgHeight = CodeWidth / 2;
				codeFormat = BarcodeFormat.CODE_39;
			}
			break;
		case CodeType.CODE_128:
			{
				imgWidth = CodeWidth;
				imgHeight = CodeWidth / 2;
				codeFormat = BarcodeFormat.CODE_128;
			}
			break;
		case CodeType.EAN_8:
			{
				imgWidth = CodeWidth;
				imgHeight = CodeWidth / 2;
				codeFormat = BarcodeFormat.EAN_8;
			}
			break;
		case CodeType.EAN_13:
			{
				imgWidth = CodeWidth;
				imgHeight = CodeWidth / 2;
				codeFormat = BarcodeFormat.EAN_13;
			}
			break;
		}

		var writer = new MultiFormatWriter();// new the writer controller
		Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>(); 
		//set the code type
		hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
		hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);

		try
		{
			byteMatrix = writer.encode( content, codeFormat, imgWidth, imgHeight ,hints); 
		}
		catch(Exception e) {
			onCodeEncodeError (getErrorInfo (type));
			Debug.Log ( "current :" +e);
			return false;
		}

		mCodeTex = new Texture2D(byteMatrix.Width,  byteMatrix.Height);
		//set the content pixels in target image
		for (int i =0; i!= mCodeTex.width; i++) {
			for(int j = 0;j!= mCodeTex.height;j++)
			{
				if(byteMatrix[i,j])
				{
					mCodeTex.SetPixel(i,j,Color.black);
				}
				else
				{
					mCodeTex.SetPixel(i,j,Color.white);
				}
			}
		}

		Color32[] pixels = mCodeTex.GetPixels32();
		this.mCodeTex.SetPixels32(pixels);
		this.mCodeTex.Apply();
        if(e_LogoTex != null)
        {
            AddLogoToCode ();
        }

		onCodeEncodeFinished (mCodeTex);// send the message.

		return true;
	}

	/// <summary>
	/// Gets the error info.
	/// </summary>
	/// <returns>The error info.</returns>
	/// <param name="type">Type.</param>
	string getErrorInfo(CodeType type)
	{
		string error = "";
		switch (type) {
		case CodeType.QRCode:
			{
				
			}
			break;
		case CodeType.CODE_39:
			{
				error = "Code_39: Contents only contain digits !";
			}
			break;
		case CodeType.CODE_128:
			{
				error = "CODE_128: Contents length should be between 1 and 80 characters !";
			}
			break;
		case CodeType.EAN_8:
			{
				error ="EAN_8: Must contain 7 digits,the 8th digit is automatically added !";
			}
			break;
		case CodeType.EAN_13:
			{
				error = "EAN_13: Must contain 12 digits,the 13th digit is automatically added !";
			}
			break;
		}

		return error;

	}

	bool isContainDigit(string str)
	{
		for (int i = 0; i != str.Length; i++) {
			if (str [i] >= '0' && str [i] <= '9') {
				return true;
			}
		}
		return false;
	}

	bool isContainChar(string str)
	{
		for (int i = 0; i != str.Length; i++) {
			if (str [i] >= 'a' && str [i] <= 'z') {
				return true;
			}
		}
		return false;
	}

	bool bAllDigit(string str)
	{
		for (int i = 0; i != str.Length; i++) {
			if (str [i] >= '0' && str [i] <= '9') {
			} else {
				return false;
			}
		}
		return true;
	}

	public void AddLogoToCode()
	{
		if (e_LogoTex != null ) {
			e_EmbedLogoRatio = Mathf.Clamp (e_EmbedLogoRatio, 0, 0.5f);

			int maxLength = Mathf.Max (e_LogoTex.width, e_LogoTex.height);
			if (maxLength > (mCodeTex.width * e_EmbedLogoRatio)) {

				if (tempLogoTex == null) {
					tempLogoTex = new Texture2D (e_LogoTex.width, e_LogoTex.height, TextureFormat.RGBA32, true);
					tempLogoTex.SetPixels (e_LogoTex.GetPixels ());
					tempLogoTex.Apply ();
				}

				float scaleRatio = mCodeTex.width * e_EmbedLogoRatio / maxLength * 1.0f;
				int newLogoWidth = (int)(e_LogoTex.width * scaleRatio);
				int newLogoHeight = (int)(e_LogoTex.height * scaleRatio);
				tScale.Bilinear (tempLogoTex, newLogoWidth, newLogoHeight);
			} else {
				if (tempLogoTex == null) {
					tempLogoTex = new Texture2D (e_LogoTex.width, e_LogoTex.height, TextureFormat.RGBA32,true);
					//tempLogoTex.SetPixels (e_LogoTex.GetPixels());
					tempLogoTex.Apply ();
				}
			}
		}
		else
		{
			return;
		}

		int startX = (mCodeTex.width - tempLogoTex.width)/2;
		int startY =  (mCodeTex.height -  tempLogoTex.height)/2;

		for (int x = startX; x < tempLogoTex.width + startX; x++) {
			for (int y = startY; y < tempLogoTex.height + startY; y++) {
				Color bgColor = mCodeTex.GetPixel (x, y);
				Color wmColor = tempLogoTex.GetPixel (x - startX, y - startY);
				Color finalColor = Color.Lerp (bgColor, wmColor, wmColor.a / 1.0f);
				mCodeTex.SetPixel (x, y, finalColor);
			}
		}

		Destroy (tempLogoTex);
		tempLogoTex = null;

		mCodeTex.Apply ();
	}



	public void CreateQRCode_WiFi(string ssid, string password, CreateCodeManager.WIFIMode authenticationMode, bool isHiddenSSID = false, CodeType format = CodeType.QRCode)
	{
		CreateCodeManager.WiFi wiFi = new CreateCodeManager.WiFi(ssid, password, authenticationMode, isHiddenSSID);
		this.CreateCode(format,wiFi.ToString());
	}


	public void CreateQRCode_Mail(string emailAdress, CodeType format = CodeType.QRCode)
	{
		CreateCodeManager.Mail mail = new CreateCodeManager.Mail(emailAdress);
		this.CreateCode(format, mail.ToString() );
	}

	public void CreateQRCode_SMS(string number,string subject, CreateCodeManager.SMS_Mode encoding = CreateCodeManager.SMS_Mode.SMS, CodeType format = CodeType.QRCode)
	{
		CreateCodeManager.SMS sms = new CreateCodeManager.SMS(number,subject,encoding);
		this.CreateCode(format, sms.ToString());
	}

	public void CreateQRCode_MMS(string number,string subject, CreateCodeManager.MMS_Mode encoding = CreateCodeManager.MMS_Mode.MMS, CodeType format = CodeType.QRCode)
	{
		CreateCodeManager.MMS mMS = new CreateCodeManager.MMS(number,subject,encoding);
		this.CreateCode(format,mMS.ToString());
	}

	public void CreateQRCode_Geolocation(string latitude, string longitude, CreateCodeManager.GeoMode encoding = CreateCodeManager.GeoMode.GEO, CodeType format = CodeType.QRCode)
	{
		CreateCodeManager.Geolocation geolocation = new CreateCodeManager.Geolocation(latitude, longitude, encoding);
		this.CreateCode(format,geolocation.ToString());
	}

	public void CreateQRCode_PhoneNumber(string number, CodeType format = CodeType.QRCode)
	{
		CreateCodeManager.TEL phoneNumber = new CreateCodeManager.TEL(number);
		this.CreateCode(format,phoneNumber.ToString() );
	}

	public void CreateQRCode_SkypeCall(string skypeUsername, CodeType format = CodeType.QRCode)
	{
		CreateCodeManager.SkypeCall skypeCall = new CreateCodeManager.SkypeCall(skypeUsername);
		this.CreateCode(format,skypeCall.ToString());
	}

	public void CreateQRCode_Url(string url, CodeType format = CodeType.QRCode)
	{
		CreateCodeManager.Url url2 = new CreateCodeManager.Url(url);
		this.CreateCode(format, url2.ToString());
	}
	//public BusinessCard(string n, string tel, string em,string url,string companyName,string address)

	public void CreateQRCode_BusinessCard(string n, string tel, string em, string url, string companyName, string address, CodeType format = CodeType.QRCode)
	{
		CreateCodeManager.BusinessCard bcard = new CreateCodeManager.BusinessCard(n,tel,em,url,companyName,address);
		this.CreateCode(format, bcard.ToString());
	}


}
