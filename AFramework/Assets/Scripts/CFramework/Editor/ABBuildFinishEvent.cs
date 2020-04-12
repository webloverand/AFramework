
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AFramework
{
    public class ABBuildFinishEvent
    {
        /// <summary>
        /// AB包打包完成后执行的函数,这里是将AB包移到指定的路径并生成对应的info文件
        /// </summary>
        /// <param name="mTargetPath">AB包目标路径,可以在ABCommonPath脚本中设置</param>
        /// <param name="m_BunleTargetPath">AB包初步路径</param>
        /// <param name="abConfig"></param>
        /// <param name="mInfoPath">info信息文件路径</param>
        /// <param name="AppInfoConfig"></param>
        public static void BundleFinish(string mTargetPath, string m_BunleTargetPath, AF_ABConfig abConfig, string mInfoPath, AppInfoConfig AppInfoConfig)
        {
            //一个类别对应的info
            Dictionary<string, OneABClassInfo> ClassToInfo = new Dictionary<string, OneABClassInfo>();
            Dictionary<string, bool> ClassToAppType = new Dictionary<string, bool>();
            Dictionary<string, string> ABNameToPath = new Dictionary<string, string>();
            for (int i = 0; i < abConfig.m_AllClass.Count; i++)
            {
                string classStr = abConfig.m_AllClass[i].ABClassType;
                ClassToAppType.Add(classStr,abConfig.m_AllClass[i].isSameAppType(abConfig.CurrentAppType));
                OneABClassInfo oneClassInfo = new OneABClassInfo();
                oneClassInfo.RecogType = abConfig.m_AllClass[i].ARrecogType;
                switch(oneClassInfo.RecogType)
                {
                    case ARRecogType.DataSet:
                        List<OneDataSetInfo> dataSetInfos = new List<OneDataSetInfo>();
                        foreach (OneDataSet oneDataSet in abConfig.m_AllClass[i].dataSetList)
                        {
                            OneDataSetInfo oneDataSetInfo = new OneDataSetInfo();
                            oneDataSetInfo.TargetDataSet = oneDataSet.TargetDataSetName;
                            //记录识别图资料
                            oneDataSetInfo.TargetInfo = new Dictionary<string, OneTargetInfo>();
                            char[] t = abConfig.defaultDataSetPath.ToCharArray();
                            string dataSetPath = abConfig.defaultDataSetPath;
                            if (t[t.Length - 1] != '/')
                            {
                                dataSetPath += "/";
                            }
                            string targetDataSetName = oneDataSet.TargetDataSetName;
                            string xmlMD5 = "";
                            string datMD5 = "";
                            if (abConfig.packageABType == PackageABType.StreamingAssetAB)
                            {
                                string vuforiaPath = Application.streamingAssetsPath + "/Vuforia/";
                                Directory.CreateDirectory(vuforiaPath);
                                //拷贝识别资源
                                if (!FileHelper.CopyFile(dataSetPath +
                                                        targetDataSetName
                                     + ".xml", vuforiaPath + targetDataSetName + ".xml"))
                                {
                                    break;
                                }
                                xmlMD5 = FileHelper.getFileHash(vuforiaPath + targetDataSetName + ".dat");
                                if (!FileHelper.CopyFile(dataSetPath +
                                                       targetDataSetName
                                    + ".dat", vuforiaPath + targetDataSetName + ".dat"))
                                {
                                    break;
                                }
                                datMD5 = FileHelper.getFileHash(vuforiaPath + targetDataSetName + ".xml");

                                if (!oneClassInfo.ABName.ContainsKey(targetDataSetName + ".xml") &&
                                    !oneClassInfo.ABName.ContainsKey(targetDataSetName + ".dat"))
                                {
                                    //将识别资源添加到ABName中,因为也是要下载的,同时记录其大小
                                    oneClassInfo.ABName.Add(targetDataSetName + ".xml",
                                        FileHelper.GetFileSize(vuforiaPath + targetDataSetName + ".xml"));
                                    oneClassInfo.ABName.Add(targetDataSetName + ".dat",
                                        FileHelper.GetFileSize(vuforiaPath + targetDataSetName + ".dat"));
                                }
                            }
                            else
                            {
                                //拷贝识别资源
                                if (!FileHelper.CopyFile(dataSetPath +
                                                        targetDataSetName
                                     + ".xml", mTargetPath + targetDataSetName + ".xml"))
                                {
                                    break;
                                }
                                xmlMD5 = FileHelper.getFileHash(dataSetPath +
                                                                       targetDataSetName
                                     + ".xml");

                                if (!FileHelper.CopyFile(dataSetPath
                                     + targetDataSetName + ".dat",
                                     mTargetPath + targetDataSetName + ".dat"))
                                {
                                    break;
                                }
                                datMD5 = FileHelper.getFileHash(dataSetPath +
                                                                       targetDataSetName + ".dat");

                                if (!oneClassInfo.ABName.ContainsKey(targetDataSetName + ".xml") &&
                                    !oneClassInfo.ABName.ContainsKey(targetDataSetName + ".dat"))
                                {
                                    //将识别资源添加到ABName中,因为也是要下载的,同时记录其大小
                                    oneClassInfo.ABName.Add(targetDataSetName + ".xml",
                                        FileHelper.GetFileSize(mTargetPath + targetDataSetName + ".xml"));
                                    oneClassInfo.ABName.Add(targetDataSetName + ".dat",
                                        FileHelper.GetFileSize(mTargetPath + targetDataSetName + ".dat"));
                                }
                            }

                            //添加识别资源的MD5码， 
                            if (!oneClassInfo.FileMD5.ContainsKey(targetDataSetName + ".xml")
                                && !oneClassInfo.FileMD5.ContainsKey(targetDataSetName + ".dat"))
                            {
                                // Debug.Log("第一次添加xml MD5:" + CategoryOfOwnershipS + " " + targetDataSetName);
                                oneClassInfo.FileMD5.Add(targetDataSetName + ".xml", xmlMD5);
                                oneClassInfo.FileMD5.Add(targetDataSetName + ".dat", datMD5);
                            }
                            else
                            {
                                //是已经打包过的类别
                                // Debug.Log("重复添加xml MD5:" + CategoryOfOwnershipS + " " + targetDataSetName);
                            }
                            if (!ABNameToPath.ContainsKey(targetDataSetName + ".xml") &&
                                !ABNameToPath.ContainsKey(targetDataSetName + ".dat"))
                            {
                                ABNameToPath.Add(targetDataSetName + ".xml", classStr + "/" + targetDataSetName + ".xml");
                                ABNameToPath.Add(targetDataSetName + ".dat", classStr + "/" + targetDataSetName + ".dat");
                            }


                            for (int j = 0; j < oneDataSet.mImagetTargetInfo.Count; j++)
                            {
                                oneDataSetInfo.TargetInfo.Add(oneDataSet.mImagetTargetInfo[j].ImageTargetName, oneDataSet.mImagetTargetInfo[j]);
                            }
                            dataSetInfos.Add(oneDataSetInfo);
                        }
                        oneClassInfo.dataSetInfos = dataSetInfos;
                        break;
                    case ARRecogType.Plane:
                        oneClassInfo.ResInfoForPlane = abConfig.m_AllClass[i].ResInfoForPlane;
                        break;
                }
                oneClassInfo.isNeedPackageAB = abConfig.m_AllClass[i].isNeedPackageAB;
                ClassToInfo.Add(classStr, oneClassInfo);
            }
            List<AF_OneAB> allAB = new List<AF_OneAB>();
            foreach (AF_OneAB oneAB in abConfig.m_AllFileAB)
            {
                allAB.Add(oneAB);
            }
            foreach (AF_OneAB oneAB in abConfig.m_AllPrefabAB)
            {
                allAB.Add(oneAB);
            }
            //遍历
            for (int i = 0; i < allAB.Count; i++)
            {
                EditorUtility.DisplayProgressBar("整理AB包资源", "名字TargetDataSet：" + allAB[i].mABIdentifier, i * 1.0f / (allAB.Count + abConfig.m_AllPrefabAB.Count));
                //分别拷贝到对应的类别中
                for (int k = 0; k < allAB[i].CategoryOfOwnership.Count; k++)
                {
                    string CategoryOfOwnershipS = allAB[i].CategoryOfOwnership[k];
                    if(!ClassToInfo.ContainsKey(CategoryOfOwnershipS))
                    {
                        AFLogger.EditorErrorLog(allAB[i].mABIdentifier+ "所属类型错误,不在m_AllClass内,请检查");
                        EditorUtility.ClearProgressBar();
                        return;
                    }
                    //不是当前APP Type的类型，跳过
                    if (!ClassToAppType[CategoryOfOwnershipS])
                    {
                        continue;
                    }
                    if(!ClassToInfo[CategoryOfOwnershipS].isNeedPackageAB)
                    {
                        continue;
                    }
                    string ABIdentifier = allAB[i].mABIdentifier.ToLower();

                    //拷贝AB包
                    if (!FileHelper.CopyFile(m_BunleTargetPath + "/" + ABIdentifier,
                         mTargetPath + ABIdentifier))
                    {
                        break;
                    }
                    //添加AB包的MD5码
                    string ABMD5 = FileHelper.getFileHash(m_BunleTargetPath + "/" + ABIdentifier);
                    if (!ClassToInfo[CategoryOfOwnershipS].FileMD5.ContainsKey(ABIdentifier))
                    {
                        ClassToInfo[CategoryOfOwnershipS].FileMD5.Add(ABIdentifier, ABMD5);
                    }
                    //是已经打包过的类别
                    if (ABNameToPath.ContainsKey(ABIdentifier))
                    {
                        ABNameToPath.Add(ABIdentifier, CategoryOfOwnershipS + "/"+ ABIdentifier);
                    }
                    //保存文件的大小
                    ClassToInfo[CategoryOfOwnershipS].ABName.Add(ABIdentifier,
                        FileHelper.GetFileSize(mTargetPath + ABIdentifier));
                }
            }
            //保存info文件
            for (int i = 0; i < abConfig.m_AllClass.Count; i++)
            {
                if (abConfig.m_AllClass[i].isNeedPackageAB)
                {
                    SerializeHelper.SaveJson<OneABClassInfo>(ClassToInfo[abConfig.m_AllClass[i].ABClassType.ToString()],
                        mInfoPath + abConfig.m_AllClass[i].ABClassType + "Info.txt");
                }
            }
            if(abConfig.configWritingMode == ConfigWritingMode.Binary)
            {
                FileHelper.DeleteFile(PathTool.ProjectPath + "Assets/AssetbundleConfig.bytes");
                FileHelper.CopyFile(m_BunleTargetPath + "assetbundleconfig", mTargetPath + "AssetbundleConfig");
            }
            AssetDatabase.Refresh();
            Debug.Log("资源拷贝完毕,保存路径为 : " + mTargetPath);
            EditorUtility.RevealInFinder(mTargetPath);
        }
    }
}
#endif
