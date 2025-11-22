using UnityEngine;
using UIFramework.Navigation;
using UIFramework.DI;
using VContainer;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example bootstrap script that demonstrates how to initialize the UIFramework
    /// and start the application with the main menu.
    ///
    /// Setup Instructions:
    /// 1. Create a new scene called "Bootstrap"
    /// 2. Add an empty GameObject called "UIFramework"
    /// 3. Add UIFrameworkInstaller component
    /// 4. Assign the UIFrameworkConfig ScriptableObject
    /// 5. Add this GameBootstrap script to another GameObject
    /// 6. Play the scene!
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool autoStartMainMenu = true;

        private UINavigator _navigator;

        private async void Start()
        {
            Debug.Log("[GameBootstrap] Starting application...");

            // Wait one frame to ensure UIFrameworkInstaller has initialized
            await System.Threading.Tasks.Task.Yield();

            // Get navigator from service locator
            _navigator = ServiceLocator.Get<UINavigator>();

            if (_navigator == null)
            {
                Debug.LogError("[GameBootstrap] UINavigator not found! " +
                    "Make sure UIFrameworkInstaller is in the scene and properly configured.");
                return;
            }

            // Register UI states
            RegisterStates();

            // Start with menu state
            if (autoStartMainMenu)
            {
                await StartMainMenuAsync();
            }

            Debug.Log("[GameBootstrap] Bootstrap complete!");
        }

        private void RegisterStates()
        {
            Debug.Log("[GameBootstrap] Registering UI states...");

            // Register example states
            _navigator.RegisterState(new MenuUIState(_navigator));
            _navigator.RegisterState(new GameplayUIState(_navigator));

            Debug.Log("[GameBootstrap] States registered.");
        }

        private async System.Threading.Tasks.Task StartMainMenuAsync()
        {
            try
            {
                // Transition to menu state
                await _navigator.ChangeStateAsync("Menu");

                Debug.Log("[GameBootstrap] Main menu loaded!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameBootstrap] Failed to start main menu: {ex.Message}");
            }
        }

        private void Update()
        {
            // Update state machine (if states use OnUpdate)
            _navigator?.Update();
        }
    }

    #region Example UI States

    /// <summary>
    /// Example menu UI state.
    /// </summary>
    public class MenuUIState : UIFramework.Core.UIStateBase
    {
        private readonly UINavigator _navigator;

        public override string StateId => "Menu";

        public MenuUIState(UINavigator navigator)
        {
            _navigator = navigator;
        }

        public override async System.Threading.Tasks.Task OnEnterAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            Debug.Log("[MenuUIState] Entering menu state...");

            // Show main menu
            try
            {
                await _navigator.PushAsync<MainMenuView, MainMenuViewModel>(cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MenuUIState] Failed to load MainMenuView: {ex.Message}");
                Debug.LogError("Make sure you have created the MainMenuView prefab in Resources/UI/ or configured it as an Addressable.");
            }
        }

        public override async System.Threading.Tasks.Task OnExitAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            Debug.Log("[MenuUIState] Exiting menu state...");

            // Clear menu stack
            _navigator.ClearStack();
        }
    }

    /// <summary>
    /// Example gameplay UI state.
    /// </summary>
    public class GameplayUIState : UIFramework.Core.UIStateBase
    {
        private readonly UINavigator _navigator;

        public override string StateId => "Gameplay";

        public GameplayUIState(UINavigator navigator)
        {
            _navigator = navigator;
        }

        public override async System.Threading.Tasks.Task OnEnterAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            Debug.Log("[GameplayUIState] Entering gameplay state...");

            // Show HUD
            try
            {
                await _navigator.PushAsync<HUDView, HUDViewModel>(cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameplayUIState] Failed to load HUDView: {ex.Message}");
            }
        }

        public override async System.Threading.Tasks.Task OnExitAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            Debug.Log("[GameplayUIState] Exiting gameplay state...");

            // Clear gameplay UI
            _navigator.ClearStack();
        }

        public override void OnUpdate()
        {
            // Per-frame gameplay UI updates (if needed)
        }
    }

    #endregion
}
