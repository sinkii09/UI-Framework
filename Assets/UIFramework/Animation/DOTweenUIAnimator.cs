using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UIFramework.Core;
using UnityEngine;

namespace UIFramework.Animation
{
    /// <summary>
    /// UI animator implementation using DOTween.
    /// Converts UITransitions to DOTween tweens and handles async completion.
    /// </summary>
    public class DOTweenUIAnimator : IUIAnimator
    {
        /// <summary>
        /// Plays a UI transition asynchronously.
        /// </summary>
        /// <param name="transition">The transition to play.</param>
        /// <param name="cancellationToken">Token to cancel the animation.</param>
        public async Task AnimateAsync(UITransition transition, CancellationToken cancellationToken = default)
        {
            if (transition == null || transition.Target == null)
            {
                Debug.LogWarning("[DOTweenUIAnimator] Transition or target is null. Skipping animation.");
                return;
            }

            Tween tween = null;

            try
            {
                // Create the tween
                tween = transition.CreateTween(transition.Target);

                if (tween == null)
                {
                    Debug.LogWarning($"[DOTweenUIAnimator] Failed to create tween for {transition.GetType().Name}");
                    return;
                }

                // Wait for completion
                await WaitForTweenAsync(tween, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Kill the tween if cancelled
                tween?.Kill();
                Debug.Log($"[DOTweenUIAnimator] Animation cancelled: {transition.GetType().Name}");
                throw;
            }
            catch (Exception ex)
            {
                tween?.Kill();
                Debug.LogError($"[DOTweenUIAnimator] Error during animation: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Waits for a tween to complete with cancellation support.
        /// </summary>
        private async Task WaitForTweenAsync(Tween tween, CancellationToken cancellationToken)
        {
            if (tween == null)
                return;

            // Create a TaskCompletionSource for the tween
            var tcs = new TaskCompletionSource<bool>();

            // Register cancellation
            using (cancellationToken.Register(() =>
            {
                tween.Kill();
                tcs.TrySetCanceled();
            }))
            {
                // Set up completion callback
                tween.OnComplete(() => tcs.TrySetResult(true));
                tween.OnKill(() => tcs.TrySetCanceled());

                // Wait for completion or cancellation
                await tcs.Task;
            }
        }
    }
}
