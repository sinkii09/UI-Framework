using UnityEngine;
using UnityEditor;
using UIFramework.DI;
using UIFramework.Configuration;

namespace UIFramework.Editor
{
    /// <summary>
    /// Custom IMGUI inspector for UIFrameworkInstaller.
    /// Overrides VContainer's UIElements inspector to avoid Unity threading bugs.
    /// </summary>
    [CustomEditor(typeof(UIFrameworkInstaller))]
    public class UIFrameworkInstallerEditor : UnityEditor.Editor
    {
        private SerializedProperty _configProperty;
        private SerializedProperty _uiCanvasProperty;

        private void OnEnable()
        {
            _configProperty = serializedObject.FindProperty("config");
            _uiCanvasProperty = serializedObject.FindProperty("uiCanvas");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("UIFramework Dependency Injection", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "VContainer LifetimeScope for UIFramework services.\nAll framework services are registered automatically.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Configuration
            EditorGUILayout.PropertyField(_configProperty, new GUIContent("Framework Config"));

            var config = _configProperty.objectReferenceValue as UIFrameworkConfig;
            if (config == null)
            {
                EditorGUILayout.HelpBox("Config not assigned! Click to create.", MessageType.Error);
                if (GUILayout.Button("Create Config"))
                {
                    CreateConfig();
                }
            }
            else
            {
                if (GUILayout.Button("Open Config"))
                {
                    Selection.activeObject = config;
                    EditorGUIUtility.PingObject(config);
                }
            }

            EditorGUILayout.Space(10);

            // UI Canvas
            EditorGUILayout.PropertyField(_uiCanvasProperty, new GUIContent("UI Canvas"));

            if (_uiCanvasProperty.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("UI Canvas not assigned! Assign a Canvas from your scene.", MessageType.Error);
            }

            EditorGUILayout.Space(10);

            // Show registered services info
            if (config != null)
            {
                EditorGUILayout.LabelField("Registered Services:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("• UIEventBus", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• UINavigator", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• UIStateMachine", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• IUIViewFactory", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• IUIAnimator", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"• IUILoader ({(config.UseAddressables ? "Addressables" : "Resources")})", EditorStyles.miniLabel);
                if (config.EnablePooling)
                {
                    EditorGUILayout.LabelField("• IUIObjectPool", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateConfig()
        {
            string path = "Assets/UIFramework/Resources";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            var config = ScriptableObject.CreateInstance<UIFrameworkConfig>();
            string fullPath = System.IO.Path.Combine(path, "UIFrameworkConfig.asset");

            AssetDatabase.CreateAsset(config, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _configProperty.objectReferenceValue = config;
            serializedObject.ApplyModifiedProperties();

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
    }
}
