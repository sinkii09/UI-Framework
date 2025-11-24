using UIFramework.Core;
using UIFramework.Events;
using UIFramework.MVVM;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example ViewModel for an in-game HUD (Heads-Up Display).
    /// Demonstrates event subscription and real-time data updates.
    /// </summary>
    public class HUDViewModel : ViewModelBase
    {
        private readonly UIEventBus _eventBus;

        // Reactive Properties
        public ReactiveProperty<int> Health { get; } = new ReactiveProperty<int>(100);
        public ReactiveProperty<int> MaxHealth { get; } = new ReactiveProperty<int>(100);
        public ReactiveProperty<int> Score { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<int> Coins { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<string> TimerText { get; } = new ReactiveProperty<string>("00:00");

        // Commands
        public ReactiveCommand PauseCommand { get; }

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        public HUDViewModel(UIEventBus eventBus)
        {
            _eventBus = eventBus;

            PauseCommand = new ReactiveCommand();
            PauseCommand.Subscribe(OnPauseClicked).AddTo(Disposables);
        }

        public override void Initialize()
        {
            // Subscribe to game events
            _eventBus.Subscribe<HealthChangedEvent>(OnHealthChanged)
                .AddTo(Disposables);

            _eventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged)
                .AddTo(Disposables);

            _eventBus.Subscribe<CoinsChangedEvent>(OnCoinsChanged)
                .AddTo(Disposables);

            _eventBus.Subscribe<GameTimeUpdatedEvent>(OnGameTimeUpdated)
                .AddTo(Disposables);
        }

        public override void OnViewShown()
        {
            base.OnViewShown();
            UnityEngine.Debug.Log("[HUDViewModel] HUD shown - game started!");
        }

        private void OnHealthChanged(HealthChangedEvent evt)
        {
            Health.Value = evt.CurrentHealth;
            MaxHealth.Value = evt.MaxHealth;

            // Show warning if health is low
            if (evt.CurrentHealth < evt.MaxHealth * 0.3f)
            {
                UnityEngine.Debug.LogWarning("[HUDViewModel] Health is low!");
            }
        }

        private void OnScoreChanged(ScoreChangedEvent evt)
        {
            Score.Value = evt.NewScore;
            UnityEngine.Debug.Log($"[HUDViewModel] Score updated: {evt.NewScore} (+{evt.Delta})");
        }

        private void OnCoinsChanged(CoinsChangedEvent evt)
        {
            Coins.Value = evt.NewAmount;
        }

        private void OnGameTimeUpdated(GameTimeUpdatedEvent evt)
        {
            var minutes = (int)(evt.ElapsedSeconds / 60);
            var seconds = (int)(evt.ElapsedSeconds % 60);
            TimerText.Value = $"{minutes:00}:{seconds:00}";
        }

        private async void OnPauseClicked()
        {
            UnityEngine.Debug.Log("[HUDViewModel] Pause button clicked!");
            _eventBus.Publish(new GamePausedEvent());
            await Core.UIManager.ChangeStateAsync<MenuUIState>();
        }
    }

    #region Example Game Events

    /// <summary>
    /// Event: Health changed.
    /// </summary>
    public class HealthChangedEvent : IUIEvent
    {
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public int Delta { get; set; }
    }

    /// <summary>
    /// Event: Score changed.
    /// </summary>
    public class ScoreChangedEvent : IUIEvent
    {
        public int NewScore { get; set; }
        public int Delta { get; set; }
    }

    /// <summary>
    /// Event: Coins collected/spent.
    /// </summary>
    public class CoinsChangedEvent : IUIEvent
    {
        public int NewAmount { get; set; }
        public int Delta { get; set; }
    }

    /// <summary>
    /// Event: Game timer updated.
    /// </summary>
    public class GameTimeUpdatedEvent : IUIEvent
    {
        public float ElapsedSeconds { get; set; }
    }

    /// <summary>
    /// Event: Game paused.
    /// </summary>
    public class GamePausedEvent : IUIEvent
    {
    }

    #endregion
}
