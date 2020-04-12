
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: Tip.cs
	* 简要描述: 
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
	
	public class Tip : MonoBehaviour
	{
	    public Text TipText;
	    public Button OneBtn;
	    public Button TwoEnsure;
	    public Button TwoCancel;
	    private void Start()
	    {
	        OneBtn.onClick.AddListener(()=>
	        {
	            OneBtn.gameObject.SetActive(false);
	            gameObject.SetActive(false);
	        });
	        TwoEnsure.onClick.AddListener(() =>
	        {
	            TwoEnsure.gameObject.SetActive(false);
	            TwoCancel.gameObject.SetActive(false);
	            gameObject.SetActive(false);
	        });
	        TwoCancel.onClick.AddListener(() =>
	        {
	            TwoEnsure.gameObject.SetActive(false);
	            TwoCancel.gameObject.SetActive(false);
	            gameObject.SetActive(false);
	        });
	    }
	    public void ShowTip(string TipStr, TipType tipType,UnityAction OneBtnEvent = null, 
	        UnityAction TwoEnsureEvent = null, UnityAction TwoCancelEvent = null)
	    {
	        gameObject.SetActive(true);
	        TipText.text = TipStr;
	        if(tipType == TipType.OneBnt)
	        {
	            OneBtn.gameObject.SetActive(true);
	            if(OneBtnEvent != null)
	            {
	                OneBtn.onClick.AddListener(OneBtnEvent);
	            }
	        }
	        else if (tipType == TipType.TwoBtn)
	        {
	            TwoEnsure.gameObject.SetActive(true);
	            TwoCancel.gameObject.SetActive(true);
	            if (TwoEnsureEvent != null)
	            {
	                TwoEnsure.onClick.AddListener(TwoEnsureEvent);
	            }
	            if (TwoCancelEvent != null)
	            {
	                TwoCancel.onClick.AddListener(TwoCancelEvent);
	            }
	        }
	    }
	}
	public enum TipType
	{
	    NoBtn,
	    OneBnt,
	    TwoBtn
	}
}
