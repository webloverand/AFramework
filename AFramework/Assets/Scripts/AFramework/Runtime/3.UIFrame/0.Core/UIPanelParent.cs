
namespace AFramework
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Reflection;

    public class UIDataParent
    {

    }
    public class UIPanelParent : MonoBehaviour
    {
        protected int UITag;
        protected bool isInit = false;
        protected ResFromType resFromType;
        private ResLoader resLoader;
        protected string UIPath;

        //父级面板
        protected UIPanelParent ParentPanel;
        protected Dictionary<string, int> scriptToUITag = new Dictionary<string, int>();
        //子面板保存
        protected Dictionary<int, List<UIPanelParent>> SubPanelDic = new Dictionary<int, List<UIPanelParent>>();

        protected virtual void InitUI(UIDataParent UIDataParent = null)
        {

        }
        /// <summary>
        /// 事件的注册也请在这里注册
        /// </summary>
        protected virtual void RegisterUIEvent()
        {

        }
        protected virtual void InitUIAll(UIDataParent UIDataParent = null)
        {

        }
        public virtual void RefreshUIByData(UIDataParent UIDataParent = null)
        {

        }
        public virtual void Show()
        {
            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);
        }
        public virtual void Hide()
        {
            if (gameObject.activeInHierarchy)
                gameObject.SetActive(false);
        }
        public virtual void CloseUI()
        {

        }
        protected ResLoader GetResLoader()
        {
            if (resLoader == null)
            {
                resLoader = ResLoader.Allocate();
            }
            return resLoader;
        }
        public void Open(int UITag, string UIPath, UIDataParent UIDataParent = null, ResFromType resFromType = ResFromType.ABRes,
            UIPanelParent ParentPanel = null)
        {
            this.UITag = UITag;
            this.resFromType = resFromType;
            this.ParentPanel = ParentPanel;
            this.UIPath = UIPath;
            if (!isInit)
            {
                TransformBind();
                InitUI(UIDataParent);
                RegisterUIEvent();
                isInit = true;
            }
            InitUIAll(UIDataParent);
        }
        void TransformBind()
        {
            //解析AFTransformPathAttribute属性
            var flag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fieldInfos = this.GetType().GetFields(flag);
            foreach (var fi in fieldInfos)
            {
                var attrs = fi.GetCustomAttributes(typeof(AFTransformPathAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    if (attrs.Length > 1)
                    {
                        Debug.LogWarning("自动注入只能标记一次，重复的会取最后一次");
                    }
                    var attr = attrs[attrs.Length - 1] as AFTransformPathAttribute;
                    Transform trans = transform.Find(attr.TransPath);
                    if (!trans)
                    {
                        AFLogger.d("节点不存在:" + attr.TransPath);
                        continue;
                    }
                    else
                    {
                        if (fi.FieldType == typeof(GameObject))
                        {
                            fi.SetValue(this, trans.gameObject);
                            continue;
                        }
                        else if (fi.FieldType == typeof(Transform))
                        {
                            fi.SetValue(this, trans);
                            continue;
                        }
                        else if (typeof(Component).IsAssignableFrom(fi.FieldType))
                        {
                            fi.SetValue(this, trans.GetComponent(fi.FieldType));
                            continue;
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
        }
        public void ChangePanelLayer()
        {

        }
        //判断路径是否一致,以防换了语言
        public bool JudgePath(string path)
        {
            return UIPath.Equals(path);
        }
        #region 回收
        /// <summary>
        /// 关闭面板 : 默认是无限数量缓存,不会销毁内存,并且回收到recycleParent
        /// CacheCount : 缓存个数,如果为0则立马销毁gameobject并根据destoryCache判断是否回收内存
        /// recycleParent : 是否回收到RecyclePoolTrs(UIRoot下)[前提是CacheCount不为0]
        /// </summary>
        /// <param name="CacheCount">为-1为无限制缓存,建议父级(1)和子级(-1)区分此属性</param>
        public virtual void CloseSelf(int CacheCount = -1, bool destoryCache = false, bool recycleParent = true, bool isRemoveFromPar = true)
        {
            CloseUI();
            if (ParentPanel == null)
            {
                UIManager.Instance.CloseUI(UITag, this, CacheCount, destoryCache, recycleParent);
            }
            else
            {
                ParentPanel.RecycleChildPanel(gameObject, CacheCount, destoryCache, recycleParent);
                if (isRemoveFromPar)
                {
                    ParentPanel.RemoveChildPanel(UITag, this);
                }
            }
        }
        public void RecycleParAndChild()
        {
            //从其父类面板中移除
            if (ParentPanel != null)
            {
                ParentPanel.RemoveChildPanel(UITag, this);
                ParentPanel = null;
            }
            //物体被销毁,其内的子物体也一同销毁,防止内存泄漏
            SubPanelDic.ForEach((uitag, panelList) =>
            {
                panelList.ForEach((panel) =>
                {
                    //isRemoveFromPar : false不要从父物体中移除,避免引起SubPanelDic的变化
                    panel.CloseSelf(0, true, isRemoveFromPar: false);
                });
            });
            Destroy();
        }
        public void RecycleChild<T>(int CacheCount = -1, bool destoryCache = false, bool recycleParent = true) where T : UIPanelParent
        {
            RecycleChild(typeof(T).ToString(), CacheCount, destoryCache, recycleParent);
        }
        public void RecycleChild(string scriptName, int CacheCount = -1, bool destoryCache = false, bool recycleParent = true)
        {
            if (!scriptToUITag.ContainsKey(scriptName))
            {
                AFLogger.e("没有此" + scriptName + "子面板");
                return;
            }
            int uit = scriptToUITag[scriptName];
            SubPanelDic.ForEach((uitag, panelList) =>
            {
                if (uitag == uit)
                {
                    panelList.ForEach((panel) =>
                    {
                        //isRemoveFromPar : false不要从父物体中移除,避免引起SubPanelDic的变化
                        panel.CloseSelf(CacheCount, destoryCache, recycleParent, isRemoveFromPar: false);
                    });
                }
            });
            RemoveChildPanel(uit);
            scriptToUITag.Remove(scriptName);
        }
        public void RecycleAllChild(int CacheCount = -1, bool destoryCache = false, bool recycleParent = true)
        {
            SubPanelDic.ForEach((uitag, panelList) =>
            {
                panelList.ForEach((panel) =>
                {
                    //isRemoveFromPar : false不要从父物体中移除,避免引起SubPanelDic的变化
                    panel.CloseSelf(CacheCount, destoryCache, recycleParent, isRemoveFromPar: false);
                });
            });
            SubPanelDic.Clear();
        }
        void RecycleChildPanel(GameObject childUIObj, int CacheCount = -1, bool destoryCache = false, bool recycleParent = true)
        {
            GetResLoader().ReleaseObj(childUIObj, CacheCount, destoryCache, recycleParent);
        }
        void Destroy()
        {
            scriptToUITag.Clear();
            SubPanelDic.Clear();
            if (GetResLoader() != null)
            {
                GetResLoader().ReleaseAllRes();
                //同时释放resloader
                GetResLoader().Recycle2Cache();
            }
        }
        protected virtual void OnDestroy()
        {
            Destroy();
        }
        #endregion
        #region 子面板
        protected virtual T CreateSubPanel<T>(int uitag, Transform parentTrans, UIDataParent UIDataParent = null,
            bool isClear = false)
            where T : UIPanelParent
        {
            AFUIAttribute uiAttri = UIManager.Instance.GetClassData(uitag).Attribute as AFUIAttribute;
            GameObject panel = GetResLoader().InstantiateObjectSync(resFromType, uiAttri.UIPath, parentTrans, isClear);
            T scripts = panel.GetComponent<T>();
            if (scripts == null)
                scripts = panel.AddComponent<T>();
            scripts.Open(uitag, uiAttri.UIPath, UIDataParent, resFromType, this);
            AddChildPanel(uitag, scripts);
            scriptToUITag[typeof(T).ToString()] = uitag;
            return scripts;
        }
        protected void AddChildPanel(int uitag, UIPanelParent childUIPanel)
        {
            if (SubPanelDic.ContainsKey(uitag))
            {
                SubPanelDic[uitag].AddValue(childUIPanel);
            }
            else
            {
                List<UIPanelParent> panelList = new List<UIPanelParent>();
                panelList.Add(childUIPanel);
                SubPanelDic.Add(uitag, panelList);
            }
        }
        protected void AddChildPanel(int uitag, List<UIPanelParent> childUIPanels)
        {
            if (SubPanelDic.ContainsKey(uitag))
            {
                childUIPanels.ForEach((childPanel) =>
                {
                    SubPanelDic[uitag].AddValue(childPanel);
                });
            }
            else
            {
                SubPanelDic.Add(uitag, childUIPanels);
            }
        }
        protected void RemoveChildPanel(int uitag, UIPanelParent childUIPanel)
        {
            if (SubPanelDic.ContainsKey(uitag))
            {
                SubPanelDic[uitag].Remove(childUIPanel);
            }
            else
            {
                AFLogger.e("父级面板并不包含此子级面板,请注意");
            }
        }
        protected void RemoveChildPanel(int uitag)
        {
            if (SubPanelDic.ContainsKey(uitag))
            {
                SubPanelDic[uitag].Clear();
                SubPanelDic.Remove(uitag);
            }
            else
            {
                AFLogger.e("父级面板并不包含此子级面板,请注意");
            }
        }
        #endregion
        #region 事件
        /// <summary>
        /// 注册事件信息
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="msgEvent"></param>
        public void RegisterEvent(int msgID, MsgEvent msgEvent)
        {
            UIManager.Instance.RegisterEvent(msgID, msgEvent);
        }
        /// <summary>
        /// 取消事件的注册
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="msgEvent"></param>
        public void UnRegisterEvent(int msgID, MsgEvent msgEvent)
        {
            UIManager.Instance.UnRegisterEvent(msgID, msgEvent);
        }
        /// <summary>
        /// 取ID全部事件的注册
        /// </summary>
        /// <param name="msgID"></param>
        public void UnAllIDRegisterEvent(int msgID)
        {
            UIManager.Instance.UnAllIDRegisterEvent(msgID);
        }
        /// <summary>
        /// 发送事件信息
        /// </summary>
        /// <param name="msgID"></param>
        public void SendMsg(int msgID, MsgInfo msgInfo)
        {
            UIManager.Instance.SendMsg(msgID, msgInfo);
        }
        #endregion
    }
}
