using System;
using UIFramework.MVVM;

namespace UIFramework.Core
{
    /// <summary>
    /// Base class for all ViewModels.
    /// Provides lifecycle management and automatic cleanup via CompositeDisposable.
    /// </summary>
    public abstract class ViewModelBase : IViewModel
    {
        /// <summary>
        /// Collection for automatic cleanup of subscriptions.
        /// Add all IDisposable subscriptions to this using .AddTo(Disposables).
        /// </summary>
        protected readonly CompositeDisposable Disposables = new CompositeDisposable();

        /// <summary>
        /// Whether this ViewModel has been disposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// Called once when the ViewModel is first created.
        /// Override to initialize data, subscribe to events, and set up state.
        /// </summary>
        public virtual void Initialize()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called when the associated View is shown to the user.
        /// Override to resume updates, start animations, or re-enable interactions.
        /// </summary>
        public virtual void OnViewShown()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called when the associated View is hidden from the user.
        /// Override to pause updates, stop animations, or save state.
        /// </summary>
        public virtual void OnViewHidden()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Disposes the ViewModel and cleans up all subscriptions.
        /// Override to add custom cleanup, but always call base.Dispose().
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            // Automatically dispose all subscriptions
            Disposables.Dispose();
        }

        /// <summary>
        /// Destructor to ensure cleanup even if Dispose() wasn't called.
        /// </summary>
        ~ViewModelBase()
        {
            Dispose();
        }
    }
}
