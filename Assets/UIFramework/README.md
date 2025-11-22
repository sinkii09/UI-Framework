# UIFramework

**A production-ready MVVM UI framework for Unity with clean architecture, reactive data binding, and professional patterns.**

## Features

- **MVVM Architecture** - Clean separation of concerns with Model-View-ViewModel pattern
- **Reactive Data Binding** - Automatic UI updates with ReactiveProperty<T> and ReactiveCommand
- **Hybrid Navigation** - Combines stack-based navigation (push/pop) with state machine for complex UI flows
- **Dependency Injection** - Built on VContainer for constructor injection and testability
- **DOTween Integration** - Smooth animations with abstraction layer for easy customization
- **Object Pooling** - Unity's ObjectPool for performance optimization
- **Async Loading** - Addressables and Resources support with async/await patterns
- **Event Bus** - Type-safe pub/sub messaging without nested callbacks
- **Editor Tools** - Wizards and inspectors to streamline development workflow
- **Zero Hardcoding** - Everything configurable via ScriptableObjects

## Requirements

- **Unity 2021.3 or later**
- **DOTween** (via Package Manager or Asset Store)
- **VContainer** (via Package Manager)
- **TextMeshPro** (usually pre-installed)
- **Addressables** (optional, for async loading)

## Quick Start

### 1. Installation

1. Install dependencies via Package Manager:
   - VContainer: `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.13.2`
   - DOTween: Import from Asset Store or use `com.demigiant.dotween`
   - TextMeshPro: Window > TextMeshPro > Import TMP Essential Resources

2. Import UIFramework into your project

3. Run the Project Setup Wizard:
   - Menu: **UIFramework > Setup > Project Setup Wizard**
   - Follow the guided steps to configure your project

### 2. Basic Scene Setup

Create a new scene with these components:

```
Hierarchy:
  - Canvas (Canvas, CanvasScaler, GraphicRaycaster)
  - EventSystem (EventSystem, InputModule)
  - UIFramework (UIFrameworkInstaller)
```

Or use the wizard: **UIFramework > Setup > Project Setup Wizard**

### 3. Create Your First View

#### Step 1: Create View and ViewModel Scripts

Use the wizard: **UIFramework > Create View & ViewModel**

Or manually create:

**MainMenuViewModel.cs**
```csharp
using UIFramework.Core;
using UIFramework.MVVM;
using UIFramework.Navigation;

public class MainMenuViewModel : ViewModelBase
{
    private readonly UINavigator _navigator;

    // Reactive Properties
    public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>("Main Menu");
    public ReactiveProperty<int> PlayerLevel { get; } = new ReactiveProperty<int>(1);

    // Commands
    public ReactiveCommand PlayCommand { get; }
    public ReactiveCommand SettingsCommand { get; }
    public ReactiveCommand QuitCommand { get; }

    public MainMenuViewModel(UINavigator navigator)
    {
        _navigator = navigator;

        // Initialize commands
        PlayCommand = new ReactiveCommand();
        PlayCommand.Subscribe(OnPlayClicked).AddTo(Disposables);

        SettingsCommand = new ReactiveCommand();
        SettingsCommand.Subscribe(OnSettingsClicked).AddTo(Disposables);

        QuitCommand = new ReactiveCommand();
        QuitCommand.Subscribe(OnQuitClicked).AddTo(Disposables);
    }

    private void OnPlayClicked()
    {
        UnityEngine.Debug.Log("Play clicked!");
        // Navigate to gameplay
    }

    private void OnSettingsClicked()
    {
        UnityEngine.Debug.Log("Settings clicked!");
        // Push settings view
    }

    private void OnQuitClicked()
    {
        UnityEngine.Debug.Log("Quit clicked!");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
```

**MainMenuView.cs**
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework.Core;

public class MainMenuView : UIView<MainMenuViewModel>
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    protected override void BindViewModel()
    {
        // Bind properties to UI
        Bind(ViewModel.Title, title => titleText.text = title);
        Bind(ViewModel.PlayerLevel, level => levelText.text = $"Level {level}");

        // Bind commands to buttons
        Bind(ViewModel.PlayCommand, playButton);
        Bind(ViewModel.SettingsCommand, settingsButton);
        Bind(ViewModel.QuitCommand, quitButton);
    }
}
```

#### Step 2: Create the UI Prefab

1. Create a UI prefab: **UIFramework > Create > UI Prefab Template**
2. Or manually:
   - Create a full-screen panel in the Canvas
   - Add **MainMenuView** component
   - Assign UI references in the inspector
   - Save as prefab in `Resources/UI/MainMenuView.prefab`

#### Step 3: Show the View

In your game bootstrap script:

```csharp
using UnityEngine;
using UIFramework.Navigation;
using UIFramework.DependencyInjection;

public class GameBootstrap : MonoBehaviour
{
    private UINavigator _navigator;

    private async void Start()
    {
        await System.Threading.Tasks.Task.Yield();

        // Get navigator from DI container
        _navigator = ServiceLocator.Get<UINavigator>();

        // Show main menu
        await _navigator.PushAsync<MainMenuView, MainMenuViewModel>();
    }

    private void Update()
    {
        _navigator?.Update();
    }
}
```

## Core Concepts

### Reactive Properties

ReactiveProperty<T> automatically notifies subscribers when the value changes:

```csharp
// In ViewModel
public ReactiveProperty<int> Health { get; } = new ReactiveProperty<int>(100);

// In View
Bind(ViewModel.Health, health =>
{
    healthText.text = $"HP: {health}";
    healthBar.value = health / 100f;
});

// Updating the value automatically updates all bound UI
ViewModel.Health.Value = 75; // UI updates automatically
```

### Commands

ReactiveCommand provides a clean way to handle button clicks:

```csharp
// In ViewModel
public ReactiveCommand AttackCommand { get; }

public MyViewModel()
{
    AttackCommand = new ReactiveCommand();
    AttackCommand.Subscribe(OnAttackClicked).AddTo(Disposables);
}

private void OnAttackClicked()
{
    Debug.Log("Attack!");
}

// In View
Bind(ViewModel.AttackCommand, attackButton);
```

### Navigation

#### Stack-based Navigation (Hierarchical)

```csharp
// Push a view onto the stack
await _navigator.PushAsync<SettingsView, SettingsViewModel>();

// Pop the current view
await _navigator.PopAsync();

// Clear the entire stack
_navigator.ClearStack();
```

#### State-based Navigation (Distinct Modes)

```csharp
// Register states
_navigator.RegisterState(new MenuUIState(_navigator));
_navigator.RegisterState(new GameplayUIState(_navigator));
_navigator.RegisterState(new PauseUIState(_navigator));

// Change state
await _navigator.ChangeStateAsync("Gameplay");
```

### Event Bus

Decouple components with type-safe events:

```csharp
// Define an event
public class PlayerDiedEvent : IUIEvent
{
    public int FinalScore { get; set; }
}

// Subscribe to event
_eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied)
    .AddTo(Disposables);

private void OnPlayerDied(PlayerDiedEvent evt)
{
    Debug.Log($"Player died! Score: {evt.FinalScore}");
}

// Publish event
_eventBus.Publish(new PlayerDiedEvent { FinalScore = 1000 });
```

### Dependency Injection

Use constructor injection for testable, decoupled code:

```csharp
public class MyViewModel : ViewModelBase
{
    private readonly UINavigator _navigator;
    private readonly UIEventBus _eventBus;
    private readonly MyGameService _gameService;

    // VContainer automatically injects dependencies
    public MyViewModel(
        UINavigator navigator,
        UIEventBus eventBus,
        MyGameService gameService)
    {
        _navigator = navigator;
        _eventBus = eventBus;
        _gameService = gameService;
    }
}
```

## Configuration

Configure UIFramework via the ScriptableObject:

**Menu: UIFramework > Configuration > Open Config**

### Key Settings

- **UseAddressables**: Enable for async loading with Addressables
- **DefaultTransitionDuration**: Animation speed (default: 0.3s)
- **MaxNavigationStackDepth**: Prevent infinite push loops (default: 10)
- **EnablePooling**: Use object pooling for performance (default: true)
- **EnableViewCaching**: Cache inactive views (default: true)
- **VerboseLogging**: Enable detailed logs for debugging

## Advanced Topics

### Custom Transitions

Create custom animations:

```csharp
public class BounceTransition : UITransition
{
    protected override Tween CreateTween(Transform target)
    {
        return target.DOScale(Vector3.one * 1.2f, Duration / 2)
            .SetEase(Ease.OutBack)
            .OnComplete(() => target.DOScale(Vector3.one, Duration / 2));
    }
}
```

### UI States

Create complex state machines:

```csharp
public class GameplayUIState : UIStateBase
{
    public override string StateId => "Gameplay";

    public override async Task OnEnterAsync(CancellationToken ct = default)
    {
        // Show HUD
        await _navigator.PushAsync<HUDView, HUDViewModel>(cancellationToken: ct);
    }

    public override async Task OnExitAsync(CancellationToken ct = default)
    {
        // Clean up
        _navigator.ClearStack();
    }

    public override void OnUpdate()
    {
        // Per-frame updates
    }
}
```

### Parameterized ViewModels

Pass data to ViewModels:

```csharp
public class PopupViewModel : ViewModelBase
{
    public void Setup(string title, string message, Action<bool> onClosed)
    {
        Title.Value = title;
        Message.Value = message;
        _onClosed = onClosed;
    }
}

// Usage
var popup = await _navigator.PushAsync<PopupView, PopupViewModel>();
popup.ViewModel.Setup("Confirm", "Are you sure?", result =>
{
    if (result) Debug.Log("Confirmed");
});
```

## Examples

Check the **Examples** folder for complete implementations:

- **MainMenuView/ViewModel** - Full-featured menu with navigation
- **PopupView/ViewModel** - Modal dialog with callbacks
- **HUDView/ViewModel** - Real-time HUD with event subscriptions
- **GameBootstrap** - Complete initialization example with states

## Editor Tools

### Wizards

- **UIFramework > Create View & ViewModel** - Generate matching View/ViewModel pairs
- **UIFramework > Create UI State** - Create state machine states
- **UIFramework > Create > UI Prefab Template** - Generate structured UI prefabs
- **UIFramework > Setup > Project Setup Wizard** - First-time project configuration

### Menu Items

- **UIFramework > Configuration > Open Config** - Access framework settings
- **UIFramework > Setup > Create UI Folders** - Generate recommended folder structure
- **UIFramework > Tools > Validate Scene Setup** - Check scene configuration
- **UIFramework > Documentation > Open Design Document** - View architecture details

## Troubleshooting

### Views Not Loading

1. Check that prefabs are in `Resources/UI/` folder (or tagged with "UI" label for Addressables)
2. Verify UIFrameworkInstaller is in the scene with config assigned
3. Check console for specific error messages

### Bindings Not Updating

1. Ensure `BindViewModel()` is implemented and calls `Bind()` methods
2. Verify ViewModel is initialized before View
3. Check that PropertyBinder component exists on View GameObject

### Navigation Errors

1. Ensure `_navigator.Update()` is called in MonoBehaviour Update()
2. Check MaxNavigationStackDepth in config
3. Verify state IDs are unique and registered

### Performance Issues

1. Enable pooling in UIFrameworkConfig
2. Enable view caching for frequently shown views
3. Use Addressables for large UI assets
4. Reduce DefaultTransitionDuration for faster animations

## Best Practices

### ViewModels

- Keep ViewModels testable - no MonoBehaviour dependencies
- Use ReactiveProperty for all data that updates the UI
- Always call `.AddTo(Disposables)` for subscriptions
- Constructor injection for dependencies

### Views

- Views should be dumb - no business logic
- Only call ViewModel properties/commands, never modify them directly
- Use `Bind()` helper methods for automatic cleanup
- Keep BindViewModel() focused on bindings only

### Navigation

- Use Stack for hierarchical flows (Settings > Graphics > Advanced)
- Use States for distinct app modes (Menu, Gameplay, Pause)
- Always handle cancellation tokens for async operations
- Clear stacks when exiting states

### Performance

- Enable pooling for frequently shown views
- Use Addressables for large assets
- Implement IPoolable on views that need custom reset logic
- Profile with Unity Profiler to identify bottlenecks

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                     View (MonoBehaviour)                 │
│  - UI References                                         │
│  - BindViewModel()                                       │
│  - Show/Hide                                             │
└────────────────────┬────────────────────────────────────┘
                     │ binds to
                     ▼
┌─────────────────────────────────────────────────────────┐
│                  ViewModel (Pure C#)                     │
│  - ReactiveProperty<T>                                   │
│  - ReactiveCommand                                       │
│  - Business Logic                                        │
└────────────────────┬────────────────────────────────────┘
                     │ uses
                     ▼
┌─────────────────────────────────────────────────────────┐
│                  Services (DI Container)                 │
│  - UINavigator                                           │
│  - UIEventBus                                            │
│  - Game Services                                         │
└─────────────────────────────────────────────────────────┘
```

## API Reference

See **DESIGN.md** for detailed architecture documentation.

### Core Classes

- `UIView<TViewModel>` - Base class for all views
- `ViewModelBase` - Base class for all view models
- `ReactiveProperty<T>` - Observable property
- `ReactiveCommand` - Command for UI interactions
- `UINavigator` - Facade for navigation (Stack + State Machine)
- `UIEventBus` - Type-safe event pub/sub system

### Interfaces

- `IViewModel` - ViewModel contract
- `IUIView` - View contract
- `IUIState` - State machine state contract
- `IUIEvent` - Event marker interface
- `IUILoader` - Asset loading abstraction
- `IPoolable` - Poolable object lifecycle

## Contributing

For bug reports and feature requests, please contact the framework maintainer.

## License

Proprietary - For internal project use only.

---

**Need Help?** Check DESIGN.md for architecture details or run the Project Setup Wizard to get started.
