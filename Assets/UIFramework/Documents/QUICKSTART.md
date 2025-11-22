# UIFramework Quick Start Guide

Get up and running with UIFramework in 5 minutes.

## Installation Checklist

### 1. Install Dependencies

**Via Unity Package Manager** (`Window > Package Manager`):

1. **VContainer** - Add package from git URL:
   ```
   https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.13.2
   ```

2. **DOTween** - Install from Asset Store or add via Package Manager:
   ```
   com.demigiant.dotween
   ```

3. **TextMeshPro** - If not already installed:
   - Window > TextMeshPro > Import TMP Essential Resources

4. **Addressables** (Optional) - For async loading:
   ```
   com.unity.addressables
   ```

### 2. Run Project Setup Wizard

1. Menu: **UIFramework > Setup > Project Setup Wizard**
2. Click through the wizard:
   - Step 1: Create Configuration
   - Step 2: Create Folder Structure
   - Step 3: Setup Scene
3. Click **Finish**

## Your First UI View in 3 Steps

### Step 1: Create Scripts

**Menu: UIFramework > Create View & ViewModel**

Fill in:
- View Name: `MainMenu`
- Namespace: `YourGame.UI`
- Options: Check all boxes

Click **Create**

This generates:
- `MainMenuViewModel.cs` - Data and logic
- `MainMenuView.cs` - UI presentation

### Step 2: Create the Prefab

**Menu: UIFramework > Create > UI Prefab Template**

Fill in:
- Template Type: `Full Screen View`
- Prefab Name: `MainMenuView`
- Save Path: `Assets/Resources/UI`
- Options: Check "Add Sample Content"

Click **Create Prefab**

### Step 3: Wire Up the Prefab

1. Open the `MainMenuView` prefab
2. Add the `MainMenuView` component
3. Assign UI references:
   - Title Text → TitleText
   - Counter Text → CounterText
   - Buttons → Confirm/Cancel buttons
4. Save the prefab

### Step 4: Show It!

Create a bootstrap script:

```csharp
using UnityEngine;
using UIFramework.Navigation;
using UIFramework.DependencyInjection;

public class GameBootstrap : MonoBehaviour
{
    private UINavigator _navigator;

    private async void Start()
    {
        // Wait for framework initialization
        await System.Threading.Tasks.Task.Yield();

        // Get navigator
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

Add this script to any GameObject in your scene and hit Play!

## Common Patterns

### Update UI When Data Changes

```csharp
// In ViewModel
public ReactiveProperty<int> Score { get; } = new ReactiveProperty<int>(0);

// In View's BindViewModel()
Bind(ViewModel.Score, score => scoreText.text = $"Score: {score}");

// In game code
viewModel.Score.Value = 100; // UI updates automatically!
```

### Handle Button Clicks

```csharp
// In ViewModel
public ReactiveCommand PlayCommand { get; }

public MainMenuViewModel(UINavigator navigator)
{
    _navigator = navigator;

    PlayCommand = new ReactiveCommand();
    PlayCommand.Subscribe(OnPlayClicked).AddTo(Disposables);
}

private void OnPlayClicked()
{
    Debug.Log("Play button clicked!");
    // Do stuff
}

// In View's BindViewModel()
Bind(ViewModel.PlayCommand, playButton);
```

### Navigate Between Views

```csharp
// Push a new view
await _navigator.PushAsync<SettingsView, SettingsViewModel>();

// Go back
await _navigator.PopAsync();

// Replace current view
_navigator.ClearStack();
await _navigator.PushAsync<GameView, GameViewModel>();
```

### Send Events Between UI

```csharp
// Define event
public class PlayerDiedEvent : IUIEvent
{
    public int Score { get; set; }
}

// Subscribe (in ViewModel constructor)
public HUDViewModel(UIEventBus eventBus)
{
    _eventBus = eventBus;
    _eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied).AddTo(Disposables);
}

private void OnPlayerDied(PlayerDiedEvent evt)
{
    Debug.Log($"Player died with score: {evt.Score}");
}

// Publish from anywhere
_eventBus.Publish(new PlayerDiedEvent { Score = 1000 });
```

## Project Structure

Recommended folder organization:

```
Assets/
├── UIFramework/              # The framework itself
├── Resources/
│   └── UI/                   # UI prefabs (if using Resources loading)
│       ├── Views/
│       ├── Popups/
│       └── HUD/
├── Scripts/
│   └── UI/
│       ├── Views/            # View scripts (MainMenuView.cs)
│       ├── ViewModels/       # ViewModel scripts (MainMenuViewModel.cs)
│       ├── States/           # UI States (MenuUIState.cs)
│       └── Events/           # Custom events (PlayerDiedEvent.cs)
└── Prefabs/
    └── UI/                   # Alternative prefab location (for Addressables)
```

## Configuration

Access settings: **UIFramework > Configuration > Open Config**

### Essential Settings

- **Use Addressables**: Check if using Addressables (requires setup)
- **Default Transition Duration**: 0.3s is recommended
- **Enable Pooling**: Keep checked for performance
- **Verbose Logging**: Check for debugging, uncheck for builds

## Troubleshooting

### "UINavigator not found"
- Make sure UIFrameworkInstaller is in your scene
- Verify the config is assigned in the inspector
- Ensure you're waiting a frame after scene load

### "View prefab not found"
- Check prefab is in `Resources/UI/` folder
- Verify prefab name matches the View class name exactly
- For Addressables: check the "UI" label is applied

### "BindViewModel not called"
- View must inherit from `UIView<YourViewModel>`
- Implement `protected override void BindViewModel()`
- Prefab must have your View component attached

### Buttons Not Working
- Check command is initialized in ViewModel constructor
- Verify `Bind(ViewModel.Command, button)` is called
- Ensure button is assigned in inspector

## Next Steps

1. **Read the README** - Full feature documentation
2. **Check Examples** - See complete implementations in Examples folder
3. **Read DESIGN.md** - Understand the architecture
4. **Use the Wizards** - Speed up development with editor tools

## Useful Menu Items

- **UIFramework > Create View & ViewModel** - Generate View/ViewModel pairs
- **UIFramework > Create UI State** - Create state machine states
- **UIFramework > Create > UI Prefab Template** - Generate UI layouts
- **UIFramework > Tools > Validate Scene Setup** - Check configuration
- **UIFramework > Documentation** - Open docs

---

**Still stuck?** Check the Examples folder for working implementations of common UI patterns.
