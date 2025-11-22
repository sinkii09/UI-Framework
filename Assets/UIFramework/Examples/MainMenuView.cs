using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework.Core;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example View for a main menu.
    /// Demonstrates property binding and command binding.
    /// </summary>
    public class MainMenuView : UIView<MainMenuViewModel>
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text welcomeText;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Loading")]
        [SerializeField] private GameObject loadingPanel;

        protected override void BindViewModel()
        {
            // Bind reactive properties to UI
            Bind(ViewModel.PlayerName, name =>
            {
                playerNameText.text = name;
                welcomeText.text = $"Welcome, {name}!";
            });

            Bind(ViewModel.Level, level =>
            {
                levelText.text = $"Level {level}";
            });

            Bind(ViewModel.IsLoading, isLoading =>
            {
                if (loadingPanel != null)
                {
                    loadingPanel.SetActive(isLoading);
                }
            });

            // Bind commands to buttons
            Bind(ViewModel.PlayCommand, playButton);
            Bind(ViewModel.SettingsCommand, settingsButton);
            Bind(ViewModel.QuitCommand, quitButton);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            Debug.Log("[MainMenuView] ViewModel set and ready!");
        }

        public override void Show()
        {
            base.Show();
            Debug.Log("[MainMenuView] Showing main menu");
        }

        public override void Hide()
        {
            Debug.Log("[MainMenuView] Hiding main menu");
            base.Hide();
        }
    }
}
