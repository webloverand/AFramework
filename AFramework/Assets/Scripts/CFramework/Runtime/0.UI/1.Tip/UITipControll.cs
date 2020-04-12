using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AFramework
{
    public class UITipControll : Singleton<UITipControll>
    {
        ResLoader resLoader;
        Dictionary<TipConType, UITip> TypeToTipPanel = new Dictionary<TipConType, UITip>();

        public void OpenUITip(TipData tipData, PanelLayer panelLayer = PanelLayer.Tip)
        {
            if (TypeToTipPanel.ContainsKey(tipData.tipConType))
            {
                //已经打开一个同类的Tip,因此是需要刷新
                TypeToTipPanel[tipData.tipConType].RefreshUIByData(tipData);
                UIManager.Instance.ChangePanelLayer(panelLayer, TypeToTipPanel[tipData.tipConType].transform);
            }
            else
            {
                //是需要新打开一个tip
                UITip uiTipPanel = CreateTipPanel<UITip>(100000, UIManager.Instance.GetPanleLayer(panelLayer),
                    tipData, true);
                TypeToTipPanel.Add(tipData.tipConType, uiTipPanel);
            }
        }
        public void OpenUITip(TipBtnType tipBtnType, TipConType tipConType, string tipStr,
            UnityAction EnsureEvent = null, UnityAction CancelEvent = null, PanelLayer panelLayer = PanelLayer.Tip)
        {
            TipData tipData = new TipData();
            tipData.tipBtnType = tipBtnType;
            tipData.tipConType = tipConType;
            tipData.EnsureEvent = EnsureEvent;
            tipData.CancelEvent = CancelEvent;
            tipData.tipStr = tipStr;
            OpenUITip(tipData, panelLayer);
        }
        protected T CreateTipPanel<T>(int uitag, Transform parentTrans, UIDataParent UIDataParent = null,
           bool isClear = false)
           where T : UIPanelParent
        {
            AFUIAttribute uiAttri = UIManager.Instance.GetClassData(uitag).Attribute as AFUIAttribute;
            if (resLoader == null)
            {
                resLoader = ResLoader.Allocate();
            }

            GameObject panel = resLoader.InstantiateObjectSync(UIManager.Instance.GetResFrom(uiAttri.UIPath), uiAttri.UIPath, parentTrans, isClear);
            T scripts = panel.GetComponent<T>();
            if (scripts == null)
                scripts = panel.AddComponent<T>();
            scripts.Open(uitag, uiAttri.UIPath, UIDataParent, UIManager.Instance.GetResFrom(uiAttri.UIPath), null);
            return scripts;
        }

        public void CloseUITip(TipConType tipContentType, int CacheCount = -1, bool destoryCache = false, bool recycleParent = true)
        {
            if (TypeToTipPanel.ContainsKey(tipContentType))
            {
                resLoader.ReleaseObj(TypeToTipPanel[tipContentType].gameObject, CacheCount, destoryCache, recycleParent);
                TypeToTipPanel.Remove(tipContentType);
            }
        }
        public void ChangePanelLayer(TipConType tipContentType, PanelLayer panelLayer)
        {
            if (TypeToTipPanel.ContainsKey(tipContentType))
            {
                //已经打开一个同类的Tip,因此是需要刷新
                UIManager.Instance.ChangePanelLayer(panelLayer, TypeToTipPanel[tipContentType].transform);
            }
        }
    }
}
