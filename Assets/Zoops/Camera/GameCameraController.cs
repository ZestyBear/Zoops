// File: Assets/Zoops/Camera/GameCameraController.cs
using UnityEngine;
using Zoops.Simulation;
using Zoops.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Zoops.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class GameCameraController : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] private SimulationRunner runner;
        [SerializeField] private WorldViewportRectProvider viewportProvider;

        [Header("Fit / Clamp")]
        [Tooltip("Extra padding in world-units around the arena when fitting and clamping.")]
        [SerializeField, Min(0f)] private float overscanWorldUnits = 1.0f;

        [Tooltip("If true, refits camera on WorldChanged. Recommended.")]
        [SerializeField] private bool refitOnWorldChanged = true;

        [Header("Zoom")]
        [SerializeField, Min(0.01f)] private float zoomSpeed = 4f;
        [SerializeField, Min(0.01f)] private float minOrthoSize = 1.0f;
        [SerializeField, Min(0.01f)] private float maxOrthoSize = 50.0f;

        [Header("Pan")]
        [SerializeField, Min(0.01f)] private float panSpeedWorldUnitsPerSecond = 15f;

        private UnityEngine.Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<UnityEngine.Camera>();
            if (_cam.orthographic == false)
                Debug.LogWarning("[GameCameraController] This controller expects an orthographic camera.");

            if (runner == null)
                runner = FindFirstObjectByType<SimulationRunner>();

            if (viewportProvider == null)
                viewportProvider = FindFirstObjectByType<WorldViewportRectProvider>();
        }

        private void OnEnable()
        {
            if (runner != null)
                runner.WorldChanged += HandleWorldChanged;
        }

        private void OnDisable()
        {
            if (runner != null)
                runner.WorldChanged -= HandleWorldChanged;
        }

        private void Start()
        {
            viewportProvider?.ApplyToCamera(force: true);

            if (refitOnWorldChanged)
                FitToWorld();
        }

        private void Update()
        {
            viewportProvider?.ApplyToCamera(force: false);

            if (runner == null || runner.World == null)
                return;

            HandleZoom();
            HandlePan();

            ClampToWorld();
        }

        private void HandleWorldChanged()
        {
            viewportProvider?.ApplyToCamera(force: true);

            if (refitOnWorldChanged)
                FitToWorld();
            else
                ClampToWorld();
        }

        private void FitToWorld()
        {
            if (runner == null || runner.World == null) return;

            SimulationWorld w = runner.World;

            float worldWidth = (w.MaxX - w.MinX) + (overscanWorldUnits * 2f);
            float worldHeight = (w.MaxY - w.MinY) + (overscanWorldUnits * 2f);

            float aspect = _cam.aspect;

            float sizeForHeight = worldHeight * 0.5f;
            float sizeForWidth = (worldWidth * 0.5f) / Mathf.Max(0.0001f, aspect);

            float targetSize = Mathf.Max(sizeForHeight, sizeForWidth);
            targetSize = Mathf.Clamp(targetSize, minOrthoSize, maxOrthoSize);

            _cam.orthographicSize = targetSize;

            float cx = (w.MinX + w.MaxX) * 0.5f;
            float cy = (w.MinY + w.MaxY) * 0.5f;

            Vector3 pos = transform.position;
            pos.x = cx;
            pos.y = cy;
            transform.position = pos;

            ClampToWorld();
        }

        private void HandleZoom()
        {
#if ENABLE_INPUT_SYSTEM
            float wheel = 0f;
            if (Mouse.current != null)
                wheel = Mouse.current.scroll.ReadValue().y / 120f; // normalize typical wheel delta

            if (Mathf.Abs(wheel) < 0.0001f) return;

            float size = _cam.orthographicSize;
            size -= wheel * zoomSpeed;
            size = Mathf.Clamp(size, minOrthoSize, maxOrthoSize);
            _cam.orthographicSize = size;
#else
            // If someone disables the Input System, do nothing rather than throw.
            return;
#endif
        }

        private void HandlePan()
        {
#if ENABLE_INPUT_SYSTEM
            float dx = 0f;
            float dy = 0f;

            if (Keyboard.current == null) return;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) dx -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) dx += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) dy -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) dy += 1f;

            if (dx == 0f && dy == 0f) return;

            Vector2 dir = new Vector2(dx, dy).normalized;
            Vector3 pos = transform.position;

            pos.x += dir.x * panSpeedWorldUnitsPerSecond * Time.unscaledDeltaTime;
            pos.y += dir.y * panSpeedWorldUnitsPerSecond * Time.unscaledDeltaTime;

            transform.position = pos;
#else
            return;
#endif
        }

        private void ClampToWorld()
        {
            if (runner == null || runner.World == null) return;

            SimulationWorld w = runner.World;

            float minX = w.MinX - overscanWorldUnits;
            float maxX = w.MaxX + overscanWorldUnits;
            float minY = w.MinY - overscanWorldUnits;
            float maxY = w.MaxY + overscanWorldUnits;

            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            float worldCx = (minX + maxX) * 0.5f;
            float worldCy = (minY + maxY) * 0.5f;

            Vector3 pos = transform.position;

            float clampMinX = minX + halfW;
            float clampMaxX = maxX - halfW;
            if (clampMinX > clampMaxX) pos.x = worldCx;
            else pos.x = Mathf.Clamp(pos.x, clampMinX, clampMaxX);

            float clampMinY = minY + halfH;
            float clampMaxY = maxY - halfH;
            if (clampMinY > clampMaxY) pos.y = worldCy;
            else pos.y = Mathf.Clamp(pos.y, clampMinY, clampMaxY);

            transform.position = pos;
        }
    }
}
