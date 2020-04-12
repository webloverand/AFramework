/*******************************************************************
* Copyright(c)
* 文件名称: ExcelHelper.cs
* 简要描述: 官方(https://github.com/JanKallman/EPPlus/wiki)
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using System.Collections.Generic;
    using System.IO;
    using OfficeOpenXml;
    using UnityEngine;

    public class ExcelHelper
    {
        #region 读取表格信息
        /// <summary>
        /// 获取表格的全部信息
        /// </summary>
        public static Dictionary<string, string[,]> GetExcelInfo(string filePath)
        {
            Dictionary<string, string[,]> sheetNameToCon = new Dictionary<string, string[,]>();
            if (FileHelper.JudgeFilePathExit(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                ExcelPackage excelPackage = new ExcelPackage(fileInfo);
                ExcelWorksheets excelWorksheets = excelPackage.Workbook.Worksheets;
                if (excelWorksheets.Count > 0)
                {
                    for (int i = 0; i < excelWorksheets.Count; i++)
                    {
                        sheetNameToCon.Add(excelWorksheets[i].Name, GetSheetInfo(excelWorksheets[i]));
                    }
                }
                else
                {
                    AFLogger.e("excel文件内没有表格");
                }
            }
            return sheetNameToCon;
        }
        /// <summary>
        /// 获取表格内单个sheet信息
        /// </summary>
        public string[,] GetSheetInfo(string filePath, string sheetName)
        {
            if (FileHelper.JudgeFilePathExit(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                ExcelPackage excelPackage = new ExcelPackage(fileInfo);
                ExcelWorksheets excelWorksheets = excelPackage.Workbook.Worksheets;
                if (excelWorksheets[sheetName] != null)
                {
                    return GetSheetInfo(excelWorksheets[sheetName]);
                }
                else
                {
                    AFLogger.e("excel文件内没有" + sheetName + "表格");
                }
            }
            return null;
        }
        public static string[,] GetFirstSheetInfo(string filePath)
        {
            if (FileHelper.JudgeFilePathExit(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                ExcelPackage excelPackage = new ExcelPackage(fileInfo);
                ExcelWorksheets excelWorksheets = excelPackage.Workbook.Worksheets;
                if (excelWorksheets.Count > 0 && excelWorksheets[1] != null)
                {
                    return GetSheetInfo(excelWorksheets[1]);
                }
                else
                {
                    AFLogger.e("excel文件内没有表格");
                }
            }
            return null;
        }
        /// <summary>
        /// 获取表格内单个sheet信息
        /// </summary>
        /// <param name="excelWorksheet"></param>
        /// <returns></returns>
        static string[,] GetSheetInfo(ExcelWorksheet excelWorksheet)
        {
            int maxColumnNum = excelWorksheet.Dimension.End.Column;//最大列
            int minColumnNum = excelWorksheet.Dimension.Start.Column;//最小列
            int maxRowNum = excelWorksheet.Dimension.End.Row;//最小行
            int minRowNum = excelWorksheet.Dimension.Start.Row;//最大行

            string[,] SheetContent = new string[maxRowNum - minRowNum + 1, maxColumnNum - minColumnNum + 1];
            for (int i = minRowNum; i <= maxRowNum; i++)
            {
                for (int j = minColumnNum; j <= maxColumnNum; j++)
                {
                    SheetContent[i - 1, j - 1] = excelWorksheet.Cells[i, j].Value as string;
                }
            }
            return SheetContent;
        }
        #endregion
        #region 写入信息
        /// <summary>
        /// 如果是新建一个表格或者要添加多条数据,建议使用这种方法
        /// </summary>
        /// <param name="excelInfo"></param>
        public static void WriteExcel(ExcelInfo excelInfo)
        {
            FileInfo fileInfo = new FileInfo(excelInfo.filePath);
            ExcelPackage excelPackage = new ExcelPackage(fileInfo);
            ExcelWorksheets excelWorksheets = excelPackage.Workbook.Worksheets;
            Dictionary<string, Dictionary<Vector2, string>> info = excelInfo.excelInfoDic;
            List<string> sheetNames = new List<string>(info.Keys);
            for (int i = 0; i < sheetNames.Count; i++)
            {
                if (excelWorksheets[sheetNames[i]] == null)
                {
                    //如果没有sheet会创建
                    excelWorksheets.Add(sheetNames[i]);
                }
                WriteCellValue(excelWorksheets[sheetNames[i]], info[sheetNames[i]]);
            }
            excelPackage.Save();
        }
        /// <summary>
        /// 如果是一个表格添加多条数据建议使用此函数
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sheetName"></param>
        /// <param name="posToValue"></param>
        public static void WriteExcel(string filePath, string sheetName, Dictionary<Vector2, string> posToValue)
        {

            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage excelPackage = new ExcelPackage(fileInfo);
            ExcelWorksheets excelWorksheets = excelPackage.Workbook.Worksheets;
            if (excelWorksheets[sheetName] == null)
            {
                //如果没有sheet会创建
                excelWorksheets.Add(sheetName);
            }
            WriteCellValue(excelWorksheets[sheetName], posToValue);
            excelPackage.Save();
        }
        /// <summary>
        /// 如果是一个表格添加多条数据建议使用此函数
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="posToValue"></param>
        public static void WriteExcel(string filePath, Dictionary<Vector2, string> posToValue)
        {

            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage excelPackage = new ExcelPackage(fileInfo);
            ExcelWorksheets excelWorksheets = excelPackage.Workbook.Worksheets;
            if (excelWorksheets.Count == 0 || excelWorksheets[1] == null)
            {
                //如果没有sheet会创建
                excelWorksheets.Add("sheet1");

            }
            WriteCellValue(excelWorksheets[1], posToValue);
            excelPackage.Save();
        }
        public static void WriteExcel(string filePath, string sheetName, Vector2 pos, string value)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage excelPackage = new ExcelPackage(fileInfo);
            ExcelWorksheets excelWorksheets = excelPackage.Workbook.Worksheets;
            if (excelWorksheets[sheetName] == null)
            {
                //如果没有sheet会创建
                excelWorksheets.Add(sheetName);
            }
            WriteCellValue(excelWorksheets[sheetName], pos, value);
            excelPackage.Save();
        }
        static void WriteCellValue(ExcelWorksheet excelWorksheet, Dictionary<Vector2, string> posToValue)
        {
            foreach (var info in posToValue)
            {
                excelWorksheet.SetValue((int)info.Key.x, (int)info.Key.y, info.Value);
            }
        }
        static void WriteCellValue(ExcelWorksheet excelWorksheet, Vector2 pos, string value)
        {
            excelWorksheet.SetValue((int)pos.x, (int)pos.y, value);
        }
        #endregion
    }
    public class ExcelInfo
    {
        public string filePath;
        public Dictionary<string, Dictionary<Vector2, string>> excelInfoDic = new Dictionary<string, Dictionary<Vector2, string>>();

        public void AddExcelinfo(string sheetName, Vector2 pos, string value)
        {
            if (excelInfoDic.ContainsKey(sheetName))
            {
                excelInfoDic[sheetName][pos] = value;
            }
            else
            {
                Dictionary<Vector2, string> t = new Dictionary<Vector2, string>();
                t.Add(pos, value);
                excelInfoDic.Add(sheetName, t);
            }
        }

        public ExcelInfo(string filePath)
        {
            this.filePath = filePath;
        }
    }
}
