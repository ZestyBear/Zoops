// File: Assets/Zoops/Scripts/Sim/Observation.cs
namespace Zoops.Simulation
{
    /// <summary>
    /// Brain input contract: deterministic, sim-owned, no Unity types.
    /// This is the "what the zoop can sense right now" snapshot for decision-making.
    /// </summary>
    public readonly struct Observation
    {
        // --- Self ---
        public readonly float Energy01;   // energy normalized 0..1 (world decides normalization)
        public readonly float X;
        public readonly float Y;

        // --- Nearest plant (today's food) ---
        public readonly bool HasNearestPlant;
        public readonly float PlantDx;
        public readonly float PlantDy;
        public readonly float PlantDist;  // world-space distance

        // --- Border avoidance hint (world can provide a push vector) ---
        public readonly float BorderPushX;
        public readonly float BorderPushY;

        // --- Genes (optional inputs for future policies/NN; safe even for hardcoded) ---
        public readonly float MoveSpeed01;
        public readonly float VisionRange01;
        public readonly float ReproThreshold01;

        public Observation(
            float energy01,
            float x,
            float y,
            bool hasNearestPlant,
            float plantDx,
            float plantDy,
            float plantDist,
            float borderPushX,
            float borderPushY,
            float moveSpeed01,
            float visionRange01,
            float reproThreshold01)
        {
            Energy01 = energy01;
            X = x;
            Y = y;

            HasNearestPlant = hasNearestPlant;
            PlantDx = plantDx;
            PlantDy = plantDy;
            PlantDist = plantDist;

            BorderPushX = borderPushX;
            BorderPushY = borderPushY;

            MoveSpeed01 = moveSpeed01;
            VisionRange01 = visionRange01;
            ReproThreshold01 = reproThreshold01;
        }
    }
}
