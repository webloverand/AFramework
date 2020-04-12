
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: AFPlaneExample.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using AFramework;
using UnityEngine;
#if AF_ARSDK_AFoudation
using UnityEngine.XR.ARFoundation;
	public class AFPlaneExample : MonoBehaviour
	{
	    public GameObject ARSessionOrign;
	    public GameObject PlaceObj;
	    public GameObject MainCamera;
	    private void Start()
	    {
	        AFSDK_AFDManager.CheckSupport(this,isSupport);
	    }
	    public void isSupport(bool result)
	    {
	        if(result)
	        {
	            transform.GetChild(0).GetComponent<ARSession>().enabled = true;
	            GameObject ARSessionObj = Instantiate(ARSessionOrign);
	            ARSessionObj.GetComponent<AFSDK_AFDPlaneHandle>().Init(PlaceObj, PlacedCall);
	        }
	        else
	        {
	            Transform mainCamera = Instantiate(MainCamera).transform;
	            Instantiate(PlaceObj, mainCamera).transform.localPosition = new Vector3(0,0,3);
	        }
	    }
	    public void PlacedCall(GameObject spawnObj)
	    {
	
	    }
	}
#endif
}
