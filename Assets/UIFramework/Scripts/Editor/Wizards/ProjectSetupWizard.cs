using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using UIFramework.Configuration;
using UIFramework.DI;
using UnityEngine.InputSystem.UI;

namespace UIFramework.Editor
{
    /// <summary>
    /// First-time setup wizard for UIFramework.
    /// Guides users through initial configuration.
    /// </summary>
    public class ProjectSetupWizard : EditorWindow
    {
        private enum SetupStep
        {
            Welcome,
            CreateConfig,
            CreateFolders,
            SetupScene,
            Complete
        }

        private SetupStep _currentStep = SetupStep.Welcome;
        private bool _useAddressables = false;
        private bool _setupResourcesFolders = true;
        private bool _setupPrefabsFolders = false;
        private bool _addExampleScripts = true;

        [MenuItem("UIFramework/Setup/Project Setup Wizard", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectSetupWizard>("UIFramework Setup");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            switch (_currentStep)
            {
                case SetupStep.Welcome:
                    DrawWelcomeStep();
                    break;
                case SetupStep.CreateConfig:
                    DrawCreateConfigStep();
                    break;
                case SetupStep.CreateFolders:
                    DrawCreateFoldersStep();
                    break;
                case SetupStep.SetupScene:
                    DrawSetupSceneStep();
                    break;
                case SetupStep.Complete:
                    DrawCompleteStep();
                    break;
            }
        }

        private void DrawWelcomeStep()
        {
            EditorGUILayout.LabelField("Welcome to UIFramework!", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This wizard will help you set up UIFramework in your project.\n\n" +
                "UIFramework is a production-ready MVVM UI system for Unity with:\n" +
                "• Clean architecture and separation of concerns\n" +
                "• Reactive data binding\n" +
                "• Stack-based + State Machine navigation\n" +
                "• VContainer dependency injection\n" +
                "• DOTween animations\n" +
                "• Object pooling and async loading",
                MessageType.Info);

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("What would you like to set up?", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            _useAddressables = EditorGUILayout.Toggle("Use Addressables for UI loading", _useAddressables);
            _setupResourcesFolders = EditorGUILayout.Toggle("Create Resources/UI folders", _setupResourcesFolders);
            _setupPrefabsFolders = EditorGUILayout.Toggle("Create Prefabs/UI folders", _setupPrefabsFolders);
            _addExampleScripts = EditorGUILayout.Toggle("Add example scripts", _addExampleScripts);

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Next: Create Configuration", GUILayout.Height(35)))
            {
                _currentStep = SetupStep.CreateConfig;
            }
        }

        private void DrawCreateConfigStep()
        {
            EditorGUILayout.LabelField("Step 1: Create Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "UIFramework uses a ScriptableObject for configuration.\n" +
                "This will be created in Resources/UIFrameworkConfig.asset",
                MessageType.Info);

            EditorGUILayout.Space(10);

            var existingConfig = Resources.Load<UIFrameworkConfig>("UIFrameworkConfig");
            if (existingConfig != null)
            {
                EditorGUILayout.HelpBox("Configuration already exists!", MessageType.Warning);
                if (GUILayout.Button("Open Existing Config"))
                {
                    Selection.activeObject = existingConfig;
                    EditorGUIUtility.PingObject(existingConfig);
                }
            }
            else
            {
                if (GUILayout.Button("Create Configuration", GUILayout.Height(35)))
                {
                    CreateConfiguration();
                }
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("< Back"))
            {
                _currentStep = SetupStep.Welcome;
            }
            if (GUILayout.Button("Next: Create Folders >"))
            {
                _currentStep = SetupStep.CreateFolders;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCreateFoldersStep()
        {
            EditorGUILayout.LabelField("Step 2: Create Folder Structure", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "UIFramework needs folders for organizing UI prefabs.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            if (_setupResourcesFolders)
            {
                EditorGUILayout.LabelField("Resources Folders:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("  • Resources/UI/Views", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("  • Resources/UI/Popups", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("  • Resources/UI/HUD", EditorStyles.miniLabel);
                EditorGUILayout.Space(5);

                if (GUILayout.Button("Create Resources Folders"))
                {
                    CreateResourcesFolders();
                }
            }

            if (_setupPrefabsFolders)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Prefabs Folders:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("  • Prefabs/UI/Views", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("  • Prefabs/UI/Popups", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("  • Prefabs/UI/HUD", EditorStyles.miniLabel);
                EditorGUILayout.Space(5);

                if (GUILayout.Button("Create Prefabs Folders"))
                {
                    CreatePrefabsFolders();
                }
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("< Back"))
            {
                _currentStep = SetupStep.CreateConfig;
            }
            if (GUILayout.Button("Next: Setup Scene >"))
            {
                _currentStep = SetupStep.SetupScene;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSetupSceneStep()
        {
            EditorGUILayout.LabelField("Step 3: Setup Current Scene", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "UIFramework requires:\n" +
                "• Canvas with CanvasScaler and GraphicRaycaster\n" +
                "• EventSystem\n" +
                "• UIFrameworkInstaller (VContainer LifetimeScope)",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Check current scene
            var canvas = Object.FindAnyObjectByType<Canvas>();
            var eventSystem = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            var installer = Object.FindAnyObjectByType<UIFrameworkInstaller>();

            EditorGUILayout.LabelField("Current Scene Status:", EditorStyles.boldLabel);
            DrawStatusLine("Canvas", canvas != null);
            DrawStatusLine("EventSystem", eventSystem != null);
            DrawStatusLine("UIFrameworkInstaller", installer != null);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Setup Current Scene", GUILayout.Height(35)))
            {
                SetupCurrentScene();
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("< Back"))
            {
                _currentStep = SetupStep.CreateFolders;
            }
            if (GUILayout.Button("Finish >"))
            {
                _currentStep = SetupStep.Complete;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCompleteStep()
        {
            EditorGUILayout.LabelField("Setup Complete!", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "UIFramework is now set up in your project!\n\n" +
                "Next steps:\n" +
                "1. Create View and ViewModel pairs using UIFramework > Create View & ViewModel\n" +
                "2. Configure UIFrameworkConfig settings\n" +
                "3. Add UI states using UIFramework > Create UI State\n" +
                "4. Check the Examples folder for sample implementations",
                MessageType.Info);

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Open Documentation"))
            {
                string designPath = "Assets/UIFramework/DESIGN.md";
                if (File.Exists(designPath))
                {
                    Application.OpenURL($"file://{Path.GetFullPath(designPath)}");
                }
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Open Configuration"))
            {
                var config = Resources.Load<UIFrameworkConfig>("UIFrameworkConfig");
                if (config != null)
                {
                    Selection.activeObject = config;
                    EditorGUIUtility.PingObject(config);
                }
            }

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Close Wizard", GUILayout.Height(35)))
            {
                Close();
            }
        }

        private void DrawStatusLine(string label, bool status)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            EditorGUILayout.LabelField(status ? "✓ Present" : "✗ Missing",
                status ? EditorStyles.label : EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void CreateConfiguration()
        {
            string configPath = "Assets/UIFramework/Resources";
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }

            var config = ScriptableObject.CreateInstance<UIFrameworkConfig>();
            config.UseAddressables = _useAddressables;

            string fullPath = Path.Combine(configPath, "UIFrameworkConfig.asset");
            AssetDatabase.CreateAsset(config, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            Debug.Log($"[ProjectSetupWizard] Created UIFrameworkConfig at {fullPath}");
            EditorUtility.DisplayDialog("Success", "Configuration created successfully!", "OK");
        }

        private void CreateResourcesFolders()
        {
            CreateDirectory("Assets/Resources/UI");
            CreateDirectory("Assets/Resources/UI/Views");
            CreateDirectory("Assets/Resources/UI/Popups");
            CreateDirectory("Assets/Resources/UI/HUD");
            AssetDatabase.Refresh();

            Debug.Log("[ProjectSetupWizard] Created Resources/UI folder structure");
            EditorUtility.DisplayDialog("Success", "Resources folders created!", "OK");
        }

        private void CreatePrefabsFolders()
        {
            CreateDirectory("Assets/Prefabs/UI");
            CreateDirectory("Assets/Prefabs/UI/Views");
            CreateDirectory("Assets/Prefabs/UI/Popups");
            CreateDirectory("Assets/Prefabs/UI/HUD");
            AssetDatabase.Refresh();

            Debug.Log("[ProjectSetupWizard] Created Prefabs/UI folder structure");
            EditorUtility.DisplayDialog("Success", "Prefabs folders created!", "OK");
        }

        private void SetupCurrentScene()
        {
            bool madeChanges = false;

            // Setup Canvas
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
                madeChanges = true;
                Debug.Log("[ProjectSetupWizard] Created Canvas");
            }

            // Setup EventSystem
            var eventSystem = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create EventSystem");
                madeChanges = true;
                Debug.Log("[ProjectSetupWizard] Created EventSystem");
            }

            // Setup UIFrameworkInstaller
            var installer = Object.FindAnyObjectByType<UIFrameworkInstaller>();
            if (installer == null)
            {
                var installerGO = new GameObject("UIFramework");
                installer = installerGO.AddComponent<UIFrameworkInstaller>();
                Undo.RegisterCreatedObjectUndo(installerGO, "Create UIFrameworkInstaller");
                madeChanges = true;
                Debug.Log("[ProjectSetupWizard] Created UIFrameworkInstaller");

                // Try to assign config
                var config = Resources.Load<UIFrameworkConfig>("UIFrameworkConfig");
                if (config != null)
                {
                    var serializedInstaller = new SerializedObject(installer);
                    var configProp = serializedInstaller.FindProperty("config");
                    if (configProp != null)
                    {
                        configProp.objectReferenceValue = config;
                        serializedInstaller.ApplyModifiedProperties();
                        Debug.Log("[ProjectSetupWizard] Assigned UIFrameworkConfig to installer");
                    }
                }
            }

            if (madeChanges)
            {
                EditorUtility.DisplayDialog("Success",
                    "Scene setup complete!\n\nAll required components have been added.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Already Setup",
                    "Scene is already configured for UIFramework.",
                    "OK");
            }
        }

        private void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[ProjectSetupWizard] Created directory: {path}");
            }
        }
    }
}
