using System.Threading.Tasks;
using TMPro;
using UIFramework.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example View for a popup/modal dialog.
    /// </summary>
    public class PopupView : UIView<PopupViewModel>
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [Header("Background")]
        [SerializeField] private Image backgroundOverlay;

        protected override void BindViewModel()
        {
            // Bind properties
            Bind(ViewModel.Title, title => titleText.text = title);
            Bind(ViewModel.Message, message => messageText.text = message);

            Bind(ViewModel.ShowCancelButton, show =>
            {
                if (cancelButton != null)
                {
                    cancelButton.gameObject.SetActive(show);
                }
            });

            // Bind commands
            Bind(ViewModel.ConfirmCommand, confirmButton);
            Bind(ViewModel.CancelCommand, cancelButton);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();

            // Darken background when popup is shown
            if (backgroundOverlay != null)
            {
                var color = backgroundOverlay.color;
                color.a = 0.7f;
                backgroundOverlay.color = color;
            }
        }

        public override async Task Show()
        {
            await base.Show();
            Debug.Log("[PopupView] Showing popup");
        }

        public override async Task Hide()
        {
            Debug.Log("[PopupView] Hiding popup");
            await base.Hide();
        }
    }
}
