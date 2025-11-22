using System;

namespace UIFramework.Core
{
    /// <summary>
    /// Base interface for all ViewModels in the MVVM pattern.
    /// ViewModels contain UI-specific business logic and expose data via ReactiveProperties.
    /// </summary>
    public interface IViewModel : IDisposable
    {
        /// <summary>
        /// Called once when the ViewModel is first created.
        /// Use this to initialize data, subscribe to events, and set up state.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called when the associated View is shown to the user.
        /// Use this to resume updates, start animations, or re-enable interactions.
        /// </summary>
        void OnViewShown();

        /// <summary>
        /// Called when the associated View is hidden from the user.
        /// Use this to pause updates, stop animations, or save state.
        /// </summary>
        void OnViewHidden();
    }
}
