
namespace AFramework {	/*******************************************************************
	* Copyright(c)
	* 文件名称: ChangeFont.cs
	* 简要描述:
	* 作者: 千喜
	* 邮箱: 2470460089@qq.com
	******************************************************************/
	
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
	
	//想扩展,排除某些文件夹的搜索
	public class ChangeFont
	{
	    [LabelText("原字体:")]
	    public Font fromChangeFont;
	    [LabelText("目标字体:")]
	    public Font toChangeFont;
	
	    [Button]
	    void Change()
	    {
	        if (Selection.objects == null || Selection.objects.Length == 0) return;
	        Object[] labels = Selection.GetFiltered(typeof(Text), SelectionMode.Deep);
	        foreach (Object item in labels)
	        {
	            Text label = (Text)item;
	            if (label.font == fromChangeFont)
	            {
	                label.font = toChangeFont;
	                Debug.Log(item.name + ":" + label.text);
	                EditorUtility.SetDirty(item);
	            }
	        }
	        AssetDatabase.Refresh();
	    }
	}
}
