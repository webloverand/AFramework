/*******************************************************************
* Copyright(c)
* 文件名称: AFPathConfigWindow.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/

namespace AFramework
{
    using Sirenix.OdinInspector;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    [CreateAssetMenu(fileName = "AFPathConfig", menuName = "AFramework/CreateAFPathConfig", order = 50)]
    public class AFPathConfig:ScriptableObject
    {
        [ReadOnly]
        public string PersistentPath;
        [ReadOnly]
        public string StreamingAssetPath;
        [ReadOnly]
        public string ServerABPath;
        [ReadOnly]
        public string StreamingABPath;
        [ReadOnly]
        public string PhoneABPath;

#if UNITY_EDITOR
        [Button(ButtonSizes.Medium)]
        [ButtonGroup("SysPath")]
        public void OpenPersistentPath()
        {
            EditorUtility.RevealInFinder(PersistentPath);
        }
        [Button(ButtonSizes.Medium)]
        [ButtonGroup("SysPath")]
        public void OpenStreamingAssetPath()
        {
            EditorUtility.RevealInFinder(StreamingAssetPath);
        }
        [Button(ButtonSizes.Medium)]
        [ButtonGroup("ABPath")]
        public void OpenServerABPath()
        {
            EditorUtility.RevealInFinder(ServerABPath);
        }
        [Button(ButtonSizes.Medium)]
        [ButtonGroup("ABPath")]
        public void OpenStreamingABPath()
        {
            EditorUtility.RevealInFinder(StreamingABPath);
        }
       
        [Button(ButtonSizes.Medium)]
        [ButtonGroup("ABPath")]
        public void OpenPhoneABPath()
        {
            EditorUtility.RevealInFinder(PhoneABPath);
        }
#endif
    }
}
