using Sirenix.OdinInspector;

namespace AFramework
{
    using UnityEngine;

    [MonoSingletonPath("[AFramework]/AFLogger")]
    public class AFLogger : MonoSingletonWithNewObject<AFLogger>
    {
        private static DebugMode _debugMode;

        private UnityFileDebug _unityFileDebug;
        [ShowIf("@_unityFileDebug != null")]
        [Button("打开输出路径")]
        public void OpenLogger()
        {
            if (_unityFileDebug != null)
            {
                _unityFileDebug.OpenInFolder();
            }
        }

        [ShowIf("@_unityFileDebug != null")]
        public string FileDebugName = "AFLog";
        public enum LogLevels
        {
            Debug,
            Info,
            Warn,
            Error,
            None
        }

        public void Init(DebugMode debugMode)
        {
            _debugMode = debugMode;
            if (_debugMode != DebugMode.NoDebug && _debugMode != DebugMode.NormalDebug && _debugMode != DebugMode.EditorDebug
                && _unityFileDebug == null)
            {
                _unityFileDebug = gameObject.AddComponent<UnityFileDebug>();
            }
            switch (_debugMode)
            {
                case DebugMode.CSVFileDebug:
                    _unityFileDebug.FileDebugInit(FileDebugName, FileType.CSV);
                    break;
                case DebugMode.JsonFileDebug:
                    _unityFileDebug.FileDebugInit(FileDebugName, FileType.JSON);
                    break;
                case DebugMode.TXTFileDebug:
                    _unityFileDebug.FileDebugInit(FileDebugName, FileType.TXT);
                    break;
            }
        }

        public static void log(object msg, params object[] args)
        {
            if (_debugMode == DebugMode.NoDebug)
                return;
            if (args == null || args.Length == 0)
            {
                UnityEngine.Debug.Log(msg);
            }
            else
            {
                UnityEngine.Debug.LogFormat(msg.ToString(), args);
            }
        }
        /// <summary>
        /// 输出
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logColor">比如"#FFFFFF"</param>
        public static void d(string message, string logColor)
        {
            if (_debugMode == DebugMode.NoDebug)
                return;
            UnityEngine.Debug.Log("<color=" + logColor + ">" + message + "</color>");
        }
        public static void d(string message)
        {
            Log(LogLevels.Debug, message);
        }

        public static void i(string message)
        {
            Log(LogLevels.Info, message);
        }

        public static void w(string message)
        {
            Log(LogLevels.Warn, message);
        }

        public static void e(string message)
        {
            Log(LogLevels.Error, message);
        }

        /// <summary>
        /// 编辑器无论如何会输出的信息,如果AppInfo配置中时FileDebug则同时写入文件
        /// </summary>
        /// <param name="message"></param>
        public static void EditorInfoLog(string message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#else 
            if (Instance._unityFileDebug != null)
            {
                Log(LogLevels.Debug, message);
            }
#endif
        }
        public static void EditorErrorLog(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(message);
#else
            if (Instance._unityFileDebug != null)
            {
                Log(LogLevels.Error, message);
            }
#endif
        }
        static void Log(LogLevels level, string message)
        {
            switch (level)
            {
                case LogLevels.Debug:
                    Debug.Log("<color=#000000>" + message + "</color>");
                    break;
                case LogLevels.Info:
                    Debug.Log("<color=#E47833>" + message + "</color>");
                    break;
                case LogLevels.Warn:
                    Debug.LogWarning("<color=#6B238E>" + message + "</color>");
                    break;
                case LogLevels.Error:
                    Debug.LogError(message);
                    break;
            }
        }
        void Awake()
        {
            gameObject.AddComponent<DontDestroyOnLoad>();
        }
    }
}
