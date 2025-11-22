using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace UIFramework.Core
{
    /// <summary>
    /// Factory for creating UI views with automatic dependency injection.
    /// Uses VContainer for ViewModel instantiation and IUILoader for prefab loading.
    /// </summary>
    public class UIViewFactory : IUIViewFactory
    {
        private readonly IUILoader _uiLoader;
        private readonly IObjectResolver _container;
        private readonly Transform _rootTransform;

        /// <summary>
        /// Creates a new UIViewFactory.
        /// </summary>
        /// <param name="uiLoader">The UI loader for loading prefabs.</param>
        /// <param name="container">The VContainer resolver for dependency injection.</param>
        public UIViewFactory(IUILoader uiLoader, IObjectResolver container)
        {
            _uiLoader = uiLoader ?? throw new ArgumentNullException(nameof(uiLoader));
            _container = container ?? throw new ArgumentNullException(nameof(container));

            // Create root transform for UI views
            var rootGo = new GameObject("[UI Views]");
            GameObject.DontDestroyOnLoad(rootGo);
            _rootTransform = rootGo.transform;
        }

        public async Task<TView> CreateAsync<TView, TViewModel>(
            TViewModel viewModel = null,
            CancellationToken cancellationToken = default)
            where TView : UIView<TViewModel>
            where TViewModel : class, IViewModel
        {
            var viewType = typeof(TView);
            var viewModelType = typeof(TViewModel);

            Debug.Log($"[UIViewFactory] Creating view: {viewType.Name} with ViewModel: {viewModelType.Name}");

            try
            {
                // Load the view prefab
                var prefab = await _uiLoader.LoadAsync<TView>(viewType.Name, cancellationToken);

                if (prefab == null)
                {
                    throw new InvalidOperationException(
                        $"Failed to load prefab for view '{viewType.Name}'. " +
                        "Ensure the prefab exists in Resources/UI/ or is addressable with the correct key.");
                }

                // Instantiate the view
                var viewInstance = GameObject.Instantiate(prefab, _rootTransform);
                viewInstance.name = viewType.Name; // Clean up name (remove "(Clone)")

                // Create or resolve ViewModel
                TViewModel viewModelInstance;

                if (viewModel != null)
                {
                    // Use provided ViewModel
                    viewModelInstance = viewModel;
                    Debug.Log($"[UIViewFactory] Using provided ViewModel instance.");
                }
                else
                {
                    // Try to resolve ViewModel from container
                    try
                    {
                        viewModelInstance = _container.Resolve<TViewModel>();
                        Debug.Log($"[UIViewFactory] Resolved ViewModel from container: {viewModelType.Name}");
                    }
                    catch
                    {
                        // If not registered, try to instantiate manually
                        Debug.LogWarning($"[UIViewFactory] ViewModel '{viewModelType.Name}' not registered in container. Attempting manual instantiation.");
                        viewModelInstance = TryCreateViewModelManually<TViewModel>();
                    }
                }

                // Initialize the view with its ViewModel
                viewInstance.Initialize(viewModelInstance);

                Debug.Log($"[UIViewFactory] View created successfully: {viewType.Name}");
                return viewInstance;
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[UIViewFactory] View creation cancelled: {viewType.Name}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIViewFactory] Failed to create view '{viewType.Name}': {ex.Message}");
                throw;
            }
        }

        public void Destroy(IUIView view)
        {
            if (view == null)
            {
                Debug.LogWarning("[UIViewFactory] Attempted to destroy null view.");
                return;
            }

            Debug.Log($"[UIViewFactory] Destroying view: {view.GetType().Name}");

            view.Destroy();
        }

        /// <summary>
        /// Attempts to create a ViewModel manually using VContainer's injection.
        /// Falls back to default constructor if VContainer injection fails.
        /// </summary>
        private TViewModel TryCreateViewModelManually<TViewModel>() where TViewModel : class, IViewModel
        {
            throw new NotImplementedException("Manual ViewModel instantiation is not implemented yet.");
            //try
            //{
            //    // Try to inject dependencies using VContainer
            //    return _container.Instantiate<TViewModel>();
            //}
            //catch (Exception ex)
            //{
            //    Debug.LogWarning($"[UIViewFactory] VContainer instantiation failed for {typeof(TViewModel).Name}: {ex.Message}. Trying default constructor.");

            //    // Fall back to default constructor
            //    try
            //    {
            //        return Activator.CreateInstance<TViewModel>();
            //    }
            //    catch (Exception activatorEx)
            //    {
            //        throw new InvalidOperationException(
            //            $"Failed to create ViewModel '{typeof(TViewModel).Name}'. " +
            //            "Ensure it has a parameterless constructor or all dependencies are registered in VContainer.",
            //            activatorEx);
            //    }
            //}
        }
    }
}
