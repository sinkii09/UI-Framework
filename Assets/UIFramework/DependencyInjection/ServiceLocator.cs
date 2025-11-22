using System;
using UnityEngine;
using VContainer;

namespace UIFramework.DependencyInjection
{
    /// <summary>
    /// Service locator wrapper for VContainer.
    /// Provides convenient global access to services for non-injected contexts.
    /// Prefer constructor injection when possible.
    /// </summary>
    public static class ServiceLocator
    {
        private static IObjectResolver _container;

        /// <summary>
        /// Gets whether the service locator has been initialized.
        /// </summary>
        public static bool IsInitialized => _container != null;

        /// <summary>
        /// Initializes the service locator with a VContainer resolver.
        /// </summary>
        /// <param name="container">The VContainer object resolver.</param>
        public static void Initialize(IObjectResolver container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (_container != null)
            {
                Debug.LogWarning("[ServiceLocator] Already initialized. Overwriting existing container.");
            }

            _container = container;
            Debug.Log("[ServiceLocator] Initialized with VContainer.");
        }

        /// <summary>
        /// Gets a service from the container.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The resolved service instance.</returns>
        public static T Get<T>()
        {
            if (_container == null)
            {
                throw new InvalidOperationException(
                    "ServiceLocator has not been initialized. " +
                    "Ensure UIFrameworkInstaller is in the scene and has been initialized.");
            }

            try
            {
                return _container.Resolve<T>();
            }
            catch (VContainerException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to resolve service of type '{typeof(T).Name}'. " +
                    "Make sure the service is registered in UIFrameworkInstaller.",
                    ex);
            }
        }

        /// <summary>
        /// Tries to get a service, returning default if not registered.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The service instance, or default if not registered.</returns>
        public static T TryGet<T>() where T : class
        {
            if (_container == null)
            {
                return null;
            }

            try
            {
                return _container.Resolve<T>();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resets the service locator (useful for testing or scene unload).
        /// </summary>
        public static void Reset()
        {
            _container = null;
            Debug.Log("[ServiceLocator] Reset.");
        }
    }
}
