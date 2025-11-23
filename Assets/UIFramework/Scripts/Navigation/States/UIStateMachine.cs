using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UIFramework.Core;
using UnityEngine;

namespace UIFramework.Navigation
{
    /// <summary>
    /// State machine for managing distinct UI modes (Menu, Gameplay, Pause, etc.).
    /// Handles state transitions and lifecycle management.
    /// Uses Type-based registration for type-safety (no string IDs).
    /// </summary>
    public class UIStateMachine
    {
        private IUIState _currentState;
        private Type _currentStateType;
        private readonly Dictionary<Type, IUIState> _states = new Dictionary<Type, IUIState>();

        /// <summary>
        /// Gets the currently active state.
        /// </summary>
        public IUIState CurrentState => _currentState;

        /// <summary>
        /// Gets the Type of the current state (null if no state is active).
        /// </summary>
        public Type CurrentStateType => _currentStateType;

        /// <summary>
        /// Registers a state with the state machine using its Type as the key.
        /// </summary>
        /// <param name="state">The state instance to register.</param>
        /// <exception cref="ArgumentNullException">If state is null.</exception>
        /// <exception cref="InvalidOperationException">If a state of the same Type is already registered.</exception>
        public void RegisterState(IUIState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var stateType = state.GetType();

            if (_states.ContainsKey(stateType))
                throw new InvalidOperationException($"State of type '{stateType.Name}' is already registered.");

            _states[stateType] = state;
            Debug.Log($"[UIStateMachine] Registered state: {stateType.Name}");
        }

        /// <summary>
        /// Registers a state by Type (creates instance automatically).
        /// </summary>
        /// <typeparam name="TState">The state Type to register.</typeparam>
        /// <exception cref="InvalidOperationException">If state is already registered.</exception>
        public void RegisterState<TState>() where TState : IUIState, new()
        {
            var stateType = typeof(TState);

            if (_states.ContainsKey(stateType))
                throw new InvalidOperationException($"State of type '{stateType.Name}' is already registered.");

            var state = new TState();
            _states[stateType] = state;
            Debug.Log($"[UIStateMachine] Registered state: {stateType.Name}");
        }

        /// <summary>
        /// Unregisters a state from the state machine by Type.
        /// </summary>
        /// <typeparam name="TState">The state Type to unregister.</typeparam>
        /// <returns>True if the state was found and removed.</returns>
        public bool UnregisterState<TState>() where TState : IUIState
        {
            var stateType = typeof(TState);

            if (_currentStateType == stateType)
            {
                Debug.LogWarning($"[UIStateMachine] Cannot unregister currently active state: {stateType.Name}");
                return false;
            }

            return _states.Remove(stateType);
        }

        /// <summary>
        /// Transitions to a new state by Type.
        /// </summary>
        /// <typeparam name="TState">The state Type to transition to.</typeparam>
        /// <param name="cancellationToken">Token to cancel the transition.</param>
        /// <exception cref="InvalidOperationException">If the state is not registered.</exception>
        public async Task TransitionToAsync<TState>(CancellationToken cancellationToken = default) where TState : IUIState
        {
            var stateType = typeof(TState);

            if (!_states.TryGetValue(stateType, out var newState))
                throw new InvalidOperationException($"State '{stateType.Name}' not found. Did you forget to register it?");

            // Already in this state
            if (_currentState == newState)
            {
                Debug.Log($"[UIStateMachine] Already in state: {stateType.Name}");
                return;
            }

            var previousStateName = _currentStateType?.Name ?? "None";
            Debug.Log($"[UIStateMachine] Transitioning: {previousStateName} -> {stateType.Name}");

            var previousState = _currentState;
            var previousStateType = _currentStateType;

            try
            {
                // Exit current state
                if (_currentState != null)
                {
                    await _currentState.OnExitAsync(cancellationToken);
                }

                // Switch to new state
                _currentState = newState;
                _currentStateType = stateType;

                // Enter new state
                await _currentState.OnEnterAsync(cancellationToken);

                Debug.Log($"[UIStateMachine] Transition complete: {stateType.Name}");
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[UIStateMachine] Transition to {stateType.Name} cancelled.");

                // Rollback to previous state
                _currentState = previousState;
                _currentStateType = previousStateType;
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIStateMachine] Error during transition to {stateType.Name}: {ex}");

                // Rollback to previous state
                _currentState = previousState;
                _currentStateType = previousStateType;
                throw;
            }
        }

        /// <summary>
        /// Updates the current state (call this every frame if using OnUpdate).
        /// </summary>
        public void Update()
        {
            _currentState?.OnUpdate();
        }

        /// <summary>
        /// Checks if a state is registered by Type.
        /// </summary>
        /// <typeparam name="TState">The state Type to check.</typeparam>
        /// <returns>True if the state is registered.</returns>
        public bool HasState<TState>() where TState : IUIState
        {
            return _states.ContainsKey(typeof(TState));
        }

        /// <summary>
        /// Gets all registered state Types.
        /// </summary>
        public IEnumerable<Type> GetRegisteredStateTypes()
        {
            return _states.Keys;
        }
    }
}
