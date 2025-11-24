using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UIFramework.Editor.CodeGen
{
    /// <summary>
    /// Generates ViewModelRegistry.cs with all ViewModel registrations.
    /// Runs automatically when scripts compile or manually via menu.
    /// </summary>
    public static class ViewModelRegistryGenerator
    {
        private const string SEARCH_PATH = "Assets/UIFramework/Scripts/UI";
        private const string OUTPUT_PATH = "Assets/UIFramework/Scripts/UI/Generated/ViewModelRegistry.cs";
        private const string NAMESPACE = "UIFramework.UI";

        [MenuItem("UIFramework/Code Generation/Generate ViewModel Registry", priority = 200)]
        public static void GenerateRegistry()
        {
            Debug.Log("[ViewModelRegistryGenerator] Scanning for ViewModels...");

            var viewModels = FindAllViewModels();

            if (viewModels.Count == 0)
            {
                Debug.LogWarning("[ViewModelRegistryGenerator] No ViewModels found.");
            }

            var code = GenerateCode(viewModels);
            WriteCodeToFile(code);

            Debug.Log($"[ViewModelRegistryGenerator] Generated registry with {viewModels.Count} ViewModel(s)");
            EditorUtility.DisplayDialog("Success",
                $"Generated ViewModelRegistry.cs with {viewModels.Count} ViewModel(s)!",
                "OK");
        }

        /// <summary>
        /// Auto-generate on script compilation (optional - can be disabled if too slow)
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            // Auto-generate registry when scripts compile
            // Comment this out if you prefer manual generation only
            GenerateRegistryIfNeeded();
        }

        private static void GenerateRegistryIfNeeded()
        {
            // Only auto-generate if registry doesn't exist or ViewModels changed
            if (!File.Exists(OUTPUT_PATH))
            {
                Debug.Log("[ViewModelRegistryGenerator] Auto-generating registry...");
                GenerateRegistry();
            }
        }

        private static List<Type> FindAllViewModels()
        {
            var viewModelBaseType = typeof(UIFramework.Core.ViewModelBase);
            var viewModels = new List<Type>();

            // Check if search directory exists
            if (!Directory.Exists(SEARCH_PATH))
            {
                Debug.LogWarning($"[ViewModelRegistryGenerator] Search path does not exist: {SEARCH_PATH}");
                return viewModels;
            }

            // Find all C# files in search directory
            var csFiles = Directory.GetFiles(SEARCH_PATH, "*.cs", SearchOption.AllDirectories);

            foreach (var filePath in csFiles)
            {
                try
                {
                    // Skip generated files
                    if (filePath.Contains("Generated"))
                    {
                        continue;
                    }

                    var typeInfos = ParseViewModelsFromFile(filePath);

                    foreach (var typeInfo in typeInfos)
                    {
                        // Try to get the actual Type object
                        var type = FindTypeByName(typeInfo.FullName);

                        if (type != null &&
                            type.IsClass &&
                            !type.IsAbstract &&
                            viewModelBaseType.IsAssignableFrom(type) &&
                            type != viewModelBaseType &&
                            // Exclude examples
                            !type.Namespace?.StartsWith("UIFramework.Examples") == true)
                        {
                            viewModels.Add(type);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ViewModelRegistryGenerator] Error parsing {filePath}: {ex.Message}");
                }
            }

            return viewModels.OrderBy(t => t.FullName).ToList();
        }

        private class TypeInfo
        {
            public string Namespace { get; set; }
            public string ClassName { get; set; }
            public string FullName => string.IsNullOrEmpty(Namespace) ? ClassName : $"{Namespace}.{ClassName}";
        }

        private static List<TypeInfo> ParseViewModelsFromFile(string filePath)
        {
            var result = new List<TypeInfo>();
            var content = File.ReadAllText(filePath);

            // Extract namespace (optional)
            string currentNamespace = null;
            var namespaceMatch = System.Text.RegularExpressions.Regex.Match(content, @"namespace\s+([\w\.]+)");
            if (namespaceMatch.Success)
            {
                currentNamespace = namespaceMatch.Groups[1].Value;
            }

            // Find all class declarations that inherit from ViewModelBase
            var classMatches = System.Text.RegularExpressions.Regex.Matches(
                content,
                @"class\s+(\w+)\s*:\s*\w*ViewModelBase",
                System.Text.RegularExpressions.RegexOptions.Multiline
            );

            foreach (System.Text.RegularExpressions.Match match in classMatches)
            {
                var className = match.Groups[1].Value;
                result.Add(new TypeInfo
                {
                    Namespace = currentNamespace,
                    ClassName = className
                });
            }

            return result;
        }

        private static Type FindTypeByName(string fullTypeName)
        {
            // Search in all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // Skip Unity and system assemblies for performance
                if (ShouldSkipAssembly(assembly.GetName().Name))
                {
                    continue;
                }

                try
                {
                    var type = assembly.GetType(fullTypeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch
                {
                    // Ignore errors in individual assemblies
                }
            }

            return null;
        }

        private static bool ShouldSkipAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("Unity") ||
                   assemblyName.StartsWith("System") ||
                   assemblyName.StartsWith("mscorlib") ||
                   assemblyName.StartsWith("netstandard") ||
                   assemblyName.StartsWith("Microsoft") ||
                   assemblyName.StartsWith("Mono.") ||
                   assemblyName.StartsWith("nunit") ||
                   assemblyName.StartsWith("VContainer") ||
                   assemblyName.Contains("Editor");
        }

        private static string GenerateCode(List<Type> viewModels)
        {
            var code = new StringBuilder();

            // Header
            code.AppendLine("// <auto-generated>");
            code.AppendLine("// This file is automatically generated by ViewModelRegistryGenerator.");
            code.AppendLine("// Do not modify manually - changes will be overwritten!");
            code.AppendLine($"// Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            code.AppendLine("// </auto-generated>");
            code.AppendLine();

            // Usings
            code.AppendLine("using VContainer;");
            code.AppendLine();

            // Namespace
            code.AppendLine($"namespace {NAMESPACE}");
            code.AppendLine("{");

            // Class
            code.AppendLine("    /// <summary>");
            code.AppendLine($"    /// Auto-generated ViewModel registry with {viewModels.Count} ViewModel(s).");
            code.AppendLine("    /// </summary>");
            code.AppendLine("    public static class ViewModelRegistry");
            code.AppendLine("    {");

            // Register method
            code.AppendLine("        /// <summary>");
            code.AppendLine("        /// Registers all ViewModels in the project.");
            code.AppendLine("        /// Call this from UIFrameworkInstaller.Configure()");
            code.AppendLine("        /// </summary>");
            code.AppendLine("        public static void RegisterAll(IContainerBuilder builder)");
            code.AppendLine("        {");

            // Register each ViewModel
            foreach (var viewModel in viewModels)
            {
                code.AppendLine($"            builder.Register<{viewModel.FullName}>(Lifetime.Transient);");
            }

            code.AppendLine("        }");
            code.AppendLine("    }");
            code.AppendLine("}");

            return code.ToString();
        }

        private static void WriteCodeToFile(string code)
        {
            var directory = Path.GetDirectoryName(OUTPUT_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(OUTPUT_PATH, code);
            AssetDatabase.Refresh();
        }
    }
}
