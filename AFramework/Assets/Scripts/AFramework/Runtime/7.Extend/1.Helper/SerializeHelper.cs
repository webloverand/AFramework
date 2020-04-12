using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace AFramework
{
    public static class SerializeHelper
    {
        /// <summary>
        /// Dictionary以json存入文件
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="name"></param>
        /// <param name="saveContent"></param>
        /// <param name="FilePath"></param>
        public static void SaveDict<T1, T2>(string name, Dictionary<T1, T2> saveContent, string FilePath)
        {
            saveContent.SaveJson(FilePath + name);
        }
        /// <summary>
        /// 读取文件为Dictionary
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="name"></param>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static Dictionary<T1, T2> LoadDict<T1, T2>(string name, string FilePath)
        {
            return LoadJson<Dictionary<T1, T2>>(FilePath + name);
        }
        /// <summary>
        /// 序列化为二进制
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool SerializeBinary(string path, object obj)
        {


            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("SerializeBinary Without Valid Path.");
                return false;
            }

            if (obj == null)
            {
                Debug.Log("SerializeBinary obj is Null.");
                return false;
            }

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf =
                    new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(fs, obj);
                return true;
            }
        }
        /// <summary>
        /// 二进制解析
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static object DeserializeBinary(Stream stream)
        {
            if (stream == null)
            {
                Debug.Log("DeserializeBinary Failed!");
                return null;
            }

            using (stream)
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf =
                    new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var data = bf.Deserialize(stream);

                // TODO:这里没风险嘛?
                return data;
            }
        }
        /// <summary>
        /// 二进制解析
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static object DeserializeBinary(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("DeserializeBinary Without Valid Path.");
                return null;
            }

            FileInfo fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
            {
                Debug.Log("DeserializeBinary File Not Exit.");
                return null;
            }

            using (FileStream fs = fileInfo.OpenRead())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf =
                    new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                object data = bf.Deserialize(fs);

                if (data != null)
                {
                    return data;
                }
            }

            Debug.Log("DeserializeBinary Failed:" + path);
            return null;
        }
        /// <summary>
        /// XML序列化
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool SerializeXML(string path, object obj)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("SerializeBinary Without Valid Path.");
                return false;
            }

            if (obj == null)
            {
                Debug.Log("SerializeBinary obj is Null.");
                return false;
            }

            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                var xmlserializer = new XmlSerializer(obj.GetType());
                xmlserializer.Serialize(fs, obj);
                return true;
            }
        }
        /// <summary>
        /// XML反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static object DeserializeXML<T>(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("DeserializeBinary Without Valid Path.");
                return null;
            }
            FileInfo fileInfo = new FileInfo(path);

            using (FileStream fs = fileInfo.OpenRead())
            {
                XmlSerializer xmlserializer = new XmlSerializer(typeof(T));
                object data = xmlserializer.Deserialize(fs);

                if (data != null)
                {
                    return data;
                }
            }

            Debug.Log("DeserializeBinary Failed:" + path);
            return null;
        }
        /// <summary>
        /// 序列化json字符串后转成UTF-8 byte数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToByteArray<T>(this T obj) where T : class
        {
            return System.Text.Encoding.UTF8.GetBytes(ToJson(obj));
        }
        public static string ToStringContent(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public static string ToJson<T>(this T obj) where T : class
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static T FromJson<T>(this string json) where T : class
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string SaveJson<T>(this T obj, string path) where T : class
        {
            var jsonContent = obj.ToJson();
            File.WriteAllText(path, jsonContent);
            return jsonContent;
        }

        public static T LoadJson<T>(string path) where T : class
        {
            return File.ReadAllText(path).FromJson<T>();
        }

        public static byte[] ToProtoBuff<T>(this T obj) where T : class
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize<T>(ms, obj);
                return ms.ToArray();
            }
        }

        public static T FromProtoBuff<T>(this byte[] bytes) where T : class
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new System.ArgumentNullException("bytes");
            }
            T t = ProtoBuf.Serializer.Deserialize<T>(new MemoryStream(bytes));
            return t;
        }

        public static void SaveProtoBuff<T>(this T obj, string path) where T : class
        {
            File.WriteAllBytes(path, obj.ToProtoBuff<T>());
        }

        public static T LoadProtoBuff<T>(string path) where T : class
        {
            return File.ReadAllBytes(path).FromProtoBuff<T>();
        }

        #region 5.0 生成随机字符串 + static string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        ///<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
        ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            System.Random r = new System.Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }
        #endregion
        /// <summary>
        /// Enum转字符串
        /// </summary>
        public static string EnumToString<T>(T e)
        {
            return e.ToString();
        }
        public static T StringToEnum<T>(string s)
        {
            return (T)Enum.Parse(typeof(T), s);
        }
    }
}
