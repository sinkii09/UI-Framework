using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace UIFramework.Core
{
    /// <summary>
    /// Factory for creating UI views with automatic dependency injection and pooling.
    /// Uses UIObjectPool for view reuse, VContainer for ViewModel instantiation.
    /// </summary>
    public class UIViewFactory : IUIViewFactory
    {
        private readonly IUIObjectPool _objectPool;
        private readonly IObjectResolver _container;
        private readonly Transform _rootTransform;

        /// <summary>
        /// Creates a new UIViewFactory.
        /// </summary>
        /// <param name="objectPool">The object pool for reusing views.</param>
        /// <param name="container">The VContainer resolver for dependency injection.</param>
        /// <param name="safeArea">The SafeArea RectTransform where UI views will be instantiated.</param>
        public UIViewFactory(IUIObjectPool objectPool, IObjectResolver container, RectTransform safeArea)
        {
            _objectPool = objectPool ?? throw new ArgumentNullException(nameof(objectPool));
            _container = container ?? throw new ArgumentNullException(nameof(container));

            if (safeArea == null)
            {
                throw new ArgumentNullException(nameof(safeArea),
                    "SafeArea is not assigned! " +
                    "This should be auto-created by UIFrameworkInstaller.");
            }

            _rootTransform = safeArea;
            Debug.Log($"[UIViewFactory] Using SafeArea: {safeArea.name}");
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
                // Get view from pool (pool handles prefab loading and instantiation)
                var viewInstance = await _objectPool.GetAsync<TView>(cancellationToken);

                // Move to SafeArea and set name
                viewInstance.transform.SetParent(_rootTransform, false);
                viewInstance.name = viewType.Name;

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
                    catch (Exception resolveEx)
                    {
                        // If not registered, try to instantiate manually
                        Debug.LogWarning($"[UIViewFactory] ViewModel '{viewModelType.Name}' not registered in container. Attempting manual instantiation with constructor injection.");
                        Debug.LogWarning($"[UIViewFactory] Resolve error: {resolveEx.Message}");
                        viewModelInstance = TryCreateViewModelManually<TViewModel>();
                    }
                }

                if (viewModelInstance == null)
                {
                    throw new InvalidOperationException(
                        $"Failed to create or resolve ViewModel '{viewModelType.Name}'. " +
                        "ViewModel instance is null after all creation attempts.");
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

            Debug.Log($"[UIViewFactory] Returning view to pool: {view.GetType().Name}");

            // Cleanup the view before returning to pool
            view.Cleanup();

            _objectPool.Return(view);
        }

        /// <summary>
        /// Attempts to create a ViewModel manually using constructor injection.
        /// Resolves constructor parameters from VContainer and falls back to default constructor.
        /// </summary>
        private TViewModel TryCreateViewModelManually<TViewModel>() where TViewModel : class, IViewModel
        {
            var viewModelType = typeof(TViewModel);

            // Get all constructors
            var constructors = viewModelType.GetConstructors();

            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"No public constructors found for {viewModelType.Name}");
            }

            // Try each constructor, starting with the one with most parameters
            foreach (var constructor in constructors.OrderByDescending(c => c.GetParameters().Length))
            {
                var parameters = constructor.GetParameters();
                var resolvedParams = new object[parameters.Length];
                bool allResolved = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        resolvedParams[i] = _container.Resolve(parameters[i].ParameterType);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[UIViewFactory] Failed to resolve {parameters[i].ParameterType.Name} for {viewModelType.Name}: {ex.Message}");
                        allResolved = false;
                        break;
                    }
                }

                if (allResolved)
                {
                    try
                    {
                        Debug.Log($"[UIViewFactory] Creating {viewModelType.Name} with {parameters.Length} injected dependencies.");
                        var instance = (TViewModel)constructor.Invoke(resolvedParams);
                        if (instance == null)
                        {
                            throw new InvalidOperationException($"Constructor returned null for {viewModelType.Name}");
                        }
                        return instance;
                    }
                    catch (Exception invokeEx)
                    {
                        throw new InvalidOperationException(
                            $"Failed to invoke constructor for {viewModelType.Name}: {invokeEx.Message}",
                            invokeEx);
                    }
                }
            }

            // If no constructor with dependencies worked, try parameterless constructor
            try
            {
                Debug.LogWarning($"[UIViewFactory] Could not resolve dependencies for {viewModelType.Name}. Trying parameterless constructor.");
                var instance = Activator.CreateInstance<TViewModel>();
                if (instance == null)
                {
                    throw new InvalidOperationException($"Activator.CreateInstance returned null for {viewModelType.Name}");
                }
                return instance;
            }
            catch (MissingMethodException)
            {
                throw new InvalidOperationException(
                    $"Failed to create ViewModel '{viewModelType.Name}'. " +
                    "No parameterless constructor found and dependencies could not be resolved. " +
                    "Make sure UINavigator, UIEventBus, and other required services are registered in VContainer.");
            }
        }
    }
}
