using UnityEngine;
using UnityEditor;
using UIFramework.Configuration;

namespace UIFramework.Editor
{
    /// <summary>
    /// Custom inspector for UIFrameworkConfig with organized sections and helpful tooltips.
    /// </summary>
    [CustomEditor(typeof(UIFrameworkConfig))]
    public class UIFrameworkConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _useAddressables;
        private SerializedProperty _uiAddressableLabel;
        private SerializedProperty _defaultTransitionDuration;
        private SerializedProperty _defaultEaseType;
        private SerializedProperty _maxNavigationStackDepth;
        private SerializedProperty _enableNavigationHistory;
        private SerializedProperty _enablePooling;
        private SerializedProperty _defaultPoolSize;
        private SerializedProperty _maxPoolSize;
        private SerializedProperty _enableViewCaching;
        private SerializedProperty _maxCachedViews;
        private SerializedProperty _verboseLogging;

        private bool _loadingFoldout = true;
        private bool _animationFoldout = true;
        private bool _navigationFoldout = true;
        private bool _poolingFoldout = true;
        private bool _performanceFoldout = true;
        private bool _debugFoldout = true;

        private void OnEnable()
        {
            _useAddressables = serializedObject.FindProperty("UseAddressables");
            _uiAddressableLabel = serializedObject.FindProperty("UIAddressableLabel");
            _defaultTransitionDuration = serializedObject.FindProperty("DefaultTransitionDuration");
            _defaultEaseType = serializedObject.FindProperty("DefaultEaseType");
            _maxNavigationStackDepth = serializedObject.FindProperty("MaxNavigationStackDepth");
            _enableNavigationHistory = serializedObject.FindProperty("EnableNavigationHistory");
            _enablePooling = serializedObject.FindProperty("EnablePooling");
            _defaultPoolSize = serializedObject.FindProperty("DefaultPoolSize");
            _maxPoolSize = serializedObject.FindProperty("MaxPoolSize");
            _enableViewCaching = serializedObject.FindProperty("EnableViewCaching");
            _maxCachedViews = serializedObject.FindProperty("MaxCachedViews");
            _verboseLogging = serializedObject.FindProperty("VerboseLogging");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("UIFramework Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure UIFramework settings. Changes take effect when the framework initializes.",
                MessageType.Info);
            EditorGUILayout.Space(10);

            // Loading Settings
            _loadingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_loadingFoldout, "Loading Settings");
            if (_loadingFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_useAddressables, new GUIContent("Use Addressables",
                    "Enable Addressables for async UI loading. Requires Addressables package."));

                if (_useAddressables.boolValue)
                {
                    EditorGUILayout.PropertyField(_uiAddressableLabel, new GUIContent("UI Label",
                        "Addressables label for UI prefabs (default: 'UI')."));

                    #if !UIFRAMEWORK_ADDRESSABLES
                    EditorGUILayout.HelpBox(
                        "Addressables package not detected. Install it via Package Manager or the framework will use Resources loading.",
                        MessageType.Warning);
                    #endif
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Using Resources loading. UI prefabs should be in Resources/UI/ folder.",
                        MessageType.Info);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);

            // Animation Settings
            _animationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_animationFoldout, "Animation Settings");
            if (_animationFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_defaultTransitionDuration, new GUIContent("Default Duration",
                    "Default transition duration in seconds (recommended: 0.2-0.5)."));
                EditorGUILayout.PropertyField(_defaultEaseType, new GUIContent("Default Ease",
                    "Default easing function for transitions."));

                if (_defaultTransitionDuration.floatValue < 0.05f)
                {
                    EditorGUILayout.HelpBox("Very short durations may cause jarring transitions.", MessageType.Warning);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);

            // Navigation Settings
            _navigationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_navigationFoldout, "Navigation Settings");
            if (_navigationFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_maxNavigationStackDepth, new GUIContent("Max Stack Depth",
                    "Maximum UI navigation stack depth. Prevents infinite push loops."));
                EditorGUILayout.PropertyField(_enableNavigationHistory, new GUIContent("Enable History",
                    "Track navigation history for debugging and analytics."));

                if (_maxNavigationStackDepth.intValue < 5)
                {
                    EditorGUILayout.HelpBox("Low stack depth may limit complex UI flows.", MessageType.Warning);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);

            // Pooling Settings
            _poolingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_poolingFoldout, "Pooling Settings");
            if (_poolingFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_enablePooling, new GUIContent("Enable Pooling",
                    "Use object pooling to reduce instantiation overhead."));

                if (_enablePooling.boolValue)
                {
                    EditorGUILayout.PropertyField(_defaultPoolSize, new GUIContent("Default Pool Size",
                        "Initial pool size for each UI prefab."));
                    EditorGUILayout.PropertyField(_maxPoolSize, new GUIContent("Max Pool Size",
                        "Maximum pool size before objects are destroyed instead of returned."));

                    if (_maxPoolSize.intValue < _defaultPoolSize.intValue)
                    {
                        EditorGUILayout.HelpBox("Max pool size should be >= default pool size.", MessageType.Error);
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);

            // Performance Settings
            _performanceFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_performanceFoldout, "Performance Settings");
            if (_performanceFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_enableViewCaching, new GUIContent("Enable View Caching",
                    "Cache inactive views in memory for faster reuse."));

                if (_enableViewCaching.boolValue)
                {
                    EditorGUILayout.PropertyField(_maxCachedViews, new GUIContent("Max Cached Views",
                        "Maximum number of views to keep cached. Higher = more memory, faster loading."));
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);

            // Debug Settings
            _debugFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_debugFoldout, "Debug Settings");
            if (_debugFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_verboseLogging, new GUIContent("Verbose Logging",
                    "Enable detailed logging for debugging (disable in production)."));

                if (_verboseLogging.boolValue)
                {
                    EditorGUILayout.HelpBox("Verbose logging enabled. Disable for production builds.", MessageType.Warning);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);

            // Quick Actions
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Defaults"))
            {
                if (EditorUtility.DisplayDialog("Reset Configuration",
                    "Are you sure you want to reset all settings to default values?",
                    "Reset", "Cancel"))
                {
                    ResetToDefaults();
                }
            }
            if (GUILayout.Button("Validate Settings"))
            {
                ValidateSettings();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void ResetToDefaults()
        {
            _useAddressables.boolValue = false;
            _uiAddressableLabel.stringValue = "UI";
            _defaultTransitionDuration.floatValue = 0.3f;
            _defaultEaseType.enumValueIndex = (int)DG.Tweening.Ease.OutCubic;
            _maxNavigationStackDepth.intValue = 10;
            _enableNavigationHistory.boolValue = true;
            _enablePooling.boolValue = true;
            _defaultPoolSize.intValue = 5;
            _maxPoolSize.intValue = 20;
            _enableViewCaching.boolValue = true;
            _maxCachedViews.intValue = 20;
            _verboseLogging.boolValue = false;

            serializedObject.ApplyModifiedProperties();
            Debug.Log("[UIFrameworkConfig] Reset to default values");
        }

        private void ValidateSettings()
        {
            var config = target as UIFrameworkConfig;
            int errors = 0;
            int warnings = 0;

            // Validate max pool size
            if (config.EnablePooling && config.MaxPoolSize < config.DefaultPoolSize)
            {
                Debug.LogError("[UIFrameworkConfig] Max pool size must be >= default pool size");
                errors++;
            }

            // Validate navigation stack
            if (config.MaxNavigationStackDepth < 1)
            {
                Debug.LogError("[UIFrameworkConfig] Max navigation stack depth must be at least 1");
                errors++;
            }

            // Validate animation duration
            if (config.DefaultTransitionDuration <= 0)
            {
                Debug.LogError("[UIFrameworkConfig] Default transition duration must be positive");
                errors++;
            }

            // Warnings
            if (config.DefaultTransitionDuration < 0.05f)
            {
                Debug.LogWarning("[UIFrameworkConfig] Very short transition duration may cause jarring animations");
                warnings++;
            }

            if (config.VerboseLogging)
            {
                Debug.LogWarning("[UIFrameworkConfig] Verbose logging is enabled - disable for production");
                warnings++;
            }

            if (config.UseAddressables)
            {
                #if !UIFRAMEWORK_ADDRESSABLES
                Debug.LogWarning("[UIFrameworkConfig] Addressables enabled but package not detected");
                warnings++;
                #endif
            }

            // Report
            if (errors == 0 && warnings == 0)
            {
                EditorUtility.DisplayDialog("Validation Complete",
                    "All settings are valid!",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Complete",
                    $"Found {errors} error(s) and {warnings} warning(s). Check console for details.",
                    "OK");
            }
        }
    }
}
