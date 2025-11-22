using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// Base interface for all UI Views.
    /// Views are responsible for visual presentation only.
    /// </summary>
    public interface IUIView
    {
        /// <summary>
        /// The GameObject this view is attached to.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// The Transform component.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// The RectTransform component (for UI elements).
        /// </summary>
        RectTransform RectTransform { get; }

        /// <summary>
        /// Shows the view to the user.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the view from the user.
        /// </summary>
        void Hide();

        /// <summary>
        /// Destroys the view and cleans up resources.
        /// </summary>
        void Destroy();
    }
}
