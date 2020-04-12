namespace AFramework
{
    using System.Collections.Generic;
    using UnityEngine;
    public class UIManager : ManagerBase<UIManager, AFUIAttribute>
    {
        Transform BGParetnTrans;
        Transform BottomParetnTrans;
        Transform CommonParetnTrans;
        Transform TopParetnTrans;
        Transform LoadParetnTrans;
        Transform TipParetnTrans;

        ResLoader resLoader;
        public override void Init()
        {
            base.Init();
            resLoader = new ResLoader();
            string pathPre = "";
            if (GameObject.Find("UIRoot") != null)
            {
                pathPre = "UIRoot/MainCanvas/";
            }
            else
            {
                pathPre = "UIRoot(Clone)/MainCanvas/";
            }
            BGParetnTrans = GameObject.Find(pathPre + "BG").transform;
            BottomParetnTrans = GameObject.Find(pathPre + "Bottom").transform;
            CommonParetnTrans = GameObject.Find(pathPre + "Common").transform;
            TopParetnTrans = GameObject.Find(pathPre + "Top").transform;
            LoadParetnTrans = GameObject.Find(pathPre + "Loading").transform;
            TipParetnTrans = GameObject.Find(pathPre + "Tip").transform;
        }
        //面板UI保存信息
        private Dictionary<int, string> UITagToScriptsName = new Dictionary<int, string>();
        private Dictionary<string, UIPanelParent> ScriptsNameToPanel = new Dictionary<string, UIPanelParent>();
        protected override int ManagerId
        {
            get
            {
                return MgrID.UIMsgID;
            }
        }

        public T OpenUI<T>(int UILabel, UIDataParent UIDataParent = null, PanelLayer panelLayer = PanelLayer.Common,
           bool isClear = false) where T : UIPanelParent
        {
            return OpenUI<T>(UILabel, GetPanleLayer(panelLayer), UIDataParent, isClear);
        }
        //此方法可用来创建panel,也可以刷新panel(不能重复创建panel),重复创建一般是子面板,在父面板中调用创建子面板方法即可
        public T OpenUI<T>(int UITag, Transform parent, UIDataParent UIDataParent = null,
             bool isClear = false) where T : UIPanelParent
        {
            AFUIAttribute uiAttri = GetClassData(UITag).Attribute as AFUIAttribute;
            string scriptsName = typeof(T).ToString();
            if (ScriptsNameToPanel.ContainsKey(scriptsName) && ScriptsNameToPanel[scriptsName].JudgePath(uiAttri.UIPath))
            {
                ScriptsNameToPanel[scriptsName].transform.SetParent(parent);
                ScriptsNameToPanel[scriptsName].RefreshUIByData(UIDataParent);
                if (!UITagToScriptsName.ContainsKey(UITag))
                {
                    UITagToScriptsName[UITag] = scriptsName;
                }
                ScriptsNameToPanel[scriptsName].Show();
                //显示
                return ScriptsNameToPanel[scriptsName] as T;
            }
            ResFromType resFromType = GetResFrom(uiAttri.UIPath);
            GameObject panel = resLoader.InstantiateObjectSync(resFromType, uiAttri.UIPath, parent, isClear);
            T scripts = panel.GetComponent<T>();
            if (scripts == null)
                scripts = panel.AddComponent<T>();
            scripts.Open(UITag, uiAttri.UIPath, UIDataParent, resFromType);
            if (ScriptsNameToPanel.ContainsKey(scriptsName))
            {
                ScriptsNameToPanel[scriptsName] = scripts as UIPanelParent;
            }
            else
            {
                ScriptsNameToPanel.Add(scriptsName, scripts as UIPanelParent);
            }
            UITagToScriptsName[UITag] = scriptsName;
            return scripts;
        }
        public ResFromType GetResFrom(string path)
        {
            ResFromType resFromType = ResFromType.ResourcesRes;
            if (path.Contains("Assets/") && path.Contains("."))
            {
                resFromType = ResFromType.ABRes;
            }
            return resFromType;
        }
        public Transform GetPanleLayer(PanelLayer panelLayer)
        {
            Transform panelPar = null;
            switch (panelLayer)
            {
                case PanelLayer.BG:
                    panelPar = BGParetnTrans;
                    break;
                case PanelLayer.Bottom:
                    panelPar = BottomParetnTrans;
                    break;
                case PanelLayer.Common:
                    panelPar = CommonParetnTrans;
                    break;
                case PanelLayer.Top:
                    panelPar = TopParetnTrans;
                    break;
                case PanelLayer.Load:
                    panelPar = LoadParetnTrans;
                    break;
                case PanelLayer.Tip:
                    panelPar = TipParetnTrans;
                    break;
                case PanelLayer.NoParent:
                    panelPar = null;
                    break;
            }
            return panelPar;
        }
        /// <summary>
        /// 释放单个UI面板,根据UITag与obj,一般是给面板的closeself调用
        /// </summary>
        public void CloseUI(int UITag, UIPanelParent panel, int CacheCount = -1, bool destoryCache = false,
            bool recycleParent = true)
        {
            if (UITagToScriptsName.ContainsKey(UITag))
            {
                CloseUI(UITagToScriptsName[UITag], panel, CacheCount, destoryCache, recycleParent);
            }
            else
            {
                AFLogger.e("当前并不存在 : " + UITag + "面板");
            }
        }
        /// <summary>
        /// 父级单个面板的释放,根据类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void CloseUI<T>(int CacheCount = -1, bool destoryCache = false, bool recycleParent = true)
            where T : UIPanelParent
        {
            CloseUI(typeof(T).ToString(), GetPanel<T>(), CacheCount, destoryCache, recycleParent);
        }
        protected void CloseUI(string scriptName, UIPanelParent panel, int CacheCount = -1,
            bool destoryCache = false, bool recycleParent = true)
        {
            //面板销毁 : 因此首先销毁其父物体与子物体
            if (CacheCount == 0)
            {
                panel.RecycleParAndChild();
            }
            resLoader.ReleaseObj(ScriptsNameToPanel[scriptName].gameObject, CacheCount, destoryCache, recycleParent);
            RemoveParentPanel(scriptName);
            UITagToScriptsName.Remove(GetUITagByScriptName(scriptName));
            if (ScriptsNameToPanel.ContainsKey(scriptName))
            {
                ScriptsNameToPanel.Remove(scriptName);
            }
        }
        public T GetPanel<T>() where T : UIPanelParent
        {
            string scriptName = typeof(T).ToString();
            if (ScriptsNameToPanel.ContainsKey(scriptName) && ScriptsNameToPanel[scriptName] != null)
            {
                return ScriptsNameToPanel[scriptName].GetComponent<T>();
            }
            else
            {
                AFLogger.e("当前并不存在 : " + scriptName + "面板");
            }
            return default(T);
        }

        public void RefreshPanel<T>(UIDataParent UIDataParent) where T : UIPanelParent
        {
            string scriptName = typeof(T).ToString();
            if (ScriptsNameToPanel.ContainsKey(scriptName))
            {
                ScriptsNameToPanel[scriptName].RefreshUIByData(UIDataParent);
            }
            else
            {
                AFLogger.e("当前并不存在 : " + scriptName + "面板");
            }
        }

        public void RemovePanel<T>()
        {
            string scriptName = typeof(T).ToString();
            if (ScriptsNameToPanel.ContainsKey(scriptName))
            {
                ScriptsNameToPanel.Remove(scriptName);
            }
            else
            {
                AFLogger.e("当前并不存在 : " + scriptName + "面板");
            }
        }
        public void RemoveParentPanel(string scriptName)
        {
            if (ScriptsNameToPanel.ContainsKey(scriptName))
            {
                ScriptsNameToPanel.Remove(scriptName);
            }
            else
            {
                AFLogger.e("当前并不存在 : " + scriptName + "面板");
            }
        }
        public void ChangePanelLayer<T>(PanelLayer panelLayer)
        {
            string scriptName = typeof(T).ToString();
            if (ScriptsNameToPanel.ContainsKey(scriptName))
            {
                ChangePanelLayer(panelLayer, ScriptsNameToPanel[scriptName].transform);
            }
            else
            {
                AFLogger.e("当前并不存在 : " + scriptName + "面板");
            }
        }
        public void ChangePanelLayer(PanelLayer panelLayer, Transform uiPanel)
        {
            switch (panelLayer)
            {
                case PanelLayer.BG:
                    uiPanel.transform.SetParent(BGParetnTrans);
                    break;
                case PanelLayer.Bottom:
                    uiPanel.transform.SetParent(BottomParetnTrans);
                    break;
                case PanelLayer.Common:
                    uiPanel.transform.SetParent(CommonParetnTrans);
                    break;
                case PanelLayer.Top:
                    uiPanel.transform.SetParent(TopParetnTrans);
                    break;
                case PanelLayer.Load:
                    uiPanel.transform.SetParent(LoadParetnTrans);
                    break;
                case PanelLayer.Tip:
                    uiPanel.transform.SetParent(TipParetnTrans);
                    break;
            }
        }
        int GetUITagByScriptName(string scriptsName)
        {
            int UItag = -1;
            UITagToScriptsName.ForEach((UITag, scriptname) =>
            {
                if (scriptname.Equals(scriptsName))
                {
                    UItag = UITag;
                }
            });
            return UItag;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            resLoader.Recycle2Cache();
            UnAllRegisterEvent();
            Inst = null;
        }
    }
    public enum PanelLayer
    {
        BG,
        Bottom,
        Common,
        Top,
        Load,
        Tip,
        NoParent
    }
}
