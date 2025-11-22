using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UIFramework.Animation
{
    /// <summary>
    /// Composite transition that plays multiple transitions sequentially or in parallel.
    /// </summary>
    public class SequenceTransition : UITransition
    {
        /// <summary>
        /// The transitions to play.
        /// </summary>
        public List<UITransition> Transitions { get; set; } = new List<UITransition>();

        /// <summary>
        /// If true, plays all transitions in parallel. If false, plays sequentially.
        /// </summary>
        public bool PlayInParallel { get; set; } = false;

        public override Tween CreateTween(Transform target)
        {
            if (Transitions == null || Transitions.Count == 0)
            {
                Debug.LogWarning("[SequenceTransition] No transitions defined.");
                return null;
            }

            var sequence = DOTween.Sequence();

            foreach (var transition in Transitions)
            {
                if (transition == null)
                    continue;

                var tween = transition.CreateTween(target);

                if (tween == null)
                    continue;

                if (PlayInParallel)
                {
                    sequence.Join(tween);
                }
                else
                {
                    sequence.Append(tween);
                }
            }

            return sequence;
        }
    }
}
