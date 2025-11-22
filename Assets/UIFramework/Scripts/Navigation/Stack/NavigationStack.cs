using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UIFramework.Core;
using UIFramework.Animation;
using UnityEngine;

namespace UIFramework.Navigation
{
    /// <summary>
    /// Stack-based navigation system for hierarchical UI (menus, submenus, etc.).
    /// </summary>
    public class NavigationStack : INavigationStack
    {
        private readonly Stack<IUIView> _viewStack = new Stack<IUIView>();
        private readonly IUIViewFactory _viewFactory;
        private readonly int _maxStackDepth;

        public int Count => _viewStack.Count;
        public IUIView CurrentView => _viewStack.Count > 0 ? _viewStack.Peek() : null;

        /// <summary>
        /// Creates a new NavigationStack.
        /// </summary>
        /// <param name="viewFactory">Factory for creating views.</param>
        /// <param name="maxStackDepth">Maximum allowed stack depth (default: 10).</param>
        public NavigationStack(IUIViewFactory viewFactory, int maxStackDepth = 10)
        {
            _viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
            _maxStackDepth = maxStackDepth;
        }

        public async Task<TView> PushAsync<TView, TViewModel>(
            TViewModel viewModel = null,
            UITransition transition = null,
            CancellationToken cancellationToken = default)
            where TView : UIView<TViewModel>
            where TViewModel : class, IViewModel
        {
            // Check stack depth limit
            if (_viewStack.Count >= _maxStackDepth)
            {
                throw new InvalidOperationException(
                    $"Maximum navigation stack depth ({_maxStackDepth}) reached. " +
                    "Consider popping views or increasing the limit.");
            }

            Debug.Log($"[NavigationStack] Pushing: {typeof(TView).Name}");

            // Hide current view
            if (CurrentView != null)
            {
                CurrentView.Hide();
            }

            // Create new view
            var view = await _viewFactory.CreateAsync<TView, TViewModel>(viewModel, cancellationToken);

            // Add to stack
            _viewStack.Push(view);

            // Show with optional transition
            view.Show();

            Debug.Log($"[NavigationStack] Push complete. Stack depth: {_viewStack.Count}");
            return view;
        }

        public async Task<IUIView> PopAsync(
            UITransition transition = null,
            CancellationToken cancellationToken = default)
        {
            if (_viewStack.Count == 0)
            {
                throw new InvalidOperationException("Cannot pop from empty navigation stack.");
            }

            if (_viewStack.Count == 1)
            {
                Debug.LogWarning("[NavigationStack] Cannot pop the root view. Use Clear() if you want to remove all views.");
                return CurrentView;
            }

            Debug.Log($"[NavigationStack] Popping: {CurrentView?.GetType().Name}");

            // Get and remove current view
            var currentView = _viewStack.Pop();

            // Hide and destroy
            currentView.Hide();
            await Task.Yield(); // Allow one frame for hide animation if needed
            currentView.Destroy();

            // Show previous view
            var previousView = CurrentView;
            if (previousView != null)
            {
                previousView.Show();
            }

            Debug.Log($"[NavigationStack] Pop complete. Stack depth: {_viewStack.Count}");
            return previousView;
        }

        public async Task PopToRootAsync(
            UITransition transition = null,
            CancellationToken cancellationToken = default)
        {
            if (_viewStack.Count <= 1)
            {
                Debug.Log("[NavigationStack] Already at root.");
                return;
            }

            Debug.Log($"[NavigationStack] Popping to root from depth: {_viewStack.Count}");

            // Store root
            var viewsToRemove = new List<IUIView>();
            while (_viewStack.Count > 1)
            {
                viewsToRemove.Add(_viewStack.Pop());
            }

            // Hide and destroy all except root
            foreach (var view in viewsToRemove)
            {
                view.Hide();
                view.Destroy();
            }

            // Show root
            if (CurrentView != null)
            {
                CurrentView.Show();
            }

            Debug.Log("[NavigationStack] Pop to root complete.");
        }

        public void Clear()
        {
            Debug.Log($"[NavigationStack] Clearing stack. Depth: {_viewStack.Count}");

            while (_viewStack.Count > 0)
            {
                var view = _viewStack.Pop();
                view.Hide();
                view.Destroy();
            }

            Debug.Log("[NavigationStack] Stack cleared.");
        }
    }
}
