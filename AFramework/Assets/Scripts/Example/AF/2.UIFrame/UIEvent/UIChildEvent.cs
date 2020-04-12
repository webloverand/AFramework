
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: UIChildEvent.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using UnityEngine;
	
using AFramework;
using UnityEngine.UI;
	
	[AFUI((int)eventUITag.UIChildEvent,"Assets/ResForAB/ABMain/Prefabs/UIChildEvent")]
	public class UIChildEvent : UIPanelParent
	{
	    [AFTransformPath("Button")]
	    Button button;
	    protected override void InitUI(UIDataParent UIDataParent = null)
	    {
	
	    }
	    protected override void InitUIAll(UIDataParent UIDataParent = null)
	    {
	
	    }
	    protected override void RegisterUIEvent()
	    {
	        button.onClick.AddListener(() =>
	        {
	            SendMsg((int)eventID.changeMainColor,new MsgInfo(Color.red));
	        });
	    }
	    public override void RefreshUIByData(UIDataParent UIDataParent = null)
	    {
	
	    }
	}
}
