using System.Collections.Generic;
namespace AFramework
{
    //MD5检测回调
    public delegate void ABMD5CallBack(ABClassDownInfo ABDowninfo,DownStatus downResult = DownStatus.Sucess, string downError = "");
    public class HttpABMD5 : HttpBase
    {
        public HttpABMD5(ABClassDownInfo aBDownInfo) : base(aBDownInfo)
        {

        }
        public override void DownloadFinish()
        {
            base.DownloadFinish();
            ABClassDownInfo aBDownInfo = (ABClassDownInfo)httpInfo;
            OneABClassInfo NewABInfo = SerializeHelper.FromJson<OneABClassInfo>(FileHelper.ReadTxtToStr(m_saveFilePath));
            List<string> downStr = new List<string>();
            bool isLocalABComplete = true;
            bool isNeedHot = false;
            if (aBDownInfo.oldClassInfo != null)
            {
                List<string> keys = new List<string>(NewABInfo.FileMD5.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    if (aBDownInfo.oldClassInfo.FileMD5.ContainsKey(keys[i]))
                    {
                        if (NewABInfo.FileMD5[keys[i]].Equals(aBDownInfo.oldClassInfo.FileMD5[keys[i]])) //  MD5相等 
                        {
                            if (!FileHelper.JudgeFilePathExit(PathTool.PersistentDataPath + keys[i])) //本地没有AB存在
                            {
                                AFLogger.d("检测MD5:本地没有AB存在111");
                                isLocalABComplete = false; //本地AB包不完整
                                isNeedHot = true;
                                downStr.Add(keys[i]);
                            }
                        }
                        else
                        {
                            isNeedHot = true;
                            downStr.Add(keys[i]);
                            if (!FileHelper.JudgeFilePathExit(PathTool.PersistentDataPath + "/" + keys[i])) //本地没有AB存在
                            {
                                AFLogger.d("检测MD5:本地没有AB存在222");
                                isLocalABComplete = false; //本地AB包不完整
                            }
                        }
                    }
                    else
                    {
                        isLocalABComplete = false; //本地AB包不完整
                        isNeedHot = true;
                        downStr.Add(keys[i]);
                    }
                }
            }
            else
            {
                isLocalABComplete = false;
                isNeedHot = true;
                List<string> keys = new List<string>(NewABInfo.FileMD5.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    FileHelper.DeleteFile(PathTool.PersistentDataPath  + "/" + keys[i]);
                    downStr.Add(keys[i]);
                }
            }
            if (aBDownInfo.ABMD5Callback != null)
            {
                ABState aBState = ABState.Newest;
                if (isNeedHot)
                {
                    if (isLocalABComplete)
                        aBState = ABState.UpdateAndLocalComplete;
                    else
                        aBState = ABState.UpdateAndLocalNotComplete;
                }
                aBDownInfo.CheckMD5Call(NewABInfo, aBState,downStr);
                aBDownInfo.ABMD5CallbackLocal(aBDownInfo,downResult, downError);
            }
        }
    }
    public enum ABState
    {
        UpdateAndLocalComplete, //AB包需要更新并且本地旧的AB包完整
        UpdateAndLocalNotComplete, //AB包需要更新并且本地旧的AB包并不完整
        Newest   //是最新的,无需更新
    }
}
