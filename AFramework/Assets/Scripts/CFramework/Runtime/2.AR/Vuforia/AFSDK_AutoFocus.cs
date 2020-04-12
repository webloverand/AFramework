namespace AFramework
{
#if AF_ARSDK_Vuforia 
    using UnityEngine;
    public class AFSDK_AutoFocus : MonoBehaviour
    {
        void Start()
        {
            Vuforia.CameraDevice.Instance.SetFocusMode(Vuforia.CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }

        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0))
#elif UNITY_ANDROID || UNITY_IPHONE
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)  
#endif
            {
                Vuforia.CameraDevice.Instance.SetFocusMode(Vuforia.CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
            }
        }
    }
#endif
}
