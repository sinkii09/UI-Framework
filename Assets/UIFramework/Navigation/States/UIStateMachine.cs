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
    /// </summary>
    public class UIStateMachine
    {
        private IUIState _currentState;
        private readonly Dictionary<string, IUIState> _states = new Dictionary<string, IUIState>();

        /// <summary>
        /// Gets the currently active state.
        /// </summary>
        public IUIState CurrentState => _currentState;

        /// <summary>
        /// Gets the ID of the current state (null if no state is active).
        /// </summary>
        public string CurrentStateId => _currentState?.StateId;

        /// <summary>
        /// Registers a state with the state machine.
        /// </summary>
        /// <param name="state">The state to register.</param>
        /// <exception cref="ArgumentNullException">If state is null.</exception>
        /// <exception cref="InvalidOperationException">If a state with the same ID is already registered.</exception>
        public void RegisterState(IUIState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (string.IsNullOrEmpty(state.StateId))
                throw new ArgumentException("State ID cannot be null or empty.", nameof(state));

            if (_states.ContainsKey(state.StateId))
                throw new InvalidOperationException($"State with ID '{state.StateId}' is already registered.");

            _states[state.StateId] = state;
            Debug.Log($"[UIStateMachine] Registered state: {state.StateId}");
        }

        /// <summary>
        /// Unregisters a state from the state machine.
        /// </summary>
        /// <param name="stateId">The ID of the state to unregister.</param>
        /// <returns>True if the state was found and removed.</returns>
        public bool UnregisterState(string stateId)
        {
            if (_currentState?.StateId == stateId)
            {
                Debug.LogWarning($"[UIStateMachine] Cannot unregister currently active state: {stateId}");
                return false;
            }

            return _states.Remove(stateId);
        }

        /// <summary>
        /// Transitions to a new state by ID.
        /// </summary>
        /// <param name="stateId">The ID of the state to transition to.</param>
        /// <param name="cancellationToken">Token to cancel the transition.</param>
        /// <exception cref="InvalidOperationException">If the state is not found.</exception>
        public async Task TransitionToAsync(string stateId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(stateId))
                throw new ArgumentException("State ID cannot be null or empty.", nameof(stateId));

            if (!_states.TryGetValue(stateId, out var newState))
                throw new InvalidOperationException($"State '{stateId}' not found. Did you forget to register it?");

            // Already in this state
            if (_currentState == newState)
            {
                Debug.Log($"[UIStateMachine] Already in state: {stateId}");
                return;
            }

            Debug.Log($"[UIStateMachine] Transitioning: {_currentState?.StateId ?? "None"} -> {stateId}");

            var previousState = _currentState;

            try
            {
                // Exit current state
                if (_currentState != null)
                {
                    await _currentState.OnExitAsync(cancellationToken);
                }

                // Switch to new state
                _currentState = newState;

                // Enter new state
                await _currentState.OnEnterAsync(cancellationToken);

                Debug.Log($"[UIStateMachine] Transition complete: {stateId}");
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[UIStateMachine] Transition to {stateId} cancelled.");

                // Rollback to previous state
                _currentState = previousState;
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIStateMachine] Error during transition to {stateId}: {ex}");

                // Rollback to previous state
                _currentState = previousState;
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
        /// Checks if a state is registered.
        /// </summary>
        /// <param name="stateId">The state ID to check.</param>
        /// <returns>True if the state is registered.</returns>
        public bool HasState(string stateId)
        {
            return _states.ContainsKey(stateId);
        }

        /// <summary>
        /// Gets all registered state IDs.
        /// </summary>
        public IEnumerable<string> GetRegisteredStateIds()
        {
            return _states.Keys;
        }
    }
}
