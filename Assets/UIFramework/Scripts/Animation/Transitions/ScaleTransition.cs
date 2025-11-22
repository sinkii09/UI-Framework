using DG.Tweening;
using UnityEngine;

namespace UIFramework.Animation
{
    /// <summary>
    /// Scale transition with optional punch effect.
    /// </summary>
    public class ScaleTransition : UITransition
    {
        /// <summary>
        /// Starting scale.
        /// </summary>
        public Vector3 FromScale { get; set; } = Vector3.zero;

        /// <summary>
        /// Ending scale.
        /// </summary>
        public Vector3 ToScale { get; set; } = Vector3.one;

        /// <summary>
        /// If true, adds a punch effect after scaling.
        /// </summary>
        public bool UsePunchEffect { get; set; } = false;

        /// <summary>
        /// Strength of the punch effect (0-1).
        /// </summary>
        public float PunchStrength { get; set; } = 0.1f;

        public override Tween CreateTween(Transform target)
        {
            target.localScale = FromScale;

            if (UsePunchEffect)
            {
                var sequence = DOTween.Sequence();
                sequence.Append(target.DOScale(ToScale, Duration).SetEase(EaseType));
                sequence.Append(target.DOPunchScale(
                    Vector3.one * PunchStrength,
                    Duration * 0.3f,
                    vibrato: 10,
                    elasticity: 1f));
                return sequence;
            }

            return target.DOScale(ToScale, Duration).SetEase(EaseType);
        }
    }
}
