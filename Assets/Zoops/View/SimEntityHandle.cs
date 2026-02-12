// File: SimEntityHandle.cs
using UnityEngine;

namespace Zoops.View
{
    /// <summary>
    /// View-side identity tag that associates a Unity GameObject
    /// with a simulation entity identity.
    ///
    /// This never accesses the sim. It only stores the EntityId assigned by a view system.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SimEntityHandle : MonoBehaviour
    {
        [Tooltip("Authoritative simulation EntityId this view represents. -1 = unbound.")]
        [SerializeField] private int entityId = -1;

        public int EntityId
        {
            get => entityId;
            set => entityId = value;
        }
    }
}
