
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: UIChild.cs
	* 简要描述: 
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using AFramework;
using UnityEngine.UI;
	
	public class UIChildData : UIDataParent
	{
	    public int index;
	}
	
	[AFUI((int)testChildUIEnum.UIChild, "Assets/ResForAB/ABMain/Prefabs/ChildUI")]
	public class UIChild : UIPanelParent
	{
	    [AFTransformPath("Text")]
	    Text indexText;
	    protected override void InitUI(UIDataParent UIDataParent = null)
	    {
	        base.InitUI(UIDataParent);
	        AFLogger.d("UIChild InitUI");
	        if (UIDataParent == null)
	            return;
	
	        UIChildData uIChildData = UIDataParent as UIChildData;
	        AFLogger.log(transform);
	        indexText.text = "子物体:" + uIChildData.index.ToString();
	    }
	    protected override void InitUIAll(UIDataParent UIDataParent = null)
	    {
	        base.InitUIAll(UIDataParent);
	        AFLogger.d("UIChild InitUIAll");
	    }
	    protected override void RegisterUIEvent()
	    {
	        base.RegisterUIEvent();
	        AFLogger.d("UIChild RegisterUIEvent");
	    }
	    public override void RefreshUIByData(UIDataParent UIDataParent = null)
	    {
	        base.RefreshUIByData(UIDataParent);
	        AFLogger.d("UIChild RefreshUIByData");
	    }
	}
}
