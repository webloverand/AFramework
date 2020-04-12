/*******************************************************************
* Copyright(c)
* 文件名称: ObjectExtensions.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;
    public static class ObjectExtensions 
	{
        // 为对象获取一个独特的ish字符串散列代码
        public static string Hash(this object obj)
        {
            if (obj is Object)
                return obj.GetHashCode().ToString();

            return obj.GetHashCode() + obj.GetType().Name;
        }

        // 检查对象是资产还是场景对象
        public static bool IsAsset(this object obj)
        {
            return obj is Object && AssetDatabase.Contains((Object)obj);
        }

        //检查对象是否是文件夹资产
        public static bool IsFolder(this Object obj)
        {
            return obj is DefaultAsset && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj));
        }

        // 检查字段是否为serializable
        public static bool IsSerializable(this FieldInfo fieldInfo)
        {
            // see Serialization Rules: https://docs.unity3d.com/Manual/script-Serialization.html
            if (fieldInfo.IsInitOnly || ((!fieldInfo.IsPublic || fieldInfo.IsNotSerialized) &&
               !Attribute.IsDefined(fieldInfo, typeof(SerializeField))))
                return false;

            return IsTypeSerializable(fieldInfo.FieldType);
        }

        // 检查属性是否可序列化
        public static bool IsSerializable(this PropertyInfo propertyInfo)
        {
            return IsTypeSerializable(propertyInfo.PropertyType);
        }

        // 检查type是否可序列化
        private static bool IsTypeSerializable(Type type)
        {
            // see Serialization Rules: https://docs.unity3d.com/Manual/script-Serialization.html
            if (typeof(Object).IsAssignableFrom(type))
                return true;

            if (type.IsArray)
            {
                if (type.GetArrayRank() != 1)
                    return false;

                type = type.GetElementType();

                if (typeof(Object).IsAssignableFrom(type))
                    return true;
            }
            else if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() != typeof(List<>))
                    return false;

                type = type.GetGenericArguments()[0];

                if (typeof(Object).IsAssignableFrom(type))
                    return true;
            }

            if (type.IsGenericType)
                return false;

            return Attribute.IsDefined(type, typeof(SerializableAttribute), false);
        }
    }
#endif
}
