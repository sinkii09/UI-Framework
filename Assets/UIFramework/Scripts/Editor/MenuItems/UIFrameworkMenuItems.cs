using UnityEngine;
using UnityEditor;
using System.IO;
using UIFramework.Configuration;
using UIFramework.Core;

namespace UIFramework.Editor
{
    /// <summary>
    /// Menu items for quick access to UIFramework tools and utilities.
    /// </summary>
    public static class UIFrameworkMenuItems
    {
        private const string ConfigAssetPath = "Assets/UIFramework/Resources/UIFrameworkConfig.asset";
        private const string ResourcesUIPath = "Assets/Resources/UI";
        private const string PrefabsUIPath = "Assets/Prefabs/UI";

        #region Config Management

        [MenuItem("UIFramework/Configuration/Open Config", priority = 10)]
        public static void OpenConfig()
        {
            var config = Resources.Load<UIFrameworkConfig>("UIFrameworkConfig");
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
            else
            {
                if (EditorUtility.DisplayDialog("Config Not Found",
                    "UIFrameworkConfig not found in Resources folder. Create one?",
                    "Create", "Cancel"))
                {
                    CreateConfig();
                }
            }
        }

        [MenuItem("UIFramework/Configuration/Create Config", priority = 11)]
        public static void CreateConfig()
        {
            // Ensure Resources directory exists
            string resourcesDir = Path.GetDirectoryName(ConfigAssetPath);
            if (!Directory.Exists(resourcesDir))
            {
                Directory.CreateDirectory(resourcesDir);
            }

            // Create config
            var config = ScriptableObject.CreateInstance<UIFrameworkConfig>();
            AssetDatabase.CreateAsset(config, ConfigAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            Debug.Log($"[UIFramework] Created UIFrameworkConfig at {ConfigAssetPath}");
        }

        #endregion

        #region Folder Setup

        [MenuItem("UIFramework/Setup/Create UI Folders (Resources)", priority = 30)]
        public static void CreateUIFoldersResources()
        {
            CreateDirectory(ResourcesUIPath);
            CreateDirectory($"{ResourcesUIPath}/Views");
            CreateDirectory($"{ResourcesUIPath}/Popups");
            CreateDirectory($"{ResourcesUIPath}/HUD");

            AssetDatabase.Refresh();
            Debug.Log("[UIFramework] Created Resources/UI folder structure for Resources loading");

            EditorUtility.DisplayDialog("Folders Created",
                "Created UI folder structure in Resources/UI/\n\n" +
                "• Views/\n" +
                "• Popups/\n" +
                "• HUD/\n\n" +
                "Place your UI prefabs here for Resources loading.",
                "OK");
        }

        [MenuItem("UIFramework/Setup/Create UI Folders (Prefabs)", priority = 31)]
        public static void CreateUIFoldersPrefabs()
        {
            CreateDirectory(PrefabsUIPath);
            CreateDirectory($"{PrefabsUIPath}/Views");
            CreateDirectory($"{PrefabsUIPath}/Popups");
            CreateDirectory($"{PrefabsUIPath}/HUD");

            AssetDatabase.Refresh();
            Debug.Log("[UIFramework] Created Prefabs/UI folder structure for Addressables");

            EditorUtility.DisplayDialog("Folders Created",
                "Created UI folder structure in Prefabs/UI/\n\n" +
                "• Views/\n" +
                "• Popups/\n" +
                "• HUD/\n\n" +
                "Tag these with 'UI' label for Addressables loading.",
                "OK");
        }

        private static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[UIFramework] Created directory: {path}");
            }
        }

        #endregion

        #region Prefab Creation

        [MenuItem("UIFramework/Create/Empty UI View", priority = 50)]
        public static void CreateEmptyUIView()
        {
            // Create canvas if needed
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
            }

            // Create UI view GameObject
            var viewGO = new GameObject("NewUIView");
            var rectTransform = viewGO.AddComponent<RectTransform>();
            viewGO.AddComponent<CanvasGroup>();

            // Set as child of canvas
            viewGO.transform.SetParent(canvas.transform, false);

            // Stretch to fill canvas
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add background
            var bgGO = new GameObject("Background");
            var bgRect = bgGO.AddComponent<RectTransform>();
            var bgImage = bgGO.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            bgGO.transform.SetParent(viewGO.transform, false);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            Undo.RegisterCreatedObjectUndo(viewGO, "Create UI View");
            Selection.activeGameObject = viewGO;

            Debug.Log("[UIFramework] Created empty UI View. Add a UIView component and configure bindings.");
        }

        [MenuItem("GameObject/UIFramework/UI View", priority = 0)]
        public static void CreateUIViewFromGameObject()
        {
            CreateEmptyUIView();
        }

        #endregion

        #region Documentation

        [MenuItem("UIFramework/Documentation/Open Design Document", priority = 100)]
        public static void OpenDesignDocument()
        {
            string designPath = "Assets/UIFramework/DESIGN.md";
            if (File.Exists(designPath))
            {
                Application.OpenURL($"file://{Path.GetFullPath(designPath)}");
            }
            else
            {
                EditorUtility.DisplayDialog("File Not Found",
                    $"DESIGN.md not found at {designPath}",
                    "OK");
            }
        }

        [MenuItem("UIFramework/Documentation/Open README", priority = 101)]
        public static void OpenReadme()
        {
            string readmePath = "Assets/UIFramework/README.md";
            if (File.Exists(readmePath))
            {
                Application.OpenURL($"file://{Path.GetFullPath(readmePath)}");
            }
            else
            {
                EditorUtility.DisplayDialog("File Not Found",
                    $"README.md not found at {readmePath}",
                    "OK");
            }
        }

        #endregion

        #region Validation

        [MenuItem("UIFramework/Tools/Validate Scene Setup", priority = 120)]
        public static void ValidateSceneSetup()
        {
            int errors = 0;
            int warnings = 0;
            var log = new System.Text.StringBuilder();
            log.AppendLine("=== UIFramework Scene Validation ===\n");

            // Check for Canvas
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                log.AppendLine("✗ ERROR: No Canvas found in scene");
                errors++;
            }
            else
            {
                log.AppendLine($"✓ Canvas found: {canvas.name}");

                // Check for CanvasScaler
                if (canvas.GetComponent<UnityEngine.UI.CanvasScaler>() == null)
                {
                    log.AppendLine("  ⚠ WARNING: Canvas missing CanvasScaler");
                    warnings++;
                }

                // Check for GraphicRaycaster
                if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                {
                    log.AppendLine("  ⚠ WARNING: Canvas missing GraphicRaycaster");
                    warnings++;
                }
            }

            // Check for UIFrameworkInstaller
            var installer = Object.FindAnyObjectByType<UIFramework.DI.UIFrameworkInstaller>();
            if (installer == null)
            {
                log.AppendLine("✗ ERROR: No UIFrameworkInstaller found in scene");
                log.AppendLine("  Add UIFrameworkInstaller component to a GameObject");
                errors++;
            }
            else
            {
                log.AppendLine($"✓ UIFrameworkInstaller found: {installer.name}");

                // Check config
                var config = Resources.Load<UIFrameworkConfig>("UIFrameworkConfig");
                if (config == null)
                {
                    log.AppendLine("  ✗ ERROR: UIFrameworkConfig not found in Resources");
                    errors++;
                }
                else
                {
                    log.AppendLine("  ✓ UIFrameworkConfig loaded");
                }
            }

            // Check for EventSystem
            var eventSystem = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                log.AppendLine("✗ ERROR: No EventSystem found in scene");
                log.AppendLine("  Add EventSystem (GameObject > UI > Event System)");
                errors++;
            }
            else
            {
                log.AppendLine($"✓ EventSystem found: {eventSystem.name}");
            }

            log.AppendLine($"\n=== Summary ===");
            log.AppendLine($"Errors: {errors}");
            log.AppendLine($"Warnings: {warnings}");

            Debug.Log(log.ToString());

            if (errors == 0 && warnings == 0)
            {
                EditorUtility.DisplayDialog("Validation Complete",
                    "Scene is properly configured for UIFramework!",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Complete",
                    $"Found {errors} error(s) and {warnings} warning(s).\n\nCheck console for details.",
                    "OK");
            }
        }

        [MenuItem("UIFramework/Tools/Validate All UI Prefabs", priority = 121)]
        public static void ValidateAllUIPrefabs()
        {
            var allPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            int totalPrefabs = 0;
            int viewsFound = 0;
            int missingCanvasGroup = 0;

            foreach (var guid in allPrefabs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null) continue;

                var view = prefab.GetComponent<UIViewBase>();
                if (view != null)
                {
                    totalPrefabs++;
                    viewsFound++;

                    if (prefab.GetComponent<CanvasGroup>() == null)
                    {
                        Debug.LogWarning($"[UIFramework] UI Prefab missing CanvasGroup: {path}", prefab);
                        missingCanvasGroup++;
                    }
                }
            }

            Debug.Log($"[UIFramework] Validation complete: {viewsFound} UIView prefabs found, {missingCanvasGroup} missing CanvasGroup");

            EditorUtility.DisplayDialog("Prefab Validation Complete",
                $"Found {viewsFound} UIView prefabs\n" +
                $"{missingCanvasGroup} missing CanvasGroup components",
                "OK");
        }

        #endregion

        #region Quick Access

        [MenuItem("UIFramework/Quick Access/Find UINavigator", priority = 140)]
        public static void FindUINavigator()
        {
            var installer = Object.FindAnyObjectByType<UIFramework.DI.UIFrameworkInstaller>();
            if (installer != null)
            {
                Selection.activeGameObject = installer.gameObject;
                EditorGUIUtility.PingObject(installer.gameObject);
                Debug.Log("[UIFramework] UINavigator is managed by UIFrameworkInstaller (DI Container)");
            }
            else
            {
                EditorUtility.DisplayDialog("Not Found",
                    "UIFrameworkInstaller not found in scene.\n\nAdd it to initialize UINavigator.",
                    "OK");
            }
        }

        #endregion
    }
}
