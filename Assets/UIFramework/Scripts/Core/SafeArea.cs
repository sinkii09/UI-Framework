using UnityEngine;

namespace UIFramework.Core
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect currentSafeArea;
        private Vector2Int currentScreenSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (currentSafeArea != Screen.safeArea ||
                currentScreenSize.x != Screen.width ||
                currentScreenSize.y != Screen.height)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            currentSafeArea = Screen.safeArea;
            currentScreenSize = new Vector2Int(Screen.width, Screen.height);

            // Convert safe area from screen pixels to normalized anchor coordinates
            Vector2 anchorMin = currentSafeArea.position;
            Vector2 anchorMax = currentSafeArea.position + currentSafeArea.size;

            anchorMin.x /= currentScreenSize.x;
            anchorMin.y /= currentScreenSize.y;
            anchorMax.x /= currentScreenSize.x;
            anchorMax.y /= currentScreenSize.y;

            // Apply to RectTransform
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}