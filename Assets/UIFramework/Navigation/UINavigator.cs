using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UIFramework.Core;
using UIFramework.Animation;

namespace UIFramework.Navigation
{
    /// <summary>
    /// Facade that combines NavigationStack and UIStateMachine for unified navigation API.
    /// Provides both hierarchical navigation (push/pop) and state-based navigation.
    /// </summary>
    public class UINavigator
    {
        private readonly INavigationStack _navigationStack;
        private readonly UIStateMachine _stateMachine;

        /// <summary>
        /// Gets the current UI state.
        /// </summary>
        public IUIState CurrentState => _stateMachine.CurrentState;

        /// <summary>
        /// Gets the current state ID.
        /// </summary>
        public string CurrentStateId => _stateMachine.CurrentStateId;

        /// <summary>
        /// Gets the currently visible view from the navigation stack.
        /// </summary>
        public IUIView CurrentView => _navigationStack.CurrentView;

        /// <summary>
        /// Gets the current navigation stack depth.
        /// </summary>
        public int StackCount => _navigationStack.Count;

        /// <summary>
        /// Creates a new UINavigator.
        /// </summary>
        /// <param name="navigationStack">The navigation stack instance.</param>
        /// <param name="stateMachine">The state machine instance.</param>
        public UINavigator(INavigationStack navigationStack, UIStateMachine stateMachine)
        {
            _navigationStack = navigationStack ?? throw new ArgumentNullException(nameof(navigationStack));
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        }

        #region State Machine Operations

        /// <summary>
        /// Registers a UI state with the state machine.
        /// </summary>
        /// <param name="state">The state to register.</param>
        public void RegisterState(IUIState state)
        {
            _stateMachine.RegisterState(state);
        }

        /// <summary>
        /// Unregisters a UI state from the state machine.
        /// </summary>
        /// <param name="stateId">The state ID to unregister.</param>
        /// <returns>True if the state was unregistered.</returns>
        public bool UnregisterState(string stateId)
        {
            return _stateMachine.UnregisterState(stateId);
        }

        /// <summary>
        /// Transitions to a new UI state.
        /// </summary>
        /// <param name="stateId">The state ID to transition to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task ChangeStateAsync(string stateId, CancellationToken cancellationToken = default)
        {
            return _stateMachine.TransitionToAsync(stateId, cancellationToken);
        }

        /// <summary>
        /// Checks if a state is registered.
        /// </summary>
        /// <param name="stateId">The state ID.</param>
        /// <returns>True if registered.</returns>
        public bool HasState(string stateId)
        {
            return _stateMachine.HasState(stateId);
        }

        /// <summary>
        /// Gets all registered state IDs.
        /// </summary>
        public IEnumerable<string> GetRegisteredStateIds()
        {
            return _stateMachine.GetRegisteredStateIds();
        }

        /// <summary>
        /// Updates the current state (call every frame if using OnUpdate).
        /// </summary>
        public void Update()
        {
            _stateMachine.Update();
        }

        #endregion

        #region Navigation Stack Operations

        /// <summary>
        /// Pushes a new view onto the navigation stack.
        /// </summary>
        /// <typeparam name="TView">The view type.</typeparam>
        /// <typeparam name="TViewModel">The view model type.</typeparam>
        /// <param name="viewModel">Optional view model instance.</param>
        /// <param name="transition">Optional transition animation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created view.</returns>
        public Task<TView> PushAsync<TView, TViewModel>(
            TViewModel viewModel = null,
            UITransition transition = null,
            CancellationToken cancellationToken = default)
            where TView : UIView<TViewModel>
            where TViewModel : class, IViewModel
        {
            return _navigationStack.PushAsync<TView, TViewModel>(viewModel, transition, cancellationToken);
        }

        /// <summary>
        /// Pops the current view from the navigation stack.
        /// </summary>
        /// <param name="transition">Optional transition animation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The view now at the top of the stack.</returns>
        public Task<IUIView> PopAsync(
            UITransition transition = null,
            CancellationToken cancellationToken = default)
        {
            return _navigationStack.PopAsync(transition, cancellationToken);
        }

        /// <summary>
        /// Pops all views except the root.
        /// </summary>
        /// <param name="transition">Optional transition animation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task PopToRootAsync(
            UITransition transition = null,
            CancellationToken cancellationToken = default)
        {
            return _navigationStack.PopToRootAsync(transition, cancellationToken);
        }

        /// <summary>
        /// Clears the entire navigation stack.
        /// </summary>
        public void ClearStack()
        {
            _navigationStack.Clear();
        }

        #endregion
    }
}
