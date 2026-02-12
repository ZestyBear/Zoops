// File: Assets/Zoops/UI/WorldViewportRectProvider.cs
using UnityEngine;

namespace Zoops.UI
{
    /// <summary>
    /// Single source of truth for which portion of the screen is the "world view".
    /// Converts a UI RectTransform into a normalized viewport rect
    /// and applies it to a target Camera.
    /// </summary>
    public sealed class WorldViewportRectProvider : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] private RectTransform worldViewportRect;
        [SerializeField] private UnityEngine.Camera targetCamera;

        [Header("Behaviour")]
        [Tooltip("If enabled, applies the camera rect every frame (useful while iterating UI layout).")]
        [SerializeField] private bool applyEveryFrame = true;

        private Rect _lastApplied;

        /// <summary>
        /// The normalized (0..1) viewport rect derived from the UI RectTransform.
        /// </summary>
        public Rect NormalizedViewportRect
        {
            get
            {
                if (worldViewportRect == null)
                    return new Rect(0f, 0f, 1f, 1f);

                return RectTransformToNormalizedViewport(worldViewportRect);
            }
        }

        private void Reset()
        {
            targetCamera = UnityEngine.Camera.main;
        }

        private void Awake()
        {
            ApplyToCamera(force: true);
        }

        private void LateUpdate()
        {
            if (!applyEveryFrame) return;
            ApplyToCamera(force: false);
        }

        public void ApplyToCamera(bool force)
        {
            if (targetCamera == null)
                return;

            Rect r = NormalizedViewportRect;

            if (!force && ApproximatelyEqual(_lastApplied, r))
                return;

            targetCamera.rect = r;
            _lastApplied = r;
        }

        private static Rect RectTransformToNormalizedViewport(RectTransform rt)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            float xMin = corners[0].x / Screen.width;
            float yMin = corners[0].y / Screen.height;
            float xMax = corners[2].x / Screen.width;
            float yMax = corners[2].y / Screen.height;

            xMin = Mathf.Clamp01(xMin);
            yMin = Mathf.Clamp01(yMin);
            xMax = Mathf.Clamp01(xMax);
            yMax = Mathf.Clamp01(yMax);

            float width = Mathf.Max(0f, xMax - xMin);
            float height = Mathf.Max(0f, yMax - yMin);

            return new Rect(xMin, yMin, width, height);
        }

        private static bool ApproximatelyEqual(Rect a, Rect b)
        {
            const float eps = 0.0005f;

            return Mathf.Abs(a.x - b.x) < eps
                && Mathf.Abs(a.y - b.y) < eps
                && Mathf.Abs(a.width - b.width) < eps
                && Mathf.Abs(a.height - b.height) < eps;
        }
    }
}
