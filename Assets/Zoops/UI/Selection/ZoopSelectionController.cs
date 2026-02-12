// File: ZoopSelectionController.cs
using UnityEngine;
using Zoops.View;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Zoops.UI.Selection
{
    /// <summary>
    /// Unity-side selection controller.
    /// Click a Zoop (Collider2D) to select by EntityId.
    /// Input System only (guards included).
    /// </summary>
    public sealed class ZoopSelectionController : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] private UnityEngine.Camera worldCamera;

        [Header("Output")]
        [SerializeField] private Zoops.UI.SimUIPresenter presenter;

        [Header("Behaviour")]
        [SerializeField] private bool clearSelectionOnEmptyClick = true;

        private void Awake()
        {
            if (worldCamera == null)
                worldCamera = UnityEngine.Camera.main;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            if (worldCamera == null || presenter == null) return;

            Vector2 screen = Mouse.current.position.ReadValue();
            Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));

            RaycastHit2D hit = Physics2D.Raycast((Vector2)world, Vector2.zero);

            if (hit.collider != null)
            {
                var handle = hit.collider.GetComponent<SimEntityHandle>();
                if (handle != null)
                {
                    presenter.SetSelectedEntity(handle.EntityId);
                    return;
                }
            }

            if (clearSelectionOnEmptyClick)
                presenter.SetSelectedEntity(null);
#else
            // If Input System isn't enabled, do nothing rather than throw.
            return;
#endif
        }
    }
}
