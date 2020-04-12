using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

public class CreateCodeManager
{
    public enum GeoMode
    {
        GEO,
        GoogleMaps
    }

    public enum WIFIMode
    {
        WEP,
        WPA,
        nopass
    }

    public enum MMS_Mode
    {
        MMS,
        MMSTO
    }

    public enum SMS_Mode
    {
        SMS,
        SMSTO,
        SMS_iOS
    }

    /// <summary>
    /// create the location position
    /// </summary>
    public class Geolocation
    {
        private readonly string latitude;

        private readonly string longitude;

        private readonly GeoMode encoding;

        public Geolocation(string latitude, string longitude, GeoMode encoding = GeoMode.GEO)
        {
            this.latitude = latitude.Replace(",", ".");
            this.longitude = longitude.Replace(",", ".");
            this.encoding = encoding;
        }

        public override string ToString()
        {
            GeoMode geolocation_Encoding = this.encoding;
            if (geolocation_Encoding == GeoMode.GEO)
            {
                return "Geo :" + this.latitude + "," + this.longitude;
            }
            if (geolocation_Encoding != GeoMode.GoogleMaps)
            {
                return "Geo :";
            }
            return "http://maps.google.com/maps?q=" + this.latitude + "," + this.longitude;
        }
    }

    public class WiFi
    {
        private readonly string ssid;

        private readonly string password;

        private readonly string authenticationMode;
        
        public WiFi(string ssid, string password, WIFIMode authenticationMode, bool isHiddenSSID = false)
        {
            this.ssid = ssid;
            this.password = password;
            this.authenticationMode = authenticationMode.ToString();
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                    "WIFI:T:",
                    this.authenticationMode,
                    ";S:",
                    this.ssid,
                    ";P:",
                    this.password,
                    ";"
            });
        }
    }

    public class BusinessCard
    {
        private readonly string mName;

        private readonly string mTEL;

        private readonly string mEmail;

        private readonly string mUrl;

        private readonly string mCompany;

        private readonly string mAddress;
        
        public BusinessCard(string n, string tel, string em,string url,string companyName,string address)
        {
            this.mName = n;
            this.mTEL = tel;
			if(IsValidIEmail(em))
			{
				this.mEmail = em;
			}
            this.mUrl = url;
            this.mCompany = companyName;
            this.mAddress = address;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                    "Name : ",
                    this.mName,
                    "\nTEL : ",
                    this.mTEL,
                    "\nEmail : ",
                    this.mEmail,
                    "\nURL : ",
                    this.mUrl,
                    "\nCompany : ",
                    this.mCompany,
                    "\nAddress : ",
                    this.mAddress,
                    ";"
            });
        }
    }
    
    public class SMS
    {
        private readonly string number;

        private readonly string subject;

        private readonly SMS_Mode encoding;

        public SMS(string number, SMS_Mode encoding = SMS_Mode.SMS)
        {
            this.number = number;
            this.subject = string.Empty;
            this.encoding = encoding;
        }

        public SMS(string number, string subject, SMS_Mode encoding = SMS_Mode.SMS)
        {
            this.number = number;
            this.subject = subject;
            this.encoding = encoding;
        }

        public override string ToString()
        {
            switch (this.encoding)
            {
                case SMS_Mode.SMS:
                    return "sms:" + this.number + "?body=" + Uri.EscapeDataString(this.subject);
                case SMS_Mode.SMSTO:
                    return "SMSTO:" + this.number + ":" + this.subject;
                case SMS_Mode.SMS_iOS:
                    return "sms:" + this.number + ";body=" + Uri.EscapeDataString(this.subject);
                default:
                    return "sms:";
            }
        }
    }

    public class MMS
    {
        private readonly string number;

        private readonly string subject;

        private readonly MMS_Mode encoding;

        public MMS(string number, MMS_Mode encoding = MMS_Mode.MMS)
        {
            this.number = number;
            this.subject = string.Empty;
            this.encoding = encoding;
        }

        public MMS(string number, string subject, MMS_Mode encoding = MMS_Mode.MMS)
        {
            this.number = number;
            this.subject = subject;
            this.encoding = encoding;
        }

        public override string ToString()
        {
            MMS_Mode mMS_Encoding = this.encoding;
            if (mMS_Encoding == MMS_Mode.MMS)
            {
                return "mms:" + this.number + "?body=" + Uri.EscapeDataString(this.subject);
            }
            if (mMS_Encoding != MMS_Mode.MMSTO)
            {
                return "mms:";
            }
            return "mmsto:" + this.number + "?subject=" + Uri.EscapeDataString(this.subject);
        }
    }
    
    public class TEL
    {
        private readonly string mTelnumber;

        public TEL(string number)
        {
            this.mTelnumber = number;
        }

        public override string ToString()
        {
            return "tel:" + this.mTelnumber + string.Empty;
        }
    }

    public class SkypeCall
    {
        private readonly string skypeUsername;

        public SkypeCall(string skypeUsername)
        {
            this.skypeUsername = skypeUsername;
        }

        public override string ToString()
        {
            return "skype:" + this.skypeUsername + "?call";
        }
    }

    public class Url
    {
        private readonly string url;

        public Url(string url)
        {
            this.url = url;
        }

        public override string ToString()
        {
            return this.url.StartsWith("http") ? this.url : ("http://" + this.url);
        }
    }


   


    public class Mail
    {
        private readonly string mEmail = "";

        public Mail(string email)
        {
			if(IsValidIEmail(email))
			{
				this.mEmail = email;	
			}
        }

        public override string ToString()
        {
            return "Email:" + this.mEmail + string.Empty;
        }
    }
	private static bool IsValidIEmail(string imail)
	{
		Regex r = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");

		if (r.IsMatch(imail))
		{
			return true;
		}
		else
		{
			return false;
		}


	}




}
