using UnityEngine;
using UnityEditor;
using System.IO;

namespace UIFramework.Editor
{
    /// <summary>
    /// Wizard to create UI state classes for the state machine.
    /// </summary>
    public class StateCreatorWizard : EditorWindow
    {
        private string _stateName = "MenuState";
        private string _namespace = "MyGame.UI";
        private string _scriptPath = "Assets/Scripts/UI/States";
        private bool _addOnUpdateMethod = false;
        private bool _addViewPushExample = true;

        [MenuItem("UIFramework/Create/UI State", priority = 3)]
        public static void ShowWindow()
        {
            var window = GetWindow<StateCreatorWizard>("Create UI State");
            window.minSize = new Vector2(400, 280);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("UI State Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Create a UI state class for the state machine (e.g., MenuState, GameplayState, PauseState).",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Settings
            _stateName = EditorGUILayout.TextField("State Name", _stateName);
            _namespace = EditorGUILayout.TextField("Namespace", _namespace);
            _scriptPath = EditorGUILayout.TextField("Script Path", _scriptPath);

            EditorGUILayout.Space(10);

            // Options
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            _addOnUpdateMethod = EditorGUILayout.Toggle("Add OnUpdate Method", _addOnUpdateMethod);
            _addViewPushExample = EditorGUILayout.Toggle("Add View Push Example", _addViewPushExample);

            EditorGUILayout.Space(10);

            // Preview
            EditorGUILayout.LabelField("Will Create:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  • {GetStateClassName()}.cs", EditorStyles.miniLabel);

            EditorGUILayout.Space(10);

            // Create Button
            GUI.enabled = !string.IsNullOrWhiteSpace(_stateName) && !string.IsNullOrWhiteSpace(_namespace);
            if (GUILayout.Button("Create State", GUILayout.Height(30)))
            {
                CreateState();
            }
            GUI.enabled = true;
        }

        private string GetStateClassName()
        {
            string name = _stateName.Replace("State", "").Trim();
            return $"{name}UIState";
        }

        private void CreateState()
        {
            // Ensure directory exists
            if (!Directory.Exists(_scriptPath))
            {
                Directory.CreateDirectory(_scriptPath);
            }

            string className = GetStateClassName();
            string filePath = Path.Combine(_scriptPath, $"{className}.cs");

            File.WriteAllText(filePath, GenerateStateCode(className));

            // Refresh asset database
            AssetDatabase.Refresh();

            Debug.Log($"[StateCreatorWizard] Created {className} at {_scriptPath}");
            EditorUtility.DisplayDialog("Success",
                $"Created {className}.cs successfully!",
                "OK");
        }

        private string GenerateStateCode(string className)
        {
            var code = new System.Text.StringBuilder();
            string baseName = _stateName.Replace("State", "").Replace("UI", "").Trim();

            code.AppendLine("using System.Threading;");
            code.AppendLine("using System.Threading.Tasks;");
            code.AppendLine("using UIFramework.Core;");
            code.AppendLine("using UIFramework.Navigation;");
            code.AppendLine();
            code.AppendLine($"namespace {_namespace}");
            code.AppendLine("{");
            code.AppendLine($"    /// <summary>");
            code.AppendLine($"    /// UI state for {baseName}.");
            code.AppendLine($"    /// </summary>");
            code.AppendLine($"    public class {className} : UIStateBase");
            code.AppendLine("    {");
            code.AppendLine("        private readonly UINavigator _navigator;");
            code.AppendLine();
            code.AppendLine($"        public override string StateId => \"{baseName}\";");
            code.AppendLine();
            code.AppendLine($"        public {className}(UINavigator navigator)");
            code.AppendLine("        {");
            code.AppendLine("            _navigator = navigator;");
            code.AppendLine("        }");
            code.AppendLine();
            code.AppendLine("        public override async Task OnEnterAsync(CancellationToken cancellationToken = default)");
            code.AppendLine("        {");
            code.AppendLine($"            UnityEngine.Debug.Log(\"[{className}] Entering state...\");");
            code.AppendLine();

            if (_addViewPushExample)
            {
                code.AppendLine("            // Example: Push a view when entering this state");
                code.AppendLine("            try");
                code.AppendLine("            {");
                code.AppendLine("                // TODO: Replace with your actual View and ViewModel types");
                code.AppendLine("                // await _navigator.PushAsync<YourView, YourViewModel>(cancellationToken: cancellationToken);");
                code.AppendLine("            }");
                code.AppendLine("            catch (System.Exception ex)");
                code.AppendLine("            {");
                code.AppendLine($"                UnityEngine.Debug.LogError($\"[{className}] Failed to load view: {{ex.Message}}\");");
                code.AppendLine("            }");
            }
            else
            {
                code.AppendLine("            // TODO: Initialize state");
            }

            code.AppendLine();
            code.AppendLine("            await Task.CompletedTask;");
            code.AppendLine("        }");
            code.AppendLine();
            code.AppendLine("        public override async Task OnExitAsync(CancellationToken cancellationToken = default)");
            code.AppendLine("        {");
            code.AppendLine($"            UnityEngine.Debug.Log(\"[{className}] Exiting state...\");");
            code.AppendLine();
            code.AppendLine("            // Clean up views and state");
            code.AppendLine("            _navigator.ClearStack();");
            code.AppendLine();
            code.AppendLine("            await Task.CompletedTask;");
            code.AppendLine("        }");

            if (_addOnUpdateMethod)
            {
                code.AppendLine();
                code.AppendLine("        public override void OnUpdate()");
                code.AppendLine("        {");
                code.AppendLine("            // Per-frame updates for this state");
                code.AppendLine("        }");
            }

            code.AppendLine("    }");
            code.AppendLine("}");

            return code.ToString();
        }
    }
}
