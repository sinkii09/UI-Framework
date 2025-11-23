using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

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

        private async void Start()
        {
            Debug.Log("[GameBootstrap] Starting application...");

            // Wait one frame to ensure UIFrameworkInstaller has initialized
            await Task.Yield();

            // Check if UIFramework is ready
            if (!Core.UIFramework.IsInitialized)
            {
                Debug.LogError("[GameBootstrap] UIFramework not initialized! " +
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

            // Register example states using static UIFramework API
            Core.UIFramework.RegisterState(new MenuUIState());
            Core.UIFramework.RegisterState(new GameplayUIState());

            Debug.Log("[GameBootstrap] States registered.");
        }

        private async Task StartMainMenuAsync()
        {
            try
            {
                // Transition to menu state using static UIFramework API
                await Core.UIFramework.ChangeStateAsync("Menu");

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
            Core.UIFramework.Update();
        }
    }

    #region Example UI States

    /// <summary>
    /// Example menu UI state using static UIFramework API.
    /// Demonstrates loading screen and navigation.
    /// </summary>
    public class MenuUIState : Core.UIStateBase
    {
        public override string StateId => "Menu";

        public override async Task OnEnterAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("[MenuUIState] Entering menu state...");
            // Show main menu (loading is fully hidden now)
            try
            {
                await Core.UIFramework.PushAsync<MainMenuView, MainMenuViewModel>(cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MenuUIState] Failed to load MainMenuView: {ex.Message}");
                Debug.LogError("Make sure you have created the MainMenuView prefab in Resources/UI/ or configured it as an Addressable.");
            }
        }

        public override async Task OnExitAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("[MenuUIState] Exiting menu state...");
            await Core.UIFramework.ClearStackAsync(true);
        }
    }

    /// <summary>
    /// Example gameplay UI state using static UIFramework API.
    /// Demonstrates state transition with loading.
    /// </summary>
    public class GameplayUIState : Core.UIStateBase
    {
        public override string StateId => "Gameplay";

        public override async Task OnEnterAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("[GameplayUIState] Entering gameplay state...");

            // Show HUD (loading is fully hidden now)
            try
            {
                await Core.UIFramework.PushAsync<HUDView, HUDViewModel>(cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameplayUIState] Failed to load HUDView: {ex.Message}");
            }
        }

        public override async Task OnExitAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("[GameplayUIState] Exiting gameplay state...");
            await Core.UIFramework.ClearStackAsync();
        }

        public override void OnUpdate()
        {
            // Per-frame gameplay UI updates (if needed)
        }
    }

    #endregion
}
