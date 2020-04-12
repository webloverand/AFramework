/*******************************************************************
* Copyright(c)
* 文件名称: tipexample.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
	using UnityEngine;
    using UnityEngine.UI;

    public class tipexample : MonoBehaviour
	{
        public Button OneEnsureBtn;
        public Button TwoEnsureBtn;
        public Button TwoCancelBtn;

        public void HideSelf()
        {
            if (tipContent.EnsureEvent != null)
            {
                OneEnsureBtn.onClick.RemoveListener(tipContent.EnsureEvent);
                TwoEnsureBtn.onClick.RemoveListener(tipContent.EnsureEvent);
            }
            if (tipContent.CancelEvent != null)
                TwoCancelBtn.onClick.RemoveListener(tipContent.CancelEvent);
            gameObject.SetActive(false);
        }
        TipData tipContent;
        public void InitUIByData(UIDataParent UIDataParent)
        {
            tipContent = (TipData)UIDataParent;
            for (int i = 1; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(i == (int)tipContent.tipBtnType + 1);
            }
            transform.GetChild((int)tipContent.tipBtnType + 1).GetChild(0).GetComponent<Text>().text = tipContent.tipStr;
            if (tipContent.EnsureEvent != null)
            {
                OneEnsureBtn.onClick.AddListener(tipContent.EnsureEvent);
                TwoEnsureBtn.onClick.AddListener(tipContent.EnsureEvent);
            }
            if (tipContent.CancelEvent != null)
                TwoCancelBtn.onClick.AddListener(tipContent.CancelEvent);
            gameObject.SetActive(true);
        }
    }
}	
