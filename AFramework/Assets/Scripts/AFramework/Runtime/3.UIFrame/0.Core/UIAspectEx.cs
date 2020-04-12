using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AFramework {	
	public class UIAspectEx : MonoBehaviour {
	
		// Use this for initialization
		void Awake () {
	
	    }
	    private  float standard_width = 1280f;
	    private  float standard_height = 720f;
	    /// <summary>
	    /// Canvas的画布适配
	    /// </summary>
	    public void AspectTheScreen()
	    {
	        float device_width = 0f; //当前设备宽度  
	        float device_height = 0f; //当前设备高度  
	        float adjustor = 0f; //屏幕矫正比例  
	        //获取设备宽高  
	        device_width = Screen.width;
	        device_height = Screen.height;
	        //计算宽高比例  
	        float standard_aspect = standard_width / standard_height;
	        float device_aspect = device_width / device_height;
	        //计算矫正比例  
	        if (device_aspect < standard_aspect)
	        {
	            adjustor = standard_aspect / device_aspect;
	        }
	
	        CanvasScaler canvasScalerTemp = GetComponent<CanvasScaler>();
	        if (adjustor == 0)
	        {
	            canvasScalerTemp.matchWidthOrHeight = 1;
	        }
	        else
	        {
	            canvasScalerTemp.matchWidthOrHeight = 0;
	        }
	    }
	}
}
