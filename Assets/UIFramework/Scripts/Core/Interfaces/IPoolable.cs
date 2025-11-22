namespace UIFramework.Core
{
    /// <summary>
    /// Interface for objects that can be pooled.
    /// Implement this to receive callbacks when spawned from or returned to a pool.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when this object is retrieved from the pool.
        /// Use this to reset state to default values.
        /// </summary>
        void OnSpawnedFromPool();

        /// <summary>
        /// Called when this object is returned to the pool.
        /// Use this to cleanup (stop animations, clear references, etc.)
        /// </summary>
        void OnReturnedToPool();
    }
}
