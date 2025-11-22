using UnityEngine;
using UnityEditor;
using System.IO;

namespace UIFramework.Editor
{
    /// <summary>
    /// Editor wizard to quickly create matching View and ViewModel pairs.
    /// Generates boilerplate code with proper naming conventions.
    /// </summary>
    public class ViewViewModelCreatorWizard : EditorWindow
    {
        private string _viewName = "MyView";
        private string _namespace = "MyGame.UI";
        private string _scriptPath = "Assets/Scripts/UI";
        private bool _createWithReactiveProperties = true;
        private bool _createWithCommands = true;
        private bool _addExampleBindings = true;

        [MenuItem("UIFramework/Create View & ViewModel", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<ViewViewModelCreatorWizard>("Create View & ViewModel");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("View & ViewModel Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "This wizard creates a matching View and ViewModel pair with proper MVVM structure.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Basic Settings
            _viewName = EditorGUILayout.TextField("View Name (e.g., 'MainMenu')", _viewName);
            _namespace = EditorGUILayout.TextField("Namespace", _namespace);
            _scriptPath = EditorGUILayout.TextField("Script Path", _scriptPath);

            EditorGUILayout.Space(10);

            // Options
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            _createWithReactiveProperties = EditorGUILayout.Toggle("Add Example Properties", _createWithReactiveProperties);
            _createWithCommands = EditorGUILayout.Toggle("Add Example Commands", _createWithCommands);
            _addExampleBindings = EditorGUILayout.Toggle("Add Example Bindings", _addExampleBindings);

            EditorGUILayout.Space(10);

            // Preview
            EditorGUILayout.LabelField("Will Create:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  • {GetViewModelName()}ViewModel.cs", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"  • {GetViewModelName()}View.cs", EditorStyles.miniLabel);

            EditorGUILayout.Space(10);

            // Create Button
            GUI.enabled = !string.IsNullOrWhiteSpace(_viewName) && !string.IsNullOrWhiteSpace(_namespace);
            if (GUILayout.Button("Create View & ViewModel", GUILayout.Height(30)))
            {
                CreateViewAndViewModel();
            }
            GUI.enabled = true;
        }

        private string GetViewModelName()
        {
            return _viewName.Replace("View", "").Trim();
        }

        private void CreateViewAndViewModel()
        {
            // Ensure directory exists
            if (!Directory.Exists(_scriptPath))
            {
                Directory.CreateDirectory(_scriptPath);
            }

            string baseName = GetViewModelName();
            string viewModelName = $"{baseName}ViewModel";
            string viewName = $"{baseName}View";

            // Create ViewModel
            string viewModelPath = Path.Combine(_scriptPath, $"{viewModelName}.cs");
            File.WriteAllText(viewModelPath, GenerateViewModelCode(viewModelName));

            // Create View
            string viewPath = Path.Combine(_scriptPath, $"{viewName}.cs");
            File.WriteAllText(viewPath, GenerateViewCode(viewName, viewModelName));

            // Refresh asset database
            AssetDatabase.Refresh();

            Debug.Log($"[ViewViewModelCreator] Created {viewModelName} and {viewName} at {_scriptPath}");
            EditorUtility.DisplayDialog("Success",
                $"Created {viewModelName}.cs and {viewName}.cs successfully!",
                "OK");
        }

        private string GenerateViewModelCode(string viewModelName)
        {
            var code = new System.Text.StringBuilder();

            code.AppendLine("using UIFramework.Core;");
            code.AppendLine("using UIFramework.MVVM;");

            if (_createWithCommands)
            {
                code.AppendLine("using UIFramework.Navigation;");
            }

            code.AppendLine();
            code.AppendLine($"namespace {_namespace}");
            code.AppendLine("{");
            code.AppendLine($"    /// <summary>");
            code.AppendLine($"    /// ViewModel for {viewModelName.Replace("ViewModel", "")}.");
            code.AppendLine($"    /// </summary>");
            code.AppendLine($"    public class {viewModelName} : ViewModelBase");
            code.AppendLine("    {");

            // Add example reactive properties
            if (_createWithReactiveProperties)
            {
                code.AppendLine("        // Reactive Properties");
                code.AppendLine("        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>(\"Title\");");
                code.AppendLine("        public ReactiveProperty<int> Counter { get; } = new ReactiveProperty<int>(0);");
                code.AppendLine();
            }

            // Add example commands
            if (_createWithCommands)
            {
                code.AppendLine("        // Commands");
                code.AppendLine("        public ReactiveCommand ConfirmCommand { get; }");
                code.AppendLine("        public ReactiveCommand CancelCommand { get; }");
                code.AppendLine();
            }

            // Constructor
            if (_createWithCommands)
            {
                code.AppendLine("        private readonly UINavigator _navigator;");
                code.AppendLine();
                code.AppendLine($"        public {viewModelName}(UINavigator navigator)");
                code.AppendLine("        {");
                code.AppendLine("            _navigator = navigator;");
                code.AppendLine();
                code.AppendLine("            // Initialize commands");
                code.AppendLine("            ConfirmCommand = new ReactiveCommand();");
                code.AppendLine("            ConfirmCommand.Subscribe(OnConfirmClicked).AddTo(Disposables);");
                code.AppendLine();
                code.AppendLine("            CancelCommand = new ReactiveCommand();");
                code.AppendLine("            CancelCommand.Subscribe(OnCancelClicked).AddTo(Disposables);");
                code.AppendLine("        }");
            }
            else
            {
                code.AppendLine($"        public {viewModelName}()");
                code.AppendLine("        {");
                code.AppendLine("        }");
            }

            code.AppendLine();

            // Initialize method
            code.AppendLine("        public override void Initialize()");
            code.AppendLine("        {");
            code.AppendLine("            base.Initialize();");
            code.AppendLine("            UnityEngine.Debug.Log($\"[{viewModelName}] Initialized\");");
            code.AppendLine("        }");
            code.AppendLine();

            // OnViewShown
            code.AppendLine("        public override void OnViewShown()");
            code.AppendLine("        {");
            code.AppendLine("            base.OnViewShown();");
            code.AppendLine("            UnityEngine.Debug.Log($\"[{viewModelName}] View shown\");");
            code.AppendLine("        }");

            // Add command handlers if needed
            if (_createWithCommands)
            {
                code.AppendLine();
                code.AppendLine("        private void OnConfirmClicked()");
                code.AppendLine("        {");
                code.AppendLine("            UnityEngine.Debug.Log($\"[{viewModelName}] Confirm clicked\");");
                code.AppendLine("            // TODO: Implement confirm logic");
                code.AppendLine("        }");
                code.AppendLine();
                code.AppendLine("        private void OnCancelClicked()");
                code.AppendLine("        {");
                code.AppendLine("            UnityEngine.Debug.Log($\"[{viewModelName}] Cancel clicked\");");
                code.AppendLine("            // TODO: Implement cancel logic");
                code.AppendLine("        }");
            }

            code.AppendLine("    }");
            code.AppendLine("}");

            return code.ToString();
        }

        private string GenerateViewCode(string viewName, string viewModelName)
        {
            var code = new System.Text.StringBuilder();

            code.AppendLine("using UnityEngine;");
            code.AppendLine("using UnityEngine.UI;");
            code.AppendLine("using TMPro;");
            code.AppendLine("using UIFramework.Core;");
            code.AppendLine();
            code.AppendLine($"namespace {_namespace}");
            code.AppendLine("{");
            code.AppendLine($"    /// <summary>");
            code.AppendLine($"    /// View for {viewName.Replace("View", "")}.");
            code.AppendLine($"    /// </summary>");
            code.AppendLine($"    public class {viewName} : UIView<{viewModelName}>");
            code.AppendLine("    {");

            // Add example UI references
            if (_addExampleBindings)
            {
                code.AppendLine("        [Header(\"UI References\")]");
                code.AppendLine("        [SerializeField] private TMP_Text titleText;");
                code.AppendLine("        [SerializeField] private TMP_Text counterText;");
                code.AppendLine("        [SerializeField] private Button confirmButton;");
                code.AppendLine("        [SerializeField] private Button cancelButton;");
                code.AppendLine();
            }

            // BindViewModel method
            code.AppendLine("        protected override void BindViewModel()");
            code.AppendLine("        {");

            if (_addExampleBindings)
            {
                code.AppendLine("            // Bind reactive properties to UI elements");
                code.AppendLine("            Bind(ViewModel.Title, title =>");
                code.AppendLine("            {");
                code.AppendLine("                if (titleText != null)");
                code.AppendLine("                {");
                code.AppendLine("                    titleText.text = title;");
                code.AppendLine("                }");
                code.AppendLine("            });");
                code.AppendLine();
                code.AppendLine("            Bind(ViewModel.Counter, counter =>");
                code.AppendLine("            {");
                code.AppendLine("                if (counterText != null)");
                code.AppendLine("                {");
                code.AppendLine("                    counterText.text = $\"Counter: {counter}\";");
                code.AppendLine("                }");
                code.AppendLine("            });");
                code.AppendLine();
                code.AppendLine("            // Bind commands to buttons");
                code.AppendLine("            if (confirmButton != null)");
                code.AppendLine("            {");
                code.AppendLine("                Bind(ViewModel.ConfirmCommand, confirmButton);");
                code.AppendLine("            }");
                code.AppendLine();
                code.AppendLine("            if (cancelButton != null)");
                code.AppendLine("            {");
                code.AppendLine("                Bind(ViewModel.CancelCommand, cancelButton);");
                code.AppendLine("            }");
            }
            else
            {
                code.AppendLine("            // TODO: Bind ViewModel properties to UI elements");
                code.AppendLine("            // Example: Bind(ViewModel.SomeProperty, value => someText.text = value);");
            }

            code.AppendLine("        }");
            code.AppendLine();

            // Show/Hide methods
            code.AppendLine("        public override void Show()");
            code.AppendLine("        {");
            code.AppendLine("            base.Show();");
            code.AppendLine($"            Debug.Log(\"[{viewName}] Shown\");");
            code.AppendLine("        }");
            code.AppendLine();
            code.AppendLine("        public override void Hide()");
            code.AppendLine("        {");
            code.AppendLine($"            Debug.Log(\"[{viewName}] Hidden\");");
            code.AppendLine("            base.Hide();");
            code.AppendLine("        }");

            code.AppendLine("    }");
            code.AppendLine("}");

            return code.ToString();
        }
    }
}
