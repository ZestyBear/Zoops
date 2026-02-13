// File: Assets/Zoops/Scripts/Sim/Intent.cs
namespace Zoops.Simulation
{
    /// <summary>
    /// Brain output contract: deterministic, sim-owned, no Unity types.
    /// The brain proposes; physics applies.
    /// </summary>
    public struct Intent
    {
        /// <summary>
        /// Desired movement direction in world space (not necessarily normalized).
        /// Physics lane will normalize/clamp as needed.
        /// </summary>
        public float MoveX;
        public float MoveY;

        public static Intent None => new Intent { MoveX = 0f, MoveY = 0f };
    }
}
