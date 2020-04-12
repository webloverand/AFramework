namespace AFramework
{


#if UNITY_EDITOR
    using System;
    using System.IO;
    using UnityEditor;
    using Object = UnityEngine.Object;
/// <summary>
/// 编辑器获取资源
/// </summary>
public static class AssetDatabasex
{
	public enum SearchFilter
	{
		All,
		Assets,
		Packages
	}
	/// <summary>
	/// 如果获取的是.asset资源,那么其对应的脚本如果包含编辑器代码,要注意用宏替换,同时assetname一定是资源的全名称,否则获取不到
	/// </summary>
	/// <param name="assetname"></param>
	/// <param name="searchAssets"></param>
	/// <param name="error"></param>
	/// <param name="success"></param>
	/// <returns></returns>
	public static string GetAssetPathStr(string assetname = "", SearchFilter searchAssets = SearchFilter.All, Action error = null, Action success = null)
	{
		string[] assetGUIDs = AssetDatabase.FindAssets($"t:{assetname}", GetSearchDirectories(searchAssets));

		foreach (var assetGUID in assetGUIDs)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
			if (string.IsNullOrEmpty(assetPath) || assetPath.EndsWith(".cs", StringComparison.Ordinal) || assetname == "" || !Path.GetFileName(assetPath).Contains(assetname))
			{
				continue;
			}
			return assetPath;
		}
		return "";
	}
	public static T LoadAssetOfType<T>(string contains = null, SearchFilter searchAssets = SearchFilter.All, Action error = null, Action success = null) where T : Object
	{
		//判断是否是脚本
		bool allowScriptAssets = typeof(T) == typeof(MonoScript);

		T t = null;
		string[] assetGUIDs = AssetDatabase.FindAssets($"t:{typeof(T).Name}", GetSearchDirectories(searchAssets));
		foreach (var assetGUID in assetGUIDs)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
			if (string.IsNullOrEmpty(assetPath) || !allowScriptAssets && assetPath.EndsWith(".cs", StringComparison.Ordinal) || contains != null && !Path.GetFileName(assetPath).Contains(contains))
				continue;
			t = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			break;
		}

		if (t == null)
			error?.Invoke();
		else
			success?.Invoke();

		return t;
	}

	private static string[] GetSearchDirectories(SearchFilter searchAssets)
	{
		string[] searchDirs;
		switch (searchAssets)
		{
			case SearchFilter.All:
				searchDirs = new[] { "Assets", "Packages" };
				break;
			case SearchFilter.Assets:
				searchDirs = new[] { "Assets" };
				break;
			case SearchFilter.Packages:
				searchDirs = new[] { "Packages" };
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(searchAssets), searchAssets, null);
		}
		return searchDirs;
	}
}
#endif
}
