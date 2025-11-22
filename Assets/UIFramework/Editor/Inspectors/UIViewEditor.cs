using UnityEngine;
using UnityEditor;
using UIFramework.Core;

namespace UIFramework.Editor
{
    /// <summary>
    /// Custom inspector for UIView with helpful runtime testing tools.
    /// </summary>
    [CustomEditor(typeof(UIViewBase), true)]
    public class UIViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("UIView Tools", EditorStyles.boldLabel);

            var view = target as UIViewBase;
            if (view == null) return;

            // Runtime Testing Tools
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Runtime testing tools - use these to test Show/Hide behavior.", MessageType.Info);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Show View"))
                {
                    view.Show();
                    Debug.Log($"[UIViewEditor] Manually called Show() on {view.name}");
                }

                if (GUILayout.Button("Hide View"))
                {
                    view.Hide();
                    Debug.Log($"[UIViewEditor] Manually called Hide() on {view.name}");
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                // View State Info
                EditorGUILayout.LabelField("View State", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                var canvasGroup = view.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    EditorGUILayout.LabelField("Canvas Group", "Present");
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Slider("Alpha", canvasGroup.alpha, 0f, 1f);
                    EditorGUILayout.Toggle("Interactable", canvasGroup.interactable);
                    EditorGUILayout.Toggle("Block Raycasts", canvasGroup.blocksRaycasts);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    EditorGUILayout.HelpBox("No CanvasGroup found. UIView uses CanvasGroup for Show/Hide.", MessageType.Warning);
                }

                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to access runtime testing tools.", MessageType.Info);
            }

            EditorGUILayout.Space(5);

            // Setup Validation
            EditorGUILayout.LabelField("Setup Validation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            var rectTransform = view.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                EditorGUILayout.LabelField("RectTransform", "✓ Present", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("RectTransform", "✗ Missing (Required!)", EditorStyles.miniLabel);
            }

            var canvasGroupCheck = view.GetComponent<CanvasGroup>();
            if (canvasGroupCheck != null)
            {
                EditorGUILayout.LabelField("CanvasGroup", "✓ Present", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("CanvasGroup", "✗ Missing (Recommended)", EditorStyles.miniLabel);
                if (GUILayout.Button("Add CanvasGroup"))
                {
                    Undo.AddComponent<CanvasGroup>(view.gameObject);
                    Debug.Log($"[UIViewEditor] Added CanvasGroup to {view.name}");
                }
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);

            // Quick Setup
            if (GUILayout.Button("Setup UIView Components"))
            {
                SetupUIView(view);
            }
        }

        private void SetupUIView(UIViewBase view)
        {
            bool madeChanges = false;

            // Ensure RectTransform exists (Canvas UI requirement)
            if (view.GetComponent<RectTransform>() == null)
            {
                Debug.LogWarning($"[UIViewEditor] UIView requires RectTransform. GameObject {view.name} is not a UI element!");
            }

            // Add CanvasGroup if missing
            if (view.GetComponent<CanvasGroup>() == null)
            {
                Undo.AddComponent<CanvasGroup>(view.gameObject);
                Debug.Log($"[UIViewEditor] Added CanvasGroup to {view.name}");
                madeChanges = true;
            }

            if (madeChanges)
            {
                EditorUtility.SetDirty(view);
                Debug.Log($"[UIViewEditor] Setup complete for {view.name}");
            }
            else
            {
                Debug.Log($"[UIViewEditor] {view.name} already has all required components");
            }
        }
    }
}
