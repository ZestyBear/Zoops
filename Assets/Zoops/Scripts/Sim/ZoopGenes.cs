// File: Assets/Zoops/Scripts/Sim/ZoopGenes.cs
namespace Zoops.Simulation
{
    /// <summary>
    /// Heritable, mostly-static configuration for a Zoop.
    /// These values are copied parent -> child with mutation.
    /// </summary>
    public struct ZoopGenes
    {
        // --- Body genes (minimal set for MVP selection pressure) ---

        /// <summary>
        /// World-space movement speed multiplier.
        /// </summary>
        public float MoveSpeed;

        /// <summary>
        /// How far this Zoop can "see" plants (world units).
        /// </summary>
        public float VisionRange;

        /// <summary>
        /// Energy threshold required to reproduce.
        /// </summary>
        public float ReproThreshold;

        public ZoopGenes(
            float moveSpeed,
            float visionRange,
            float reproThreshold)
        {
            MoveSpeed = moveSpeed;
            VisionRange = visionRange;
            ReproThreshold = reproThreshold;
        }
    }
}
