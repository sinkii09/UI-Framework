using DG.Tweening;
using UnityEngine;

namespace UIFramework.Animation
{
    /// <summary>
    /// Abstract base class for UI transitions.
    /// Provides an abstraction layer over DOTween for UI animations.
    /// </summary>
    public abstract class UITransition
    {
        /// <summary>
        /// Duration of the transition in seconds.
        /// </summary>
        public float Duration { get; set; } = 0.3f;

        /// <summary>
        /// Easing function for the transition.
        /// </summary>
        public Ease EaseType { get; set; } = Ease.OutCubic;

        /// <summary>
        /// The target transform for this transition (set by the animator).
        /// </summary>
        public Transform Target { get; set; }

        /// <summary>
        /// Creates a DOTween tween for this transition.
        /// </summary>
        /// <param name="target">The transform to animate.</param>
        /// <returns>The created tween.</returns>
        public abstract Tween CreateTween(Transform target);

        /// <summary>
        /// Creates a copy of this transition (for reuse).
        /// </summary>
        /// <returns>A cloned transition.</returns>
        public virtual UITransition Clone()
        {
            return (UITransition)this.MemberwiseClone();
        }
    }
}
