using UIFramework.Core;
using UIFramework.MVVM;
using UIFramework.Events;
using UIFramework.Navigation;
using System.Threading.Tasks;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example ViewModel for a main menu.
    /// Demonstrates reactive properties, commands, navigation, and events.
    /// </summary>
    public class MainMenuViewModel : ViewModelBase
    {
        // Dependencies (injected via constructor)
        private readonly UINavigator _navigator;
        private readonly UIEventBus _eventBus;

        // Reactive Properties
        public ReactiveProperty<string> PlayerName { get; } = new ReactiveProperty<string>("Player");
        public ReactiveProperty<int> Level { get; } = new ReactiveProperty<int>(1);
        public ReactiveProperty<bool> IsLoading { get; } = new ReactiveProperty<bool>(false);

        // Commands
        public ReactiveCommand PlayCommand { get; }
        public ReactiveCommand SettingsCommand { get; }
        public ReactiveCommand QuitCommand { get; }

        /// <summary>
        /// Constructor with dependency injection.
        /// VContainer will automatically resolve and inject dependencies.
        /// </summary>
        public MainMenuViewModel(UINavigator navigator, UIEventBus eventBus)
        {
            _navigator = navigator;
            _eventBus = eventBus;

            // Initialize commands
            PlayCommand = new ReactiveCommand();
            PlayCommand.Subscribe(() => OnPlayClickedAsync()).AddTo(Disposables);

            SettingsCommand = new ReactiveCommand();
            SettingsCommand.Subscribe(() => OnSettingsClickedAsync()).AddTo(Disposables);

            QuitCommand = new ReactiveCommand();
            QuitCommand.Subscribe(OnQuitClicked).AddTo(Disposables);
        }

        public override void Initialize()
        {
            // Subscribe to events
            _eventBus.Subscribe<PlayerDataLoadedEvent>(OnPlayerDataLoaded)
                .AddTo(Disposables);

            // Load player data (simulated)
            LoadPlayerData();
        }

        public override void OnViewShown()
        {
            base.OnViewShown();
            UnityEngine.Debug.Log("[MainMenuViewModel] View shown - welcome player!");
        }

        private void LoadPlayerData()
        {
            // Simulate loading player data
            PlayerName.Value = "Hero";
            Level.Value = 42;
        }

        private async void OnPlayClickedAsync()
        {
            UnityEngine.Debug.Log("[MainMenuViewModel] Play button clicked!");

            // Publish event
            _eventBus.Publish(new GameStartRequestedEvent
            {
                PlayerName = PlayerName.Value,
                Level = Level.Value
            });

            // Navigate to gameplay (example - would need GameplayView/ViewModel)
            await _navigator.ChangeStateAsync("Gameplay");
        }

        private async Task OnSettingsClickedAsync()
        {
            UnityEngine.Debug.Log("[MainMenuViewModel] Settings button clicked!");

            // Navigate to settings (example - would need SettingsView/ViewModel)
            await _navigator.PushAsync<PopupView, PopupViewModel>();
        }

        private void OnQuitClicked()
        {
            UnityEngine.Debug.Log("[MainMenuViewModel] Quit button clicked!");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            UnityEngine.Application.Quit();
            #endif
        }

        private void OnPlayerDataLoaded(PlayerDataLoadedEvent evt)
        {
            UnityEngine.Debug.Log($"[MainMenuViewModel] Player data loaded: {evt.PlayerName}");
            PlayerName.Value = evt.PlayerName;
            Level.Value = evt.Level;
        }
    }

    #region Example Events

    /// <summary>
    /// Example event: Published when player data is loaded.
    /// </summary>
    public class PlayerDataLoadedEvent : IUIEvent
    {
        public string PlayerName { get; set; }
        public int Level { get; set; }
    }

    /// <summary>
    /// Example event: Published when game start is requested.
    /// </summary>
    public class GameStartRequestedEvent : IUIEvent
    {
        public string PlayerName { get; set; }
        public int Level { get; set; }
    }

    #endregion
}
