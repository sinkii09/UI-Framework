using UnityEngine;
using DG.Tweening;

namespace UIFramework.Configuration
{
    /// <summary>
    /// Main configuration for the UIFramework.
    /// Create via: Right-click → Create → UI Framework → Framework Config
    /// </summary>
    [CreateAssetMenu(fileName = "UIFrameworkConfig", menuName = "UI Framework/Framework Config", order = 1)]
    public class UIFrameworkConfig : ScriptableObject
    {
        [Header("Loading")]
        [Tooltip("Use Addressables for loading UI prefabs. If false, uses Resources.")]
        public bool UseAddressables = false;

        [Tooltip("Addressable label for UI assets (if using Addressables).")]
        public string UIAddressableLabel = "UI";

        [Header("Animation")]
        [Tooltip("Default duration for UI transitions (seconds).")]
        [Range(0.1f, 2f)]
        public float DefaultTransitionDuration = 0.3f;

        [Tooltip("Default easing function for UI transitions.")]
        public Ease DefaultEaseType = Ease.OutCubic;

        [Header("Navigation")]
        [Tooltip("Maximum depth of the navigation stack before error.")]
        [Range(5, 50)]
        public int MaxNavigationStackDepth = 10;

        [Tooltip("Whether to maintain navigation history for back button support.")]
        public bool EnableNavigationHistory = true;

        [Header("Pooling")]
        [Tooltip("Enable object pooling for frequently instantiated UI.")]
        public bool EnablePooling = true;

        [Tooltip("Default pool size for UI elements.")]
        [Range(1, 50)]
        public int DefaultPoolSize = 5;

        [Tooltip("Maximum pool size before objects are destroyed instead of pooled.")]
        [Range(5, 100)]
        public int MaxPoolSize = 20;

        [Header("Performance")]
        [Tooltip("Cache inactive views instead of destroying them.")]
        public bool EnableViewCaching = true;

        [Tooltip("Maximum number of views to cache before destroying oldest.")]
        [Range(5, 100)]
        public int MaxCachedViews = 20;

        [Header("Debug")]
        [Tooltip("Enable verbose logging for debugging.")]
        public bool VerboseLogging = false;

        #region Validation

        private void OnValidate()
        {
            // Ensure sensible values
            MaxNavigationStackDepth = Mathf.Max(1, MaxNavigationStackDepth);
            DefaultPoolSize = Mathf.Max(1, DefaultPoolSize);
            MaxPoolSize = Mathf.Max(DefaultPoolSize, MaxPoolSize);
            MaxCachedViews = Mathf.Max(0, MaxCachedViews);
            DefaultTransitionDuration = Mathf.Max(0.01f, DefaultTransitionDuration);
        }

        #endregion
    }
}
