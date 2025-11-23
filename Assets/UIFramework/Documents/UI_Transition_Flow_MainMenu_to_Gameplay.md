# UI Transition Flow: MainMenu → Gameplay State

## Complete Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│ USER CLICKS "PLAY" BUTTON                                           │
└────────────────────────┬────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 1. MainMenuView.cs:53 - PlayButton onClick triggers                 │
│    └─> MainMenuViewModel.PlayCommand.Subscribe()                    │
└────────────────────────┬────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 2. MainMenuViewModel.cs:72-84 - OnPlayClickedAsync()                │
│    ├─> Logs: "[MainMenuViewModel] Play button clicked!"             │
│    ├─> Publishes event: GameStartRequestedEvent                     │
│    └─> await _navigator.ChangeStateAsync("Gameplay")                │
└────────────────────────┬────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 3. UINavigator.cs:76-79 - ChangeStateAsync("Gameplay")              │
│    └─> Calls: _stateMachine.TransitionToAsync("Gameplay")           │
└────────────────────────┬────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 4. UIStateMachine.cs:72-123 - TransitionToAsync()                   │
│    ├─> Logs: "[UIStateMachine] Transitioning: Menu -> Gameplay"     │
│    │                                                                 │
│    ├─> STEP 4A: EXIT MENU STATE                                     │
│    │   └─> await currentState.OnExitAsync()                         │
│    │                                                                 │
│    ├─> _currentState = new state (Gameplay)                         │
│    │                                                                 │
│    └─> STEP 4B: ENTER GAMEPLAY STATE                                │
│        └─> await newState.OnEnterAsync()                            │
└────────────────────────┬────────────────────────────────────────────┘
                         │
        ┌────────────────┴────────────────┐
        │                                  │
        ▼                                  ▼
┌──────────────────────┐        ┌──────────────────────┐
│ EXIT MENU STATE      │        │ ENTER GAMEPLAY STATE │
│ (STEP 4A)            │        │ (STEP 4B)            │
└──────┬───────────────┘        └──────┬───────────────┘
       │                               │
       ▼                               ▼

┌─────────────────────────────────────────────────────────────────────┐
│ STEP 4A DETAILS: MenuUIState.OnExitAsync()                          │
│ (GameBootstrap.cs:121-127)                                          │
│                                                                      │
│ ├─> Logs: "[MenuUIState] Exiting menu state..."                     │
│ └─> _navigator.ClearStack()                                         │
│     └─> NavigationStack.cs:148-160 - Clear()                        │
│         ├─> Logs: "[NavigationStack] Clearing stack. Depth: 1"      │
│         ├─> Pops MainMenuView from stack                            │
│         ├─> Calls: MainMenuView.Hide() (NOT AWAITED - sync!)       │
│         │   └─> MainMenuView.cs:102-106                             │
│         │       ├─> Logs: "[MainMenuView] Hiding main menu"         │
│         │       ├─> ViewModel.OnViewHidden()                        │
│         │       ├─> UIView.Hide() (UIView.cs:88-122)                │
│         │       │   ├─> canvasGroup.interactable = false            │
│         │       │   ├─> canvasGroup.blocksRaycasts = false          │
│         │       │   ├─> GetHideTransition() returns FadeTransition  │
│         │       │   │   - FromAlpha: 1f, ToAlpha: 0f                │
│         │       │   │   - Duration: 0.2s, Ease: InCubic             │
│         │       │   ├─> await animator.AnimateAsync(transition)     │
│         │       │   │   (DOTween fade 0.2 seconds)                  │
│         │       │   ├─> canvasGroup.alpha = 0                       │
│         │       │   └─> gameObject.SetActive(false)                 │
│         │       └─> async Task completes                            │
│         ├─> Calls: MainMenuView.Destroy()                           │
│         │   └─> ViewModel.Dispose()                                 │
│         │   └─> UnityEngine.Object.Destroy(gameObject)              │
│         └─> Logs: "[NavigationStack] Stack cleared."                │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│ STEP 4B DETAILS: GameplayUIState.OnEnterAsync()                     │
│ (GameBootstrap.cs:144-156)                                          │
│                                                                      │
│ ├─> Logs: "[GameplayUIState] Entering gameplay state..."            │
│ └─> await _navigator.PushAsync<HUDView, HUDViewModel>()             │
│     └─> NavigationStack.cs:34-68 - PushAsync()                      │
│         ├─> Logs: "[NavigationStack] Pushing: HUDView"              │
│         │                                                            │
│         ├─> STEP 1: Hide current view (stack is empty, skipped)     │
│         │                                                            │
│         ├─> STEP 2: Create new view                                 │
│         │   └─> await _viewFactory.CreateAsync<HUDView, HUDViewModel>()│
│         │       ├─> Instantiate HUDView prefab                       │
│         │       ├─> Create/Inject HUDViewModel                      │
│         │       ├─> Call view.Initialize(viewModel)                 │
│         │       │   └─> UIView.cs:162-195                           │
│         │       │       ├─> Add CanvasGroup component               │
│         │       │       ├─> Add PropertyBinder component            │
│         │       │       ├─> viewModel.Initialize()                  │
│         │       │       ├─> OnViewModelSet()                        │
│         │       │       └─> BindViewModel()                         │
│         │       │           └─> HUDView.cs:29-93                    │
│         │       │               ├─> Bind Health to healthBar/Text   │
│         │       │               ├─> Bind Score to scoreText         │
│         │       │               ├─> Bind Coins to coinsText         │
│         │       │               ├─> Bind TimerText to timerText     │
│         │       │               └─> Bind PauseCommand to button     │
│         │       └─> Returns HUDView instance                        │
│         │                                                            │
│         ├─> STEP 3: Add to stack                                    │
│         │   └─> _viewStack.Push(view)                               │
│         │                                                            │
│         ├─> STEP 4: Show view with animation (AWAITED)              │
│         │   └─> await view.Show()                                   │
│         │       └─> HUDView.cs:118-122                              │
│         │           ├─> Logs: "[HUDView] HUD displayed"             │
│         │           └─> UIView.Show() (UIView.cs:46-83)             │
│         │               ├─> gameObject.SetActive(true)              │
│         │               ├─> canvasGroup.interactable = false        │
│         │               ├─> canvasGroup.blocksRaycasts = false      │
│         │               ├─> GetShowTransition()                     │
│         │               │   └─> HUDView has no transition (null)    │
│         │               │       (instant show, no animation)        │
│         │               ├─> canvasGroup.alpha = 1                   │
│         │               ├─> canvasGroup.interactable = true         │
│         │               └─> canvasGroup.blocksRaycasts = true       │
│         │               └─> ViewModel.OnViewShown()                 │
│         │                   └─> Logs: "[HUDViewModel] HUD shown"    │
│         │                                                            │
│         └─> Logs: "[NavigationStack] Push complete. Stack depth: 1" │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│ 5. UIStateMachine.cs:105 - Logs completion                          │
│    └─> "[UIStateMachine] Transition complete: Gameplay"             │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│ FINAL STATE                                                          │
│ ✓ MenuUIState exited                                                │
│ ✓ MainMenuView destroyed                                            │
│ ✓ Navigation stack cleared                                          │
│ ✓ GameplayUIState entered                                           │
│ ✓ HUDView created, bound, and shown                                 │
│ ✓ User sees HUD (Health, Score, Coins, Timer)                       │
└─────────────────────────────────────────────────────────────────────┘
```

## Key File References for Debugging

| Component | File Path | Line Numbers |
|-----------|-----------|--------------|
| User clicks Play | MainMenuView.cs | 53 (button binding) |
| Play command handler | MainMenuViewModel.cs | 72-84 |
| State transition start | UIStateMachine.cs | 72-123 |
| Menu state exit | GameBootstrap.cs | 121-127 |
| Clear navigation stack | NavigationStack.cs | 148-160 |
| MainMenu hide animation | MainMenuView.cs | 77-86 (0.2s fade) |
| Gameplay state enter | GameBootstrap.cs | 144-156 |
| HUD view creation | NavigationStack.cs | 34-68 |
| HUD view binding | HUDView.cs | 29-93 |
| HUD show (no animation) | HUDView.cs | 118-122 |

## Critical Timing

1. **MainMenu hide**: 0.2 seconds (FadeTransition, NOT awaited in Clear())
2. **HUD show**: Instant (no transition defined in HUDView)
3. **Total state transition**: Depends on Clear() + PushAsync() execution

## Important Notes

### Potential Bug: Clear() doesn't await Hide()
In `NavigationStack.cs:148-160`, the `Clear()` method calls `view.Hide()` but doesn't await it. This means:
- The hide animation may still be playing when the view is destroyed
- Could cause DOTween errors if GameObject is destroyed mid-animation
- This is different from `PopAsync()` which properly awaits the hide animation

### Animation Asymmetry
- **MainMenuView** has both show (0.3s fade in) and hide (0.2s fade out) animations
- **HUDView** has NO animations (instant show/hide)
- This creates a visual gap where MainMenu fades out but HUD appears instantly

## Debug Breakpoint Locations

If you want to debug step-by-step, set breakpoints at:

1. `MainMenuViewModel.cs:74` - When Play clicked
2. `UIStateMachine.cs:87` - State transition starts
3. `UIStateMachine.cs:96` - Before menu exit
4. `NavigationStack.cs:150` - Stack clearing starts
5. `UIStateMachine.cs:103` - Before gameplay enter
6. `NavigationStack.cs:49` - HUD push starts
7. `NavigationStack.cs:64` - HUD show starts

## Expected Debug Log Sequence

You should see these logs in order:

1. `[MainMenuViewModel] Play button clicked!`
2. `[UIStateMachine] Transitioning: Menu -> Gameplay`
3. `[MenuUIState] Exiting menu state...`
4. `[NavigationStack] Clearing stack. Depth: 1`
5. `[MainMenuView] Hiding main menu`
6. `[NavigationStack] Stack cleared.`
7. `[GameplayUIState] Entering gameplay state...`
8. `[NavigationStack] Pushing: HUDView`
9. `[HUDView] HUD displayed`
10. `[HUDViewModel] HUD shown - game started!`
11. `[NavigationStack] Push complete. Stack depth: 1`
12. `[UIStateMachine] Transition complete: Gameplay`

## Code Flow Summary

### High-Level Overview

```
MainMenuView (User Click)
    ↓
MainMenuViewModel.OnPlayClickedAsync()
    ↓
UINavigator.ChangeStateAsync("Gameplay")
    ↓
UIStateMachine.TransitionToAsync()
    ↓
    ├─> MenuUIState.OnExitAsync()
    │   └─> ClearStack() → MainMenuView.Hide() → Destroy()
    │
    └─> GameplayUIState.OnEnterAsync()
        └─> PushAsync<HUDView>() → Create → Bind → Show()
```

### State Machine Behavior

The `UIStateMachine` ensures:
- Current state exits completely before new state enters
- If transition fails, it rolls back to previous state
- State transitions are cancellable via CancellationToken
- States can use `OnUpdate()` for per-frame logic

### Navigation Stack Behavior

The `NavigationStack` ensures:
- Only one view is visible at a time
- Previous views are hidden (not destroyed) when pushing
- Hide animations complete before show animations start (in PushAsync/PopAsync)
- Stack has a maximum depth limit (default: 10)

## Related Files

### Core Files
- `Assets/UIFramework/Scripts/Navigation/UINavigator.cs` - Facade combining stack + state machine
- `Assets/UIFramework/Scripts/Navigation/States/UIStateMachine.cs` - State transition logic
- `Assets/UIFramework/Scripts/Navigation/Stack/NavigationStack.cs` - Stack-based navigation
- `Assets/UIFramework/Scripts/Core/MVVM/UIView.cs` - Base view class with Show/Hide

### Example Files
- `Assets/UIFramework/Scripts/Examples/GameBootstrap.cs` - Bootstrap and state definitions
- `Assets/UIFramework/Scripts/Examples/MainMenuView.cs` - Main menu UI
- `Assets/UIFramework/Scripts/Examples/MainMenuViewModel.cs` - Main menu logic
- `Assets/UIFramework/Scripts/Examples/HUDView.cs` - HUD UI
- `Assets/UIFramework/Scripts/Examples/HUDViewModel.cs` - HUD logic

---

**Created:** 2025-01-23
**Framework Version:** UIFramework 1.0
**Purpose:** Documentation for debugging UI state transitions
