/*******************************************************************
* Copyright(c)
* 文件名称: iOSInfoPlist.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor.iOS.Xcode;
    using UnityEngine;
    /// <summary>
    /// 修改XCODE项目中的info.plist文件
    /// </summary>
	public class iOSInfoPlist : MonoBehaviour
    {
        string _path;

        PlistDocument _plist;

        /// <summary>
        /// info.plist文件的文档
        /// </summary>
        /// <value>The document.</value>
        public PlistDocument document
        {
            get { return _plist; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">info.plist文件的路径</param>
        public iOSInfoPlist(string path)
        {
            _path = path;
            _plist = new PlistDocument();
            string plistStr = File.ReadAllText(path);
            _plist.ReadFromString(plistStr);
        }

        /// <summary>
        /// 保存修改的Plist文件
        /// </summary>
        public void Save()
        {
            File.WriteAllText(_path, _plist.WriteToString());
        }

        /// <summary>
        /// 更新内容到PList
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Update(string key, string value, XcodeInfoPListType infoType = XcodeInfoPListType.StringInfo, bool isAdd = true)
        {
            PlistElementDict root = _plist.root;
            if (isAdd)
            {
                switch (infoType)
                {
                    case XcodeInfoPListType.BoolInfo:
                        bool vb = bool.Parse(value);
                        if (root[key] != null)
                        {
                            root[key] = new PlistElementBoolean(vb);
                        }
                        else if (isAdd)
                        {
                            root.SetBoolean(key, vb);
                        }
                        break;
                    case XcodeInfoPListType.IntInfo:
                        int vi = int.Parse(value);
                        if (root[key] != null)
                        {
                            root[key] = new PlistElementInteger(vi);
                        }
                        else if (isAdd)
                        {
                            root.SetInteger(key, vi);
                        }
                        break;
                    case XcodeInfoPListType.StringInfo:
                        if (root[key] != null)
                        {
                            root[key] = new PlistElementString(value);
                        }
                        else
                        {
                            root.SetString(key, value);
                        }
                        break;
                }
            }
            else
            {
                root.values.Remove(key);
            }
        }

        /// <summary>
        /// 更新urlscheme白名单
        /// </summary>
        /// <param name="urlScheme">URL scheme.</param>
        public void UpdateLSApplicationQueriesScheme(string urlScheme, bool isAdd = true)
        {
            const string KEY = "LSApplicationQueriesSchemes";
            PlistElementDict root = _plist.root;
            PlistElementArray urlSchemeList = root[KEY] as PlistElementArray;

            if (null == urlSchemeList)
            {
                if (isAdd)
                {
                    urlSchemeList = root.CreateArray(KEY);
                    urlSchemeList.AddString(urlScheme);
                }
            }
            else
            {
                bool isExit = false;
                for (int i = 0; i < urlSchemeList.values.Count; i++)
                {
                    if (urlSchemeList.values[i].AsString().Equals(urlScheme))
                    {
                        isExit = true;
                        //需要移除
                        if (!isAdd)
                        {
                            urlSchemeList.values.Remove(urlSchemeList.values[i]);
                        }
                        break;
                    }
                }
                //需要添加
                if (!isExit && isAdd)
                {
                    urlSchemeList.AddString(urlScheme);
                }
            }
        }


        /// <summary>
        /// 更新URLSchemes
        /// </summary>
        /// <param name="identifier">Identifier.</param>
        /// <param name="urlScheme">URL scheme.</param>
        public void UpdateUrlScheme(string identifier, string urlScheme, bool isAdd = true)
        {
            const string KEY = "CFBundleURLTypes";
            const string IDENTIFIER_KEY = "CFBundleURLName";
            const string URLSCHEMES_KEY = "CFBundleURLSchemes";

            PlistElementDict root = _plist.root;
            PlistElementArray urlTypeList = root[KEY] as PlistElementArray;
            if (null == urlTypeList)
            {
                //需要添加,原本为空
                if (isAdd)
                {
                    urlTypeList = root.CreateArray(KEY);
                    PlistElementDict urlType = urlTypeList.AddDict();
                    urlType.SetString(IDENTIFIER_KEY, identifier);
                    urlType.CreateArray(URLSCHEMES_KEY).AddString(urlScheme);
                }
            }
            else
            {
                //需要添加 或者 不需要添加但是原本不清楚有没有
                PlistElementDict urlType = null;
                foreach (PlistElementDict item in urlTypeList.values)
                {
                    if (item[IDENTIFIER_KEY].AsString() == identifier)
                    {
                        urlType = item;
                        break;
                    }
                }
                //原本没有
                if (null == urlType)
                {
                    if (isAdd)
                    {
                        //直接创建IDENTIFIER_KEY与URLSCHEMES_KEY
                        urlType = urlTypeList.AddDict();
                        urlType.SetString(IDENTIFIER_KEY, identifier);
                        urlType.CreateArray(URLSCHEMES_KEY).AddString(urlScheme);
                    }
                }
                else
                {
                    PlistElementArray urlSchemes = urlType[URLSCHEMES_KEY] as PlistElementArray;
                    if (null == urlSchemes)
                    {
                        //有IDENTIFIER_KEY但是没有URLSCHEMES_KEY,因此直接创建URLSCHEMES_KEY
                        if (isAdd)
                        {
                            urlSchemes = urlType.CreateArray(URLSCHEMES_KEY);
                            urlSchemes.AddString(urlScheme);
                        }
                    }
                    else
                    {
                        if (isAdd)
                        {
                            //判断URLSCHEMES_KEY中有没有urlScheme,没有则添加
                            bool isExit = false;
                            for (int i = 0; i < urlSchemes.values.Count; i++)
                            {
                                if (urlSchemes.values[i].AsString().Equals(urlScheme))
                                {
                                    isExit = true;
                                    break;
                                }
                            }
                            if (!isExit)
                            {
                                urlSchemes.AddString(urlScheme);
                            }
                        }
                        else
                        {
                            //全部都有,需要删除
                            urlType.values.Remove(URLSCHEMES_KEY);
                            if (urlType.values.Count == 1 && urlType.values.ContainsKey(IDENTIFIER_KEY) &&
                                urlType.values[IDENTIFIER_KEY].AsString().Equals(identifier))
                            {
                                urlType.values.Clear();
                                urlTypeList.values.Remove(urlType);
                            }
                            if (urlTypeList.values.Count == 0)
                            {
                                root.values.Remove(KEY);
                            }
                        }
                    }
                }
            }
        }
    }
}
