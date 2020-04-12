namespace AFramework
{
#if AF_ARSDK_Vuforia 
    using UnityEngine;
    //因为运行ARCamera的fieldofview会改变,此脚本用来保证ModelCamera与ARCamera的fieldofview保持一致
    public class AFSDK_CameraFieldFit : MonoBehaviour
    {
        private Camera ModelCamera;
        private Camera ARCamera;

        private void Start()
        {
            ModelCamera = transform.GetComponent<Camera>();
            ARCamera = transform.parent.GetComponent<Camera>();
        }
        private void Update()
        {
            ModelCamera.fieldOfView = ARCamera.fieldOfView;
        }
    }
#endif
}
