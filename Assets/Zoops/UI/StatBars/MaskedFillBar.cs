using UnityEngine;

namespace Zoops.UI
{
    /// <summary>
    /// Generic width-driven bar fill for rounded (9-sliced) sprites using a RectMask2D clip.
    /// Feed it a normalized 0..1 value; it sizes Fill to match the current bar width.
    /// </summary>
    public sealed class MaskedFillBar : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private RectTransform barArea;   // usually the bar root RectTransform
        [SerializeField] private RectTransform fillRect;  // Mask/Fill RectTransform (left-anchored)

        [Header("Visual safety")]
        [Tooltip("Minimum visible width (px) so 9-sliced rounded caps don't look broken at tiny values.")]
        [SerializeField, Min(0f)] private float minFillWidthPx = 6f;

        private float _last01 = -1f;

        private void Reset()
        {
            barArea = transform as RectTransform;
        }

        public void Set01(float value01)
        {
            if (barArea == null || fillRect == null) return;

            value01 = Mathf.Clamp01(value01);
            if (Mathf.Approximately(value01, _last01)) return;
            _last01 = value01;

            float fullWidth = barArea.rect.width;
            float targetWidth = fullWidth * value01;

            if (value01 > 0f)
                targetWidth = Mathf.Max(targetWidth, minFillWidthPx);

            // Assumes Fill anchors Min(0,0) Max(0,1) and pivot (0,0.5).
            // Width is controlled by sizeDelta.x in that configuration.
            fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        }
    }
}
