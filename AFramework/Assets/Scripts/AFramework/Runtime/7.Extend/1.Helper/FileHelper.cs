using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace AFramework
{
    public class FileHelper : MonoBehaviour
    {
        #region Json与对象转换
        /// <summary>
        /// 将Dictionary数据存入文件
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
        /// 从文件中读取Dictionary信息
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="name"></param>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static Dictionary<T1, T2> LoadDict<T1, T2>(string name, string FilePath)
        {
            return SerializeHelper.LoadJson<Dictionary<T1, T2>>(FilePath + name);
        }
        #endregion

        #region File 创建与删除/复制操作
        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <returns><c>true</c>, if file path exit was judged, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        public static bool JudgeFilePathExit(string path)
        {
            return File.Exists(path);
        }
        /// <summary>
        /// 创建文件并写入数据
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="data">Data.</param>
        /// <param name="isNeedNew">当已经有文件存在时是否需要新建,默认不需要新建</param>
        public static void CreatFile(string path, byte[] data = default(byte[]), bool isNeedNew = false)
        {
            Stream sw;
            if (JudgeFilePathExit(path))
            {
                FileInfo File = new FileInfo(path);
                if (isNeedNew)  //需要新建时删除文件
                {
                    DeleteFile(path);
                    sw = File.Create();
                }
                else
                {
                    sw = File.OpenWrite();
                }
            }
            else  //文件不存在
            {
                FileInfo File = new FileInfo(path);
                sw = File.Create();
            }
            if (data != null && data.Length > 0)
            {
                sw.Write(data, 0, data.Length);
            }
            sw.Close();
            sw.Dispose();
        }
        /// <summary>
        /// 如果文件存在则删除文件
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFile(string path)
        {
            //Debug.Log("删除了:"+ path);
            if (path != null && path != "")
            {
                FileInfo File = new FileInfo(path);
                if (File.Exists)
                {
                    File.Delete();
                }
            }
        }
        /// <summary>
        /// 删除所有文件,不包括文件夹
        /// </summary>
        /// <returns><c>true</c>, if all file was deleted, <c>false</c> otherwise.</returns>
        /// <param name="fullPath">Full path.</param>
        public static bool DeleteAllFile(string fullPath)
        {
            //获取指定路径下面的所有资源文件  然后进行删除
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo direction = new DirectoryInfo(fullPath);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

                for (int i = 0; i < files.Length; i++)
                {
                    string FilePath = fullPath + "/" + files[i].Name;
                    File.Delete(FilePath);
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <returns><c>true</c>, if file was copyed, <c>false</c> otherwise.</returns>
        /// <param name="sourcePath">Source path.</param>
        /// <param name="targetPath">Target path.</param>
        public static bool CopyFile(string sourcePath, string targetPath, NeedCopyFile NeedCopy = null)
        {
            if (File.Exists(sourcePath))
            {
                if (File.Exists(targetPath))
                    File.Copy(sourcePath, targetPath, true);
                else if (NeedCopy == null || (NeedCopy != null && NeedCopy(Path.GetFileName(sourcePath))))
                    File.Copy(sourcePath, targetPath);
                return true;
            }
            else
            {
                Debug.LogError("要拷贝的文件为空:" + sourcePath);
                return false;
            }
        }
        #endregion
        #region File 读写操作
        /// <summary>
        /// 写入.TXT
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="message"></param>
        public void WriteStringToTxt(string filePath, string message)
        {
            StreamWriter writer;
            FileInfo file = new FileInfo(filePath);
            if (!file.Exists)
            {
                writer = file.CreateText();
            }
            else
            {
                writer = file.AppendText();
            }
            writer.WriteLine(message);
            writer.Flush();
            writer.Dispose();
            writer.Close();
        }
        /// <summary>
        /// 读取文件第一行内容
        /// </summary>
        /// <returns>The text only one line.</returns>
        /// <param name="filePath">File path.</param>
        public static string ReadTxtOnlyOneLine(string filePath)
        {
            string tempStr = "";
            if (JudgeFilePathExit(filePath))
            {
                StreamReader reader = new StreamReader(filePath, Encoding.UTF8);
                tempStr = reader.ReadLine();
                reader.Dispose();
                reader.Close();
            }
            return tempStr;
        }
        public static T ReadXmlToObject<T>(string filePath) where T : class
        {
            if (JudgeFilePathExit(filePath))
            {
                return SerializeHelper.DeserializeXML<T>(filePath) as T;
            }
            return default(T);
        }
        /// <summary>
        /// 将filePath路径内容(对象转的json字符串)转换成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T ReadJsonTxtToObject<T>(string filePath) where T : class
        {
            if (JudgeFilePathExit(filePath))
            {
                return SerializeHelper.FromJson<T>(ReadTxtToStr(filePath));
            }
            return default(T);
        }
        /// <summary>
        /// 读取filePath转成列表List<string>
        /// </summary>
        /// <returns>The text to list.</returns>
        /// <param name="filePath">File path.</param>
        public static List<string> ReadTxtToList(string filePath)
        {
            List<string> tempList = new List<string>();
            if (JudgeFilePathExit(filePath))
            {
                StreamReader reader = new StreamReader(filePath, Encoding.UTF8);
                string text;
                while ((text = reader.ReadLine()) != null)
                {
                    tempList.Add(text);
                }
                reader.Dispose();
                reader.Close();
            }
            return tempList;
        }
        /// <summary>
        /// 读取filePath路径为string
        /// </summary>
        /// <returns>The text to string.</returns>
        /// <param name="filePath">File path.</param>
        public static string ReadTxtToStr(string filePath)
        {
            string text = "";
            if (JudgeFilePathExit(filePath))
            {
                StreamReader reader = new StreamReader(filePath, Encoding.UTF8);
                text = reader.ReadToEnd();
                while (reader.EndOfStream)
                {
                    reader.Close();
                    reader.Dispose();
                    break;
                }
            }
            return text;
        }
        /// <summary>
        /// 读取filePath路径为byte[]
        /// </summary>
        /// <returns>The byte array.</returns>
        /// <param name="filePath">File path.</param>
        public static byte[] ReadByteArray(string filePath)
        {
            byte[] buffer = null;
            if (JudgeFilePathExit(filePath))
            {
                //通过路径加载本地图片
                FileStream fs = new FileStream(filePath, FileMode.Open);
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();
                fs.Dispose();
            }
            return buffer;
        }
        /// <summary>
        /// 读取filePath路径为Texture
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Texture2D ReadToTexture2D(string filePath, int width, int height)
        {
            Texture2D texture2D = new Texture2D(width, height);
            texture2D.LoadImage(ReadByteArray(filePath));
            return texture2D;
        }
        /// <summary>
        /// 读取filePath路径为Sprite
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Sprite ReadToSprite(string filePath, int width, int height)
        {
            Texture2D texture2D = new Texture2D(width, height);
            texture2D.LoadImage(ReadByteArray(filePath));
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            //Destroy(texture2D);
            return sprite;
        }
        /// <summary>
        ///  读取byteContent(byte[])为Sprite
        /// </summary>
        /// <param name="byteContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Sprite ReadToSprite(byte[] byteContent, int width, int height)
        {
            Texture2D texture2D = new Texture2D(width, height);
            texture2D.LoadImage(byteContent);
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            //Destroy(texture2D);
            return sprite;
        }

        #endregion
        #region 文件夹操作
        /// <summary>
        /// 如果文件夹不存在则创建
        /// </summary>
        /// <param name="filePath">File path.</param>
        public static void CreateDirectory(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
            }
        }
        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="fullPath"></param>
        public static void DeleteDir(string fullPath)
        {
            if (Directory.Exists(fullPath))
            {
                foreach (string f in Directory.GetFileSystemEntries(fullPath))
                {
                    if (File.Exists(f))
                    {
                        //如果有子文件删除文件
                        File.Delete(f);
                        Console.WriteLine(f);
                    }
                    else
                    {
                        //循环递归删除子文件夹
                        DeleteDir(f);
                    }
                }

                //删除空文件夹
                Directory.Delete(fullPath);
            }
        }
        /// <summary>
        /// 删除文件夹，如果存在(待测试,是否会删除里面的文件夹)
        /// </summary>
        public static void DeleteDirIfExists(string dirFullPath)
        {
            if (Directory.Exists(dirFullPath))
            {
                Directory.Delete(dirFullPath, true);
            }
        }

        /// <summary>
        /// 清空 Dir（保留目录),如果存在。
        /// </summary>
        public static void EmptyDirIfExists(string dirFullPath)
        {
            if (Directory.Exists(dirFullPath))
            {
                Directory.Delete(dirFullPath, true);
            }

            Directory.CreateDirectory(dirFullPath);
        }
        /// <summary>
        /// 拷贝文件夹
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        /// <param name="NeedCopy"></param>
        public static void CopyFolder(string srcPath, string dstPath, NeedCopyFile NeedCopy = null)
        {
            if (Directory.Exists(dstPath))
                Directory.Delete(dstPath);
            if (File.Exists(dstPath))
                File.Delete(dstPath);

            Directory.CreateDirectory(dstPath);

            foreach (var file in Directory.GetFiles(srcPath))
            {
                if (NeedCopy == null || (NeedCopy != null && NeedCopy(Path.GetFileName(file))))
                {
                    File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
                }
            }

            foreach (var dir in Directory.GetDirectories(srcPath))
                CopyFolder(dir, Path.Combine(dstPath, Path.GetFileName(dir)), NeedCopy);
        }
        /// <summary>
        /// 获取文件夹下所有目录
        /// </summary>
        /// <param name="dirABSPath"></param>
        /// <param name="isRecursive">是否寻找所有子文件夹下的文件</param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static List<string> GetDirSubFilePathList(string dirABSPath, bool isRecursive = true, string suffix = "")
        {
            var pathList = new List<string>();
            var di = new DirectoryInfo(dirABSPath);

            if (!di.Exists)
            {
                return pathList;
            }

            var files = di.GetFiles();
            foreach (var fi in files)
            {
                if (!string.IsNullOrEmpty(suffix))
                {
                    if (!fi.FullName.EndsWith(suffix, System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                }

                pathList.Add(fi.FullName);
            }

            if (isRecursive)
            {
                var dirs = di.GetDirectories();
                foreach (var d in dirs)
                {
                    pathList.AddRange(GetDirSubFilePathList(d.FullName, isRecursive, suffix));
                }
            }

            return pathList;
        }
        /// <summary>
        /// 获取文件下下子文件夹的名称
        /// </summary>
        /// <param name="dirABSPath"></param>
        /// <returns></returns>
        public static List<string> GetDirSubDirNameList(string dirABSPath)
        {
            var di = new DirectoryInfo(dirABSPath);

            var dirs = di.GetDirectories();

            return dirs.Select(d => d.Name).ToList();
        }
        #endregion
        #region 文件信息获取(Hash码/文件名/文件后缀/文件size)
        /// <summary>
        /// 获取文件的hash码.(MD5 编码)
        /// </summary>
        /// <returns>The file hash.</returns>
        /// <param name="filePath">File path.</param>
        public static string getFileHash(string filePath)
        {
            try
            {
                //Debug.Log (filePath);
                FileStream fs = new FileStream(filePath, FileMode.Open);
                int len = (int)fs.Length;
                byte[] data = new byte[len];
                fs.Read(data, 0, len);
                fs.Close();
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] result = md5.ComputeHash(data);
                string fileMD5 = "";
                foreach (byte b in result)
                {
                    fileMD5 += Convert.ToString(b, 16);
                }
                //Debug.Log (filePath + "的MD5 : " + fileMD5);
                return fileMD5;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }
        /// <summary>
        /// 通过路径或者URL获取文件名称(带后缀)
        /// </summary>
        /// <param name="absOrAssetsPath"></param>
        /// <returns></returns>
        public static string GetFileNameWithEx(string absOrAssetsPath)
        {
            var name = MakePathStandard(absOrAssetsPath);
            var lastIndex = name.LastIndexOf("/", StringComparison.Ordinal);
            return lastIndex >= 0 ? name.Substring(lastIndex + 1) : name;
        }
        /// <summary>
        /// 通过路径或者URL获取文件名称(不带后缀)
        /// </summary>
        /// <param name="absOrAssetsPath"></param>
        /// <returns></returns>
        public static string GetFileNameWithOutEx(string absOrAssetsPath)
        {
            var path = MakePathStandard(absOrAssetsPath);
            return GetFileNameWithEx(GetFilePathWithoutExtention(path));
        }
        /// <summary>
        /// 通过路径或者URL获取文件后缀
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetFileEx(string url)
        {
            string[] str = url.Split('.');
            return str[str.Length - 1];
        }
        /// <summary>
        /// 获取文件夹名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetDirectoryName(string fileName)
        {
            fileName = MakePathStandard(fileName);
            return fileName.Substring(0, fileName.LastIndexOf('/'));
        }
        /// <summary>
        /// 获取不带后缀的文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFilePathWithoutExtention(string fileName)
        {
            if (fileName.Contains("."))
                return fileName.Substring(0, fileName.LastIndexOf('.'));
            return fileName;
        }
        /// <summary>
        /// 获取父文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathParentFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return Path.GetDirectoryName(path);
        }
        /// <summary>
        /// 获取文件的size
        /// </summary>
        /// <param name="path">指定文件的路径</param>
        public static long GetFileSize(string path)
        {
            if (JudgeFilePathExit(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                return fileInfo.Length;
            }
            else
            {
                Debug.LogError("获取size路径不存在:" + path);
            }
            return 0;
        }
        #endregion

        /// <summary>
        /// 拷贝条件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public delegate bool NeedCopyFile(string filePath);



        /// <summary>
        /// 合并路径
        /// </summary>
        /// <param name="selfPath"></param>
        /// <param name="toCombinePath"></param>
        /// <returns> 合并后的路径 </returns>
        public static string CombinePath(string selfPath, string toCombinePath)
        {
            return Path.Combine(selfPath, toCombinePath);
        }

        /// <summary>
        /// 结合目录
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string Combine(params string[] paths)
        {
            string result = "";
            foreach (string path in paths)
            {
                result = Path.Combine(result, path);
            }

            result = MakePathStandard(result);
            return result;
        }
        /// <summary>
        /// 使路径标准化，去除空格并将所有'\'转换为'/'
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string MakePathStandard(string path)
        {
            return path.Trim().Replace("\\", "/");
        }
    }
}
