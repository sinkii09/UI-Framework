using DG.Tweening;
using UnityEngine;

namespace UIFramework.Animation
{
    /// <summary>
    /// Slide transition from a specified direction.
    /// </summary>
    public class SlideTransition : UITransition
    {
        public enum SlideDirection
        {
            Left,
            Right,
            Up,
            Down
        }

        /// <summary>
        /// Direction to slide from.
        /// </summary>
        public SlideDirection Direction { get; set; } = SlideDirection.Left;

        /// <summary>
        /// Distance to slide (default: screen width/height).
        /// </summary>
        public float Distance { get; set; } = 1920f;

        /// <summary>
        /// If true, uses the element's anchored position. If false, uses world position.
        /// </summary>
        public bool UseAnchoredPosition { get; set; } = true;

        public override Tween CreateTween(Transform target)
        {
            var rectTransform = target as RectTransform;
            if (rectTransform == null)
            {
                Debug.LogError($"[SlideTransition] Target must have RectTransform. Found: {target.GetType().Name}");
                return null;
            }

            var offset = GetOffset();
            var startPos = rectTransform.anchoredPosition + offset;
            var endPos = rectTransform.anchoredPosition;

            rectTransform.anchoredPosition = startPos;
            return rectTransform.DOAnchorPos(endPos, Duration).SetEase(EaseType);
        }

        private Vector2 GetOffset()
        {
            return Direction switch
            {
                SlideDirection.Left => new Vector2(-Distance, 0),
                SlideDirection.Right => new Vector2(Distance, 0),
                SlideDirection.Up => new Vector2(0, Distance),
                SlideDirection.Down => new Vector2(0, -Distance),
                _ => Vector2.zero
            };
        }
    }
}
