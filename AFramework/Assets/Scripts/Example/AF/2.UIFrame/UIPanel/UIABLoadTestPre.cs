
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: UIABLoadTestPre.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
using AFramework;
	
	[AFUI((int)testUIEnum.ABLoadTestPre, "Assets/ResForAB/ABMain/Prefabs/ABLoadTestPre")]
	public class UIABLoadTestPre : UIPanelParent
	{
	    protected override void InitUI(UIDataParent UIDataParent = null)
	    {
	        base.InitUI(UIDataParent);
	        AFLogger.d("UIABLoadTestPre InitUI");
	
	        UIChildData uIChildData = new UIChildData();
	
	        //初始化子物体
	        for (int i = 0; i < 3; i++)
	        {
	            //TODO : transformPath完成替换
	            uIChildData.index = i;
	            CreateSubPanel<UIChild>((int)testChildUIEnum.UIChild,
	                transform.GetChild(1).GetChild(0).GetChild(0), UIDataParent: uIChildData, this);
	        }
	    }
	    protected override void InitUIAll(UIDataParent UIDataParent = null)
	    {
	        base.InitUIAll(UIDataParent);
	        AFLogger.d("UIABLoadTestPre InitUIAll");
	    }
	    protected override void RegisterUIEvent()
	    {
	        base.RegisterUIEvent();
	        AFLogger.d("UIABLoadTestPre RegisterUIEvent");
	    }
	    public override void RefreshUIByData(UIDataParent UIDataParent = null)
	    {
	        base.RefreshUIByData(UIDataParent);
	        AFLogger.d("UIABLoadTestPre RefreshUIByData");
	    }
	}
	public enum testUIEnum
	{
	    ABLoadTestPre
	}
	public enum testChildUIEnum
	{
	    UIChild = 3000
	}
}
