using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework.Core;
using System.Threading.Tasks;

namespace UIFramework.Examples
{
    /// <summary>
    /// Example View for an in-game HUD (Heads-Up Display).
    /// Demonstrates dynamic UI updates and progress bars.
    /// </summary>
    public class HUDView : UIView<HUDViewModel>
    {
        [Header("Health")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TMP_Text healthText;

        [Header("Score & Coins")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text coinsText;

        [Header("Timer")]
        [SerializeField] private TMP_Text timerText;

        [Header("Controls")]
        [SerializeField] private Button pauseButton;

        protected override void BindViewModel()
        {
            // Bind health
            Bind(ViewModel.Health, health =>
            {
                if (healthText != null)
                {
                    healthText.text = $"{health}/{ViewModel.MaxHealth.Value}";
                }

                if (healthBar != null)
                {
                    healthBar.value = (float)health / ViewModel.MaxHealth.Value;
                }

                // Change color based on health
                UpdateHealthBarColor(health, ViewModel.MaxHealth.Value);
            });

            Bind(ViewModel.MaxHealth, maxHealth =>
            {
                if (healthBar != null)
                {
                    healthBar.maxValue = 1f; // Normalized
                }

                if (healthText != null)
                {
                    healthText.text = $"{ViewModel.Health.Value}/{maxHealth}";
                }
            });

            // Bind score
            Bind(ViewModel.Score, score =>
            {
                if (scoreText != null)
                {
                    scoreText.text = $"Score: {score}";
                }
            });

            // Bind coins
            Bind(ViewModel.Coins, coins =>
            {
                if (coinsText != null)
                {
                    coinsText.text = $"Coins: {coins}";
                }
            });

            // Bind timer
            Bind(ViewModel.TimerText, time =>
            {
                if (timerText != null)
                {
                    timerText.text = time;
                }
            });

            // Bind pause command
            if (pauseButton != null)
            {
                Bind(ViewModel.PauseCommand, pauseButton);
            }
        }

        private void UpdateHealthBarColor(int currentHealth, int maxHealth)
        {
            if (healthBar == null) return;

            var fillImage = healthBar.fillRect?.GetComponent<Image>();
            if (fillImage == null) return;

            float healthPercent = (float)currentHealth / maxHealth;

            if (healthPercent > 0.6f)
            {
                fillImage.color = Color.green;
            }
            else if (healthPercent > 0.3f)
            {
                fillImage.color = Color.yellow;
            }
            else
            {
                fillImage.color = Color.red;
            }
        }

        public override async Task Show()
        {
            await base.Show();
            Debug.Log("[HUDView] HUD displayed");
        }

        public override async Task Hide()
        {
            Debug.Log("[HUDView] HUD hidden");
            await base.Hide();
        }
    }
}
