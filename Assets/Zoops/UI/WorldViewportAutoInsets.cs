using UnityEngine;

namespace Zoops.UI
{
    /// <summary>
    /// Automatically insets the UI_WorldViewport so it never overlaps
    /// the left panel or bottom bar.
    /// 
    /// UI drives viewport.
    /// Viewport drives Camera.rect.
    /// </summary>
    public sealed class WorldViewportAutoInsets : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] private RectTransform worldViewportRect;

        [Header("Inset Sources")]
        [SerializeField] private RectTransform leftPanelRect;   // width → left inset
        [SerializeField] private RectTransform bottomBarRect;   // height → bottom inset

        [Header("Behaviour")]
        [SerializeField] private bool applyEveryFrame = true;

        private float _lastLeft;
        private float _lastBottom;

        private void OnEnable()
        {
            ApplyNow(force: true);
        }

        private void LateUpdate()
        {
            if (!applyEveryFrame) return;
            ApplyNow(force: false);
        }

        public void ApplyNow()
        {
            ApplyNow(force: true);
        }

        private void ApplyNow(bool force)
        {
            if (worldViewportRect == null || leftPanelRect == null || bottomBarRect == null)
                return;

            float left = leftPanelRect.rect.width;
            float bottom = bottomBarRect.rect.height;

            if (!force &&
                Mathf.Approximately(left, _lastLeft) &&
                Mathf.Approximately(bottom, _lastBottom))
                return;

            _lastLeft = left;
            _lastBottom = bottom;

            var min = worldViewportRect.offsetMin;
            var max = worldViewportRect.offsetMax;

            min.x = left;
            min.y = bottom;

            max.x = 0f;
            max.y = 0f;

            worldViewportRect.offsetMin = min;
            worldViewportRect.offsetMax = max;
        }
    }
}
