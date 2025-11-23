using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework.Core;
using UIFramework.Animation;
using DG.Tweening;
using System.Threading.Tasks;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example View for a main menu.
    /// Demonstrates property binding, command binding, and animations.
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

        #region Animations

        /// <summary>
        /// Define the show animation - fade in + scale up.
        /// </summary>
        protected override UITransition GetShowTransition()
        {
            return new FadeTransition
            {
                FromAlpha = 0f,
                ToAlpha = 1f,
                Duration = 0.3f,
                EaseType = Ease.OutCubic
            };
        }

        /// <summary>
        /// Define the hide animation - fade out.
        /// </summary>
        protected override UITransition GetHideTransition()
        {
            return new FadeTransition
            {
                FromAlpha = 1f,
                ToAlpha = 0f,
                Duration = 0.2f,
                EaseType = Ease.InCubic
            };
        }

        #endregion

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            Debug.Log("[MainMenuView] ViewModel set and ready!");
        }

        public override async Task Show()
        {
            await base.Show();
            Debug.Log("[MainMenuView] Showing main menu");
        }

        public override async Task Hide()
        {
            Debug.Log("[MainMenuView] Hiding main menu");
            await base.Hide();
        }
    }
}
