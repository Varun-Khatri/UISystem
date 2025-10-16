# UI Management System

A layer-based UI management system for Unity with built-in navigation, state tracking, and event-driven architecture. Perfect for complex game UIs with multiple screens and panels.

## 🎯 Features

- **Layer-Based Management** - Organized UI hierarchy with 5 distinct layers
- **Screen Navigation** - Built-in back stack and history management
- **Event-Driven Architecture** - UnityEvents for easy integration
- **State Tracking** - Complete visibility and focus state management
- **Persistent UI** - Support for always-visible UI elements
- **CanvasGroup Integration** - Smooth alpha, interactability, and raycast control

## 🏗️ Architecture

### Core Components

| Component | Description |
|-----------|-------------|
| `UIComponent` | Base class for all UI elements |
| `UIScreen` | Full-screen views (menus, gameplay) |
| `UIPanel` | Modal dialogs and popups |
| `UIHUD` | Persistent gameplay UI |
| `UIOverlay` | Tooltips, notifications |
| `UIManager` | Main controller managing all UI components |

### UI Layers

| Layer | Purpose | Examples |
|-------|---------|----------|
| **HUD** | Persistent gameplay UI | Health bars, score, minimap |
| **Screen** | Full-screen views | Main menu, settings, gameplay |
| **Panel** | Modal dialogs | Pause menu, confirmation dialogs |
| **Overlay** | Temporary elements | Tooltips, notifications |
| **Debug** | Developer tools | Debug info, cheat console |

## 📦 Installation

1. Download or clone this repository
2. Add the files to your Unity project (2019.4+ recommended)
3. Set up a Canvas with proper scaling for your game

## 🚀 Quick Start

### 1. Create Your UI Components

**Create a HUD:**
```csharp
public class GameHUD : UIHUD
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI scoreText;
    
    protected override void OnShow()
    {
        Debug.Log("HUD shown");
        UpdateDisplay();
    }
    
    protected override void OnHide()
    {
        Debug.Log("HUD hidden");
    }
    
    private void UpdateDisplay()
    {
        healthText.text = $"Health: {playerHealth}";
        scoreText.text = $"Score: {playerScore}";
    }
}
```
**Create a Screen:**

```csharp
public class MainMenuScreen : UIScreen
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    
    protected override void Awake()
    {
        base.Awake();
        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
    }
    
    private void OnStartClicked()
    {
        FindObjectOfType<UIManager>().ShowUI<GameScreen>();
    }
    
    private void OnSettingsClicked()
    {
        FindObjectOfType<UIManager>().ShowUI<SettingsPanel>();
    }
}
```

**Create a Panel:**

```csharp
public class PausePanel : UIPanel
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;
    
    protected override void Awake()
    {
        base.Awake();
        resumeButton.onClick.AddListener(OnResumeClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }
    
    protected override void OnShow()
    {
        Time.timeScale = 0f; // Pause game
    }
    
    protected override void OnHide()
    {
        Time.timeScale = 1f; // Resume game
    }
    
    private void OnResumeClicked()
    {
        FindObjectOfType<UIManager>().HideUI<PausePanel>();
    }
}
```
### 2. Set Up the UIManager

**In the Unity Editor:**

- Create empty GameObject named "UIManager"
- Add UIManager component
- Drag all UI components into the UI Components list
- Set the Default Screen (usually your main menu)

### 3. Automatic Hierarchy Setup

The system automatically creates this structure:

- All UI components are automatically reparented to appropriate layer containers
- No manual hierarchy setup required
- Containers are created for each layer (HUD, Screen, Panel, Overlay, Debug)

### 4. That's It!

The system handles:

✅ Automatic container creation
✅ Proper layer organization
✅ RectTransform setup
✅ Initial visibility state

Your UI components will be automatically organized like this:

```text
Root Canvas [UIManager (your GameObject)]
├── Container_HUD (auto-created)
│   └── GameHUD (your component)
├── Container_Screen (auto-created)  
│   └── MainMenuScreen (your component)
└── Container_Panel (auto-created)
    └── SettingsPanel (your component)
```

## 📖 API Reference

### UIManager Methods

| Method |	Description |
|--------|--------------|
| ShowUI<T>() |	Shows a UI component of type T |
| HideUI<T>() |	Hides a UI component of type T |
| GoBack() |	Returns to previous screen |
| GetUI<T>() |	Gets reference to UI component |
| IsUIVisible<T>() |	Checks if UI is currently visible |

### UIComponent Properties

| Property |	Description |
|----------|--------------|
| Layer |	Which UI layer this belongs to |
| IsPersistent |	Whether UI stays visible during transitions |
| CanvasGroup |	Reference to CanvasGroup component |

## 🔧 Configuration

### Layer Behavior
| Layer |	Behavior |
|-------|----------|
| HUD |	Multiple can be active, often persistent |
| Screen |	Only one active at a time (auto-hides previous) |
| Panel |	Only one active at a time (unless persistent) |
| Overlay |	Multiple can be active |
| Debug |	Developer controlled |

### Setting Up Persistent UI

```csharp
public class PersistentHUD : UIHUD
{
    // In Inspector, check "Is Persistent"
    // This HUD will remain visible during screen transitions
}
```

## 💡 Usage Examples

**Basic Navigation Flow**

```csharp
public class NavigationExample : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    
    public void StartGame()
    {
        uiManager.ShowUI<GameScreen>();
        uiManager.ShowUI<GameHUD>();
        uiManager.HideUI<MainMenuScreen>();
    }
    
    public void PauseGame()
    {
        uiManager.ShowUI<PausePanel>();
    }
    
    public void ShowSettings()
    {
        uiManager.ShowUI<SettingsPanel>();
    }
    
    public void GoBackToMenu()
    {
        uiManager.ShowUI<MainMenuScreen>();
        uiManager.HideUI<GameScreen>();
        uiManager.HideUI<GameHUD>();
    }
}
```

**Event-Driven UI Updates**

```csharp
public class PlayerUI : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameHUD gameHUD;
    
    private void OnEnable()
    {
        // Listen for game events
        EventService.OnPlayerHealthChanged += UpdateHealth;
        EventService.OnScoreChanged += UpdateScore;
    }
    
    private void UpdateHealth(int newHealth)
    {
        // Update HUD directly if visible
        if (uiManager.IsUIVisible<GameHUD>())
        {
            gameHUD.UpdateHealthDisplay(newHealth);
        }
    }
}
```

**Advanced Navigation with History**

```csharp
public class ComplexNavigation : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    
    public void ShowStore()
    {
        // Store current screen in history
        uiManager.ShowUI<StoreScreen>();
    }
    
    public void ShowItemDetails()
    {
        uiManager.ShowUI<ItemDetailScreen>();
    }
    
    public void NavigateBack()
    {
        // Goes back through: ItemDetailScreen → StoreScreen → Previous
        uiManager.GoBack();
    }
}
```

## 🛡️ Best Practices

### 1. Proper Layer Usage

```csharp
// ✅ Correct layer assignment
public class HealthBar : UIHUD { }      // HUD layer - persistent gameplay
public class MainMenu : UIScreen { }    // Screen layer - full screen
public class Dialog : UIPanel { }       // Panel layer - modal popup
public class Tooltip : UIOverlay { }    // Overlay layer - temporary info
```

### 2. Memory Management

```csharp
public class UIWithEvents : UIScreen
{
    [SerializeField] private Button myButton;
    
    protected override void Awake()
    {
        base.Awake();
        myButton.onClick.AddListener(OnButtonClick);
    }
    
    protected override void OnDestroy()
    {
        myButton.onClick.RemoveListener(OnButtonClick);
        base.OnDestroy();
    }
}
```

### 3. State Management

```csharp
protected override void OnShow()
{
    // Enable input, start animations
    Time.timeScale = 0f; // Pause game for menus
}

protected override void OnHide()
{
    // Clean up, save state
    Time.timeScale = 1f; // Resume game
}
```

## 🎯 Common UI Patterns

**Main Menu Flow**

MainMenuScreen 
    → (Play) GameScreen + GameHUD
    → (Settings) SettingsPanel
    → (Quit) ConfirmationPanel
    
**In-Game Flow**

GameScreen + GameHUD
    → (Pause) PausePanel
    → (Inventory) InventoryPanel  
    → (Map) MapScreen
    
**Modal Workflow**

AnyScreen → ConfirmationPanel → ResultPanel

## 🤔 Why This System?

### Advantages Over Manual UI Management

| Feature |	This System |	Manual Management |
|---------|-------------|-------------------|
| Navigation Stack |	✅ Built-in back button |	❌ Manual implementation |
| Layer Management |	✅ Automatic sorting |	❌ Manual ordering |
| State Tracking |	✅ Complete visibility state |	❌ Manual bool tracking |
| Event System |	✅ Built-in UnityEvents |	❌ Custom events needed |
| Memory Safety |	✅ Automatic cleanup |	❌ Potential leaks |

### Perfect For

- Games with complex UI navigation
- Teams needing consistent UI architecture
- Projects requiring UI state persistence
- Games with multiple UI flows (menus, gameplay, pause)

## 🤝 Contributing

This system is part of my professional portfolio. Feel free to:

- Use in your personal or commercial projects
- Extend with animation support
- Add localization integration
- Adapt for your specific UI needs
