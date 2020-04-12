/*******************************************************************
* Copyright(c)
* 文件名称: AFStart.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using Sirenix.OdinInspector;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// 此脚本需要AppInfoConfig赋值,继承MonoSingleton,同时是整个项目的驱动脚本
    /// </summary>
    public class AFStart : MonoSingleton<AFStart>
    {
        [InlineEditor]
        public AppInfoConfig appInfo;
        [InlineEditor]
        public AF_ABConfig ABConfig;

        string LanuageSuffix = "CHA";
        public string GetLanuageSuffix()
        {
            return LanuageSuffix;
        }
        public void ChangeLanuageSuffix(string newLanuageSuffix)
        {
			if (!LanuageSuffix.Equals(newLanuageSuffix))
			{
                AFLogger.d("更改语言");
				LanuageSuffix = newLanuageSuffix;
				UIManager.Instance.UpdateIntClassData();
			}
        }

        static Action OnStart;
        static Action OnUpdate;
        static Action OnLateUpdate;
        public static void RegisterStart(Action action)
        {
            OnStart += action;
        }
        public static void RegisterUpdate(Action action)
        {
            OnUpdate += action;
        }
        public static void RegisterLateUpdate(Action action)
        {
            OnLateUpdate += action;
        }
        
        public void Start()
        {
            ABDataHolder.Instance.ABSysInit(appInfo, ABConfig);
            AFLogger.Instance.Init(appInfo.debugMode);
            //管理类的初始化
            MgrStart();
            ResManager.Init();
            OnStart.InvokeGracefully();
        }

        public void Update()
        {
            OnUpdate.InvokeGracefully();
        }
        private void LateUpdate()
        {
            OnLateUpdate.InvokeGracefully();
        }

        public void MgrStart()
        {
            List<Type> allTypes = GetAllType();
            if (allTypes == null || allTypes.Count <= 0)
                return;
            var mgrs = GetIMgr(allTypes) ;
            //类型注册
            foreach (var t in allTypes)
            {
                foreach (var iMgr in mgrs)
                {
                    iMgr.CheckType(t);
                }
            }

            //管理器初始化
            foreach (var m in mgrs)
            {
                m.Init();
            }
            //所有管理器开始工作
            foreach (var m in mgrs)
            {
                m.Start();
            }
        }
        public List<Type> GetAllType()
        {
            var assembly = Assembly.GetAssembly(typeof(AFStart));
            if (assembly == null)
            {
                AFLogger.e("当前dll is null");
                return null;
            }
            return assembly.GetTypes().Distinct().ToList();
        }
        public List<IMgr> GetIMgr(List<Type> allTypes)
        {
            if (allTypes == null || allTypes.Count <= 0)
                return null;
            var mgrs = new List<IMgr>();
            foreach (var t in allTypes)
            {
                if (t != null && t.BaseType != null && t.BaseType.FullName != null &&
                    t.BaseType.FullName.Contains(".ManagerBase"))
                {
                    var i = t.BaseType.GetProperty("Instance").GetValue(null, null) as IMgr;
                    mgrs.Add(i);
                    continue;
                }
            }
            return mgrs;
        }
    }
}
