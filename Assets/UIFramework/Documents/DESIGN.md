# UIFramework - Technical Design Document
**Version:** 1.0
**Author:** System Architect
**Date:** 2025-11-22
**Status:** Implementation Ready

---

## Table of Contents

1. [Overview](#1-overview)
2. [System Architecture](#2-system-architecture)
3. [Core Components](#3-core-components)
4. [Data Flow & Communication](#4-data-flow--communication)
5. [API Reference](#5-api-reference)
6. [Implementation Details](#6-implementation-details)
7. [Performance & Optimization](#7-performance--optimization)
8. [Integration Guide](#8-integration-guide)
9. [Examples & Patterns](#9-examples--patterns)
10. [Appendix](#10-appendix)

---

## 1. Overview

### 1.1 Executive Summary

UIFramework is a production-ready UGUI system for Unity that implements clean architecture principles with a focus on:

| Priority | Description |
|----------|-------------|
| **Reusability** | Drop into any Unity project without modification |
| **Maintainability** | Clear separation of concerns, testable code |
| **Performance** | Optimized with pooling, async loading, minimal GC |
| **Extensibility** | Interface-based design, plugin architecture |
| **Developer Experience** | Intuitive API, editor tools, comprehensive examples |

### 1.2 Technology Stack

```
┌─────────────────────────────────────┐
│         Unity UGUI Layer            │
│  (Canvas, RectTransform, etc.)      │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      UIFramework (This System)      │
│  - MVVM Pattern                     │
│  - Reactive Binding                 │
│  - Navigation System                │
│  - Event Bus                        │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│       External Dependencies         │
│  - DOTween (Animation)              │
│  - Addressables (Asset Loading)     │
│  - TextMeshPro (Text Rendering)     │
└─────────────────────────────────────┘
```

### 1.3 Key Differentiators

| Feature | Traditional Approach | UIFramework Approach |
|---------|---------------------|----------------------|
| **Data Binding** | Manual UpdateUI() calls | Reactive properties with auto-update |
| **Navigation** | Direct GameObject manipulation | Stack + State machine hybrid |
| **Dependencies** | Static singletons, FindObject | Constructor injection with DI |
| **Async Operations** | Coroutines with callbacks | async/await with cancellation |
| **UI Lifecycle** | Manual Instantiate/Destroy | Pooling with automatic management |
| **Events** | Nested UnityEvents/Actions | Type-safe event bus |

### 1.4 Design Principles

```
┌──────────────────────────────────────────────────────┐
│                 SOLID Principles                     │
├──────────────────────────────────────────────────────┤
│ S - Single Responsibility                            │
│     Each class has one reason to change              │
│                                                       │
│ O - Open/Closed                                      │
│     Extend via interfaces, not modification          │
│                                                       │
│ L - Liskov Substitution                              │
│     Interfaces are substitutable                     │
│                                                       │
│ I - Interface Segregation                            │
│     Small, focused interfaces                        │
│                                                       │
│ D - Dependency Inversion                             │
│     Depend on abstractions, not concretions          │
└──────────────────────────────────────────────────────┘
```

---

## 2. System Architecture

### 2.1 High-Level Architecture

```
┌────────────────────────────────────────────────────────────────┐
│                        Application Layer                       │
│                                                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│  │   MainMenu   │  │   Gameplay   │  │   Settings   │        │
│  │   ViewModel  │  │   ViewModel  │  │   ViewModel  │        │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘        │
│         │                 │                  │                 │
│         │    ReactiveProperty Binding        │                 │
│         │                 │                  │                 │
│  ┌──────▼───────┐  ┌──────▼───────┐  ┌──────▼───────┐        │
│  │   MainMenu   │  │   Gameplay   │  │   Settings   │        │
│  │     View     │  │     View     │  │     View     │        │
│  └──────────────┘  └──────────────┘  └──────────────┘        │
│                                                                │
└────────────────────────────────────────────────────────────────┘
                             │
                             │ Uses
                             ▼
┌────────────────────────────────────────────────────────────────┐
│                     Framework Services                         │
│                                                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│  │  Navigation  │  │   Animation  │  │   Event Bus  │        │
│  │    System    │  │    System    │  │              │        │
│  └──────────────┘  └──────────────┘  └──────────────┘        │
│                                                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│  │   Pooling    │  │    Loading   │  │      DI      │        │
│  │    System    │  │    System    │  │  Container   │        │
│  └──────────────┘  └──────────────┘  └──────────────┘        │
│                                                                │
└────────────────────────────────────────────────────────────────┘
                             │
                             │ Built on
                             ▼
┌────────────────────────────────────────────────────────────────┐
│                      Core Foundation                           │
│                                                                │
│  • Interfaces (IUIView, IViewModel, IUILoader, etc.)          │
│  • Base Classes (ViewModelBase, UIView<T>, UIStateBase)       │
│  • Reactive Properties (ReactiveProperty<T>)                  │
│  • Utilities (CompositeDisposable, UnsubscribeHandle)         │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

### 2.2 Layer Responsibilities

#### **Presentation Layer (Views)**
- MonoBehaviour components attached to UI GameObjects
- Responsible ONLY for visual representation
- No business logic, only binding to ViewModel
- Access to Unity UI components (Text, Button, Image, etc.)

**Example:**
```csharp
// ✅ GOOD - View only handles visuals
protected override void BindViewModel()
{
    Bind(ViewModel.PlayerName, name => nameText.text = name);
    Bind(ViewModel.Score, score => scoreText.text = $"Score: {score}");
}

// ❌ BAD - Business logic in View
private void OnPlayButtonClicked()
{
    int score = CalculateScore(); // ❌ Logic in View
    SaveToDatabase(score);        // ❌ Data access in View
}
```

#### **Application Layer (ViewModels)**
- Contains UI-specific business logic
- Exposes ReactiveProperties for data binding
- Handles user commands via ReactiveCommand
- Interacts with domain services
- No references to Unity UI components

**Example:**
```csharp
// ✅ GOOD - ViewModel handles logic
public class GameViewModel : ViewModelBase
{
    private readonly IScoreService _scoreService;

    public ReactiveProperty<int> Score { get; } = new(0);
    public ReactiveCommand PlayCommand { get; }

    public GameViewModel(IScoreService scoreService)
    {
        _scoreService = scoreService;
        PlayCommand = new ReactiveCommand();
        PlayCommand.Subscribe(OnPlayClicked).AddTo(Disposables);
    }

    private void OnPlayClicked()
    {
        var score = _scoreService.CalculateScore();
        Score.Value = score;
    }
}
```

#### **Service Layer**
- Domain logic and data management
- No UI dependencies
- Fully testable without Unity
- Examples: GameService, PlayerService, SaveService

### 2.3 Data Flow Diagram

```
User Interaction Flow:
─────────────────────

   User clicks button
         │
         ▼
   ┌──────────┐
   │   View   │  (Captures UI event)
   └────┬─────┘
        │ Invokes
        ▼
   ┌──────────┐
   │ ViewModel│  (Executes command)
   └────┬─────┘
        │ Updates
        ▼
   ┌──────────┐
   │Reactive  │  (Notifies subscribers)
   │Property  │
   └────┬─────┘
        │ Triggers
        ▼
   ┌──────────┐
   │   View   │  (Updates UI automatically)
   └──────────┘


Navigation Flow:
────────────────

   ViewModel calls Navigator
         │
         ▼
   ┌────────────────┐
   │  UINavigator   │  (Facade)
   └────┬───────────┘
        │
        ├─────────────────────┬─────────────────────┐
        ▼                     ▼                     ▼
   ┌──────────┐        ┌──────────┐         ┌──────────┐
   │Navigation│        │   State  │         │Animation │
   │  Stack   │        │ Machine  │         │  System  │
   └────┬─────┘        └────┬─────┘         └────┬─────┘
        │                   │                     │
        └───────────────────┴─────────────────────┘
                            │
                            ▼
                    ┌──────────────┐
                    │ View Factory │
                    └──────┬───────┘
                           │
                    ┌──────┴────────┐
                    ▼               ▼
               ┌─────────┐    ┌─────────┐
               │  Loader │    │  Pool   │
               └─────────┘    └─────────┘
```

---

## 3. Core Components

### 3.1 Reactive Property System

#### **Overview**

The reactive property system enables automatic UI updates when data changes, forming the foundation of MVVM data binding.

#### **Architecture**

```
┌─────────────────────────────────────────────────┐
│         IReadOnlyReactiveProperty<T>            │
│  - T Value { get; }                             │
│  - IDisposable Subscribe(Action<T>)             │
└─────────────────┬───────────────────────────────┘
                  │
                  │ Implements
                  │
┌─────────────────▼───────────────────────────────┐
│           ReactiveProperty<T>                   │
│                                                 │
│  Private:                                       │
│  - T _value                                     │
│  - event Action<T> _onValueChanged              │
│                                                 │
│  Public:                                        │
│  + T Value { get; set; }                        │
│  + IDisposable Subscribe(Action<T>)             │
│  + void Dispose()                               │
│                                                 │
│  Features:                                      │
│  • Equality check before notification           │
│  • Type-safe generic implementation             │
│  • IDisposable for cleanup                      │
│  • Read-only interface for safety              │
└─────────────────────────────────────────────────┘
```

#### **Implementation Pattern**

```csharp
public class ReactiveProperty<T> : IReadOnlyReactiveProperty<T>, IDisposable
{
    private T _value;
    private event Action<T> _onValueChanged;

    public ReactiveProperty(T initialValue = default)
    {
        _value = initialValue;
    }

    public T Value
    {
        get => _value;
        set
        {
            // Only notify if value actually changed
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                _onValueChanged?.Invoke(_value);
            }
        }
    }

    public IDisposable Subscribe(Action<T> onValueChanged)
    {
        _onValueChanged += onValueChanged;
        return new UnsubscribeHandle(() => _onValueChanged -= onValueChanged);
    }

    public void Dispose()
    {
        _onValueChanged = null;
        _value = default;
    }
}
```

#### **Usage Examples**

```csharp
// Declaration in ViewModel
public class PlayerViewModel : ViewModelBase
{
    // Read-write property (internal use)
    private ReactiveProperty<int> _health = new(100);

    // Read-only exposure (prevents external modification)
    public IReadOnlyReactiveProperty<int> Health => _health;

    // Public read-write (when needed)
    public ReactiveProperty<string> PlayerName { get; } = new("Player");
}

// Binding in View
protected override void BindViewModel()
{
    Bind(ViewModel.Health, hp =>
    {
        healthText.text = $"HP: {hp}";
        healthBar.fillAmount = hp / 100f;
    });
}

// Using in ViewModel
public void TakeDamage(int damage)
{
    _health.Value -= damage; // Automatically updates all subscribers
}
```

---

### 3.2 MVVM Implementation

#### **ViewModel Lifecycle**

```
ViewModel Lifecycle Phases:
───────────────────────────

1. Construction
   │ - Dependencies injected via constructor
   │ - Initialize ReactiveProperties
   │ - Create ReactiveCommands
   │
   ▼
2. Initialize()
   │ - Subscribe to events
   │ - Load initial data
   │ - Setup state
   │
   ▼
3. OnViewShown()
   │ - Start animations
   │ - Resume updates
   │ - Re-enable interactions
   │
   ▼
4. Active State
   │ - Handle user input
   │ - Update reactive properties
   │ - Publish events
   │
   ▼
5. OnViewHidden()
   │ - Pause updates
   │ - Stop animations
   │ - Save state
   │
   ▼
6. Dispose()
   │ - Unsubscribe from events
   │ - Release resources
   │ - Cleanup references
   │
   ▼
7. Garbage Collected
```

#### **View Lifecycle**

```
View Lifecycle Phases:
──────────────────────

1. Instantiation
   │ - Prefab instantiated
   │ - MonoBehaviour Awake() called
   │
   ▼
2. Initialize(viewModel)
   │ - ViewModel assigned
   │ - ViewModel.Initialize() called
   │ - OnViewModelSet() called (optional override)
   │ - BindViewModel() called
   │
   ▼
3. Show()
   │ - GameObject.SetActive(true)
   │ - ViewModel.OnViewShown() called
   │ - Show animation played
   │
   ▼
4. Active State
   │ - UI responsive
   │ - Bindings update UI automatically
   │ - User interactions trigger commands
   │
   ▼
5. Hide()
   │ - ViewModel.OnViewHidden() called
   │ - Hide animation played
   │ - GameObject.SetActive(false) OR pooled
   │
   ▼
6. Destruction/Pool Return
   │ - ViewModel.Dispose() called
   │ - Bindings unsubscribed automatically
   │ - GameObject destroyed OR returned to pool
```

---

### 3.3 Navigation System

#### **Hybrid Architecture**

The navigation system combines two complementary patterns:

```
┌─────────────────────────────────────────────────────────┐
│                    UINavigator                          │
│                     (Facade)                            │
│                                                         │
│  Provides unified API for both navigation patterns     │
└───────────┬─────────────────────────┬───────────────────┘
            │                         │
            ▼                         ▼
┌───────────────────────┐   ┌────────────────────────┐
│   NavigationStack     │   │    UIStateMachine      │
│                       │   │                        │
│  Purpose:             │   │  Purpose:              │
│  Hierarchical nav     │   │  Distinct UI modes     │
│                       │   │                        │
│  Use Cases:           │   │  Use Cases:            │
│  • Menus/Submenus     │   │  • Menu State          │
│  • Settings pages     │   │  • Gameplay State      │
│  • Wizards/flows      │   │  • Pause State         │
│  • Modal dialogs      │   │  • Loading State       │
│                       │   │                        │
│  Operations:          │   │  Operations:           │
│  • Push               │   │  • TransitionTo        │
│  • Pop                │   │  • RegisterState       │
│  • PopToRoot          │   │  • CurrentState        │
│  • Peek               │   │                        │
└───────────────────────┘   └────────────────────────┘
```

#### **State Machine Design**

```
State Machine Visualization:
────────────────────────────

    ┌─────────┐   ChangeState("Gameplay")   ┌──────────┐
    │  Menu   │ ──────────────────────────> │ Gameplay │
    │  State  │ <────────────────────────── │  State   │
    └────┬────┘   ChangeState("Menu")       └────┬─────┘
         │                                       │
         │ ChangeState("Settings")               │
         │                                       │ Pause()
         ▼                                       ▼
    ┌─────────┐                             ┌──────────┐
    │Settings │                             │  Pause   │
    │  State  │                             │  State   │
    └─────────┘                             └──────────┘


State Lifecycle:
────────────────

1. Current State
   │
   │ TransitionTo("NewState")
   │
   ▼
2. OnExitAsync() ────> Current state cleanup
   │
   ▼
3. Switch Reference ──> _currentState = newState
   │
   ▼
4. OnEnterAsync() ───> New state initialization
   │
   ▼
5. OnUpdate() ───────> Per-frame updates (optional)
```

---

### 3.4 Animation System (DOTween Integration)

#### **Architecture**

```
┌──────────────────────────────────────────────────────┐
│                  IUIAnimator                         │
│  + Task AnimateAsync(UITransition, CancellationToken)│
└─────────────────────┬────────────────────────────────┘
                      │
                      │ Implements
                      │
┌─────────────────────▼────────────────────────────────┐
│              DOTweenUIAnimator                       │
│  - Converts UITransition to DOTween Tween            │
│  - Handles async completion                          │
│  - Supports cancellation                             │
└──────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────┐
│                UITransition (Abstract)               │
│  + float Duration                                    │
│  + Ease EaseType                                     │
│  + abstract Tween CreateTween(Transform)             │
└─────────────────────┬────────────────────────────────┘
                      │
        ┌─────────────┼─────────────┬──────────────┐
        │             │             │              │
        ▼             ▼             ▼              ▼
┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
│   Fade   │  │  Slide   │  │  Scale   │  │ Sequence │
│Transition│  │Transition│  │Transition│  │Transition│
└──────────┘  └──────────┘  └──────────┘  └──────────┘
```

#### **Built-in Transitions**

- **FadeTransition** - Fade in/out using CanvasGroup alpha
- **SlideTransition** - Slide from any direction (Left, Right, Up, Down)
- **ScaleTransition** - Scale with optional punch effect
- **SequenceTransition** - Composite multiple transitions (sequential or parallel)

---

## 4. Data Flow & Communication

### 4.1 Event Bus System

#### **Why Event Bus?**

**Problem with Nested Callbacks:**
```csharp
// ❌ BAD - Callback hell
button.onClick.AddListener(() => {
    ShowLoadingPopup(() => {
        LoadPlayerData(data => {
            UpdateUI(() => {
                ShowSuccessMessage(() => {
                    // Deep nesting, memory leaks, hard to debug
                });
            });
        });
    });
});
```

**Solution with Event Bus:**
```csharp
// ✅ GOOD - Flat, decoupled
public class UIController : ViewModelBase
{
    public override void Initialize()
    {
        _eventBus.Subscribe<ButtonClickedEvent>(OnButtonClicked)
            .AddTo(Disposables);
        _eventBus.Subscribe<DataLoadedEvent>(OnDataLoaded)
            .AddTo(Disposables);
        _eventBus.Subscribe<UIUpdatedEvent>(OnUIUpdated)
            .AddTo(Disposables);
    }

    private void OnButtonClicked(ButtonClickedEvent evt)
    {
        _eventBus.Publish(new LoadDataRequestEvent());
    }
}
```

#### **Event Flow**

```
    Publisher                                   Subscribers
        │                                            │
        │ Publish(new ScoreChangedEvent())          │
        │                                            │
        ▼                                            ▼
┌──────────────┐                          ┌─────────────────┐
│  UIEventBus  │ ────────────────────────>│  HUDViewModel   │
│              │ ────────────────────────>│  StatsViewModel │
│              │ ────────────────────────>│  LeaderboardVM  │
└──────────────┘                          └─────────────────┘
```

---

## 5. API Reference

### 5.1 Quick Start

```csharp
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 1. Bootstrap (Run once at game start)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private UIFrameworkConfig config;

    private async void Start()
    {
        UIServiceInstaller.Install(config);

        var navigator = ServiceLocator.Get<UINavigator>();

        navigator.RegisterState(new MenuUIState());
        navigator.RegisterState(new GameplayUIState());

        await navigator.ChangeStateAsync("Menu");
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 2. Create a ViewModel
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class MyViewModel : ViewModelBase
{
    public ReactiveProperty<string> Title { get; } = new("Hello");
    public ReactiveCommand ClickCommand { get; }

    private readonly UINavigator _navigator;

    public MyViewModel(UINavigator navigator)
    {
        _navigator = navigator;

        ClickCommand = new ReactiveCommand();
        ClickCommand.Subscribe(OnClick).AddTo(Disposables);
    }

    private void OnClick()
    {
        Title.Value = "Clicked!";
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 3. Create a View
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class MyView : UIView<MyViewModel>
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button clickButton;

    protected override void BindViewModel()
    {
        Bind(ViewModel.Title, title => titleText.text = title);
        Bind(ViewModel.ClickCommand, clickButton);
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 4. Navigate to View
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

var navigator = ServiceLocator.Get<UINavigator>();
await navigator.PushAsync<MyView, MyViewModel>();
```

### 5.2 Common Patterns

```csharp
// ────── Pattern: Parameterized Navigation ──────
var viewModel = new PlayerViewModel(playerData);
await navigator.PushAsync<PlayerView, PlayerViewModel>(viewModel);

// ────── Pattern: Animated Navigation ──────
var slideTransition = new SlideTransition { Direction = SlideDirection.Left };
await navigator.PushAsync<SettingsView, SettingsViewModel>(
    transition: slideTransition);

// ────── Pattern: Event Publishing from ViewModel ──────
public class GameViewModel : ViewModelBase
{
    private readonly UIEventBus _eventBus;

    public void OnGameOver()
    {
        _eventBus.Publish(new GameOverEvent
        {
            FinalScore = _score.Value
        });
    }
}

// ────── Pattern: Pooled UI ──────
var pool = ServiceLocator.Get<IUIObjectPool>();

// Get from pool
var damageNumber = await pool.GetAsync<DamageNumberUI>();
damageNumber.Show(damage);

// Return to pool when done
await Task.Delay(2000);
pool.Return(damageNumber);

// ────── Pattern: Cancellation Support ──────
private CancellationTokenSource _cts;

private async void LoadData()
{
    _cts = new CancellationTokenSource();

    try
    {
        var data = await _dataService.LoadAsync(_cts.Token);
        ProcessData(data);
    }
    catch (OperationCanceledException)
    {
        Debug.Log("Load cancelled");
    }
}

private void OnDisable()
{
    _cts?.Cancel();
    _cts?.Dispose();
}
```

---

## 6. Implementation Details

### 6.1 Dependency Injection

```csharp
// Service Lifetimes
public enum ServiceLifetime
{
    Transient,  // New instance every time
    Singleton,  // Single instance for app
    Scoped      // One instance per scope (e.g., per scene)
}

// Service Registration
public class UIServiceInstaller
{
    public static void Install(UIFrameworkConfig config)
    {
        var container = new DIContainer();

        container.RegisterSingleton<IDIContainer>(container);
        container.RegisterSingleton(new UIEventBus());
        container.Register<IUIViewFactory, UIViewFactory>(ServiceLifetime.Singleton);
        container.Register<IUIAnimator, DOTweenUIAnimator>(ServiceLifetime.Singleton);
        container.Register<INavigationStack, NavigationStack>(ServiceLifetime.Singleton);
        container.RegisterSingleton(new UIStateMachine());
        container.Register<UINavigator, UINavigator>(ServiceLifetime.Singleton);

        if (config.enablePooling)
            container.Register<IUIObjectPool, UIObjectPool>(ServiceLifetime.Singleton);

        if (config.useAddressables)
            container.Register<IUILoader, AddressablesUILoader>(ServiceLifetime.Singleton);
        else
            container.Register<IUILoader, ResourcesUILoader>(ServiceLifetime.Singleton);

        ServiceLocator.Initialize(container);
    }
}

// Constructor Injection (recommended)
public class MyViewModel : ViewModelBase
{
    private readonly UINavigator _navigator;
    private readonly IDataService _dataService;

    public MyViewModel(UINavigator navigator, IDataService dataService)
    {
        _navigator = navigator;
        _dataService = dataService;
    }
}

// Service Locator (for non-DI contexts)
public class LegacyCode : MonoBehaviour
{
    private void Start()
    {
        var navigator = ServiceLocator.Get<UINavigator>();
        var eventBus = ServiceLocator.Get<UIEventBus>();
    }
}
```

### 6.2 Object Pooling

```csharp
// Poolable Interface
public interface IPoolable
{
    void OnSpawnedFromPool();
    void OnReturnedToPool();
}

// Example Implementation
public class DamageNumberUI : MonoBehaviour, IPoolable
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup canvasGroup;

    public void OnSpawnedFromPool()
    {
        transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }

    public void OnReturnedToPool()
    {
        DOTween.Kill(transform);
        gameObject.SetActive(false);
    }
}

// Using Pool
var pool = ServiceLocator.Get<IUIObjectPool>();

// Get from pool
var damageUI = await pool.GetAsync<DamageNumberUI>();

// Return to pool
pool.Return(damageUI);

// Warmup (pre-instantiate)
pool.Warmup<DamageNumberUI>(20);
```

---

## 7. Performance & Optimization

### 7.1 Memory Management Best Practices

```
✅ Always dispose ViewModels
✅ Use CompositeDisposable for subscriptions
✅ Pool frequently instantiated UI
✅ Unload Addressables when not needed
✅ Limit navigation stack depth
✅ Cache inactive views (configurable)
✅ Use read-only reactive properties
✅ Pass CancellationTokens to async methods
✅ Don't subscribe to events without cleanup
✅ Avoid closures capturing large objects
```

### 7.2 Performance Targets

```
Target Performance:
───────────────────

Navigation:
• Push/Pop: < 16ms (60 FPS)
• State transition: < 33ms (30 FPS)
• Pooled instantiation: < 1ms

Memory:
• GC allocation per navigation: < 10KB
• Pooled UI reuse: 0 allocation
• Event publishing: < 100 bytes

Loading:
• Addressables UI load: < 100ms
• Resources UI load: < 50ms
```

---

## 8. Integration Guide

### 8.1 Adding to New Project

```
Step-by-Step:
─────────────

1. Import UIFramework folder to Assets/

2. Install dependencies:
   • DOTween (Asset Store or Package Manager)
   • Addressables (Package Manager)
   • TextMeshPro (Package Manager)

3. Create configuration:
   Right-click → Create → UI Framework → Framework Config

4. Create bootstrap scene:
   • Create new scene "Bootstrap"
   • Add empty GameObject "UIBootstrap"
   • Add UIServiceInstaller component
   • Assign config

5. Create first view:
   Use Editor → UI Framework → Create View/ViewModel

6. Done! Start using the framework
```

### 8.2 Migration from Existing UI

```
Migration Strategy:
───────────────────

Phase 1: Setup (Week 1)
• Install framework
• Setup bootstrap
• Configure settings

Phase 2: New Features (Week 2-3)
• Build new UI with framework
• Test integration
• Gather feedback

Phase 3: Migration (Week 4-8)
• Convert one screen at a time
• Maintain backward compatibility
• Refactor incrementally

Phase 4: Cleanup (Week 9-10)
• Remove old systems
• Optimize performance
• Final polish
```

---

## 9. Examples & Patterns

### 9.1 Complete Example: Shop System

```csharp
// ViewModel
public class ShopViewModel : ViewModelBase
{
    private readonly IShopService _shopService;
    private readonly UIEventBus _eventBus;

    public ReactiveProperty<int> Gold { get; } = new(0);
    public ReactiveProperty<string> SelectedItemName { get; } = new("");
    public ReactiveProperty<int> SelectedItemPrice { get; } = new(0);
    public ReactiveProperty<bool> CanAffordItem { get; } = new(false);

    public ReactiveCommand<string> SelectItemCommand { get; }
    public ReactiveCommand PurchaseCommand { get; }

    public ShopViewModel(IShopService shopService, UIEventBus eventBus)
    {
        _shopService = shopService;
        _eventBus = eventBus;

        SelectItemCommand = new ReactiveCommand<string>();
        SelectItemCommand.Subscribe(OnItemSelected).AddTo(Disposables);

        PurchaseCommand = new ReactiveCommand();
        PurchaseCommand.Subscribe(OnPurchaseClicked).AddTo(Disposables);
    }

    public override void Initialize()
    {
        _eventBus.Subscribe<GoldChangedEvent>(OnGoldChanged)
            .AddTo(Disposables);

        Gold.Value = _shopService.GetPlayerGold();
    }

    private void OnItemSelected(string itemId)
    {
        var item = _shopService.GetItem(itemId);
        SelectedItemName.Value = item.Name;
        SelectedItemPrice.Value = item.Price;
        CanAffordItem.Value = Gold.Value >= item.Price;
    }

    private void OnPurchaseClicked()
    {
        if (!CanAffordItem.Value) return;

        _shopService.PurchaseItem(SelectedItemName.Value);
        _eventBus.Publish(new ItemPurchasedEvent
        {
            ItemName = SelectedItemName.Value
        });
    }

    private void OnGoldChanged(GoldChangedEvent evt)
    {
        Gold.Value = evt.NewAmount;
        CanAffordItem.Value = Gold.Value >= SelectedItemPrice.Value;
    }
}

// View
public class ShopView : UIView<ShopViewModel>
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text selectedItemNameText;
    [SerializeField] private TMP_Text selectedItemPriceText;
    [SerializeField] private Button purchaseButton;

    protected override void BindViewModel()
    {
        Bind(ViewModel.Gold, gold => goldText.text = $"Gold: {gold}");
        Bind(ViewModel.SelectedItemName, name => selectedItemNameText.text = name);
        Bind(ViewModel.SelectedItemPrice, price => selectedItemPriceText.text = $"${price}");
        Bind(ViewModel.CanAffordItem, canAfford => purchaseButton.interactable = canAfford);

        Bind(ViewModel.PurchaseCommand, purchaseButton);
    }
}
```

---

## 10. Appendix

### 10.1 Folder Structure

```
Assets/UIFramework/
├── Core/                   # Foundation
│   ├── Interfaces/
│   ├── Base/
│   └── Attributes/
├── MVVM/                   # Data binding
│   ├── Binding/
│   ├── ViewModels/
│   └── Views/
├── Navigation/             # Screen management
│   ├── Stack/
│   ├── States/
│   └── UINavigator.cs
├── Animation/              # Transitions
│   ├── Transitions/
│   └── Presets/
├── Pooling/                # Performance
├── Loading/                # Asset loading
├── Events/                 # Communication
├── DependencyInjection/    # Service management
├── Configuration/          # Settings
├── Editor/                 # Tools
└── Examples/               # Reference implementations
```

### 10.2 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| DOTween | 1.2.x+ | Animation |
| Addressables | 1.21.x+ | Asset loading |
| TextMeshPro | 3.x+ | Text rendering |
| Unity | 2021.3+ | Engine |

### 10.3 Best Practices Summary

#### DO's ✅

- Use ReactiveProperties for all bindable data
- Dispose subscriptions in ViewModel.Dispose()
- Use async/await for asynchronous operations
- Pass CancellationTokens to all async methods
- Use events for cross-component communication
- Pool frequently instantiated UI elements
- Use TransitionPresets instead of hardcoded animations
- Constructor inject dependencies
- Keep Views thin - logic in ViewModels
- Use CompositeDisposable for automatic cleanup

#### DON'Ts ❌

- Don't access Views from ViewModels
- Don't use nested action callbacks
- Don't hardcode animation values
- Don't forget to dispose subscriptions
- Don't use GameObject.Find() or similar
- Don't put business logic in Views
- Don't create memory leaks with event handlers
- Don't use static singletons for services
- Don't ignore CancellationTokens
- Don't manually destroy pooled objects

---

**End of Design Document**
