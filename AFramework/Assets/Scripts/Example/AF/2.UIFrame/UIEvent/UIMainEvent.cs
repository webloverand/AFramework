
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: UIMainEvent.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using UnityEngine;
using AFramework;
using UnityEngine.UI;
using System;
	
	[AFUI((int)eventUITag.UIMainEvent, "Assets/ResForAB/ABMain/Prefabs/UIMainEvent")]
	public class UIMainEvent : UIPanelParent
	{
	    [AFTransformPath("Button")]
		Button trans;
	    [AFTransformPath("Image")]
	    Image bg;
	
	    protected override void InitUI(UIDataParent UIDataParent = null)
	    {
	    }
	    protected override void InitUIAll(UIDataParent UIDataParent = null)
	    {
	    }
	    protected override void RegisterUIEvent()
	    {
	        trans.onClick.AddListener(()=>
	        {
	            UIManager.Instance.OpenUI<UIChildEvent>((int)eventUITag.UIChildEvent, null, panelLayer: PanelLayer.Load, isClear: true);
	        });
	        RegisterEvent((int)eventID.changeMainColor, ChangeColor);
	    }
	
	    private void ChangeColor(int key,MsgInfo param)
	    {
	        if (param != null && param.DataList.Count > 0)
	        {
	            Color color = (Color)param.DataList[0];
	            bg.color = color;
	        }
	    }
	
	    public override void RefreshUIByData(UIDataParent UIDataParent = null)
	    {
	
	    }
	}
	public enum eventUITag
	{
	    UIMainEvent = 1000,
	    UIChildEvent
	}
	public enum eventID
	{
	    changeMainColor
	}
}
