using DG.Tweening;
using UnityEngine;

namespace UIFramework.Animation
{
    /// <summary>
    /// Fade in/out transition using CanvasGroup alpha.
    /// </summary>
    public class FadeTransition : UITransition
    {
        /// <summary>
        /// Starting alpha value.
        /// </summary>
        public float FromAlpha { get; set; } = 0f;

        /// <summary>
        /// Ending alpha value.
        /// </summary>
        public float ToAlpha { get; set; } = 1f;

        public override Tween CreateTween(Transform target)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = FromAlpha;
            return canvasGroup.DOFade(ToAlpha, Duration).SetEase(EaseType);
        }
    }
}
