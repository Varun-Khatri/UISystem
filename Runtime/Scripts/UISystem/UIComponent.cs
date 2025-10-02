using System;
using UnityEngine;

namespace VK.UI
{
    // UI State data structure
    public struct UIStateData
    {
        public Type UIType;
        public bool IsVisible;
        public bool HasFocus;
        public UILayer Layer;
    }

    public enum UILayer
    {
        HUD,        // Persistent gameplay UI
        Screen,     // Full-screen views (menus, etc.)
        Panel,      // Modal dialogs and popups
        Overlay,    // Tooltips, notifications
        Debug       // Debug/developer UI
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIComponent : MonoBehaviour
    {
        [SerializeField] private UILayer _layer = UILayer.Screen;
        [SerializeField] private bool _isPersistent = false;

        private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();
        public UILayer Layer => _layer;
        public bool IsPersistent => _isPersistent;
        public Type UIType => GetType();

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Initialize() { }

        public void Show()
        {
            CanvasGroup.alpha = 1;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            OnShow();
        }

        public void Hide()
        {
            CanvasGroup.alpha = 0;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            OnHide();
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }

    // Specific UI types for organization
    public abstract class UIScreen : UIComponent { }
    public abstract class UIPanel : UIComponent { }
    public abstract class UIHUD : UIComponent { }
    public abstract class UIOverlay : UIComponent { }
}