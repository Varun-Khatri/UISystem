using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VK.UI.Samples
{
    public class UIManager : MonoBehaviour
    {
        // UI Components
        [Header("UI Components")]
        [SerializeField] private List<UIComponent> _UIComponents = new List<UIComponent>();
        [SerializeField] private UIScreen _defaultScreen;

        // Event Channels
        [Header("Event Channels")]
        public UnityEvent<Type> showUIEvent;
        public UnityEvent<Type> hideUIEvent;
        public UnityEvent goBackEvent;
        public UnityEvent<UILayer> hideAllUIEvent;
        public UnityEvent<UIStateData> uiStateEvent;

        private Dictionary<Type, UIComponent> _UIComponentDict = new();
        private Dictionary<UILayer, List<UIComponent>> _activeComponents = new();
        private Stack<UIComponent> _screenHistory = new();
        private UIComponent _currentScreen;
        private Dictionary<UILayer, RectTransform> _layerContainers = new();
        private bool _isInitialized = false;

        private void Awake()
        {
            InitializeContainers();
            CacheAllUIComponents();
            RegisterEventListeners();
            _isInitialized = true;
        }

        private void Start()
        {
            // Hide all UI first, then show default screen
            HideAllUI();
            if (_defaultScreen != null)
                ShowUI(_defaultScreen.GetType());
        }

        private void OnDestroy()
        {
            UnregisterEventListeners();
        }

        private void InitializeContainers()
        {
            // Create simple RectTransform containers instead of Canvases
            // This preserves the original RectTransform settings
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var containerObj = new GameObject($"Container_{layer}");
                containerObj.transform.SetParent(transform);

                var rectTransform = containerObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;

                _layerContainers[layer] = rectTransform;
            }
        }

        private void RegisterEventListeners()
        {
            if (showUIEvent != null) showUIEvent.AddListener(OnShowUIRequested);
            if (hideUIEvent != null) hideUIEvent.AddListener(OnHideUIRequested);
            if (goBackEvent != null) goBackEvent.AddListener(OnGoBackRequested);
            if (hideAllUIEvent != null) hideAllUIEvent.AddListener(OnHideAllRequested);
        }

        private void UnregisterEventListeners()
        {
            if (showUIEvent != null) showUIEvent.RemoveListener(OnShowUIRequested);
            if (hideUIEvent != null) hideUIEvent.RemoveListener(OnHideUIRequested);
            if (goBackEvent != null) goBackEvent.RemoveListener(OnGoBackRequested);
            if (hideAllUIEvent != null) hideAllUIEvent.RemoveListener(OnHideAllRequested);
        }

        private void CacheAllUIComponents()
        {
            _UIComponentDict.Clear();

            foreach (var component in _UIComponents)
            {
                if (component == null) continue;
                var type = component.GetType();

                if (_UIComponentDict.ContainsKey(type))
                {
                    Debug.LogWarning($"Duplicate UI component type found: {type.Name}");
                    continue;
                }

                _UIComponentDict[type] = component;

                // Store original parent to restore if needed
                var originalParent = component.transform.parent;

                // Reparent to appropriate layer container
                if (_layerContainers.ContainsKey(component.Layer))
                {
                    component.transform.SetParent(_layerContainers[component.Layer], false);

                    // Ensure RectTransform is properly set up
                    var rectTransform = component.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchorMin = Vector2.zero;
                        rectTransform.anchorMax = Vector2.one;
                        rectTransform.sizeDelta = Vector2.zero;
                        rectTransform.anchoredPosition = Vector2.zero;
                        rectTransform.localScale = Vector3.one;
                    }
                }

                component.Initialize();

                // Force hide all components during initialization
                SetUIState(component, false, true);
            }

            // Initialize active components dictionary
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                _activeComponents[layer] = new List<UIComponent>();
            }
        }

        private void HideAllUI()
        {
            foreach (var component in _UIComponents)
            {
                if (component != null)
                {
                    SetUIState(component, false, true);
                }
            }

            _activeComponents.Clear();
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                _activeComponents[layer] = new List<UIComponent>();
            }

            _screenHistory.Clear();
            _currentScreen = null;
        }

        private void OnShowUIRequested(Type uiType)
        {
            ShowUI(uiType);
        }

        private void OnHideUIRequested(Type uiType)
        {
            HideUI(uiType);
        }

        private void OnGoBackRequested()
        {
            GoBack();
        }

        private void OnHideAllRequested(UILayer layer)
        {
            HideAllInLayer(layer);
        }

        // Public API Methods
        public void ShowUI<T>() where T : UIComponent
        {
            ShowUI(typeof(T));
        }

        public void ShowUI(Type uiType)
        {
            if (_UIComponentDict.TryGetValue(uiType, out UIComponent component))
            {
                ShowUIInternal(component, true);
            }
            else
            {
                Debug.LogWarning($"UI component of type {uiType} not found!");
            }
        }

        public void HideUI<T>() where T : UIComponent
        {
            HideUI(typeof(T));
        }

        public void HideUI(Type uiType)
        {
            if (_UIComponentDict.TryGetValue(uiType, out UIComponent component))
            {
                HideUIInternal(component);
            }
        }

        public void GoBack()
        {
            if (_screenHistory.Count > 0)
            {
                var previousScreen = _screenHistory.Pop();
                ShowUIInternal(previousScreen, false);
            }
            else if (_defaultScreen != null)
            {
                ShowUIInternal(_defaultScreen, false);
            }
            else
            {
                Debug.LogWarning("No screen history and no default screen set!");
            }
        }

        private void ShowUIInternal(UIComponent component, bool rememberInHistory)
        {
            if (component == null) return;

            // Handle screen history
            if (component is UIScreen)
            {
                if (rememberInHistory && _currentScreen != null && _currentScreen != component)
                {
                    _screenHistory.Push(_currentScreen);
                }

                // Hide current screen if it's not persistent
                if (_currentScreen != null && _currentScreen != component && !_currentScreen.IsPersistent)
                {
                    HideUIInternal(_currentScreen);
                }

                _currentScreen = component as UIScreen;
            }

            // Handle panel layer - only one active panel at a time (unless persistent)
            if (component is UIPanel && !component.IsPersistent)
            {
                HideAllInLayer(UILayer.Panel, false);
            }

            // Show the component
            SetUIState(component, true);
            AddToActive(component);

            // Notify UI state change
            if (_isInitialized)
            {
                uiStateEvent?.Invoke(new UIStateData
                {
                    UIType = component.UIType,
                    IsVisible = true,
                    HasFocus = true,
                    Layer = component.Layer
                });
            }
        }

        private void HideUIInternal(UIComponent component)
        {
            if (component == null) return;

            SetUIState(component, false);
            RemoveFromActive(component);

            // Notify UI state change
            if (_isInitialized)
            {
                uiStateEvent?.Invoke(new UIStateData
                {
                    UIType = component.UIType,
                    IsVisible = false,
                    HasFocus = false,
                    Layer = component.Layer
                });
            }

            if (component is UIScreen && _currentScreen == component)
            {
                _currentScreen = null;
            }
        }

        private void SetUIState(UIComponent component, bool show, bool force = false)
        {
            if (component == null) return;

            // Only process if we're initialized or forcing
            if (!_isInitialized && !force) return;

            if (show)
            {
                component.Show();
            }
            else
            {
                component.Hide();
            }
        }

        private void HideAllInLayer(UILayer layer, bool includePersistent = false)
        {
            var componentsToHide = new List<UIComponent>(_activeComponents[layer]);

            foreach (var component in componentsToHide)
            {
                if (!includePersistent && component.IsPersistent)
                    continue;

                HideUIInternal(component);
            }
        }

        private void AddToActive(UIComponent component)
        {
            var layerList = _activeComponents[component.Layer];
            if (!layerList.Contains(component))
            {
                layerList.Add(component);
                // Sort by sibling order for proper layering within the same container
                layerList.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            }
        }

        private void RemoveFromActive(UIComponent component)
        {
            _activeComponents[component.Layer].Remove(component);
        }

        // Public API
        public T GetUI<T>() where T : UIComponent
        {
            _UIComponentDict.TryGetValue(typeof(T), out UIComponent component);
            return component as T;
        }

        public bool IsUIVisible<T>() where T : UIComponent
        {
            var component = GetUI<T>();
            return component != null && component.CanvasGroup.alpha > 0;
        }

        public bool IsUIRegistered<T>() where T : UIComponent
        {
            return _UIComponentDict.ContainsKey(typeof(T));
        }

        public UIComponent CurrentScreen => _currentScreen;
        public bool HasHistory => _screenHistory.Count > 0;

        // Editor helper to find all UI components in scene
        [ContextMenu("Find All UI Components in Scene")]
        private void FindAllUIComponentsInScene()
        {
            _UIComponents.Clear();
            var allComponents = FindObjectsByType<UIComponent>(FindObjectsSortMode.None);
            _UIComponents.AddRange(allComponents);
            Debug.Log($"Found {allComponents.Length} UI components in scene");
        }

        // Debug method to show current UI state
        [ContextMenu("Debug UI State")]
        private void DebugUIState()
        {
            Debug.Log($"Current Screen: {(_currentScreen != null ? _currentScreen.name : "None")}");
            Debug.Log($"History Count: {_screenHistory.Count}");

            foreach (var layer in _activeComponents.Keys)
            {
                Debug.Log($"Layer {layer}: {_activeComponents[layer].Count} active components");
                foreach (var component in _activeComponents[layer])
                {
                    Debug.Log($"  - {component.name} (Persistent: {component.IsPersistent})");
                }
            }
        }
    }
}