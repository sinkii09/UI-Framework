using System;
using UIFramework.Core;
using UIFramework.MVVM;
using UIFramework.Navigation;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example ViewModel for a popup/modal dialog.
    /// Demonstrates parameterized ViewModels and callbacks.
    /// </summary>
    public class PopupViewModel : ViewModelBase
    {
        private readonly UINavigator _navigator;

        // Reactive Properties
        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>("Popup");
        public ReactiveProperty<string> Message { get; } = new ReactiveProperty<string>("This is a popup message.");
        public ReactiveProperty<bool> ShowCancelButton { get; } = new ReactiveProperty<bool>(true);

        // Commands
        public ReactiveCommand ConfirmCommand { get; }
        public ReactiveCommand CancelCommand { get; }

        // Callback for when popup is closed
        private Action<bool> _onClosed;

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        public PopupViewModel(UINavigator navigator)
        {
            _navigator = navigator;

            ConfirmCommand = new ReactiveCommand();
            ConfirmCommand.Subscribe(OnConfirmClicked).AddTo(Disposables);

            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(OnCancelClicked).AddTo(Disposables);
        }

        /// <summary>
        /// Sets up the popup with specific parameters.
        /// Call this after construction to configure the popup.
        /// </summary>
        public void Setup(string title, string message, bool showCancel = true, Action<bool> onClosed = null)
        {
            Title.Value = title;
            Message.Value = message;
            ShowCancelButton.Value = showCancel;
            _onClosed = onClosed;
        }

        private async void OnConfirmClicked()
        {
            UnityEngine.Debug.Log("[PopupViewModel] Confirm clicked");

            // Invoke callback
            _onClosed?.Invoke(true);

            // Close popup
            await _navigator.PopAsync();
        }

        private async void OnCancelClicked()
        {
            UnityEngine.Debug.Log("[PopupViewModel] Cancel clicked");

            // Invoke callback
            _onClosed?.Invoke(false);

            // Close popup
            await _navigator.PopAsync();
        }
    }
}
