/*******************************************************************
* Copyright(c)
* 文件名称: NativeExample.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
	using UnityEngine;
    using UnityEngine.UI;
    
    public class NativeExample : MonoBehaviour
    {
        public Tip tip;
        public Text RegionText;
        public Text ManufacturerText;
        public Text UUIDText;
        public Text MaxVolumnText;
        public Text CurrentVolumnText;
        private void Start()
        {
            NativeInterface.Instance.RegisterVolumnChaged(CurrentVolumnCallBack);
        }
        public void GetRegion()
        {
            NativeInterface.Instance.GetDeviceRegion(GetRegionCallBack);
        }
        public void GetManufacturer()
        {
            NativeInterface.Instance.DeviceManufacturer(getResultEvent:GetManufacturer);
        }
        public void GetUUID()
        {
            NativeInterface.Instance.GetStoragePermission(StoragePermissionCallBack);
        }
        public void StartMusic()
        {
            NativeInterface.Instance.StartMusic();
        }
        public void StopMusic()
        {
            NativeInterface.Instance.StopMusic();
        }
        public void GetMaxVolumn()
        {
            NativeInterface.Instance.GetMaxVolumn(MaxVolumnCallBack);
        }
        public void GetCurrentVolumn()
        {
            NativeInterface.Instance.GetCurrentVolumn(CurrentVolumnCallBack);
        }

       
        void GetRegionCallBack(string s)
        {
            RegionText.text = "Region : " + s;
        }
        void GetManufacturer(string s)
        {
            ManufacturerText.text = "Manufacturer : " + s;
        }
        void UUIDCallBack(string s)
        {
            UUIDText.text = "UUID : " + s;
        }
        void MaxVolumnCallBack(string s)
        {
            MaxVolumnText.text = "Max Volumn : " + s;
        }
        void CurrentVolumnCallBack(string s)
        {
            CurrentVolumnText.text = "Current Volumn : " + s;
        }
        private void OnApplicationQuit()
        {
            NativeInterface.Instance.UnRegisterVolumnChaged();
        }

        void StoragePermissionCallBack(string isHas)
        {
            if (!isHas.Equals("0"))
            {
                NativeInterface.Instance.GetUUID(UUIDCallBack);
            }
            else
            {
                tip.ShowTip("没有相册权限,将前往设置面板!", TipType.OneBnt, ToAPPSetting);
            }
        }
        void ToAPPSetting()
        {
            NativeInterface.Instance.gotoPermissionSettings();
        }
    }
}
