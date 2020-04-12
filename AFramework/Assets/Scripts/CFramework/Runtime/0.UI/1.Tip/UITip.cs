namespace AFramework
{
    using UnityEngine.Events;
    using UnityEngine.UI;
    //增加叠加提示
    public enum TipBtnType
    {
        NoBtn, OneBtn, TwoBtn
    }
    public class TipData : UIDataParent
    {
        public TipConType tipConType;
        public TipBtnType tipBtnType;
        public UnityAction EnsureEvent;
        public UnityAction CancelEvent;
        public string tipStr;
    }
    public delegate void SetTipBtnText();

    [AFUI(100000, "Assets/ResForAB/ABMain/Tip/TipCHA")]
    public class UITip : UIPanelParent
    {
        TipData tipContent;
        [AFTransformPath("OneBtn/Ensure")]
        Button OneEnsureBtn;
        [AFTransformPath("TwoBtn/Ensure")]
        Button TwoEnsureBtn;
        [AFTransformPath("TwoBtn/Cancel")]
        Button TwoCancelBtn;
        protected override void InitUI(UIDataParent UIDataParent = null)
        {
            if (UIDataParent != null)
                tipContent = (TipData)UIDataParent;
            OneEnsureBtn.onClick.AddListener(() =>
            {
                CloseSelf();
            });
            TwoEnsureBtn.onClick.AddListener(() =>
            {
                CloseSelf();
            });
            TwoCancelBtn.onClick.AddListener(() =>
            {
                CloseSelf();
            });
        }
        protected override void InitUIAll(UIDataParent UIDataParent = null)
        {
            InitUIByData(UIDataParent);
        }
        protected override void RegisterUIEvent()
        {
            if (tipContent.EnsureEvent != null)
            {
                OneEnsureBtn.onClick.AddListener(tipContent.EnsureEvent);
                TwoEnsureBtn.onClick.AddListener(tipContent.EnsureEvent);
            }
            if (tipContent.CancelEvent != null)
                TwoCancelBtn.onClick.AddListener(tipContent.CancelEvent);
        }
        public override void RefreshUIByData(UIDataParent UIDataParent = null)
        {
            InitUIByData(UIDataParent);
        }


        public void InitUIByData(UIDataParent UIDataParent)
        {
            tipContent = (TipData)UIDataParent;
            for (int i = 1; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(i == (int)tipContent.tipBtnType + 1);
            }
            transform.GetChild((int)tipContent.tipBtnType + 1).GetChild(0).GetComponent<Text>().text = tipContent.tipStr;
        }
        public override void CloseSelf(int CacheCount = -1, bool destoryCache = false, bool recycleParent = true, bool isRemoveFromPar = true)
        {
            UITipControll.Instance.CloseUITip(tipContent.tipConType, CacheCount, destoryCache, recycleParent);
        }
    }

    public enum TipConType
    {
        TestUITip
    }
}
